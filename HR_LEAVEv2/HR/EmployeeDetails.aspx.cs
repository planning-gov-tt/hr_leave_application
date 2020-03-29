using HR_LEAVEv2.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
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
            status_class = 11
        };

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
            endDateRequiredValidator.Enabled = empTypeList.SelectedValue == "Public Service";

            if (!this.IsPostBack)
            {

                ViewState["Gridview1_dataSource"] = null;
                if (Request.QueryString.HasKeys())
                {
                    string mode = Request.QueryString["mode"];
                    string empId = Request.QueryString["empId"];

                    if (mode == "edit")
                    {
                        // populates page and authorizes HR user based on employee's employment type
                        populatePage(empId);
                        this.adjustPageForEditMode();
                    }
                        
                    else
                        this.adjustPageForCreateMode();
                } 
            } else
            {
                // set is
                if (Request.QueryString["mode"] != null)
                    isEditMode = Request.QueryString["mode"] == "edit";
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

            // submit
            submitFullFormPanel.Visible = true;
            editFormPanel.Visible = false;

        }
        //_________________________________________________________________________

        
        // EMPLOYMENT RECORDS GRIDVIEW METHODS
        protected void bindGridview()
        {
            // sets data for gridview table
            DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;
            if (dt != null)
            {
                DataView dataView = dt.AsDataView();
                dataView.Sort = "start_date DESC, actual_end_date ASC";
                dt = dataView.ToTable();

                ViewState["Gridview1_dataSource"] = dt;
                GridView1.DataSource = dt;
                GridView1.DataBind();
            }
        }
   
        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            this.bindGridview();
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;
            if (dt != null)
            {
                // sets isChanged to 1
                int indexOfRowInDt = e.RowIndex;
                dt.Rows[indexOfRowInDt].SetField<string>((int)emp_records_columns.isChanged, "1");
                ViewState["Gridview1_dataSource"] = dt;

            }
            this.bindGridview();
        }
 
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
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
                LinkButton endEmpRecordBtn = (LinkButton)e.Row.FindControl("endEmpRecordBtn");
                if (!String.IsNullOrEmpty(e.Row.Cells[i].Text.ToString()) && e.Row.Cells[i].Text.ToString() != "&nbsp;")
                    endEmpRecordBtn.Visible = false;
                else
                    endEmpRecordBtn.Visible = true;
            }
        }

        protected void GridView1_DataBound(object sender, EventArgs e)
        {
            // this function executes after all data is bound for the Gridview containing employment records
            // it checks to see if all rows are deleted (isChanged = 1)
            // if the above condition is true then it hides the header row of the gridview
            if (GridView1.HeaderRow != null)
            {
                bool isTableEmpty = true;
                if (ViewState["Gridview1_dataSource"] != null)
                {
                    DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row.ItemArray[(int)emp_records_columns.isChanged].ToString() != "1")
                                isTableEmpty = false;
                        }
                    }
                }
                GridView1.HeaderRow.Visible = !isTableEmpty;
            }
                

        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
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
                if (ViewState["record_being_edited"] != null)
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
                string posSearchString = GridView1.Rows[index].Cells[GetColumnIndexByName(GridView1.Rows[index], "pos_name")].Text.ToString(),
                    emptypeSearchString = GridView1.Rows[index].Cells[GetColumnIndexByName(GridView1.Rows[index], "employment_type")].Text.ToString(),
                    deptSearchString = GridView1.Rows[index].Cells[GetColumnIndexByName(GridView1.Rows[index], "dept_name")].Text.ToString();

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
                txtStartDate.Text = GridView1.Rows[index].Cells[GetColumnIndexByName(GridView1.Rows[index], "start_date")].Text.ToString();
                // expected end date (if any)
                txtEndDate.Text = GridView1.Rows[index].Cells[GetColumnIndexByName(GridView1.Rows[index], "expected_end_date")].Text.ToString();
                // actual end date (if any)
                string actualEndDate = GridView1.Rows[index].Cells[GetColumnIndexByName(GridView1.Rows[index], "actual_end_date")].Text.ToString();

                // show actual end date textbox if actual end date is populated
                if (!String.IsNullOrEmpty(actualEndDate) && !String.IsNullOrWhiteSpace(actualEndDate) && actualEndDate != "&nbsp;")
                {
                    actualEndDateSpan.Style.Add("display", "inline");
                    txtActualEndDate.Text = actualEndDate;
                }

                // highlight row being edited
                GridView1.Rows[index].CssClass = "highlighted-record";

                // to be used when saving edits made to employment record to DataTable dt for Employment Records. User must still save all their changes
                ViewState["record_being_edited"] = index;
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

            if (!String.IsNullOrEmpty(endDate))
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
            if (!String.IsNullOrEmpty(actualEndDate))
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
                    if (!String.IsNullOrEmpty(startDate))
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
                    end = DateTime.Now;
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
            this.resetAddEmploymentRecordFormFields();
            addEmpRecordForm.Visible = false;
            showFormBtn.Visible = true;
            addNewRecordBtn.Visible = true;
            editRecordBtn.Visible = false;
        }

        protected void removeHighlightFromEditedRecord()
        {
            if (ViewState["record_being_edited"] != null)
            {
                int index = Convert.ToInt32(ViewState["record_being_edited"].ToString());
                GridView1.Rows[index].CssClass = GridView1.Rows[index].CssClass.Replace("highlighted-record", "");
                ViewState["record_being_edited"] = null;
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
                dept_name = deptList.SelectedItem.Text;

            Boolean isValidated = validateDates(startDate, endDate),
                isRecordAllowed = true;

            if (isValidated)
            {

                DataTable dt;
                if (ViewState["Gridview1_dataSource"] == null)
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
                }
                else
                    // get pre-existing data table
                    dt = ViewState["Gridview1_dataSource"] as DataTable;

                // check if there already exists an active record. Two active records cannot exist at the same time
                foreach (DataRow dr in dt.Rows)
                {
                    // check if there is any record that is active, the record cannot be deleted
                    if (dr.ItemArray[(int)emp_records_columns.isChanged].ToString() != "1" && dr.ItemArray[(int)emp_records_columns.status].ToString() == "Active")
                    {
                        multipleActiveRecordsAddRecordPanel.Style.Add("display", "inline-block");
                        isRecordAllowed = false;
                    }

                }

                if (isRecordAllowed)
                {
                    // case: where no end date is entered. If the employment record is for Contract then 3 years are added to the start date and 
                    // this date is entered for the expected end date value
                    DateTime expected_end_date = DateTime.MinValue;
                    if (String.IsNullOrEmpty(endDate))
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
                        if(dr.ItemArray[(int)emp_records_columns.isChanged].ToString() != "1")
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
                        //record_id, employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date, isChanged, actual_end_date, status, status_class
                        dt.Rows.Add(-1, emp_type, dept_id, dept_name, position_id, position_name, DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture), expected_end_date, "0", "", "Active", "label-success");
                        ViewState["Gridview1_dataSource"] = dt;
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

            if(ViewState["Gridview1_dataSource"] != null && Session["empRecordRowIndex"] != null)
            {
                DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;
                int indexInDt = Convert.ToInt32(Session["empRecordRowIndex"]);

                string startDate = Convert.ToDateTime(dt.Rows[indexInDt][(int)emp_records_columns.start_date]).ToString("d/MM/yyyy");
                DateTime end = validateActualEndDate(startDate, txtEmpRecordEndDate.Text, invalidEndDatePanel, actualEndDateIsWeekendPanel, endDateBeforeStartDatePanel, emptyEndDatePanel);
                if (end != DateTime.MinValue)
                {
                    // edit datatable and rebind gridview

                    if (isActualEndDateValid(end.ToString("d/MM/yyyy"), indexInDt))
                    {
                        // set record to be edited
                        dt.Rows[indexInDt].SetField<string>((int)emp_records_columns.isChanged, "2");

                        // set actual end date
                        dt.Rows[indexInDt].SetField<string>((int)emp_records_columns.actual_end_date, end.ToString("d/MM/yyyy"));

                        // change status 
                        if (isRecordActive(end.ToString("d/MM/yyyy")))
                        {
                            dt.Rows[indexInDt][(int)emp_records_columns.status] = "Active";
                            dt.Rows[indexInDt][(int)emp_records_columns.status_class] = "label-success";
                        }
                        else
                        {
                            dt.Rows[indexInDt][(int)emp_records_columns.status] = "Inactive";
                            dt.Rows[indexInDt][(int)emp_records_columns.status_class] = "label-danger";
                        }

                        ViewState["Gridview1_dataSource"] = dt;
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
                    else
                        multipleActiveRecordsEndRecordPanel.Style.Add("display", "inline-block");
                }
            }
            

        }

        protected Boolean isRecordActive(string proposedEndDate)
        {
            // returns a Boolean which represents whether the proposed actual end date passed will make a record active or inactive. The date passed is assumed to be validated

            // if the passed end date is empty then the record is automaticaly Active
            if (String.IsNullOrEmpty(proposedEndDate) || String.IsNullOrWhiteSpace(proposedEndDate) || proposedEndDate == "&nbsp;")
                return true;
            else
                // if today is before the passed actual end date then the record is active and otherwise, inactive
                return (DateTime.Compare(DateTime.Now, DateTime.ParseExact(proposedEndDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)) < 0);
        }

        protected Boolean isActualEndDateValid(string proposedEndDate, int index)
        {
            // returns a Boolean representing whether the proposed end date passed is valid in terms of the rest of existing records. This method checks the other records to see if
            // any other active records exist in order to validate the passed end date.
            if (ViewState["Gridview1_dataSource"] != null)
            {
                int numActiveRows = 0, i = 0;

                // state of passed end date and corresponding record
                bool isProposedEndDateActive = isRecordActive(proposedEndDate);

                DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;
                foreach (DataRow dr in dt.Rows)
                {
                    // once the record is not deleted and is not the record currently being edited (the record to which the proposed end date will belong to)
                    if (dr[(int)emp_records_columns.isChanged].ToString() != "1" && i != index)
                    {
                        if (dr[(int)emp_records_columns.status].ToString() == "Active")
                            numActiveRows++;
                    }
                    i++;
                }
                if (isProposedEndDateActive)
                    numActiveRows++;

                return numActiveRows <= 1;
            }

            return false;

        }

        protected void editRecordBtn_Click(object sender, EventArgs e)
        {
            // edits the selected record

            clearErrors();
            if (ViewState["record_being_edited"] != null)
            {

                // the values being proposed for edit. These may or may not have been changed so must be checked in order to know which to edit or not
                string position_id = positionList.SelectedValue,
                    position_name = positionList.SelectedItem.Text,
                    startDate = txtStartDate.Text.ToString(),
                    expectedEndDate = txtEndDate.Text.ToString(),
                    actualEndDate = txtActualEndDate.Text.ToString(),
                    emp_type = empTypeList.SelectedValue,
                    dept_id = deptList.SelectedValue,
                    dept_name = deptList.SelectedItem.Text;

                // validate start and expected end date
                Boolean isStartAndExpectedEndDateValidated = validateDates(startDate, expectedEndDate);

                // validate actual end date
                Boolean isActualEndDateValidated = validateActualEndDate(startDate, actualEndDate, recordEditEndDateInvalidPanel, recordEditEndDateOnWeekend, recordEditEndDateBeforeStartDate, null) != DateTime.MinValue;

                // index of record being edited
                int index = Convert.ToInt32(ViewState["record_being_edited"].ToString());

                if (ViewState["Gridview1_dataSource"] != null)
                {
                    DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;
                    if (isStartAndExpectedEndDateValidated && isActualEndDateValidated)
                    {
                        // check if any values are changed and edit relevant values in datatable
                        Boolean isPositionChanged = dt.Rows[index].ItemArray[(int)emp_records_columns.pos_id].ToString() != position_id,
                            isDeptChanged = dt.Rows[index].ItemArray[(int)emp_records_columns.dept_id].ToString() != dept_id,
                            isEmpTypeChanged = dt.Rows[index].ItemArray[(int)emp_records_columns.employment_type].ToString() != emp_type,
                            isStartDateChanged = Convert.ToDateTime(dt.Rows[index].ItemArray[(int)emp_records_columns.start_date]).ToString("d/MM/yyyy") != startDate,
                            isExpectedEndDateChanged = Convert.ToDateTime(dt.Rows[index].ItemArray[(int)emp_records_columns.expected_end_date]).ToString("d/MM/yyyy") != expectedEndDate,
                            isActualEndDateChanged = dt.Rows[index][(int)emp_records_columns.actual_end_date].ToString() != actualEndDate,
                            isEditActualEndDateSuccessful = true;

                        if (isPositionChanged || isDeptChanged || isEmpTypeChanged || isStartDateChanged || isExpectedEndDateChanged || isActualEndDateChanged)
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


                                if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrWhiteSpace(startDate) && isStartDateChanged)
                                {
                                    dt.Rows[index][(int)emp_records_columns.start_date] = DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                    editsMade.Add("Start Date");
                                }
                                else if (isStartDateChanged && (String.IsNullOrEmpty(startDate) || String.IsNullOrWhiteSpace(startDate) || startDate == "&nbsp;"))
                                    invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");

                                if (isExpectedEndDateChanged)
                                {
                                    editsMade.Add("Expected End Date");

                                    if (!String.IsNullOrEmpty(expectedEndDate) && !String.IsNullOrWhiteSpace(expectedEndDate) && expectedEndDate != "&nbsp;")
                                        dt.Rows[index][(int)emp_records_columns.expected_end_date] = DateTime.ParseExact(expectedEndDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                    else
                                        dt.Rows[index][(int)emp_records_columns.expected_end_date] = DateTime.MinValue;
                                }

                                if (isActualEndDateChanged)
                                {
                                    // does edit make employee have more than one record that is active (end date is not null and today is before end date) or (end date is null)
                                    if (isActualEndDateValid(actualEndDate, index))
                                    {
                                        editsMade.Add("Actual End Date");

                                        if (!String.IsNullOrEmpty(actualEndDate) && !String.IsNullOrWhiteSpace(actualEndDate) && actualEndDate != "&nbsp;")
                                            dt.Rows[index][(int)emp_records_columns.actual_end_date] = actualEndDate;
                                        else
                                            dt.Rows[index][(int)emp_records_columns.actual_end_date] = string.Empty;

                                        if (isRecordActive(actualEndDate))
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
                                    {
                                        isEditActualEndDateSuccessful = false;
                                        multipleActiveRecordsEditRecordPanel.Style.Add("display", "inline-block");
                                    }

                                }


                                if (!(isActualEndDateChanged && !isEditActualEndDateSuccessful && !(isPositionChanged || isDeptChanged || isEmpTypeChanged || isStartDateChanged || isExpectedEndDateChanged)))
                                {
                                    dt.Rows[index].SetField<string>((int)emp_records_columns.isChanged, "2");

                                    ViewState["Gridview1_dataSource"] = dt;
                                    bindGridview();

                                    // set Dictionary containing info including: id of record edited (KEY) and a list of the fields edited in the record (VALUE)
                                    Dictionary<string, HashSet<string>> recordEdits = new Dictionary < string, HashSet< string >> ();
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
                                else
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
            fullFormSubmitSuccessPanel.Style.Add("display", "none");

            // ERRORS
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

            // EMPLOYMENT RECORD

            // ADD
            invalidStartDateValidationMsgPanel.Style.Add("display", "none");
            invalidEndDateValidationMsgPanel.Style.Add("display", "none");
            dateComparisonValidationMsgPanel.Style.Add("display", "none");
            startDateIsWeekendPanel.Style.Add("display", "none");
            endDateIsWeekendPanel.Style.Add("display", "none");
            duplicateRecordPanel.Style.Add("display", "none");
            multipleActiveRecordsAddRecordPanel.Style.Add("display", "none");

            // EDIT
            recordEditEndDateInvalidPanel.Style.Add("display", "none");
            recordEditEndDateOnWeekend.Style.Add("display", "none");
            recordEditEndDateBeforeStartDate.Style.Add("display", "none");
            editEmploymentRecordSuccessful.Style.Add("display", "none");
            editUnsuccessful.Style.Add("display", "none");
            noEditsToRecordsMade.Style.Add("display", "none");
            multipleActiveRecordsEditRecordPanel.Style.Add("display", "none");

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
                4. Employment 

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
                                IIF(ep.actual_end_date IS NULL OR GETDATE() < ep.actual_end_date, 'Active', 'Inactive') as status, 
                                IIF(ep.actual_end_date IS NULL OR GETDATE() < ep.actual_end_date, 'label-success', 'label-danger') as status_class
                                
                            FROM [dbo].[employeeposition] ep

                            LEFT JOIN [dbo].[department] d
                            ON d.dept_id = ep.dept_id
    
                            LEFT JOIN [dbo].[position] p
                            ON p.pos_id = ep.position_id

                            WHERE employee_id = {empId}
                            ORDER BY start_date DESC;
                    ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                        dataTable = new DataTable();
                        sqlDataAdapter.Fill(dataTable);

                        ViewState["Gridview1_dataSource"] = dataTable;
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
                string empType = string.Empty;
                int index = 0;
                foreach(DataRow record in dataTable.Rows)
                {
                    if (isRecordActive(record.ItemArray[(int)emp_records_columns.actual_end_date].ToString()) || index == 0)
                    {
                        empType = record.ItemArray[(int)emp_records_columns.employment_type].ToString();
                        break;
                    }
                    index++;
                }

                if (!permissions.Contains("hr1_permissions"))
                {
                    // the HR 2, HR 3 must have permissions to view data for the same employment type as for the employee who submitted the application
                    //check if hr can view applications from the relevant employment type 
                    if (String.IsNullOrEmpty(empType) || (empType == "Contract" && !permissions.Contains("contract_permissions")) || (empType == "Public Service" && !permissions.Contains("public_officer_permissions")))
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
                            List<string> previousRoles = new List<string>();
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


                            ViewState["previousRoles"] = previousRoles;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //exception logic
                throw ex;
            }

            // get leave balances and populate textboxes
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    string sql = $@"
                        SELECT [vacation]
                                ,[personal]
                                ,[casual]
                                ,[sick]
                                ,[bereavement]
                                ,[maternity]
                                ,[pre_retirement],
                                [first_name] + ' ' + [last_name] as 'employee_name'
                            FROM [dbo].[employee]
                            WHERE employee_id = {empId};
                    ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                vacationLeaveInput.Text = reader["vacation"].ToString();
                                personalLeaveInput.Text = reader["personal"].ToString();
                                casualLeaveInput.Text = reader["casual"].ToString();
                                sickLeaveInput.Text = reader["sick"].ToString();
                                bereavementLeaveInput.Text = reader["bereavement"].ToString();
                                maternityLeaveInput.Text = reader["maternity"].ToString();
                                preRetirementLeaveInput.Text = reader["pre_retirement"].ToString();

                                empNameHeader.InnerText = reader["employee_name"].ToString();
                            }

                            // store initial leave balances
                            Dictionary<string, string> previousLeaveBalances = new Dictionary<string, string>();

                            previousLeaveBalances = new Dictionary<string, string>();
                            previousLeaveBalances["vacation"] = vacationLeaveInput.Text;
                            previousLeaveBalances["personal"] = personalLeaveInput.Text;
                            previousLeaveBalances["casual"] = casualLeaveInput.Text;
                            previousLeaveBalances["sick"] = sickLeaveInput.Text;
                            previousLeaveBalances["bereavement"] = bereavementLeaveInput.Text;
                            previousLeaveBalances["maternity"] = maternityLeaveInput.Text;
                            previousLeaveBalances["pre_retirement"] = preRetirementLeaveInput.Text;

                            ViewState["previousLeaveBalances"] = previousLeaveBalances;
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
             *      Since the previous roles held by the employee is stored in ViewState["previousRoles"], the current roles being submitted to edit are used to find out which roles were deleted. As such,
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
                Index:         0             1            2         3        4        5          6               7              8           9
                Columns : record_id, employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date, isChanged, actual_end_date

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
            DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;

            // used to check whether any value is changed 
            Boolean isRolesChanged, isLeaveBalancesChanged, isEmpRecordChanged;
            isRolesChanged = isLeaveBalancesChanged = isEmpRecordChanged = false;

            // used to check if there was any error in changing values
            Boolean isRolesEditSuccessful, isLeaveEditSuccessful, isEmpRecordEditSuccessful, isEmpRecordDeleteSuccessful, isEmpRecordInsertSuccessful, isEmpRecordEditFieldsSuccessful;
            isRolesEditSuccessful = isLeaveEditSuccessful = isEmpRecordEditSuccessful = isEmpRecordDeleteSuccessful = isEmpRecordInsertSuccessful = isEmpRecordEditFieldsSuccessful = true;

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
                List<string> previousRoles = new List<string>();
                if (ViewState["previousRoles"] != null)
                    previousRoles = (List < string > )ViewState["previousRoles"];

                foreach (string role in previousRoles)
                {
                    if (!authorizations.Contains(role))
                        deletedRoles.Add(role);
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
                foreach (string role in authorizations)
                {
                    if (!previousRoles.Contains(role))
                        addedRoles.Add(role);
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
            currentLeaveBalances["vacation"] = String.IsNullOrEmpty(vacationLeaveInput.Text) ? "0" : vacationLeaveInput.Text;
            currentLeaveBalances["personal"] = String.IsNullOrEmpty(personalLeaveInput.Text) ? "0" : personalLeaveInput.Text;
            currentLeaveBalances["casual"] = String.IsNullOrEmpty(casualLeaveInput.Text) ? "0" : casualLeaveInput.Text;
            currentLeaveBalances["sick"] = String.IsNullOrEmpty(sickLeaveInput.Text) ? "0" : sickLeaveInput.Text;
            currentLeaveBalances["bereavement"] = String.IsNullOrEmpty(bereavementLeaveInput.Text) ? "0" : bereavementLeaveInput.Text;
            currentLeaveBalances["maternity"] = String.IsNullOrEmpty(maternityLeaveInput.Text) ? "0" : maternityLeaveInput.Text;
            currentLeaveBalances["pre_retirement"] = String.IsNullOrEmpty(preRetirementLeaveInput.Text) ? "0" : preRetirementLeaveInput.Text;

            //checks which leave balances were edited and adds them to a new dictionary (editedLeaveBalances)
            // As such, editedLeaveBalances contains the dictionary of leave balances which were edited
            Dictionary<string, string> editedLeaveBalances = new Dictionary<string, string>();

            // used to store the initial leave balances loaded when in edit mode. These are used to determine the edits made for auditing purposes
            Dictionary<string, string> previousLeaveBalances = new Dictionary<string, string>();
            if (ViewState["previousLeaveBalances"] != null)
                previousLeaveBalances = (Dictionary<string, string>)ViewState["previousLeaveBalances"];

            foreach (KeyValuePair<string, string> entry in previousLeaveBalances)
            {
                if (currentLeaveBalances[entry.Key] != previousLeaveBalances[entry.Key])
                    editedLeaveBalances[entry.Key] = currentLeaveBalances[entry.Key];
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
                * Index:         0             1            2         3        4        5          6               7              8           9
                * Columns : record_id, employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date, isChanged, actual_end_date
            * */

            if (dt.Rows.Count > 0)
            {
                // a list to contain the ids of deleted employment records. This list is used to create audit logs for deletion of records
                List<string> deletedRecords = new List<string>();

                //delete the records with an 'isChanged' field of '1'
                foreach (DataRow dr in dt.Rows)
                {
                    // once the row does not represent a newly added row (which is not in DB) and the 'isChanged' field is '1' (representing a deleted row)
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
                                        ,[dept_id])
                                OUTPUT INSERTED.id
                                VALUES
                                    ( @EmployeeId
                                    ,@PositionId
                                    ,@StartDate
                                    ,@ExpectedEndDate
                                    ,@EmploymentType
                                    ,@DeptId
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

                                    string new_record_id = command.ExecuteScalar().ToString();
                                    isEmpRecordInsertSuccessful = !String.IsNullOrWhiteSpace(new_record_id);


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

                List<string> editedRecords = new List<string>();
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
                                    dept_id= {dr.ItemArray[(int)emp_records_columns.dept_id].ToString()}
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

                                    if (!String.IsNullOrEmpty(dr.ItemArray[(int)emp_records_columns.actual_end_date].ToString()))
                                        command.Parameters.AddWithValue("@ActualEndDate", DateTime.ParseExact(dr.ItemArray[(int)emp_records_columns.actual_end_date].ToString(), "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy"));
                                    else
                                        command.Parameters.AddWithValue("@ActualEndDate", DBNull.Value);

                                    command.Parameters.AddWithValue("@RecordId", dr.ItemArray[(int)emp_records_columns.record_id]);

                                    int rowsAffected = command.ExecuteNonQuery();
                                    isEmpRecordEditFieldsSuccessful = rowsAffected > 0;

                                    if (isEmpRecordEditFieldsSuccessful)
                                    {
                                        editedRecords.Add(dr.ItemArray[(int)emp_records_columns.record_id].ToString());
                                        if (Session["editedRecords"] != null)
                                        {
                                            Dictionary<string, HashSet<string>> editedRecordsDict = (Dictionary < string, HashSet< string >>) Session["editedRecords"];

                                            foreach(KeyValuePair<string, HashSet<string>> entry in editedRecordsDict)
                                            {
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
                if (editedRecords.Count > 0)
                {
                    /*
                    * Add info about edits made to employee such as:
                    * Employee ID
                    * ID of Employment Records edited and the end date added
                    * */
                    string action = $"Edited employment records; {String.Join("; ", editedEmpRecordsLog.ToArray())}";
                    util.addAuditLog(Session["emp_id"].ToString(), empId, action);
                }

                if (deletedRecords.Count > 0 || addedRecords.Count > 0 || editedRecords.Count > 0)
                    isEmpRecordChanged = true;
            }
            else
            {
                noEmploymentRecordEnteredErrorPanel.Style.Add("display", "inline-block");
                isEmpRecordInsertSuccessful = isEmpRecordDeleteSuccessful = false;
            }
            isEmpRecordEditSuccessful = isEmpRecordDeleteSuccessful && isEmpRecordInsertSuccessful && isEmpRecordEditFieldsSuccessful;

            // END EMPLOYMENT RECORDS--------------------------------------------------------------------------------------------------------------------------

            // reset page so it is ready for new edits
            populatePage(empId);
            hideEmploymentRecordForm();

            // USER FEEDBACK--------------------------------------------------------------------------------------------------------------------------
            if (!isRolesChanged && !isLeaveBalancesChanged && !isEmpRecordChanged)
            {
                noChangesMadePanel.Style.Add("display", "inline-block");
            }
            else
            {

                // SUCCESS MESSAGES---------------------------------------------------------------

                // general success message
                if ((isRolesChanged && isLeaveBalancesChanged && isEmpRecordChanged) && (isRolesEditSuccessful && isLeaveEditSuccessful && isEmpRecordEditSuccessful))
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

                    // ERROR MESSAGES------------------------------------------------------------------

                    // general error message if all aspects of edit fail
                    if (!isRolesEditSuccessful && !isLeaveEditSuccessful && !isEmpRecordEditSuccessful)
                        editEmpErrorPanel.Style.Add("display", "inline-block");

                    // roles edit error
                    if (!isRolesEditSuccessful)
                        editRolesErrorPanel.Style.Add("display", "inline-block");

                    // leave balances edit error
                    if (!isLeaveEditSuccessful)
                        editLeaveBalancesErrorPanel.Style.Add("display", "inline-block");

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
            //TODO: add more specific error messages. Follow edit function's example

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


            if (!String.IsNullOrEmpty(nameFromAd))
            {
                // set first and last name
                string[] name = nameFromAd.Split(' ');
                firstname = name[0];
                lastname = name[1];

                // set username
                username = $"PLANNING\\ {firstname} {lastname}";
            }

            // Leave Balances 
            Dictionary<string, string> currentLeaveBalances = new Dictionary<string, string>()
            {
                { "vacation", String.IsNullOrEmpty(vacationLeaveInput.Text) ? "0" : vacationLeaveInput.Text},
                { "personal", String.IsNullOrEmpty(personalLeaveInput.Text) ? "0" : personalLeaveInput.Text},
                { "casual", String.IsNullOrEmpty(casualLeaveInput.Text) ? "0" : casualLeaveInput.Text},
                { "sick", String.IsNullOrEmpty(sickLeaveInput.Text) ? "0" : sickLeaveInput.Text},
                { "bereavement", String.IsNullOrEmpty(bereavementLeaveInput.Text) ? "0" : bereavementLeaveInput.Text},
                { "maternity", String.IsNullOrEmpty(maternityLeaveInput.Text) ? "0" : maternityLeaveInput.Text},
                { "preRetirement", String.IsNullOrEmpty(preRetirementLeaveInput.Text) ? "0" : preRetirementLeaveInput.Text}
            };


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
            DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;

            Boolean isInsertSuccessful, isDuplicateIdentifier;
            isInsertSuccessful = isDuplicateIdentifier = false;

            // name from AD must be populated because that means the user exists in AD
            // dt cannot be null because that means that no employment record is added
            if (!String.IsNullOrEmpty(nameFromAd) && dt != null)
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
                              ,[vacation]
                              ,[personal]
                              ,[casual]
                              ,[sick]
                              ,[bereavement]
                              ,[maternity]
                              ,[pre_retirement])
                        VALUES
                           ( @EmployeeId
                            ,@IhrisId
                            ,@Username
                            ,@FirstName
                            ,@LastName
                            ,@Email
                            ,@Vacation
                            ,@Personal
                            ,@Casual
                            ,@Sick
                            ,@Bereavement
                            ,@Maternity
                            ,@PreRetirement
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
                            command.Parameters.AddWithValue("@Vacation", currentLeaveBalances["vacation"]);
                            command.Parameters.AddWithValue("@Personal", currentLeaveBalances["personal"]);
                            command.Parameters.AddWithValue("@Casual", currentLeaveBalances["casual"]);
                            command.Parameters.AddWithValue("@Sick", currentLeaveBalances["sick"]);
                            command.Parameters.AddWithValue("@Bereavement", currentLeaveBalances["bereavement"]);
                            command.Parameters.AddWithValue("@Maternity", currentLeaveBalances["maternity"]);
                            command.Parameters.AddWithValue("@PreRetirement", currentLeaveBalances["preRetirement"]);

                            int rowsAffected = command.ExecuteNonQuery();
                            isInsertSuccessful = rowsAffected > 0;

                        }
                    }
                }
                catch (Exception ex)
                {
                    // exception logic
                    if (ex.Message.ToString().Contains("Violation of PRIMARY KEY constraint") || ex.Message.ToString().Contains("Cannot insert duplicate key in object 'dbo.employee'"))
                        isDuplicateIdentifier = true;
                    isInsertSuccessful = false;
                }

                // Roles 
                if (isInsertSuccessful)
                {
                    isInsertSuccessful = false;
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
                                    isInsertSuccessful = rowsAffected > 0;

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
                    if (isInsertSuccessful)
                    {
                        isInsertSuccessful = false;
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
                                            ,[dept_id])
                                    VALUES
                                        ( @EmployeeId
                                        ,@PositionId
                                        ,@StartDate
                                        ,@ExpectedEndDate
                                        ,@EmploymentType
                                        ,@DeptId
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

                                            int rowsAffected = command.ExecuteNonQuery();
                                            if (rowsAffected > 0)
                                            {
                                                isInsertSuccessful = true;
                                                fullFormSubmitSuccessPanel.Style.Add("display", "inline-block");
                                            }
                                            else
                                                isInsertSuccessful = false;

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
            }
            else
            {
                fullFormErrorPanel.Style.Add("display", "inline-block");
                if (String.IsNullOrEmpty(nameFromAd))
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

        
        protected void refreshForm(object sender, EventArgs e)
        {
            string pathname = string.Empty;
            if (String.IsNullOrEmpty(Request.QueryString["empId"]))
                pathname = $"~/HR/EmployeeDetails?mode={ Request.QueryString["mode"]}";
            else
                pathname = $"~/HR/EmployeeDetails?mode={Request.QueryString["mode"]}&empId={Request.QueryString["empId"]}";
            Response.Redirect(pathname);
        }

        protected void returnToPreviousBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/HR/AllEmployees.aspx");
        } 
    }
}