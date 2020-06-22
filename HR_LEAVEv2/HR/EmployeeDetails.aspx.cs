using HR_LEAVEv2.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.HR
{
    public partial class EmployeeDetails : System.Web.UI.Page
    {
        Util util = new Util();

        // the following enum is used when getting or setting data in datatables 
        // as related to the employment records
        enum emp_records_columns
        {
            record_id = 0,
            employment_type = 1,
            dept_id = 2,
            dept_name = 3,
            pos_id = 4,
            pos_name = 5,
            start_date = 6,
            expected_end_date = 7,
            isChanged = 8,
            actual_end_date = 9,
            status = 10,
            status_class = 11,
            annual_vacation_amt = 12,
            max_vacation_accumulation = 13
        };

        private DataTable empRecordsDataSource
        {
            get { return ViewState["empRecordsDataSource"] != null ? ViewState["empRecordsDataSource"] as DataTable : null; } 
            set { ViewState["empRecordsDataSource"] = value; }
        }

        private int recordBeingEdited
        {
            get { return ViewState["recordBeingEdited"] != null ? Convert.ToInt32(ViewState["recordBeingEdited"]) : -1; }
            set { ViewState["recordBeingEdited"] = value; }
        }

        private List<string> previousRoles
        {
            get { return ViewState["previousRoles"] != null ? (List<string>)ViewState["previousRoles"] : null; }
            set { ViewState["previousRoles"] = value; }
        }
        private Dictionary<string, string> previousLeaveBalances
        {
            get { return ViewState["previousLeaveBalances"] != null ? (Dictionary<string, string>)ViewState["previousLeaveBalances"] : null; }
            set { ViewState["previousLeaveBalances"] = value; }
        }


        
        public bool isEditMode { get; set; }

        // used to check if user can view page 
        List<string> permissions = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            permissions = (List<string>)Session["permissions"];

            if (permissions == null || !(permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions") || permissions.Contains("hr3_permissions")))
                Response.Redirect("~/AccessDenied.aspx");

            // validator for end date when adding employment records
            // an end date is required for Public service employment types but is autofilled if empty for Contract employment types
            //endDateRequiredValidator.Enabled = empTypeList.SelectedValue == "Public Service";

            if (!this.IsPostBack)
            {

                empRecordsDataSource = null;
                if (Request.QueryString.HasKeys())
                {
                    string mode = Request.QueryString["mode"];
                    string empId = Request.QueryString["empId"];

                    // create leave balance elements
                    try
                    {
                        string sql = $@"
                                SELECT [type_id] as 'leave_type', 'Please enter valid ' + [type_id] + ' Leave number' as 'validation', 'Enter ' + [type_id] + ' Leave balance' as 'placeholder'
                                FROM [dbo].[leavetype] 
                            ";

                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                SqlDataAdapter ad = new SqlDataAdapter(command);

                                DataTable dt = new DataTable();
                                ad.Fill(dt);

                                foreach (DataRow dr in dt.Rows)
                                {
                                    if (util.isLeaveTypeWithoutBalance(dr.ItemArray[0].ToString()))
                                        dr.Delete();
                                }
                                leaveBalancesListView.DataSource = dt;
                                leaveBalancesListView.DataBind();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    // set clear all files button to not visible
                    clearAllFilesBtn.Visible = false;

                    if (mode == "edit")
                    {
                        // populates page and authorizes HR user based on employee's employment type
                        populatePage(empId);
                        this.adjustPageForEditMode();
                    }

                    else
                    {
                        this.adjustPageForCreateMode();
                    }

                }
            } else
            {
                // set isEditMode
                if (Request.QueryString["mode"] != null)
                    isEditMode = Request.QueryString["mode"] == "edit";

                bool test = filesToDownloadPanel.Visible;
            }

        }

        // INITIAL PREP OF PAGE BASED ON MODE
        protected void adjustPageForEditMode()
        {
            isEditMode = true;

            // title
            editModeTitle.Visible = true;
            createModeTitle.Visible = false;

            // clear form btn text
            clearFormTxt.InnerText = "Reset Form";

            // employee name
            empNamePanel.Visible = true;

            // basic info like id, ihris id, email
            empBasicInfoPanel.Visible = false;

            // authorizations
            // if hr 1 then authorizations can be edited

            if (permissions.Contains("hr1_permissions"))
            {
                authorizationLevelPanel.Visible = true;
            }
            else
            {
                if (permissions.Contains("hr2_permissions"))
                    hr1CheckDiv.Visible = false;
                else if (permissions.Contains("hr3_permissions"))
                    hr1CheckDiv.Visible = hr2CheckDiv.Visible = false;
            }

            // add emp record
            addEmpRecordForm.Visible = false;
            showFormBtn.Visible = true;

            // submit
            submitFullFormPanel.Visible = false;
            editFormPanel.Visible = true;
        }

        protected void adjustPageForCreateMode()
        {
            // title
            editModeTitle.Visible = false;
            createModeTitle.Visible = true;

            // clear form btn text
            clearFormTxt.InnerText = "Clear Form";

            // employee name
            empNamePanel.Visible = false;

            // basic info like id, ihris id, email
            empBasicInfoPanel.Visible = true;

            // authorization
            authorizationLevelPanel.Visible = true;

            // add emp record
            addEmpRecordForm.Visible = false;
            showFormBtn.Visible = true;

            accPastLimitContainerPanel.Visible = false;

            // submit
            submitFullFormPanel.Visible = true;
            editFormPanel.Visible = false;

        }
        //_________________________________________________________________________


        // EMPLOYMENT RECORDS GRIDVIEW METHODS
        protected void bindGridview()
        {
            // sets data for gridview table
            DataTable dt = empRecordsDataSource;
            if (dt != null)
            {
                DataView dataView = dt.AsDataView();
                dataView.Sort = "start_date DESC, actual_end_date ASC";
                dt = dataView.ToTable();

                empRecordsDataSource = dt;
                empRecordGridView.DataSource = dt;
                empRecordGridView.DataBind();
            }
        }

        protected void empRecordGridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            empRecordGridView.PageIndex = e.NewPageIndex;
            this.bindGridview();
        }

        protected void empRecordGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            DataTable dt = empRecordsDataSource;
            if (dt != null)
            {
                // sets isChanged to 1
                int indexOfRowInDt = e.RowIndex;
                dt.Rows[indexOfRowInDt].SetField<string>((int)emp_records_columns.isChanged, "1");
                empRecordsDataSource = dt;

            }
            this.bindGridview();
        }

        protected void empRecordGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Executes after data is bound for each row. This method is used to make changes as to what a user sees on the gridview based on the data in the table

            int i;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // hide deleted row, row is a deleted record if isChanged = 1
                i = GetColumnIndexByName(e.Row, "isChanged");
                if (e.Row.Cells[i].Text.ToString() == "1" && this.isEditMode)
                    e.Row.Visible = false;

                // adds badge to new records
                i = GetColumnIndexByName(e.Row, "record_id");
                if (e.Row.Cells[i].Text.ToString() == "-1" && this.isEditMode)
                {
                    // row is a new record if record_id = -1
                    Label newRecordLabel = (Label)e.Row.FindControl("new_record_label");
                    newRecordLabel.Visible = true;
                }

                // add badge to edited records
                i = GetColumnIndexByName(e.Row, "isChanged");
                if (e.Row.Cells[i].Text.ToString() == "2" && this.isEditMode)
                {
                    // row is an edited record if isChanged = 2
                    Label editedRecordLabel = (Label)e.Row.FindControl("edited_record_label");
                    editedRecordLabel.Visible = true;
                }

                // hide end employment record button if actual_end_date is already populated
                i = GetColumnIndexByName(e.Row, "actual_end_date");
                LinkButton endEmpRecordActionBtn = (LinkButton)e.Row.FindControl("endEmpRecordActionBtn");
                if (!util.isNullOrEmpty(e.Row.Cells[i].Text.ToString()))
                    endEmpRecordActionBtn.Visible = false;
                else
                    endEmpRecordActionBtn.Visible = true;
            }
        }

        protected void empRecordGridView_DataBound(object sender, EventArgs e)
        {
            // this function executes after all data is bound for the Gridview containing employment records
            // it checks to see if all rows are deleted (isChanged = 1)
            // if the above condition is true then it hides the header row of the gridview
            if (empRecordGridView.HeaderRow != null)
            {
                bool isTableEmpty = true;
                if (empRecordsDataSource != null)
                {
                    DataTable dt = empRecordsDataSource;
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row.ItemArray[(int)emp_records_columns.isChanged].ToString() != "1")
                                isTableEmpty = false;
                        }
                    }
                }
                empRecordGridView.HeaderRow.Visible = !isTableEmpty;
            }


        }

        protected void empRecordGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // this method fires whenever an Action button is clicked in the Gridview

            // end employment record
            if (e.CommandName == "endEmpRecord")
            {
                // reset form
                clearErrors();
                txtEmpRecordEndDate.Text = string.Empty;

                // store data about row which must be edited
                int index = Convert.ToInt32(e.CommandArgument);
                Session["empRecordRowIndex"] = index;

                // show modal
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#cancelEmpRecordModal').modal('show');", true);
            }

            // edit employment record
            if (e.CommandName == "editEmpRecord")
            {
                // reset form
                clearErrors();

                // check if record was being edited before
                if (recordBeingEdited != -1)
                {
                    // if a record was being edited before and another record is clicked to be edited then remove the highlight from previously selected record
                    removeHighlightFromEditedRecord();

                    // set actual end date text in Add employment record form to not display
                    actualEndDateSpan.Style.Add("display", "none");

                    // resets all form fields
                    resetAddEmploymentRecordFormFields();
                }

                showEmplomentRecordForm();

                // show edit button and hide add new record button
                addNewRecordBtn.Visible = false;
                editRecordBtn.Visible = true;

                // index of row being edited
                int index = Convert.ToInt32(e.CommandArgument);

                // rebind the dropdown lists so that they can be set to the correct selected item
                empTypeList.DataBind();
                deptList.DataBind();
                positionList.DataBind();

                // these search strings are neccessary since the FindByText method will not match ASCII characters. For eg. a single quote in any of the inputs (position,
                // employment type or departmen would cause an error ). As such, these search strins must be sanitized for COVID-19
                string posSearchString = empRecordGridView.Rows[index].Cells[GetColumnIndexByName(empRecordGridView.Rows[index], "pos_name")].Text.ToString(),
                    emptypeSearchString = empRecordGridView.Rows[index].Cells[GetColumnIndexByName(empRecordGridView.Rows[index], "employment_type")].Text.ToString(),
                    deptSearchString = empRecordGridView.Rows[index].Cells[GetColumnIndexByName(empRecordGridView.Rows[index], "dept_name")].Text.ToString();

                // sanitize search strings for any ASCII characters (and COVID-19) that may cause trouble with the FindByText function below
                // such as single quote ('), double quote ("), open bracket and close bracket

                posSearchString = util.sanitizeStringForAsciiCharacters(posSearchString);
                emptypeSearchString = util.sanitizeStringForAsciiCharacters(emptypeSearchString);
                deptSearchString = util.sanitizeStringForAsciiCharacters(deptSearchString);

                // POPULATE FIELDS

                // employment type
                empTypeList.Items.FindByText(emptypeSearchString).Selected = true;
                // dept
                deptList.Items.FindByText(deptSearchString).Selected = true;
                // position
                positionList.Items.FindByText(posSearchString).Selected = true;
                // start date
                txtStartDate.Text = empRecordGridView.Rows[index].Cells[GetColumnIndexByName(empRecordGridView.Rows[index], "start_date")].Text.ToString();
                // expected end date (if any)
                txtEndDate.Text = empRecordGridView.Rows[index].Cells[GetColumnIndexByName(empRecordGridView.Rows[index], "expected_end_date")].Text.ToString();
                // actual end date (if any)
                string actualEndDate = empRecordGridView.Rows[index].Cells[GetColumnIndexByName(empRecordGridView.Rows[index], "actual_end_date")].Text.ToString();

                // show actual end date textbox if actual end date is populated
                if (!util.isNullOrEmpty(actualEndDate))
                {
                    actualEndDateSpan.Style.Add("display", "inline");
                    txtActualEndDate.Text = actualEndDate;
                }

                // annual vacation amt
                annualAmtOfLeaveTxt.Text = empRecordGridView.Rows[index].Cells[GetColumnIndexByName(empRecordGridView.Rows[index], "annual_vacation_amt")].Text.ToString();

                // max vacation accumulation possible
                maxAmtOfLeaveTxt.Text = empRecordGridView.Rows[index].Cells[GetColumnIndexByName(empRecordGridView.Rows[index], "max_vacation_accumulation")].Text.ToString();

                // highlight row being edited
                empRecordGridView.Rows[index].CssClass = "highlighted-record";

                // to be used when saving edits made to employment record to DataTable dt for Employment Records. User must still save all their changes
                recordBeingEdited = index;
            }

        }
        //_________________________________________________________________________


        // EMPLOYMENT RECORDS UTILITY METHODS
        int GetColumnIndexByName(GridViewRow row, string columnName)
        {
            int columnIndex = 0;
            foreach (DataControlFieldCell cell in row.Cells)
            {
                if (cell.ContainingField is BoundField)
                    if (((BoundField)cell.ContainingField).DataField.Equals(columnName))
                        break;
                columnIndex++; // keep adding 1 while we don't have the correct name
            }
            return columnIndex;
        }

        protected Boolean validateDates(string startDate, string endDate)
        {
            DateTime start, end;
            start = end = DateTime.MinValue;
            Boolean isValidated = true;

            // validate start date is a date
            if (!DateTime.TryParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start))
            {
                invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }

            // ensure start date is not a weekend
            if (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday)
            {
                startDateIsWeekendPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }

            if (!util.isNullOrEmpty(endDate))
            {
                // validate end date is a date
                if (!DateTime.TryParseExact(endDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out end))
                {
                    invalidEndDateValidationMsgPanel.Style.Add("display", "inline-block");
                    isValidated = false;
                }
                else
                {
                    // compare dates to ensure end date is not before start date
                    if (DateTime.Compare(start, end) > 0)
                    {
                        dateComparisonValidationMsgPanel.Style.Add("display", "inline-block");
                        isValidated = false;
                    }

                    // ensure end date is not weekend
                    if (end.DayOfWeek == DayOfWeek.Saturday || end.DayOfWeek == DayOfWeek.Sunday)
                    {
                        endDateIsWeekendPanel.Style.Add("display", "inline-block");
                        isValidated = false;
                    }
                }
            }

            return isValidated;
        }

        protected DateTime validateActualEndDate(string startDate, string actualEndDate, Panel endDateInvalidMsg, Panel endDateIsWeekendMsg, Panel endDateIsBeforeStartDateMsg, Panel emptyDate)
        {
            // validates the actual end date. Returns the DateTime value of the actual end date once it is valid and DateTime.MinValue if not. If an empty actual end date is an accepted value then
            // this method returns the current DateTime via the DateTime.Now object. This is for the case where actual end date is edited and can be removed. 


            Boolean isValidated = true;
            DateTime end = DateTime.MinValue;
            if (!util.isNullOrEmpty(actualEndDate))
            {

                // validate end date is a date
                if (!DateTime.TryParseExact(actualEndDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out end))
                {
                    endDateInvalidMsg.Style.Add("display", "inline-block");
                    isValidated = false;
                }
                else
                {
                    // end date is valid

                    // ensure end date is not weekend
                    if (end.DayOfWeek == DayOfWeek.Saturday || end.DayOfWeek == DayOfWeek.Sunday)
                    {
                        endDateIsWeekendMsg.Style.Add("display", "inline-block");
                        isValidated = false;
                    }

                    // compare dates to ensure end date is not before start date
                    if (!util.isNullOrEmpty(startDate))
                    {
                        if (DateTime.Compare(DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture), end) > 0)
                        {
                            endDateInvalidMsg.Style.Add("display", "inline-block");
                            endDateIsBeforeStartDateMsg.Style.Add("display", "inline-block");
                            isValidated = false;
                        }
                    }
                    else
                        isValidated = false;
                }
            }
            else
            {
                if (emptyDate != null)
                {
                    emptyDate.Style.Add("display", "inline-block");
                    isValidated = false;
                }
                else
                {
                    // end is set to the current date so that the method will not return a DateTime.MinValue value which corresponds to a false validation
                    isValidated = true;
                    end = util.getCurrentDate();
                }


            }

            end = isValidated ? end : DateTime.MinValue;
            return end;
        }
        //_________________________________________________________________________


        // EMPLOYMENT RECORDS METHODS
        protected void showEmplomentRecordForm()
        {
            // shows the employment record form
            addEmpRecordForm.Visible = true;
            showFormBtn.Visible = false;
        }

        protected void hideEmploymentRecordForm()
        {
            // hides and resets employment records form
            resetAddEmploymentRecordFormFields();
            addEmpRecordForm.Visible = false;
            showFormBtn.Visible = true;
            addNewRecordBtn.Visible = true;
            editRecordBtn.Visible = false;
        }

        protected void removeHighlightFromEditedRecord()
        {
            if (recordBeingEdited != -1)
            {
                int index = Convert.ToInt32(recordBeingEdited.ToString());
                empRecordGridView.Rows[index].CssClass = empRecordGridView.Rows[index].CssClass.Replace("highlighted-record", "");
                recordBeingEdited = -1;
            }
        }

        protected void showFormBtn_Click(object sender, EventArgs e)
        {
            showEmplomentRecordForm();
        }

        protected void addNewRecordBtn_Click(object sender, EventArgs e)
        {
            // adds new record to datatable 

            /* data to be submitted
             * 1. position
             * 2. Start date
             * 3. End date
             * 4. employment type
             * 5. dept
             */
            clearErrors();
            string position_id = positionList.SelectedValue,
                position_name = positionList.SelectedItem.Text,
                startDate = txtStartDate.Text.ToString(),
                endDate = txtEndDate.Text.ToString(),
                emp_type = empTypeList.SelectedValue,
                dept_id = deptList.SelectedValue,
                dept_name = deptList.SelectedItem.Text,
                annual_vacation_amt = annualAmtOfLeaveTxt.Text,
                max_amt_of_vacation_accumulation = maxAmtOfLeaveTxt.Text;

            Boolean isValidated = validateDates(startDate, endDate),
                isRecordAllowed = true;

            if (isValidated)
            {

                DataTable dt;
                if (empRecordsDataSource == null)
                {
                    // if there is no pre-existing data table (no previous employment records), then create the columns for the new data table
                    dt = new DataTable();
                    dt.Columns.Add("record_id", typeof(string));
                    dt.Columns.Add("employment_type", typeof(string));
                    dt.Columns.Add("dept_id", typeof(string));
                    dt.Columns.Add("dept_name", typeof(string));
                    dt.Columns.Add("pos_id", typeof(string));
                    dt.Columns.Add("pos_name", typeof(string));
                    dt.Columns.Add("start_date", typeof(DateTime));
                    dt.Columns.Add("expected_end_date", typeof(DateTime));
                    dt.Columns.Add("isChanged", typeof(string));
                    dt.Columns.Add("actual_end_date", typeof(string));
                    dt.Columns.Add("status", typeof(string));
                    dt.Columns.Add("status_class", typeof(string));
                    dt.Columns.Add("annual_vacation_amt", typeof(int));
                    dt.Columns.Add("max_vacation_accumulation", typeof(int));
                }
                else
                    // get pre-existing data table
                    dt = empRecordsDataSource;

                isRecordAllowed = isRecordValid(startDate, string.Empty, -1, multipleActiveRecordsAddRecordPanel, startDateClashAddRecordPanel);

                if (isRecordAllowed)
                {
                    // case: where no end date is entered. If the employment record is for Contract then 3 years are added to the start date and 
                    // this date is entered for the expected end date value
                    DateTime expected_end_date = DateTime.MinValue;
                    if (util.isNullOrEmpty(endDate))
                    {
                        if (emp_type == "Contract")
                        {
                            expected_end_date = DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            expected_end_date = expected_end_date.AddYears(3);
                        }
                    }
                    else
                        expected_end_date = DateTime.ParseExact(endDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                    // check that row is not a duplicate row by comparing employment_type, dept_id, pos_id and start_date
                    bool isDuplicate = false;
                    foreach (DataRow dr in dt.Rows)
                    {
                        // once row is not deleted
                        if (dr.ItemArray[(int)emp_records_columns.isChanged].ToString() != "1")
                        {
                            string dtEmpType = dr.ItemArray[(int)emp_records_columns.employment_type].ToString(),
                               dtDeptId = dr.ItemArray[(int)emp_records_columns.dept_id].ToString(),
                               dtPosId = dr.ItemArray[(int)emp_records_columns.pos_id].ToString(),
                               dtStartDate = ((DateTime)dr.ItemArray[(int)emp_records_columns.start_date]).ToString("d/MM/yyyy");

                            if (dtEmpType == emp_type && dtDeptId == dept_id && dtPosId == position_id && dtStartDate == startDate)
                            {
                                isDuplicate = true;
                                break;
                            }
                        }

                    }

                    if (!isDuplicate)
                    {

                        int annualVacationLeaveAmt = Convert.ToInt32(annual_vacation_amt),
                            maxVacationLeaveAmt = Convert.ToInt32(max_amt_of_vacation_accumulation);

                        if (maxVacationLeaveAmt > annualVacationLeaveAmt)
                        {
                            // if start date is before or on Today then the record is active, otherwise it is not
                            if (DateTime.Compare(DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture), util.getCurrentDate()) <= 0)
                                //record_id, employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date, isChanged, actual_end_date, status, status_class, annual_vacation_amt, max_vacation_accumulation
                                dt.Rows.Add(-1, emp_type, dept_id, dept_name, position_id, position_name, DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture), expected_end_date, "0", "", "Active", "label-success", annualVacationLeaveAmt, maxVacationLeaveAmt);
                            else
                                //record_id, employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date, isChanged, actual_end_date, status, status_class, annual_vacation_amt, max_vacation_accumulation
                                dt.Rows.Add(-1, emp_type, dept_id, dept_name, position_id, position_name, DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture), expected_end_date, "0", "", "Inactive", "label-danger", annualVacationLeaveAmt, maxVacationLeaveAmt);
                            empRecordsDataSource = dt;
                        }
                        else
                            invalidAnnualOrMaximumVacationLeaveAmtPanel.Style.Add("display", "inline-block");


                    }
                    else
                        duplicateRecordPanel.Style.Add("display", "inline-block");

                    this.bindGridview();
                }
            }
        }

        protected void resetAddEmploymentRecordFormFields()
        {
            // resets employment record form fields to its initial state. Clears all errors encountered previously

            clearErrors();
            txtStartDate.Text = String.Empty;
            txtEndDate.Text = String.Empty;
            txtActualEndDate.Text = String.Empty;
            actualEndDateSpan.Style.Add("display", "none");

            // rebind dropdown lists
            empTypeList.DataBind();
            deptList.DataBind();
            positionList.DataBind();

            annualAmtOfLeaveTxt.Text = string.Empty;
            maxAmtOfLeaveTxt.Text = string.Empty;
        }

        protected void cancelNewRecordBtn_Click(object sender, EventArgs e)
        {
            // cancels employment record form
            hideEmploymentRecordForm();
            removeHighlightFromEditedRecord();
        }

        protected void submitEndEmpRecordBtn_Click(object sender, EventArgs e)
        {
            // actual end date is submitted for validation before being added to the employment records datatable or not

            clearErrors();

            if (empRecordsDataSource != null && Session["empRecordRowIndex"] != null)
            {
                DataTable dt = empRecordsDataSource;
                int indexInDt = Convert.ToInt32(Session["empRecordRowIndex"]);

                string startDate = Convert.ToDateTime(dt.Rows[indexInDt][(int)emp_records_columns.start_date]).ToString("d/MM/yyyy");
                DateTime end = validateActualEndDate(startDate, txtEmpRecordEndDate.Text, invalidEndDatePanel, actualEndDateIsWeekendPanel, endDateBeforeStartDatePanel, emptyEndDatePanel);
                if (end != DateTime.MinValue)
                {
                    // edit datatable and rebind gridview

                    if (isRecordValid(startDate, end.ToString("d/MM/yyyy"), indexInDt, multipleActiveRecordsEndRecordPanel, employmentRecordClashPanel))
                    {
                        // set record to be edited
                        dt.Rows[indexInDt].SetField<string>((int)emp_records_columns.isChanged, "2");

                        // set actual end date
                        dt.Rows[indexInDt].SetField<string>((int)emp_records_columns.actual_end_date, end.ToString("d/MM/yyyy"));

                        // change status 
                        if (isRecordActive(startDate, end.ToString("d/MM/yyyy")))
                        {
                            dt.Rows[indexInDt][(int)emp_records_columns.status] = "Active";
                            dt.Rows[indexInDt][(int)emp_records_columns.status_class] = "label-success";
                        }
                        else
                        {
                            dt.Rows[indexInDt][(int)emp_records_columns.status] = "Inactive";
                            dt.Rows[indexInDt][(int)emp_records_columns.status_class] = "label-danger";
                        }

                        empRecordsDataSource = dt;
                        this.bindGridview();

                        // set Dictionary containing info including: id of record edited (KEY) and a list of the fields edited in the record (VALUE). Used for audit logs
                        Dictionary<string, HashSet<string>> recordEdits = new Dictionary<string, HashSet<string>>();
                        if (Session["editedRecords"] != null)
                        {
                            recordEdits = (Dictionary<string, HashSet<string>>)Session["editedRecords"];

                            // record already exists
                            if (recordEdits.Keys.Contains(dt.Rows[indexInDt][(int)emp_records_columns.record_id].ToString()))
                                recordEdits[dt.Rows[indexInDt][(int)emp_records_columns.record_id].ToString()].Add("Actual End Date");
                            else
                            {
                                // record does not exist
                                recordEdits.Add(dt.Rows[indexInDt][(int)emp_records_columns.record_id].ToString(), new HashSet<string>());
                                recordEdits[dt.Rows[indexInDt][(int)emp_records_columns.record_id].ToString()].Add("Actual End Date");
                            }


                            Session["editedRecords"] = recordEdits;
                        }
                        else
                        {
                            recordEdits.Add(dt.Rows[indexInDt][(int)emp_records_columns.record_id].ToString(), new HashSet<string>());
                            recordEdits[dt.Rows[indexInDt][(int)emp_records_columns.record_id].ToString()].Add("Actual End Date");
                            Session["editedRecords"] = recordEdits;
                        }

                        Session["empRecordRowIndex"] = null;

                        // hide modal
                        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#cancelEmpRecordModal').modal('hide');", true);
                    }
                }
            }


        }

        protected Boolean isRecordActive(string proposedStartDate, string proposedEndDate)
        {
            // returns a Boolean which represents whether the proposed actual end date passed will make a record active or inactive. The date passed is assumed to be validated

            // check if start date of record is a day in the future, meaning the record is currently inactive
            if (DateTime.Compare(DateTime.ParseExact(proposedStartDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture), util.getCurrentDate()) > 0)
                return false;

            // if the passed end date is empty then the record is automaticaly Active
            if (util.isNullOrEmpty(proposedEndDate))
                return true;
            else
                // if today is before the passed actual end date then the record is active and otherwise, inactive
                return (DateTime.Compare(util.getCurrentDate(), DateTime.ParseExact(proposedEndDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)) < 0);
        }

        protected Boolean isRecordValid(string proposedStartDate, string proposedEndDate, int index, Panel multipleActiveRecordsPanel, Panel clashingRecords)
        {
            // returns a Boolean representing whether the proposed start date and proposed end date passed is valid in terms of the rest of existing records. This method checks the other records to see if
            // any other active records exist in order to validate the record.

            if (empRecordsDataSource != null)
            {
                int numActiveRows = 0, currIndex = 0;

                // state of passed end date and corresponding record
                bool isProposedRecordActive = isRecordActive(proposedStartDate, proposedEndDate);

                DateTime proposedSD = DateTime.ParseExact(proposedStartDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                        proposedAED = !util.isNullOrEmpty(proposedEndDate) ? DateTime.ParseExact(proposedEndDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) : DateTime.MinValue;


                DataTable dt = empRecordsDataSource;
                foreach (DataRow dr in dt.Rows)
                {
                    // once the record is not deleted and is not the record currently being edited (the record to which the proposed end date will belong to)
                    if (dr[(int)emp_records_columns.isChanged].ToString() != "1" && currIndex != index)
                    {
                        if (dr[(int)emp_records_columns.status].ToString() == "Active")
                            numActiveRows++;


                        DateTime dtRowStartDate = (DateTime)dr[(int)emp_records_columns.start_date];

                        DateTime dtRowEndDate = !util.isNullOrEmpty(dr[(int)emp_records_columns.actual_end_date].ToString()) ? DateTime.ParseExact(dr[(int)emp_records_columns.actual_end_date].ToString(), "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) : DateTime.MinValue;

                        // ensure that record does not overlap with another record
                        bool isProposedStartDateInRowPeriod = false, isProposedEndDateInRowPeriod = false;


                        // if record being checked has an end date
                        if (dtRowEndDate != DateTime.MinValue)
                        {

                            bool isRowStartDateInProposedPeriod = false, isRowEndDateinProposedPeriod = false;
                            // proposed actual end date is not empty
                            if (proposedAED != DateTime.MinValue)
                            {
                                // check if period represented by proposed start date to proposed end date coincides with the given data row's period
                                isProposedStartDateInRowPeriod = DateTime.Compare(proposedSD, dtRowStartDate) >= 0 && DateTime.Compare(proposedSD, dtRowEndDate) <= 0;
                                isProposedEndDateInRowPeriod = DateTime.Compare(proposedAED, dtRowStartDate) >= 0 && DateTime.Compare(proposedAED, dtRowEndDate) <= 0;

                                isRowStartDateInProposedPeriod = DateTime.Compare(dtRowStartDate, proposedSD) >= 0 && DateTime.Compare(dtRowStartDate, proposedAED) <= 0;
                                isRowEndDateinProposedPeriod = DateTime.Compare(dtRowEndDate, proposedSD) >= 0 && DateTime.Compare(dtRowEndDate, proposedAED) <= 0;

                            }
                            // proposed actual end date is empty- proposed record is active
                            else
                            {
                                isProposedStartDateInRowPeriod = DateTime.Compare(proposedSD, dtRowEndDate) <= 0 || DateTime.Compare(proposedSD, dtRowStartDate) <= 0;
                            }

                            if (isProposedStartDateInRowPeriod || isProposedEndDateInRowPeriod || isRowStartDateInProposedPeriod || isRowEndDateinProposedPeriod)
                            {
                                if (clashingRecords != null)
                                    clashingRecords.Style.Add("display", "inline-block");
                                return false;
                            }

                        }
                        // if record being checked is active
                        else
                        {
                            // proposed actual end date is not empty
                            if (proposedAED != DateTime.MinValue)
                            {
                                // check if period represented by proposed start date is in active record's period
                                isProposedStartDateInRowPeriod = DateTime.Compare(proposedSD, dtRowStartDate) >= 0;
                                isProposedEndDateInRowPeriod = DateTime.Compare(proposedAED, dtRowStartDate) >= 0;
                            }
                            // proposed actual end date is empty - proposed record is active
                            else
                            {
                                if (multipleActiveRecordsPanel != null)
                                    multipleActiveRecordsPanel.Style.Add("display", "inline-block");
                                return false; // proposed record is invalid since record already exists that is active and proposed record is active
                            }


                            if (isProposedStartDateInRowPeriod || isProposedEndDateInRowPeriod)
                            {
                                if (clashingRecords != null)
                                    clashingRecords.Style.Add("display", "inline-block");
                                return false;
                            }

                        }

                    }
                    currIndex++;
                }
                if (isProposedRecordActive)
                    numActiveRows++;

                if (numActiveRows <= 1)
                    return true;
                else if (numActiveRows > 1 && multipleActiveRecordsPanel != null)
                    multipleActiveRecordsPanel.Style.Add("display", "inline-block");
            }

            return true;

        }

        protected void editRecordBtn_Click(object sender, EventArgs e)
        {
            // edits the selected record

            clearErrors();
            if (recordBeingEdited != -1)
            {

                // the values being proposed for edit. These may or may not have been changed so must be checked in order to know which to edit or not
                string position_id = positionList.SelectedValue,
                    position_name = positionList.SelectedItem.Text,
                    startDate = txtStartDate.Text.ToString(),
                    expectedEndDate = txtEndDate.Text.ToString(),
                    actualEndDate = txtActualEndDate.Text.ToString(),
                    emp_type = empTypeList.SelectedValue,
                    dept_id = deptList.SelectedValue,
                    dept_name = deptList.SelectedItem.Text,
                    annual_vacation_amt = annualAmtOfLeaveTxt.Text,
                    max_amt_of_vacation_accumulation = maxAmtOfLeaveTxt.Text;

                // validate start and expected end date
                Boolean isStartAndExpectedEndDateValidated = validateDates(startDate, expectedEndDate);

                // validate actual end date
                Boolean isActualEndDateValidated = validateActualEndDate(startDate, actualEndDate, recordEditEndDateInvalidPanel, recordEditEndDateOnWeekend, recordEditEndDateBeforeStartDate, null) != DateTime.MinValue;

                Boolean isAnnualAndMaxVacationAmtValidated = Convert.ToInt32(max_amt_of_vacation_accumulation) > Convert.ToInt32(annual_vacation_amt);

                // index of record being edited
                int index = recordBeingEdited;

                if (empRecordsDataSource != null)
                {
                    DataTable dt = empRecordsDataSource;
                    if (isStartAndExpectedEndDateValidated && isActualEndDateValidated && isAnnualAndMaxVacationAmtValidated)
                    {
                        // check if any values are changed and edit relevant values in datatable
                        Boolean isPositionChanged = dt.Rows[index].ItemArray[(int)emp_records_columns.pos_id].ToString() != position_id,
                            isDeptChanged = dt.Rows[index].ItemArray[(int)emp_records_columns.dept_id].ToString() != dept_id,
                            isEmpTypeChanged = dt.Rows[index].ItemArray[(int)emp_records_columns.employment_type].ToString() != emp_type,
                            isStartDateChanged = Convert.ToDateTime(dt.Rows[index].ItemArray[(int)emp_records_columns.start_date]).ToString("d/MM/yyyy") != startDate,
                            isExpectedEndDateChanged = Convert.ToDateTime(dt.Rows[index].ItemArray[(int)emp_records_columns.expected_end_date]).ToString("d/MM/yyyy") != expectedEndDate,
                            isActualEndDateChanged = dt.Rows[index][(int)emp_records_columns.actual_end_date].ToString() != actualEndDate,
                            isAnnualVacationAmtChanged = Convert.ToInt32(dt.Rows[index][(int)emp_records_columns.annual_vacation_amt]) != Convert.ToInt32(annual_vacation_amt),
                            isMaxAmtOfVacationAccChanged = Convert.ToInt32(dt.Rows[index][(int)emp_records_columns.max_vacation_accumulation]) != Convert.ToInt32(max_amt_of_vacation_accumulation),
                            isEditedRecordValid = true;

                        if (isPositionChanged || isDeptChanged || isEmpTypeChanged || isStartDateChanged || isExpectedEndDateChanged || isActualEndDateChanged || isAnnualVacationAmtChanged || isMaxAmtOfVacationAccChanged)
                        {
                            List<string> editsMade = new List<string>();
                            try
                            {
                                if (isPositionChanged)
                                {
                                    dt.Rows[index][(int)emp_records_columns.pos_id] = position_id;
                                    dt.Rows[index][(int)emp_records_columns.pos_name] = position_name;
                                    editsMade.Add("Position");
                                }
                                if (isDeptChanged)
                                {
                                    dt.Rows[index][(int)emp_records_columns.dept_id] = dept_id;
                                    dt.Rows[index][(int)emp_records_columns.dept_name] = dept_name;
                                    editsMade.Add("Department");
                                }
                                if (isEmpTypeChanged)
                                {
                                    dt.Rows[index][(int)emp_records_columns.employment_type] = emp_type;
                                    editsMade.Add("Employment Type");
                                }


                                if (!util.isNullOrEmpty(startDate) && isStartDateChanged)
                                {
                                    if (isRecordValid(startDate, actualEndDate, index, multipleActiveRecordsEditRecordPanel, employmentRecordClashPanel))
                                    {
                                        editsMade.Add("Start Date");
                                        dt.Rows[index][(int)emp_records_columns.start_date] = DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                                        if (isRecordActive(startDate, actualEndDate))
                                        {
                                            dt.Rows[index][(int)emp_records_columns.status] = "Active";
                                            dt.Rows[index][(int)emp_records_columns.status_class] = "label-success";
                                        }
                                        else
                                        {
                                            dt.Rows[index][(int)emp_records_columns.status] = "Inactive";
                                            dt.Rows[index][(int)emp_records_columns.status_class] = "label-danger";
                                        }
                                    }
                                    else
                                        isEditedRecordValid = false;

                                }
                                else if (isStartDateChanged && util.isNullOrEmpty(startDate))
                                    invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");

                                if (isExpectedEndDateChanged)
                                {
                                    editsMade.Add("Expected End Date");

                                    if (!util.isNullOrEmpty(expectedEndDate))
                                        dt.Rows[index][(int)emp_records_columns.expected_end_date] = DateTime.ParseExact(expectedEndDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                    else
                                        dt.Rows[index][(int)emp_records_columns.expected_end_date] = DateTime.MinValue;
                                }

                                if (isActualEndDateChanged)
                                {
                                    if (isEditedRecordValid && !isStartDateChanged)
                                        isEditedRecordValid = isRecordValid(startDate, actualEndDate, index, multipleActiveRecordsEditRecordPanel, employmentRecordClashPanel);

                                    // does edit make employee have more than one record that is active (end date is not null and today is before end date) or (end date is null)
                                    if (isEditedRecordValid) {
                                        editsMade.Add("Actual End Date");

                                        if (!util.isNullOrEmpty(actualEndDate))
                                            dt.Rows[index][(int)emp_records_columns.actual_end_date] = actualEndDate;
                                        else
                                            dt.Rows[index][(int)emp_records_columns.actual_end_date] = string.Empty;

                                        if (isRecordActive(startDate, actualEndDate))
                                        {
                                            dt.Rows[index][(int)emp_records_columns.status] = "Active";
                                            dt.Rows[index][(int)emp_records_columns.status_class] = "label-success";
                                        }
                                        else
                                        {
                                            dt.Rows[index][(int)emp_records_columns.status] = "Inactive";
                                            dt.Rows[index][(int)emp_records_columns.status_class] = "label-danger";
                                        }
                                    }

                                }

                                if (isAnnualVacationAmtChanged)
                                {
                                    dt.Rows[index][(int)emp_records_columns.annual_vacation_amt] = Convert.ToInt32(annual_vacation_amt);
                                    editsMade.Add("Annual amount of Vacation leave");
                                }

                                if (isMaxAmtOfVacationAccChanged)
                                {
                                    dt.Rows[index][(int)emp_records_columns.max_vacation_accumulation] = Convert.ToInt32(max_amt_of_vacation_accumulation);
                                    editsMade.Add("Maximum accumulated Vacation leave");
                                }

                                if (isEditedRecordValid && (isActualEndDateChanged || isPositionChanged || isDeptChanged || isEmpTypeChanged || isStartDateChanged || isExpectedEndDateChanged || isAnnualVacationAmtChanged || isMaxAmtOfVacationAccChanged))
                                {
                                    dt.Rows[index].SetField<string>((int)emp_records_columns.isChanged, "2");

                                    empRecordsDataSource = dt;
                                    bindGridview();

                                    // set Dictionary containing info including: id of record edited (KEY) and a list of the fields edited in the record (VALUE). Used for adding audit log. 
                                    Dictionary<string, HashSet<string>> recordEdits = new Dictionary<string, HashSet<string>>();
                                    if (Session["editedRecords"] != null)
                                    {
                                        recordEdits = (Dictionary<string, HashSet<string>>)Session["editedRecords"];

                                        // record already exists
                                        if (recordEdits.Keys.Contains(dt.Rows[index][(int)emp_records_columns.record_id].ToString()))
                                            recordEdits[dt.Rows[index][(int)emp_records_columns.record_id].ToString()].UnionWith(editsMade);
                                        else
                                        {
                                            // record does not exist
                                            recordEdits.Add(dt.Rows[index][(int)emp_records_columns.record_id].ToString(), new HashSet<string>());
                                            recordEdits[dt.Rows[index][(int)emp_records_columns.record_id].ToString()].UnionWith(editsMade);
                                        }


                                        Session["editedRecords"] = recordEdits;
                                    } else
                                    {
                                        recordEdits.Add(dt.Rows[index][(int)emp_records_columns.record_id].ToString(), new HashSet<string>());
                                        recordEdits[dt.Rows[index][(int)emp_records_columns.record_id].ToString()].UnionWith(editsMade);
                                        Session["editedRecords"] = recordEdits;
                                    }

                                    editEmpRecordSuccTxt.InnerText = $"{String.Join(", ", editsMade.ToArray())} edits successfully made to employment record. Don't forget to save your changes";
                                    editEmploymentRecordSuccessful.Style.Add("display", "inline-block");
                                }
                                else if (!(isActualEndDateChanged || isStartDateChanged || isPositionChanged || isDeptChanged || isEmpTypeChanged || isExpectedEndDateChanged))
                                    noEditsToRecordsMade.Style.Add("display", "inline-block");

                            }
                            catch (Exception exc)
                            {
                                editUnsuccessful.Style.Add("display", "inline-block");
                            }
                        }
                        else
                            noEditsToRecordsMade.Style.Add("display", "inline-block");
                    }
                    else if (!isAnnualAndMaxVacationAmtValidated)
                    {
                        invalidAnnualOrMaximumVacationLeaveAmtPanel.Style.Add("display", "inline-block");
                    }
                }
            }
        }

        protected void closeEndEmpRecordModalBtn_Click(object sender, EventArgs e)
        {
            // closes modal where employee can end an employment record (populate the actual end date field)

            Session["empRecordRowIndex"] = null;
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#cancelEmpRecordModal').modal('hide');", true);
        }
        //_________________________________________________________________________


        // EDIT EMPLOYEE METHODS
        protected void clearErrors()
        {
            // clears all validation/ informative or warning messages from every section of the entire page

            // SAVE EMPLOYEE

            // SUCCESSES
            editFullSuccessPanel.Style.Add("display", "none");
            editRolesSuccessPanel.Style.Add("display", "none");
            editLeaveSuccessPanel.Style.Add("display", "none");
            editEmpRecordSuccessPanel.Style.Add("display", "none");
            editEmpFilesPanel.Style.Add("display", "none");
            editAccumulatePastMaxSuccessPanel.Style.Add("display", "none");
            fullFormSubmitSuccessPanel.Style.Add("display", "none");

            // ERRORS
            editAccumulatePastMaxErrorPanel.Style.Add("display", "none");
            editEmpFilesErrorPanel.Style.Add("display", "none");
            editEmpErrorPanel.Style.Add("display", "none");
            editRolesErrorPanel.Style.Add("display", "none");
            editLeaveBalancesErrorPanel.Style.Add("display", "none");
            editEmpRecordErrorPanel.Style.Add("display", "none");
            editEndDateEmpRecordsPanel.Style.Add("display", "none");
            deleteEmpRecordsErrorPanel.Style.Add("display", "none");
            addEmpRecordsErrorPanel.Style.Add("display", "none");

            fullFormErrorPanel.Style.Add("display", "none");
            duplicateIdentifierPanel.Style.Add("display", "none");
            emailNotFoundErrorPanel.Style.Add("display", "none");
            noEmploymentRecordEnteredErrorPanel.Style.Add("display", "none");
            fullFormSubmitSuccessPanel.Style.Add("display", "none");

            // NO CHANGES MADE
            noChangesMadePanel.Style.Add("display", "none");
            noChangesMadeToAccStatus.Style.Add("display", "none");

            // EMPLOYMENT RECORD
            invalidAnnualOrMaximumVacationLeaveAmtPanel.Style.Add("display", "none");

            // ADD
            invalidStartDateValidationMsgPanel.Style.Add("display", "none");
            invalidEndDateValidationMsgPanel.Style.Add("display", "none");
            dateComparisonValidationMsgPanel.Style.Add("display", "none");
            startDateIsWeekendPanel.Style.Add("display", "none");
            endDateIsWeekendPanel.Style.Add("display", "none");
            duplicateRecordPanel.Style.Add("display", "none");
            multipleActiveRecordsAddRecordPanel.Style.Add("display", "none");
            startDateClashAddRecordPanel.Style.Add("display", "none");

            // EDIT
            recordEditEndDateInvalidPanel.Style.Add("display", "none");
            recordEditEndDateOnWeekend.Style.Add("display", "none");
            recordEditEndDateBeforeStartDate.Style.Add("display", "none");
            editEmploymentRecordSuccessful.Style.Add("display", "none");
            editUnsuccessful.Style.Add("display", "none");
            noEditsToRecordsMade.Style.Add("display", "none");
            multipleActiveRecordsEditRecordPanel.Style.Add("display", "none");
            startDateClashEditRecordPanel.Style.Add("display", "none");
            employmentRecordClashPanel.Style.Add("display", "none");

            // END EMPLOYMENT RECORD
            invalidEndDatePanel.Style.Add("display", "none");
            emptyEndDatePanel.Style.Add("display", "none");
            endDateBeforeStartDatePanel.Style.Add("display", "none");
            actualEndDateIsWeekendPanel.Style.Add("display", "none");
            multipleActiveRecordsEndRecordPanel.Style.Add("display", "none");
        }

        protected void populatePage(string empId)
        {
            // This function gets all employment record(s) and populates the employment record gridview

            /*  
             *  The following sql command executed initializes the isChanged field as 0 to give each field being populated in the gridview a status of 'not deleted'
             *  
             *  WHAT DATA MUST BE RETRIEVED:
                1. Employee Permissions
                2. Leave Balances
                3. Employment Records
                4. Employee files

            *   Employment records are loaded first in order to check the employment type such that the current HR viewing the page can be authorized accordingly 
                (the above simply means that the current HR must have contract permissions if they view a contract worker and similiarly for public service workers)
            */

            // get employment records
            DataTable dataTable = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    // status- states whether the record read from the DB is active or not. This column is also used to indicate new records and edited records as the user interacts 
                    //         with the employment records data table
                    // status_class - the css class for the status for visual feedback
                    string sql = $@"
                            SELECT 
                                ep.id record_id,
                                ep.employment_type,
                                ep.dept_id,
                                d.dept_name,
                                ep.position_id pos_id,
                                p.pos_name,
                                ep.start_date, 
                                ep.expected_end_date,
                                '0' as isChanged,
                                FORMAT(ep.actual_end_date, 'd/MM/yyyy') actual_end_date,
                                IIF(ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date), 'Active', 'Inactive') as status, 
                                IIF(ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date), 'label-success', 'label-danger') as status_class,
                                annual_vacation_amt,
                                max_vacation_accumulation
                                
                            FROM [dbo].[employeeposition] ep

                            LEFT JOIN [dbo].[department] d
                            ON d.dept_id = ep.dept_id
    
                            LEFT JOIN [dbo].[position] p
                            ON p.pos_id = ep.position_id

                            WHERE employee_id = {empId}
                            ORDER BY ISNULL(ep.actual_end_date, CAST('1/1/9999' AS DATE)) DESC;
                    ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                        dataTable = new DataTable();
                        sqlDataAdapter.Fill(dataTable);

                        empRecordsDataSource = dataTable;
                        this.bindGridview();
                    }
                }
            }
            catch (Exception ex)
            {
                //exception logic
                throw ex;
            }

            // check the employee's current employment type to ensure that the current HR can view and edit the information presented 
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                string empType = dataTable.Rows[0].ItemArray[(int)emp_records_columns.employment_type].ToString();

                if (!permissions.Contains("hr1_permissions"))
                {
                    // the HR 2, HR 3 must have permissions to view data for the same employment type as for the employee who submitted the application
                    //check if hr can view applications from the relevant employment type 
                    if (util.isNullOrEmpty(empType) || (empType == "Contract" && !permissions.Contains("contract_permissions")) || (empType == "Public Service" && !permissions.Contains("public_officer_permissions")))
                        Response.Redirect("~/AccessDenied.aspx");
                }
            }


            // get roles and populate checkboxes
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    string sql = $@"
                        SELECT 
                            er.role_id 
                           
                        FROM [dbo].[employeerole] er
                        WHERE er.employee_id = {empId};
                    ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader["role_id"].ToString() == "sup")
                                    supervisorCheck.Checked = true;

                                if (reader["role_id"].ToString() == "hr1")
                                    hr1Check.Checked = true;

                                if (reader["role_id"].ToString() == "hr2")
                                    hr2Check.Checked = true;

                                if (reader["role_id"].ToString() == "hr3")
                                    hr3Check.Checked = true;

                                if (reader["role_id"].ToString() == "hr_contract")
                                    contractCheck.Checked = true;

                                if (reader["role_id"].ToString() == "hr_public_officer")
                                    publicServiceCheck.Checked = true;
                            }

                            // store initial roles
                            previousRoles = new List<string>();
                            if (supervisorCheck.Checked)
                                previousRoles.Add("sup");
                            if (hr1Check.Checked)
                                previousRoles.Add("hr1");
                            if (hr2Check.Checked)
                                previousRoles.Add("hr2");
                            if (hr3Check.Checked)
                                previousRoles.Add("hr3");
                            if (contractCheck.Checked)
                                previousRoles.Add("hr_contract");
                            if (publicServiceCheck.Checked)
                                previousRoles.Add("hr_public_officer");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //exception logic
                throw ex;
            }

            Dictionary<string, string> leaveMapping = util.getLeaveTypeMapping();

            // get leave balances and populate textboxes
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    string sql = $@"
                        SELECT {String.Join(", ", leaveMapping.Values.ToArray<string>())},
                                [first_name] + ' ' + [last_name] as 'employee_name'
                            FROM [dbo].[employee]
                            WHERE employee_id = {empId};
                    ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            // store initial leave balances
                            previousLeaveBalances = new Dictionary<string, string>();

                            while (reader.Read())
                            {
                                foreach (KeyValuePair<string, string> kvp in leaveMapping)
                                {
                                    string colName = kvp.Value.Replace("[", "").Replace("]", "");

                                    // set value on screen
                                    setLeaveBalanceText(reader[colName].ToString(), kvp.Key);

                                    // set value in backend- used to check for changes if edits are made
                                    previousLeaveBalances[colName] = reader[colName].ToString();
                                }

                                empNameHeader.InnerText = reader["employee_name"].ToString();
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //exception logic
                throw ex;
            }

            if (dataTable != null && dataTable.Rows.Count > 0 && dataTable.Rows[0].ItemArray[(int)emp_records_columns.status].ToString() == "Active")
            {
                accPastLimitContainerPanel.Visible = true;
                loadPreviouslyUploadedFiles(empId); // get employee file
            }
            else
            {
                accPastLimitContainerPanel.Visible = false;
            }

        }

        protected void editBtn_Click(object sender, EventArgs e)
        {
            /* This function is meant to handle the edit of the following aspects of an employee:
             * 
             *  1. Authorizations (the roles assigned to an employee)
             *  2. Leave Balances
             *  3. Employment Records
             *  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
             *  Who can edit what?
             *  
             *  HR 1: can edit Authorizations, Leave Balances and employment records
             *  HR 2: can edit Authorizations (but can only grant an employee an HR role up to and including their own role. For eg. an HR 2 can only grant Supervisor, HR 2, HR 3 roles)
             *        can edit Leave Balances and employment records
             *  HR 3: can edit Authorizations (up to HR 3)
             *        can edit Leave Balances and employment records
             * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
             * Authorizations
             *  -Deleting roles
             *      Since the previous roles held by the employee is stored in previousRoles, the current roles being submitted to edit are used to find out which roles were deleted. As such,
             *     only those roles which were removed from the employee would actually be deleted. No extra calls to the db are made to delete roles which the employee did not have
             *  -Add Audit log for deleted roles
             *  -Adding roles
             *      Again, using previous roles, the new roles assigned to the employee would solely be added. 
             *  -Add Audit log for added roles
             *
             * Leave Balances
             *  - Edit leave balances
             *      Using the previous balances, the edited leave balances are updated solely
             *  - Add Audit log for edited leave balances
             *  
             * Employment Records
             *  Datatable is formatted in the folowing manner in the ItemArray:
                Index:         0             1            2         3        4        5          6               7              8           9           10          11
                Columns : record_id, employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date, isChanged, actual_end_date  status  status_class

                The record_id field is used to distinguish between records that are:
                    1. Newly added records (not yet in the db but must be added): record_id = -1
                    2. Records from db; record_id = id from db
                The isChanged field is used to distinguish between records that are:
                    1. Not Deleted: isChanged = 0
                    2. Deleted: isChanged = 1
                    3. Edited: isChanged = 2

             *  - Delete Employment records
             *  - Add Audit log for deleted employment records
             *  - Add Employment records
             *  - Add Audit log for added employment records
             *  - Edit Employment records
             *      Involves populating the actual_end_date field 
             *  - Add Audit log for edited employment records
             * */


            this.clearErrors();
            string empId = Request.QueryString["empId"];

            //Leave Balances 
            Dictionary<string, string> currentLeaveBalances = new Dictionary<string, string>();

            //Roles
            List<string> authorizations = new List<string>();

            //Employment Records
            DataTable dt = empRecordsDataSource;

            // used to check whether any value is changed 
            Boolean isRolesChanged, isLeaveBalancesChanged, isEmpRecordChanged, isFilesChanged, isAccumulatePastLimitStatusChangedInDb;
            isRolesChanged = isLeaveBalancesChanged = isEmpRecordChanged = isFilesChanged = isAccumulatePastLimitStatusChangedInDb = false;

            // used to check if there was any error in changing values
            Boolean isRolesEditSuccessful, isLeaveEditSuccessful, isEmpRecordEditSuccessful, isEmpRecordDeleteSuccessful, isEmpRecordInsertSuccessful, isEmpRecordEditFieldsSuccessful, isFileUploadSuccessful, isAccumulatePastLimitSuccessful;
            isRolesEditSuccessful = isLeaveEditSuccessful = isEmpRecordEditSuccessful = isEmpRecordDeleteSuccessful = isEmpRecordInsertSuccessful = isEmpRecordEditFieldsSuccessful = isFileUploadSuccessful = isAccumulatePastLimitSuccessful =true;

            // used to store ids of inserted files in order to add to audit log
            List<string> uploadedFilesIds = new List<string>();

            // ROLES--------------------------------------------------------------------------------------------------------------------------
            if (authorizationLevelPanel.Visible)
            {
                // get data from checkboxes and set authorizations
                if (supervisorCheck.Checked)
                    authorizations.Add("sup");
                if (hr1Check.Checked)
                    authorizations.Add("hr1");
                if (hr2Check.Checked)
                    authorizations.Add("hr2");
                if (hr3Check.Checked)
                    authorizations.Add("hr3");

                if (hr2Check.Checked || hr3Check.Checked)
                {
                    if (contractCheck.Checked)
                        authorizations.Add("hr_contract");
                    if (publicServiceCheck.Checked)
                        authorizations.Add("hr_public_officer");
                }

                // previous roles holds the employee's previous roles. The following code looks at the current roles being added and checks which roles previously there that are no longer there
                // As such, the changedRoles list will contain the list of roles which were deleted
                List<string> deletedRoles = new List<string>();

                // used to store the initial roles loaded when in edit mode. These are used to determine the edits made for auditing purposes
                if(previousRoles != null)
                {
                    foreach (string role in previousRoles)
                    {
                        if (!authorizations.Contains(role))
                            deletedRoles.Add(role);
                    }
                }
                

                if (deletedRoles.Count > 0)
                {
                    //delete roles which were previously assigned to employee but which are now being removed
                    foreach (string roleToDelete in deletedRoles)
                    {
                        try
                        {
                            string sql = $@"
                            DELETE FROM [dbo].[employeerole]
                            WHERE employee_id = '{empId}' AND role_id = '{roleToDelete}';
                        ";

                            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                            {
                                connection.Open();
                                using (SqlCommand command = new SqlCommand(sql, connection))
                                {
                                    int rowsAffected = command.ExecuteNonQuery();
                                    isRolesEditSuccessful = rowsAffected > 0;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // exception logic
                            isRolesEditSuccessful = false;
                        }
                    }


                    // add audit log for deleting roles

                    /*
                    * Add info about edits made to employee such as:
                    * Employee ID
                    * Roles deleted
                    * */
                    string action = $"Edited roles; Roles Deleted= {String.Join(", ", deletedRoles.ToArray())}";
                    util.addAuditLog(Session["emp_id"].ToString(), empId, action);
                }

                // previous roles holds the employee's previous roles. The following code looks at the previous roles and checks which roles were added to the employee
                // As such, addedRoles contains the list of all roles added to the employee that were not previously there
                List<string> addedRoles = new List<string>();

                if(previousRoles != null)
                {
                    foreach (string role in authorizations)
                    {
                        if (!previousRoles.Contains(role))
                            addedRoles.Add(role);
                    }
                }
                

                if (addedRoles.Count > 0)
                {
                    // add roles back to employee 
                    foreach (string role in addedRoles)
                    {
                        try
                        {
                            string sql = $@"
                                INSERT INTO [dbo].[employeerole]
                                    ([employee_id], [role_id])
                                VALUES
                                    (@EmployeeId, @RoleId);
                            ";

                            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                            {
                                connection.Open();
                                using (SqlCommand command = new SqlCommand(sql, connection))
                                {
                                    command.Parameters.AddWithValue("@EmployeeId", empId);
                                    command.Parameters.AddWithValue("@RoleId", role);

                                    int rowsAffected = command.ExecuteNonQuery();
                                    isRolesEditSuccessful = rowsAffected > 0;

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // exception logic
                            isRolesEditSuccessful = false;
                            break;
                        }
                    }

                    // add audit log for roles added
                    /*
                    * Add info about edits made to employee such as:
                    * Employee ID
                    * Roles added
                    * */
                    string action = $"Edited roles; Roles Added= {String.Join(", ", addedRoles.ToArray())}";
                    util.addAuditLog(Session["emp_id"].ToString(), empId, action);
                }

                // there was a change made to the roles
                if (deletedRoles.Count > 0 || addedRoles.Count > 0)
                    isRolesChanged = true;

            }
            // END ROLES--------------------------------------------------------------------------------------------------------------------------


            // EDIT LEAVE BALANCES--------------------------------------------------------------------------------------------------------------------------


            // get data from leave balances
            Dictionary<string, string> leaveMapping = util.getLeaveTypeMapping();
            foreach (KeyValuePair<string, string> kvp in leaveMapping)
            {
                string balance = getLeaveBalanceText(kvp.Key);
                string newKey = kvp.Value.Replace("[", "").Replace("]", "");
                currentLeaveBalances[newKey] = util.isNullOrEmpty(balance) ? "0" : balance;
            }

            //checks which leave balances were edited and adds them to a new dictionary (editedLeaveBalances)
            // As such, editedLeaveBalances contains the dictionary of leave balances which were edited
            Dictionary<string, string> editedLeaveBalances = new Dictionary<string, string>();

            // used to store the initial leave balances loaded when in edit mode. These are used to determine the edits made for auditing purposes
            
            if(previousLeaveBalances != null)
            {
                foreach (KeyValuePair<string, string> entry in previousLeaveBalances)
                {
                    if (currentLeaveBalances[entry.Key] != previousLeaveBalances[entry.Key])
                        editedLeaveBalances[entry.Key] = currentLeaveBalances[entry.Key];
                }
            }
            

            if (editedLeaveBalances.Keys.Count > 0)
            {
                // update edited leave balance data 
                try
                {
                    string sql = $@"
                            UPDATE [dbo].[employee]
                            SET {String.Join(", ", editedLeaveBalances.Select(lb => lb.Key + "=" + lb.Value).ToArray())}
                            WHERE employee_id = {empId};
                        ";

                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            int rowsAffected = command.ExecuteNonQuery();
                            isLeaveEditSuccessful = rowsAffected > 0;

                        }
                    }
                }
                catch (Exception ex)
                {
                    // exception logic
                    isLeaveEditSuccessful = false;
                }

                // add audit log for leave balances edited
                /*
                * Add info about edits made to employee such as:
                * Employee ID
                * Leave Balance edited and the value which it was changed to
                * */
                string action = $"Edited leave balances; {String.Join(", ", editedLeaveBalances.Select(lb => lb.Key + "=" + lb.Value).ToArray())}";
                util.addAuditLog(Session["emp_id"].ToString(), empId, action);

                // leave balances were changed
                isLeaveBalancesChanged = true;
            }
            // END LEAVE BALANCES--------------------------------------------------------------------------------------------------------------------------

            // EDIT EMPLOYMENT RECORDS--------------------------------------------------------------------------------------------------------------------------
            /**
                * datatable is formatted in the folowing manner in the ItemArray:
                * Index:         0             1            2         3        4        5          6               7              8           9            10       11
                * Columns : record_id, employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date, isChanged, actual_end_date  status  status_class
            * */

            if (dt.Rows.Count > 0)
            {
                // a list to contain the ids of deleted employment records. This list is used to create audit logs for deletion of records
                List<string> deletedRecords = new List<string>();

                // delete the records with an 'isChanged' field of '1'
                foreach (DataRow dr in dt.Rows)
                {
                    // once the row does not represent a newly added row (which is not in DB thus cannot be deleted) and the 'isChanged' field is '1' (representing a deleted row)
                    if (dr.ItemArray[(int)emp_records_columns.record_id].ToString() != "-1" && dr.ItemArray[(int)emp_records_columns.isChanged].ToString() == "1")
                    {
                        try
                        {
                            string sql = $@"
                                DELETE FROM [dbo].[employeeposition]
                                WHERE employee_id = '{empId}' AND id = '{dr.ItemArray[(int)emp_records_columns.record_id]}'
                            ";

                            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                            {
                                connection.Open();
                                using (SqlCommand command = new SqlCommand(sql, connection))
                                {
                                    int rowsAffected = command.ExecuteNonQuery();
                                    isEmpRecordDeleteSuccessful = rowsAffected > 0;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // exception logic
                            isEmpRecordDeleteSuccessful = false;
                        }

                        // add id of deleted record to deletedRecords list. This is used to create an audit log for the deletion of employment records
                        if (isEmpRecordDeleteSuccessful)
                            deletedRecords.Add(dr.ItemArray[(int)emp_records_columns.record_id].ToString());
                    }
                }

                // add audit log for deleted records
                if (deletedRecords.Count > 0)
                {
                    /*
                    * Add info about edits made to employee such as:
                    * Employee ID
                    * ID of Employment Records deleted
                    * */
                    string action = $"Deleted employment records; {String.Join(", ", deletedRecords.Select(lb => "id =" + lb).ToArray())}";
                    util.addAuditLog(Session["emp_id"].ToString(), empId, action);
                }

                // a list to contain the ids of added employment records. This list is used to create audit logs for addition of records
                List<string> addedRecords = new List<string>();

                //add new records data from gridview
                foreach (DataRow dr in dt.Rows)
                {

                    // only if the record id is '-1' and isChanged is '0' (representing a undeleted row) or isChanged is '2' (representing an edited row) then insert it
                    if (dr.ItemArray[(int)emp_records_columns.record_id].ToString() == "-1" && dr.ItemArray[(int)emp_records_columns.isChanged].ToString() != "1")
                    {
                        // insert new record
                        try
                        {
                            string sql = $@"
                                INSERT INTO [dbo].[employeeposition]
                                    ([employee_id]
                                        ,[position_id]
                                        ,[start_date]
                                        ,[expected_end_date]
                                        ,[employment_type]
                                        ,[dept_id]
                                        ,[years_worked]
                                        ,[annual_vacation_amt]
                                        ,[max_vacation_accumulation])
                                OUTPUT INSERTED.id
                                VALUES
                                    ( @EmployeeId
                                    ,@PositionId
                                    ,@StartDate
                                    ,@ExpectedEndDate
                                    ,@EmploymentType
                                    ,@DeptId
                                    ,@YearsWorked
                                    ,@AnnualVacationAmt
                                    ,@MaxVacationAccumulation
                                    );
                            ";

                            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                            {
                                connection.Open();
                                using (SqlCommand command = new SqlCommand(sql, connection))
                                {
                                    command.Parameters.AddWithValue("@EmployeeId", empId);
                                    command.Parameters.AddWithValue("@EmploymentType", dr.ItemArray[(int)emp_records_columns.employment_type]);
                                    command.Parameters.AddWithValue("@DeptId", dr.ItemArray[(int)emp_records_columns.dept_id]);
                                    command.Parameters.AddWithValue("@PositionId", dr.ItemArray[(int)emp_records_columns.pos_id]);
                                    command.Parameters.AddWithValue("@StartDate", dr.ItemArray[(int)emp_records_columns.start_date]);
                                    command.Parameters.AddWithValue("@ExpectedEndDate", dr.ItemArray[(int)emp_records_columns.expected_end_date]);
                                    if (dr.ItemArray[(int)emp_records_columns.employment_type].ToString() == "Contract")
                                        command.Parameters.AddWithValue("@YearsWorked", util.getNumYearsBetween(Convert.ToDateTime(dr.ItemArray[(int)emp_records_columns.start_date]), util.getCurrentDateToday()));
                                    else
                                        command.Parameters.AddWithValue("@YearsWorked", util.getCurrentDateToday().Year - Convert.ToDateTime(dr.ItemArray[(int)emp_records_columns.start_date]).Year);

                                    command.Parameters.AddWithValue("@AnnualVacationAmt", dr.ItemArray[(int)emp_records_columns.annual_vacation_amt]);
                                    command.Parameters.AddWithValue("@MaxVacationAccumulation", dr.ItemArray[(int)emp_records_columns.max_vacation_accumulation]);
                                    string new_record_id = command.ExecuteScalar().ToString();
                                    isEmpRecordInsertSuccessful = !util.isNullOrEmpty(new_record_id);


                                    if (isEmpRecordInsertSuccessful)
                                    {
                                        addedRecords.Add(new_record_id); // adding id of inserted record in added records list
                                        dr.SetField<string>((int)emp_records_columns.record_id, new_record_id); // add new id of inserted record into datatable
                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // exception logic
                            isEmpRecordInsertSuccessful = false;
                        }
                    }
                }

                // add audit log for added records
                if (addedRecords.Count > 0)
                {
                    /*
                    * Add info about edits made to employee such as:
                    * Employee ID
                    * ID of Employment Records added
                    * */
                    string action = $"Added employment records; {String.Join(", ", addedRecords.Select(lb => "id =" + lb).ToArray())}";
                    util.addAuditLog(Session["emp_id"].ToString(), empId, action);
                }

                int numEditedRecords = 0;
                List<string> editedEmpRecordsLog = new List<string>();
                //edit records data from gridview
                foreach (DataRow dr in dt.Rows)
                {

                    // only if the record id is not '-1' and isChanged is '2' (representing a edited row) then update it
                    if (dr.ItemArray[(int)emp_records_columns.record_id].ToString() != "-1" && dr.ItemArray[(int)emp_records_columns.isChanged].ToString() == "2")
                    {
                        // edit record
                        try
                        {
                            string sql = $@"
                                UPDATE [dbo].[employeeposition]
                                SET position_id= '{dr.ItemArray[(int)emp_records_columns.pos_id].ToString()}', 
                                    start_date= @StartDate, 
                                    expected_end_date= @ExpectedEndDate, 
                                    actual_end_date = @ActualEndDate, 
                                    employment_type= '{dr.ItemArray[(int)emp_records_columns.employment_type].ToString()}', 
                                    dept_id= {dr.ItemArray[(int)emp_records_columns.dept_id].ToString()},
                                    years_worked = @YearsWorked,
                                    annual_vacation_amt = @AnnualVacationAmt,
                                    max_vacation_accumulation = @MaxVacationAccumulation
                                WHERE id= @RecordId;
                            ";

                            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                            {
                                connection.Open();
                                using (SqlCommand command = new SqlCommand(sql, connection))
                                {
                                    command.Parameters.AddWithValue("@StartDate", Convert.ToDateTime(dr.ItemArray[(int)emp_records_columns.start_date].ToString()).ToString("MM/d/yyyy"));

                                    if (Convert.ToDateTime(dr.ItemArray[(int)emp_records_columns.expected_end_date].ToString()) != DateTime.MinValue)
                                        command.Parameters.AddWithValue("@ExpectedEndDate", Convert.ToDateTime(dr.ItemArray[(int)emp_records_columns.expected_end_date].ToString()).ToString("MM/d/yyyy"));
                                    else
                                        command.Parameters.AddWithValue("@ExpectedEndDate", DBNull.Value);

                                    if (!util.isNullOrEmpty(dr.ItemArray[(int)emp_records_columns.actual_end_date].ToString()))
                                    {
                                        command.Parameters.AddWithValue("@ActualEndDate", DateTime.ParseExact(dr.ItemArray[(int)emp_records_columns.actual_end_date].ToString(), "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy"));

                                        if (dr.ItemArray[(int)emp_records_columns.employment_type].ToString() == "Contract")
                                            // calculate years worked with actual end date
                                            command.Parameters.AddWithValue("@YearsWorked", util.getNumYearsBetween(Convert.ToDateTime(dr.ItemArray[(int)emp_records_columns.start_date]), DateTime.ParseExact(dr.ItemArray[(int)emp_records_columns.actual_end_date].ToString(), "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)));
                                    }

                                    else
                                    {
                                        command.Parameters.AddWithValue("@ActualEndDate", DBNull.Value);

                                        if (dr.ItemArray[(int)emp_records_columns.employment_type].ToString() == "Contract")
                                            // calculate years worked with today's date
                                            command.Parameters.AddWithValue("@YearsWorked", util.getNumYearsBetween(Convert.ToDateTime(dr.ItemArray[(int)emp_records_columns.start_date]), util.getCurrentDateToday()));
                                    }

                                    if (dr.ItemArray[(int)emp_records_columns.employment_type].ToString() == "Public Service")
                                        command.Parameters.AddWithValue("@YearsWorked", util.getCurrentDateToday().Year - Convert.ToDateTime(dr.ItemArray[(int)emp_records_columns.start_date]).Year);

                                    command.Parameters.AddWithValue("@AnnualVacationAmt", dr.ItemArray[(int)emp_records_columns.annual_vacation_amt]);
                                    command.Parameters.AddWithValue("@MaxVacationAccumulation", dr.ItemArray[(int)emp_records_columns.max_vacation_accumulation]);

                                    command.Parameters.AddWithValue("@RecordId", dr.ItemArray[(int)emp_records_columns.record_id]);


                                    int rowsAffected = command.ExecuteNonQuery();
                                    isEmpRecordEditFieldsSuccessful = rowsAffected > 0;

                                    if (isEmpRecordEditFieldsSuccessful)
                                    {
                                        // increment counter for number of records edited
                                        numEditedRecords++;

                                        // create array of info about each Field edited for a given record. This array is used to create the action for an audit log
                                        if (Session["editedRecords"] != null)
                                        {
                                            Dictionary<string, HashSet<string>> editedRecordsDict = (Dictionary<string, HashSet<string>>)Session["editedRecords"];

                                            foreach (KeyValuePair<string, HashSet<string>> entry in editedRecordsDict)
                                            {
                                                // create List<string> with record_id and Fields edited info in it
                                                string log = $"record_id= {entry.Key}, Fields edited= {String.Join(", ", entry.Value.ToArray())}";
                                                editedEmpRecordsLog.Add(log);
                                            }
                                            Session["editedRecords"] = null;
                                        }

                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // exception logic
                            isEmpRecordEditFieldsSuccessful = false;
                        }
                    }
                }

                // add audit log for edited records
                if (numEditedRecords > 0)
                {
                    /*
                    * Add info about edits made to employee such as:
                    * Employee ID
                    * ID of Employment Records edited and the end date added
                    * */
                    string action = $"Edited employment records; {String.Join("; ", editedEmpRecordsLog.ToArray())}";
                    util.addAuditLog(Session["emp_id"].ToString(), empId, action);
                }

                if (deletedRecords.Count > 0 || addedRecords.Count > 0 || numEditedRecords > 0)
                    isEmpRecordChanged = true;
            }
            else
            {
                // if table was empty
                noEmploymentRecordEnteredErrorPanel.Style.Add("display", "inline-block");
                isEmpRecordInsertSuccessful = isEmpRecordDeleteSuccessful = false;
            }
            isEmpRecordEditSuccessful = isEmpRecordDeleteSuccessful && isEmpRecordInsertSuccessful && isEmpRecordEditFieldsSuccessful;

            // END EMPLOYMENT RECORDS--------------------------------------------------------------------------------------------------------------------------

            // ADD FILES--------------------------------------------------------------------------------------------------------------------------
            if (Session["uploadedFiles"] != null)
            {
                List<HttpPostedFile> files = (List<HttpPostedFile>)Session["uploadedFiles"];

                // save uploaded file(s)
                foreach (HttpPostedFile uploadedFile in files)
                {
                    try
                    {
                        // upload file to db
                        string file_name = Path.GetFileName(uploadedFile.FileName);
                        string file_extension = Path.GetExtension(uploadedFile.FileName);

                        using (Stream fs = uploadedFile.InputStream)
                        {
                            using (BinaryReader br = new BinaryReader(fs))
                            {
                                byte[] bytes = br.ReadBytes((Int32)fs.Length);
                                string sql = $@"
                                    INSERT INTO [dbo].[filestorage]
                                        (
                                        [file_data]
                                        ,[file_name]
                                        ,[file_extension])
                                    OUTPUT INSERTED.file_id
                                    VALUES
                                        ( @FileData
                                        ,@FileName
                                        ,@FileExtension
                                        );";
                                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                                {
                                    connection.Open();
                                    string fileId = string.Empty;
                                    using (SqlCommand command = new SqlCommand(sql, connection))
                                    {
                                        command.Parameters.AddWithValue("@FileData", bytes);
                                        command.Parameters.AddWithValue("@FileName", file_name);
                                        command.Parameters.AddWithValue("@FileExtension", file_extension);
                                        fileId = command.ExecuteScalar().ToString();

                                        isFileUploadSuccessful = !util.isNullOrEmpty(fileId);
                                    }

                                    if (isFileUploadSuccessful)
                                    {
                                        // insert record into bridge entity which associates file(s) with a given employee
                                        sql = $@"
                                                INSERT INTO [dbo].[employeefiles] ([file_id],[employee_id],[emp_record_id])
                                                VALUES(
                                                    @FileId, 
                                                    @EmployeeId, 
                                                    (
                                                        SELECT ep.id FROM [dbo].[employeeposition] ep WHERE (ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date)) AND ep.employee_id = @EmployeeId
                                                    )
                                                );";
                                        using (SqlCommand command = new SqlCommand(sql, connection))
                                        {
                                            command.Parameters.AddWithValue("@FileId", fileId);
                                            command.Parameters.AddWithValue("@EmployeeId", empId);
                                            int rowsAffected = command.ExecuteNonQuery();
                                            isFileUploadSuccessful = rowsAffected > 0;
                                        }
                                        if(isFileUploadSuccessful)
                                            // add file id to add to audit log
                                            uploadedFilesIds.Add(fileId);
                                    }

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        isFileUploadSuccessful = false;
                    }
                }

                if (isFileUploadSuccessful)
                {
                    // add audit log
                    string fileActionString = String.Empty;
                    if (uploadedFilesIds.Count > 0)
                    {
                        fileActionString = $"Files uploaded: {String.Join(", ", uploadedFilesIds.Select(lb => "id= " + lb).ToArray())}";
                        util.addAuditLog(Session["emp_id"].ToString(), Session["emp_id"].ToString(), fileActionString);
                    }
                    isFilesChanged = true;
                }
            }
            // END ADDITION OF FILES--------------------------------------------------------------------------------------------------------------

            // ACCUMULATE PAST LIMIT-------------------------------------------------------------------------------
            if(ViewState["prevAccPastLimitStatus"] != null)
            {
                if (chkOnOff.Checked != Convert.ToBoolean(ViewState["prevAccPastLimitStatus"]) && ((chkOnOff.Checked && isFilesChanged && isFileUploadSuccessful) || !chkOnOff.Checked))
                {
                    try
                    {

                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            string status = chkOnOff.Checked ? "1" : "0";
                            string sql = $@"
                            UPDATE [dbo].[employeeposition]
                            SET can_accumulate_past_max = {status}
                            WHERE employee_id = @EmployeeId;
                    ";
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@EmployeeId", empId);
                                int rowsAffected = command.ExecuteNonQuery();
                                isAccumulatePastLimitSuccessful = rowsAffected > 0;
                            }
                            if (isAccumulatePastLimitSuccessful)
                            {
                                util.addAuditLog(Session["emp_id"].ToString(), Session["emp_id"].ToString(), "Employee can now accumulate past their limit");
                                isAccumulatePastLimitStatusChangedInDb = true;
                            }
                        }

                    }
                    catch (Exception exc)
                    {
                        isAccumulatePastLimitSuccessful = false;
                    }
                }
            }
            // END ACCUMULATE PAST LIMIT-------------------------------------------------------------------------------

            // reset page so it is ready for new edits
            Boolean isAccumulatePastLimitStatusChangedOnScreen = chkOnOff.Checked != Convert.ToBoolean(ViewState["prevAccPastLimitStatus"]);
            populatePage(empId);
            hideEmploymentRecordForm();

            // USER FEEDBACK--------------------------------------------------------------------------------------------------------------------------
            if (!isRolesChanged && !isLeaveBalancesChanged && !isEmpRecordChanged && !isFilesChanged && !isAccumulatePastLimitStatusChangedInDb)
            {
                noChangesMadePanel.Style.Add("display", "inline-block");
                if (!isAccumulatePastLimitStatusChangedInDb && !isFilesChanged && isAccumulatePastLimitStatusChangedOnScreen)
                {
                    noChangesMadeToAccStatus.Style.Add("display", "inline-block");
                }
            }
            else
            {
                // SUCCESS MESSAGES---------------------------------------------------------------

                // general success message
                if ((isRolesChanged && isLeaveBalancesChanged && isEmpRecordChanged && isFilesChanged && isAccumulatePastLimitStatusChangedInDb) && (isRolesEditSuccessful && isLeaveEditSuccessful && isEmpRecordEditSuccessful && isFileUploadSuccessful && isAccumulatePastLimitSuccessful))
                    editFullSuccessPanel.Style.Add("display", "inline-block");
                else
                {
                    // successful roles edit
                    if (isRolesChanged && isRolesEditSuccessful)
                        editRolesSuccessPanel.Style.Add("display", "inline-block");

                    // successful leave balances edit
                    if (isLeaveBalancesChanged && isLeaveEditSuccessful)
                        editLeaveSuccessPanel.Style.Add("display", "inline-block");

                    // successful employment records edit
                    if (isEmpRecordChanged && isEmpRecordEditSuccessful)
                        editEmpRecordSuccessPanel.Style.Add("display", "inline-block");

                    // successful file upload edit
                    if (isFilesChanged && isFileUploadSuccessful)
                        editEmpFilesPanel.Style.Add("display", "inline-block");

                    // successful accumulation past limit edit
                    if (isAccumulatePastLimitStatusChangedInDb && isAccumulatePastLimitSuccessful)
                        editAccumulatePastMaxSuccessPanel.Style.Add("display", "inline-block");

                    // ERROR MESSAGES------------------------------------------------------------------

                    // general error message if all aspects of edit fail
                    if (!isRolesEditSuccessful && !isLeaveEditSuccessful && !isEmpRecordEditSuccessful && !isFileUploadSuccessful && !isAccumulatePastLimitSuccessful)
                        editEmpErrorPanel.Style.Add("display", "inline-block");

                    // roles edit error
                    if (!isRolesEditSuccessful)
                        editRolesErrorPanel.Style.Add("display", "inline-block");

                    // leave balances edit error
                    if (!isLeaveEditSuccessful)
                        editLeaveBalancesErrorPanel.Style.Add("display", "inline-block");

                    // files upload error
                    if (!isFileUploadSuccessful)
                        editEmpFilesErrorPanel.Style.Add("display", "inline-block");

                    // error accumulation past limit edit
                    if (!isAccumulatePastLimitSuccessful)
                        editAccumulatePastMaxErrorPanel.Style.Add("display", "inline-block");

                    // emp records errors
                    if (!isEmpRecordEditSuccessful)
                    {
                        // general emp record edit error
                        if (!isEmpRecordInsertSuccessful && !isEmpRecordDeleteSuccessful && !isEmpRecordEditFieldsSuccessful)
                            editEmpRecordErrorPanel.Style.Add("display", "inline-block");
                        else
                        {
                            // specific emp record edit errors

                            // error deleting emp record(s)
                            if (!isEmpRecordDeleteSuccessful)
                                deleteEmpRecordsErrorPanel.Style.Add("display", "inline-block");

                            // error inserting new emp record(s)
                            if (!isEmpRecordInsertSuccessful)
                                addEmpRecordsErrorPanel.Style.Add("display", "inline-block");

                            // error editing end date in emp record(s)
                            if (!isEmpRecordEditFieldsSuccessful)
                                editEndDateEmpRecordsPanel.Style.Add("display", "inline-block");
                        }

                    }
                }

            }
            // END USER FEEDBACK--------------------------------------------------------------------------------------------------------------------------

            //scroll to top of page
            Page.MaintainScrollPositionOnPostBack = false;
            this.empDetailsContainer.Focus();
        }
        //_________________________________________________________________________


        // ADD NEW EMPLOYEE METHOD
        protected void submitBtn_Click(object sender, EventArgs e)
        {
            this.clearErrors();

            Auth auth = new Auth();

            // IDs, email
            string emp_id = employeeIdInput.Text, // Employee ID
                ihris_id = ihrisNumInput.Text,    // IHRIS ID
                email = adEmailInput.Text,        // AD Email
                firstname = string.Empty,         // Employee first name
                lastname = string.Empty,          // Employee last name
                username = string.Empty,          // Employee username
                nameFromAd = auth.getUserInfoFromActiveDirectory(email); // get first name, last name from active directory


            if (!util.isNullOrEmpty(nameFromAd))
            {
                // set first and last name
                string[] name = nameFromAd.Split(' ');
                firstname = name[0];
                lastname = name[1];

                // set username
                username = $"PLANNING\\ {firstname} {lastname}";
            }

            // Leave Balances 
            Dictionary<string, string> leaveMapping = util.getLeaveTypeMapping(),
                currentLeaveBalances = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kvp in leaveMapping)
            {
                string balance = getLeaveBalanceText(kvp.Key);
                string newKey = kvp.Value.Replace("_", "").Replace("[", "").Replace("]", "");
                currentLeaveBalances[newKey] = util.isNullOrEmpty(balance) ? "0" : balance;
            }

            // Roles
            List<string> authorizations = new List<string>() { "emp" };

            // get data from checkboxes and set authorizations
            if (supervisorCheck.Checked)
                authorizations.Add("sup");
            if (hr1Check.Checked)
                authorizations.Add("hr1");
            if (hr2Check.Checked)
                authorizations.Add("hr2");
            if (hr3Check.Checked)
                authorizations.Add("hr3");

            if (hr2Check.Checked || hr3Check.Checked)
            {
                if (contractCheck.Checked)
                    authorizations.Add("hr_contract");
                if (publicServiceCheck.Checked)
                    authorizations.Add("hr_public_officer");
            }

            // Employment Records
            DataTable dt = empRecordsDataSource;

            Boolean isInsertSuccessful = false,
                isDuplicateIdentifier = false;

            // name from AD must be populated because that means the user exists in AD
            // dt cannot be null because that means that no employment record is added
            if (!util.isNullOrEmpty(nameFromAd) && dt != null)
            {

                // Employee IDs, email, name, username and leave balances
                try
                {
                    string sql = $@"
                        INSERT INTO [dbo].[employee]
                           ([employee_id]
                              ,[ihris_id]
                              ,[username]
                              ,[first_name]
                              ,[last_name]
                              ,[email]
                              , {String.Join(", ", leaveMapping.Values.ToArray<string>())})
                        VALUES
                           ( @EmployeeId
                            ,@IhrisId
                            ,@Username
                            ,@FirstName
                            ,@LastName
                            ,@Email
                            ,@{String.Join(", @", currentLeaveBalances.Keys.ToArray<string>())}
                            );
                    ";

                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@EmployeeId", emp_id);
                            command.Parameters.AddWithValue("@IhrisId", ihris_id);
                            command.Parameters.AddWithValue("@Username", username);
                            command.Parameters.AddWithValue("@FirstName", firstname);
                            command.Parameters.AddWithValue("@LastName", lastname);
                            command.Parameters.AddWithValue("@Email", email);

                            foreach (KeyValuePair<string, string> kvp in currentLeaveBalances)
                            {
                                command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value);
                            }

                            int rowsAffected = command.ExecuteNonQuery();
                            isInsertSuccessful = rowsAffected > 0;

                        }
                    }
                }
                catch (Exception ex)
                {
                    // exception logic

                    // if duplicate employee is entered, ie. if an employee ID already within the system is inputted again
                    if (ex.Message.ToString().Contains("Violation of PRIMARY KEY constraint") || ex.Message.ToString().Contains("Cannot insert duplicate key in object 'dbo.employee'"))
                        isDuplicateIdentifier = true;
                    isInsertSuccessful = false;
                }

                // Roles 
                // add roles to employee
                foreach (string role in authorizations)
                {
                    try
                    {
                        string sql = $@"
                        INSERT INTO [dbo].[employeerole]
                            ([employee_id]
                                ,[role_id])
                        VALUES
                            ( @EmployeeId
                            ,@RoleId
                            );
                    ";

                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@EmployeeId", emp_id);
                                command.Parameters.AddWithValue("@RoleId", role);

                                int rowsAffected = command.ExecuteNonQuery();
                                isInsertSuccessful = isInsertSuccessful && rowsAffected > 0;

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // exception logic
                        isInsertSuccessful = false;
                        break;
                    }
                }

                // Employment Records
                //get data from gridview
                /**
                    * datatable is formatted in the folowing manner in the ItemArray:
                    * Index:         0             1            2         3        4        5          6               7              8
                    * Columns : record_id, employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date, isChanged
                * */
                foreach (DataRow dr in dt.Rows)
                {
                    // if row is not deleted
                    if (dr.ItemArray[(int)emp_records_columns.isChanged].ToString() != "1")
                    {
                        try
                        {
                            string sql = $@"
                            INSERT INTO [dbo].[employeeposition]
                                ([employee_id]
                                    ,[position_id]
                                    ,[start_date]
                                    ,[expected_end_date]
                                    ,[employment_type]
                                    ,[dept_id]
                                    ,[years_worked]
                                    ,[annual_vacation_amt]
                                    ,[max_vacation_accumulation])
                            VALUES
                                ( @EmployeeId
                                ,@PositionId
                                ,@StartDate
                                ,@ExpectedEndDate
                                ,@EmploymentType
                                ,@DeptId
                                ,@YearsWorked
                                ,@AnnualVacationAmt
                                ,@MaxVacationAccumulation
                                );
                        ";

                            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                            {
                                connection.Open();
                                using (SqlCommand command = new SqlCommand(sql, connection))
                                {
                                    command.Parameters.AddWithValue("@EmployeeId", emp_id);
                                    command.Parameters.AddWithValue("@EmploymentType", dr.ItemArray[(int)emp_records_columns.employment_type]);
                                    command.Parameters.AddWithValue("@DeptId", dr.ItemArray[(int)emp_records_columns.dept_id]);
                                    command.Parameters.AddWithValue("@PositionId", dr.ItemArray[(int)emp_records_columns.pos_id]);
                                    command.Parameters.AddWithValue("@StartDate", dr.ItemArray[(int)emp_records_columns.start_date]);
                                    command.Parameters.AddWithValue("@ExpectedEndDate", dr.ItemArray[(int)emp_records_columns.expected_end_date]);

                                    if (dr.ItemArray[(int)emp_records_columns.employment_type].ToString() == "Contract")
                                        // get number of years worked by checking start date against current date
                                        command.Parameters.AddWithValue("@YearsWorked", util.getNumYearsBetween(Convert.ToDateTime(dr.ItemArray[(int)emp_records_columns.start_date]), util.getCurrentDateToday()));
                                    else
                                        // get number of years worked from start of year to  start of year
                                        command.Parameters.AddWithValue("@YearsWorked", util.getCurrentDateToday().Year - Convert.ToDateTime(dr.ItemArray[(int)emp_records_columns.start_date]).Year);

                                    command.Parameters.AddWithValue("@AnnualVacationAmt", dr.ItemArray[(int)emp_records_columns.annual_vacation_amt]);
                                    command.Parameters.AddWithValue("@MaxVacationAccumulation", dr.ItemArray[(int)emp_records_columns.max_vacation_accumulation]);


                                    int rowsAffected = command.ExecuteNonQuery();
                                    isInsertSuccessful = isInsertSuccessful && (rowsAffected > 0);
                                    if (rowsAffected > 0)
                                        fullFormSubmitSuccessPanel.Style.Add("display", "inline-block");
            

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // exception logic
                            isInsertSuccessful = false;
                        }
                    }

                }
            }

            if (isInsertSuccessful)
            {
                // add audit log

                /*
                * Add info about new employee created such as:
                * Leave balances: Vacation, Sick, Personal, Casual
                * Employee permissions
                * */
                string action = $"Created new Employee; {String.Join(", ", currentLeaveBalances.Select(lb => lb.Key + "=" + lb.Value).ToArray())} ; Permissions: {String.Join(",", authorizations.ToArray())}";
                util.addAuditLog(Session["emp_id"].ToString(), emp_id, action);

                // hide submit button if insert is successful
                submitBtn.Visible = false;
            }
            else
            {
                fullFormErrorPanel.Style.Add("display", "inline-block");
                if (util.isNullOrEmpty(nameFromAd))
                    emailNotFoundErrorPanel.Style.Add("display", "inline-block");
                if (dt == null)
                    noEmploymentRecordEnteredErrorPanel.Style.Add("display", "inline-block");
                if (isDuplicateIdentifier)
                    duplicateIdentifierPanel.Style.Add("display", "inline-block");
            }


            //scroll to top of page
            Page.MaintainScrollPositionOnPostBack = false;
            this.empDetailsContainer.Focus();

            
        }
        //_________________________________________________________________________

        // FILE AND ALLOW ACCUMULATION PAST LIMIT METHODS
        protected void clearFileValidationMessages()
        {
            duplicateFileNamesPanel.Style.Add("display", "none");
            invalidFileTypePanel.Style.Add("display", "none");
            fileUploadedTooLargePanel.Style.Add("display", "none");
            noFileUploaded.Style.Add("display", "none");
        }

        protected void uploadFilesBtn_Click(object sender, EventArgs e)
        {
            clearFileValidationMessages();
            // uploads file to Session storage to await upload to DB
            if (FileUpload1.HasFiles)
            {
                List<string> filesTooLarge = new List<string>();
                List<string> invalidFiles = new List<string>();

                // used in edit mode to ensure files with the same name cannot be uploaded by a single user
                List<string> duplicateFiles = new List<string>();

                // used to store list of files added
                List<HttpPostedFile> files = new List<HttpPostedFile>();

                // used to show data about added files in bulleted list
                DataTable dt = new DataTable();
                dt.Columns.Add("file_name", typeof(string));

                // used to check whether the files uploaded fit the size requirement specified in the web config
                HttpRuntimeSection section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
                int maxRequestLength = section != null ? section.MaxRequestLength : 4096;

                // used to check whether the file(s) uploaded are of a certain format
                List<string> allowedFileExtensions = new List<string>() { ".pdf", ".doc", ".docx" };

                foreach (HttpPostedFile uploadedFile in FileUpload1.PostedFiles)
                {
                    string fileName = Path.GetFileName(uploadedFile.FileName).ToString(),
                           fileExt = Path.GetExtension(uploadedFile.FileName).ToString();
                    if (uploadedFile.ContentLength < maxRequestLength)
                    {
                        if (allowedFileExtensions.Contains(fileExt))
                        {

                            // check for duplicate file name
                            if (filesToDownloadList.Items.FindByText(fileName) != null)
                            {
                                duplicateFiles.Add(fileName);
                                continue;
                            }
                            dt.Rows.Add(fileName);
                            files.Add(uploadedFile);
                        }
                        else
                            invalidFiles.Add(fileName);
                    }
                    else
                    {
                        filesTooLarge.Add(fileName);

                        if (!allowedFileExtensions.Contains(fileExt))
                            invalidFiles.Add(fileName);
                    }
                }

                if (invalidFiles.Count > 0)
                {
                    HtmlGenericControl txt = (HtmlGenericControl)invalidFileTypePanel.FindControl("invalidFileTypeErrorTxt");
                    txt.InnerText = $"Could not upload {String.Join(", ", invalidFiles.Select(fileName => "'" + fileName + "'").ToArray())}. Invalid file type(s): {String.Join(", ", invalidFiles.Select(fileName => "'" + Path.GetExtension(fileName).ToString() + "'").ToArray())}";
                    invalidFileTypePanel.Style.Add("display", "inline-block");
                }

                if (filesTooLarge.Count > 0)
                {
                    HtmlGenericControl txt = (HtmlGenericControl)invalidFileTypePanel.FindControl("fileUploadTooLargeTxt");
                    txt.InnerText = $"Could not upload {String.Join(", ", filesTooLarge.Select(fileName => "'" + fileName + "'").ToArray())}. File(s) too large";
                    fileUploadedTooLargePanel.Style.Add("display", "inline-block");
                }

                if (duplicateFiles.Count > 0)
                {
                    HtmlGenericControl txt = (HtmlGenericControl)duplicateFileNamesPanel.FindControl("duplicateFileNameTxt");
                    txt.InnerText = $"Could not upload {String.Join(", ", duplicateFiles.Select(fileName => "'" + fileName + "'").ToArray())}. Uploaded file name(s) already exist";
                    duplicateFileNamesPanel.Style.Add("display", "inline-block");
                }

                if (dt.Rows.Count > 0)
                {

                    // add files to session so they will persist after postback
                    Session["uploadedFiles"] = files;

                    // show files uploaded
                    filesUploadedPanel.Visible = true;

                    clearAllFilesBtn.Visible = true;

                    // populate files uploaded list
                    filesUploadedListView.DataSource = dt;
                    filesUploadedListView.DataBind();

                    noFilesUploadedDisclaimerPanel.Visible = false;
                }
            }
            else
            {
                List<HttpPostedFile> files = null;

                if (Session["uploadedFiles"] != null)
                    files = (List<HttpPostedFile>)Session["uploadedFiles"];

                if (Session["uploadedFiles"] == null || files == null || files.Count <= 0)
                {
                    filesUploadedPanel.Visible = false;
                    noFileUploaded.Style.Add("display", "inline-block");

                    clearAllFilesBtn.Visible = false;
                    noFilesUploadedDisclaimerPanel.Visible = true;
                }

            }

        }

        protected void resetFiles()
        {
            clearFileValidationMessages();
            filesUploadedPanel.Visible = false;
            FileUpload1.Dispose();

            filesUploadedListView.DataSource = new DataTable();
            filesUploadedListView.DataBind();

            Session["uploadedFiles"] = null;

            clearAllFilesBtn.Visible = false;
            if(chkOnOff.Checked)
                noFilesUploadedDisclaimerPanel.Visible = true;
        }

        protected void loadPreviouslyUploadedFiles(string empId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    string sql = $@"
                        SELECT 
                            (SELECT 
					            STUFF
					            (
						            (SELECT ', ' + fs.file_name
						            FROM [dbo].filestorage fs 
						            LEFT JOIN [dbo].employeefiles ef ON ef.employee_id = ep.employee_id AND ef.leave_transaction_id IS NULL AND ef.emp_record_id IS NOT NULL
						            WHERE fs.file_id = ef.file_id
						            FOR XML PATH(''))
						            ,1
						            ,1,
						            ''
					            )
				            ) as 'files',
                            (SELECT 
					            STUFF
					            (
						            (SELECT ', ' + CAST(fs.file_id AS NVARCHAR(MAX))
						            FROM [dbo].filestorage fs 
						            LEFT JOIN [dbo].employeefiles ef ON ef.employee_id = ep.employee_id AND ef.leave_transaction_id IS NULL AND ef.emp_record_id IS NOT NULL
						            WHERE fs.file_id = ef.file_id
						            FOR XML PATH(''))
						            ,1
						            ,1,
						            ''
					            )
				            ) as 'files_id',
                            ISNULL(ep.can_accumulate_past_max, 0) as can_accumulate_past_max
                        FROM [dbo].[employeeposition] ep
                        WHERE employee_id = {empId};
                    ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {

                            string files = string.Empty,
                                   filesIds = string.Empty,
                                   canAccumulatePastMax = string.Empty;
                            while (reader.Read())
                            {
                                files = reader["files"].ToString();
                                filesIds = reader["files_id"].ToString();
                                canAccumulatePastMax = reader["can_accumulate_past_max"].ToString(); ;
                            }

                            chkOnOff.Checked = canAccumulatePastMax == "True";

                            // set viewstate variable for prev value in case of change
                            ViewState["prevAccPastLimitStatus"] = chkOnOff.Checked;

                            // hide disclaimer shown when no file is uploaded but acc past limit is true 
                            // hide file upload panel
                            resetFiles();
                            noFilesUploadedDisclaimerPanel.Visible = fileUploadPanel.Visible = false;

                            if (!util.isNullOrEmpty(files) && !util.isNullOrEmpty(filesIds))
                            {
                                string[] fileNamesArr = files.Split(',');
                                string[] fileIdsArr = filesIds.Split(',');
                                DataTable dt = new DataTable();
                                dt.Columns.Add("file_id", typeof(string));
                                dt.Columns.Add("file_name", typeof(string));

                                for (int i = 0; i < fileNamesArr.Length; i++)
                                {
                                    dt.Rows.Add(fileIdsArr[i].Trim(), fileNamesArr[i].Trim());
                                }

                                filesToDownloadList.DataValueField = "file_id";
                                filesToDownloadList.DataTextField = "file_name";
                                filesToDownloadList.DataSource = dt;
                                filesToDownloadList.DataBind();
                                filesToDownloadPanel.Visible = true;
                            }
                            else
                            {
                                filesToDownloadList.DataSource = new DataTable();
                                filesToDownloadList.DataBind();
                                filesToDownloadPanel.Visible = false;
                            }
                                

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //exception logic
                throw ex;
            }
        }

        protected void clearAllFilesBtn_Click(object sender, EventArgs e)
        {
            // clears uploaded files from Session storage

            resetFiles();
        }

        protected void clearIndividualFileBtn_Click(object sender, EventArgs e)
        {
            // remove an individual file from List of uploaded files in Session

            LinkButton btn = sender as LinkButton;
            string file_name = btn.Attributes["data-id"].ToString();

            DataTable dt = new DataTable();
            dt.Columns.Add("file_name", typeof(string));

            // go through all files and create new datatable
            List<HttpPostedFile> files = null;
            if (Session["uploadedFiles"] != null)
            {
                files = (List<HttpPostedFile>)Session["uploadedFiles"];

                HttpPostedFile fileToRemove = files.SingleOrDefault<HttpPostedFile>(file => Path.GetFileName(file.FileName).ToString() == file_name);
                if (fileToRemove != null)
                    files.Remove(fileToRemove);

                if (files.Count > 0)
                {
                    foreach (HttpPostedFile file in files)
                    {
                        dt.Rows.Add(Path.GetFileName(file.FileName).ToString());
                    }
                    Session["uploadedFiles"] = files;
                    noFilesUploadedDisclaimerPanel.Visible = false;
                }
                else
                {
                    resetFiles();
                }

                filesUploadedListView.DataSource = dt;
                filesUploadedListView.DataBind();
            }
        }

        protected void btnDownloadFiles_Click(object sender, EventArgs e)
        {
            // downloads the files uploaded to a LA

            string file = filesToDownloadList.SelectedValue.ToString();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                            SELECT file_data FROM [dbo].[filestorage] WHERE file_id = '{file}';
                        ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            byte[] result = null;
                            while (reader.Read())
                            {
                                result = (byte[])reader["file_data"];
                            }
                            Response.Clear();
                            Response.AddHeader("Cache-Control", "no-cache, must-revalidate, post-check=0, pre-check=0");
                            Response.AddHeader("Pragma", "no-cache");
                            Response.AddHeader("Content-Description", "File Download");
                            Response.AddHeader("Content-Type", "application/force-download");
                            Response.AddHeader("Content-Transfer-Encoding", "binary\n");
                            Response.AddHeader("content-disposition", "attachment;filename=" + filesToDownloadList.SelectedItem.Text);
                            Response.BinaryWrite(result);
                            Response.Flush();
                            Response.Close();

                        }
                    }
                }
            }
            catch (System.Threading.ThreadAbortException exf)
            {
                //do nothing
                return;
            }
            catch (Exception ex)
            {
                //exception logic
                throw ex;
            }
        }

        protected void btnDeleteFile_Click(object sender, EventArgs e)
        {
            string file = filesToDownloadList.SelectedValue.ToString();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                            BEGIN TRANSACTION;
                                -- set employee to not be able to accumulate past max if associated file is deleted
                                UPDATE [dbo].[employeeposition] 
                                SET can_accumulate_past_max = 0
                                WHERE id = (SELECT emp_record_id FROM [dbo].[employeefiles] WHERE file_id = '{file}')

                                -- delete file itself
                                DELETE FROM [dbo].[filestorage] WHERE file_id = '{file}';
                                
                            COMMIT;
                        ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // reload uploaded files
                    if (Request.QueryString.HasKeys())
                    {
                        string empId = Request.QueryString["empId"];
                        loadPreviouslyUploadedFiles(empId);
                    }
                        
                }
            }
            catch (Exception ex)
            {
                //exception logic
                throw ex;
            }
        }

        protected void chkOnOff_CheckedChanged(object sender, EventArgs e)
        {
            fileUploadPanel.Visible = noFilesUploadedDisclaimerPanel.Visible = chkOnOff.Checked;
            if (!chkOnOff.Checked)
                resetFiles();
        }
        //_________________________________________________________________________

        protected void refreshForm(object sender, EventArgs e)
        {
            string pathname = string.Empty;
            if (util.isNullOrEmpty(Request.QueryString["empId"]))
                pathname = $"~/HR/EmployeeDetails?mode={ Request.QueryString["mode"]}";
            else
                pathname = $"~/HR/EmployeeDetails?mode={Request.QueryString["mode"]}&empId={Request.QueryString["empId"]}";
            Response.Redirect(pathname);
        }

        protected void returnToPreviousBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/HR/AllEmployees.aspx");
        }

        protected ListViewItem getSelectedLeaveBalanceListViewItem(string leaveType)
        {
            // get item 
            foreach (ListViewItem item in leaveBalancesListView.Items)
            {
                HiddenField id = (HiddenField)item.FindControl("leaveTypeHiddenField");
                if (id.Value == leaveType)
                    return item;
            }

            return null;
        }

        protected string getLeaveBalanceText(string leaveType)
        {
            ListViewItem selectedItem = getSelectedLeaveBalanceListViewItem(leaveType);
            TextBox balance = (TextBox)selectedItem.FindControl("LeaveInput");
            return balance.Text;
        }

        protected void setLeaveBalanceText(string balance, string leaveType)
        {
            ListViewItem selectedItem = getSelectedLeaveBalanceListViewItem(leaveType);
            TextBox balanceTxt = (TextBox)selectedItem.FindControl("LeaveInput");
            balanceTxt.Text = balance;
        }

        
    }
}