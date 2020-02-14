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

        // the following enum is used when getting or setting data in datatables 
        // as related to the employment records
        enum add_emp_records_columns
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
            actual_end_date = 9
        };

        public bool isEditMode { get; set; }

        // used to check if user can view page 
        List<string> permissions = null;

        // used to store the initial roles loaded when in edit mode. These are used to determine the edits made for auditing purposes
        List<string> previousRoles = null;

        // used to store the initial leave balances loaded when in edit mode. These are used to determine the edits made for auditing purposes
        Dictionary<string, string> previousLeaveBalances = null;

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
                if (ViewState["previousRoles"] != null)
                    this.previousRoles = (List<string>)ViewState["previousRoles"];
                if (ViewState["previousLeaveBalances"] != null)
                    this.previousLeaveBalances = (Dictionary<string, string>)ViewState["previousLeaveBalances"];
            }
                
        }

        protected void adjustPageForEditMode()
        {
            isEditMode = true;
            //title
            editModeTitle.Visible = true;
            createModeTitle.Visible = false;

            // clear form btn text
            clearFormTxt.InnerText = "Reset Form";

            //employee name
            empNamePanel.Visible = true;

            //basic info like id, ihris id, email
            empBasicInfoPanel.Visible = false;

            //authorizations
            //if hr 1 then authorizations can be edited

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

            //add emp record
            addEmpRecordForm.Visible = false;
            showFormBtn.Visible = true;

            //submit
            submitFullFormPanel.Visible = false;
            editFormPanel.Visible = true;
        }

        protected void adjustPageForCreateMode()
        {
            //title
            editModeTitle.Visible = false;
            createModeTitle.Visible = true;

            // clear form btn text
            clearFormTxt.InnerText = "Clear Form";

            //employee name
            empNamePanel.Visible = false;

            //basic info like id, ihris id, email
            empBasicInfoPanel.Visible = true;

            //authorization
            authorizationLevelPanel.Visible = true;

            //add emp record
            addEmpRecordForm.Visible = false;
            showFormBtn.Visible = true;

            //submit
            submitFullFormPanel.Visible = true;
            editFormPanel.Visible = false;

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

            //ensure start date is not a weekend
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
                }else
                {
                    // compare dates to ensure end date is not before start date
                    if (DateTime.Compare(start, end) > 0)
                    {
                        invalidEndDateValidationMsgPanel.Style.Add("display", "inline-block");
                        dateComparisonValidationMsgPanel.Style.Add("display", "inline-block");
                        isValidated = false;
                    }

                    //ensure end date is not weekend
                    if (end.DayOfWeek == DayOfWeek.Saturday || end.DayOfWeek == DayOfWeek.Sunday)
                    {
                        endDateIsWeekendPanel.Style.Add("display", "inline-block");
                        isValidated = false;
                    }
                }
            }

            return isValidated;
        }

        protected void showFormBtn_Click(object sender, EventArgs e)
        {
            addEmpRecordForm.Visible = true;
            showFormBtn.Visible = false;
        }

        protected void addNewRecordBtn_Click(object sender, EventArgs e)
        {
            /* data to be submitted
             * 1. position
             * 2. Start date
             * 3. End date
             * 4. employment type
             * 5. dept
             */
            clearErrors();
            string  position_id, position_name, startDate, endDate, emp_type, dept_id, dept_name;
            emp_type = empTypeList.SelectedValue;
            startDate = txtStartDate.Text.ToString();
            endDate = txtEndDate.Text.ToString();
            position_id = positionList.SelectedValue;
            position_name = positionList.SelectedItem.Text;
            dept_id = deptList.SelectedValue;
            dept_name = deptList.SelectedItem.Text;

            Boolean isValidated = validateDates(startDate, endDate);
            if (isValidated)
            {

                DataTable dt;
                if (ViewState["Gridview1_dataSource"] == null)
                {
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
                } else
                    dt = ViewState["Gridview1_dataSource"] as DataTable;

                // the following code is for the case where no end date is entered. If the employment record is for Contract then 3 years are added to the start and 
                // entered for the expected end date value
                DateTime expected_end_date = DateTime.MinValue;
                if (String.IsNullOrEmpty(endDate))
                {
                    if (emp_type == "Contract")
                    {
                        //expected_end_date = Convert.ToDateTime(startDate);
                        expected_end_date = DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        expected_end_date = expected_end_date.AddYears(3);
                    }
                }
                else
                    expected_end_date = DateTime.ParseExact(endDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

                bool isDuplicate = false;
                // check that row is not a duplicate row by comparing employment_type, dept_id, pos_id and start_date
                foreach(DataRow dr in dt.Rows)
                {
                    string dtEmpType = dr.ItemArray[(int)add_emp_records_columns.employment_type].ToString(),
                           dtDeptId = dr.ItemArray[(int)add_emp_records_columns.dept_id].ToString(),
                           dtPosId = dr.ItemArray[(int)add_emp_records_columns.pos_id].ToString(),
                           dtStartDate = ((DateTime)dr.ItemArray[(int)add_emp_records_columns.start_date]).ToString("d/MM/yyyy");

                    if (dtEmpType == emp_type && dtDeptId == dept_id && dtPosId == position_id && dtStartDate == startDate)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate)
                {
                    //record_id, employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date, isChanged
                    dt.Rows.Add(-1, emp_type, dept_id, dept_name, position_id, position_name, DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture), expected_end_date, "0", "");

                    ViewState["Gridview1_dataSource"] = dt;
                }    
                else
                    duplicateRecordPanel.Style.Add("display", "inline-block");

                this.bindGridview();

            }
            //this.resetAddEmploymentRecordFormFields();
        }

        protected void resetAddEmploymentRecordFormFields()
        {
            txtStartDate.Text = String.Empty;
            txtEndDate.Text = String.Empty;
            empTypeList.SelectedIndex = 0;
            deptList.SelectedIndex = 0;
            positionList.SelectedIndex = 0;
        }

        protected void cancelNewRecordBtn_Click(object sender, EventArgs e)
        {
            //reset fields
            this.resetAddEmploymentRecordFormFields();
            addEmpRecordForm.Visible = false;
            showFormBtn.Visible = true;
        }

        protected void bindGridview()
        {
            DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;
            if(dt != null)
            {
                DataTable frontEndDt = dt.Copy();
                // delete records that have isChanged = 1
                foreach (DataRow dr in frontEndDt.Select(""))
                {
                    if (dr["isChanged"].ToString() == "1")
                        dr.Delete();
                }
                DataView dataView = frontEndDt.AsDataView();
                dataView.Sort = "start_date DESC, actual_end_date ASC";
                frontEndDt = dataView.ToTable();

                ViewState["Gridview1_frontend_dataSource"] = frontEndDt;
                GridView1.DataSource = frontEndDt;
                GridView1.DataBind();
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            this.bindGridview();
        }

        protected int getIndexOfRelevantRowInBackendDataTable(DataRow rowToFind)
        {
            DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;
            if (dt != null)
            {
                int index = 0;

                // find index of row with same information in rowToFind
                foreach (DataRow dr in dt.Rows)
                {
                    bool isRowFound = true;
                    for (int i = 0; i < dr.ItemArray.Length; i += 1)
                    {
                        if (dr.ItemArray[i].ToString() != rowToFind.ItemArray[i].ToString())
                        {
                            isRowFound = false;
                            break;
                        }
                    }

                    // set isChanged
                    if (isRowFound)
                        return index;

                    index += 1;
                }
            }

            return -1;
        } 

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;
            if(dt != null)
            {
                // sets isChanged to 1, ie, deleted for relevant record in dt based on the info from frontEnd datatable
                DataTable frontEndDt = ViewState["Gridview1_frontend_dataSource"] as DataTable;
                DataRow rowToBeDeleted = frontEndDt.Rows[e.RowIndex];
                int indexOfRowInDt = getIndexOfRelevantRowInBackendDataTable(rowToBeDeleted);
                if(indexOfRowInDt != -1)
                {
                    dt.Rows[indexOfRowInDt].SetField<string>((int)add_emp_records_columns.isChanged, "1");
                    ViewState["Gridview1_dataSource"] = dt;
                }
                
            }
            this.bindGridview();
        }

        protected void submitBtn_Click(object sender, EventArgs e)
        {
            //TODO: add more specific error messages. Follow edit function's example

            this.clearErrors();

            // IDs, email
            string emp_id, ihris_id, email, firstname, lastname, username;

            emp_id = employeeIdInput.Text;
            ihris_id = ihrisNumInput.Text;
            email = adEmailInput.Text;

            // get first name, last name and username from active directory
            Auth auth = new Auth();
            string nameFromAd = auth.getUserInfoFromActiveDirectory(email);
            firstname = lastname = username = string.Empty;

            if (!String.IsNullOrEmpty(nameFromAd))
            {
                // set first and last name
                string[] name = nameFromAd.Split(' ');
                firstname = name[0];
                lastname = name[1];

                //set username
                username = $"PLANNING\\ {firstname} {lastname}";
            }

            // Leave Balances 
            Dictionary<string, string> currentLeaveBalances = new Dictionary<string, string>();

            // get data from leave 
            currentLeaveBalances["vacation"] = String.IsNullOrEmpty(vacationLeaveInput.Text) ? "0" : vacationLeaveInput.Text;
            currentLeaveBalances["personal"] = String.IsNullOrEmpty(personalLeaveInput.Text) ? "0" : personalLeaveInput.Text;
            currentLeaveBalances["casual"] = String.IsNullOrEmpty(casualLeaveInput.Text) ? "0" : casualLeaveInput.Text;
            currentLeaveBalances["sick"] = String.IsNullOrEmpty(sickLeaveInput.Text) ? "0" : sickLeaveInput.Text;
            currentLeaveBalances["bereavement"] = String.IsNullOrEmpty(bereavementLeaveInput.Text) ? "0" : bereavementLeaveInput.Text;
            currentLeaveBalances["maternity"] = String.IsNullOrEmpty(maternityLeaveInput.Text) ? "0" : maternityLeaveInput.Text;
            currentLeaveBalances["preRetirement"] = String.IsNullOrEmpty(preRetirementLeaveInput.Text) ? "0" : preRetirementLeaveInput.Text;

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
            if (!String.IsNullOrEmpty(nameFromAd) && dt !=null )
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
                            if(dr.ItemArray[(int)add_emp_records_columns.isChanged].ToString() == "0")
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
                                            command.Parameters.AddWithValue("@EmploymentType", dr.ItemArray[(int)add_emp_records_columns.employment_type]);
                                            command.Parameters.AddWithValue("@DeptId", dr.ItemArray[(int)add_emp_records_columns.dept_id]);
                                            command.Parameters.AddWithValue("@PositionId", dr.ItemArray[(int)add_emp_records_columns.pos_id]);
                                            command.Parameters.AddWithValue("@StartDate", dr.ItemArray[(int)add_emp_records_columns.start_date]);
                                            command.Parameters.AddWithValue("@ExpectedEndDate", dr.ItemArray[(int)add_emp_records_columns.expected_end_date]);

                                            int rowsAffected = command.ExecuteNonQuery();
                                            if (rowsAffected > 0)
                                            {
                                                isInsertSuccessful = true;
                                                fullFormSubmitSuccessPanel.Style.Add("display", "inline-block");
                                            } else
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
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = $@"
                                    INSERT INTO [dbo].[auditlog] ([acting_employee_id], [acting_employee_name], [affected_employee_id], [affected_employee_name], [action], [created_at])
                                    VALUES ( 
                                        @ActingEmployeeId, 
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @ActingEmployeeId), 
                                        @AffectedEmployeeId,
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @AffectedEmployeeId), 
                                        @Action, 
                                        @CreatedAt);
                                ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@ActingEmployeeId", Session["emp_id"].ToString());
                            command.Parameters.AddWithValue("@AffectedEmployeeId", emp_id);

                            /*
                             * Add info about new employee created such as:
                             * Leave balances: Vacation, Sick, Personal, Casual
                             * Employee permissions
                             * */
                            string action = $"Created new Employee; {String.Join(", ", currentLeaveBalances.Select(lb => lb.Key + "=" + lb.Value).ToArray())} ; Permissions: {String.Join(",", authorizations.ToArray())}";
                            command.Parameters.AddWithValue("@Action", action);

                            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM-dd-yyyy h:mm tt"));
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //exception logic
                    throw ex;
                }
            }
            else
            {
                fullFormErrorPanel.Style.Add("display", "inline-block");
                if (String.IsNullOrEmpty(nameFromAd))
                    emailNotFoundErrorPanel.Style.Add("display", "inline-block");
                if (dt == null)
                    noEmploymentRecordEnteredErrorPanel.Style.Add("display", "inline-block");
                if(isDuplicateIdentifier)
                    duplicateIdentifierPanel.Style.Add("display", "inline-block");
            }
                

            //scroll to top of page
            Page.MaintainScrollPositionOnPostBack = false;
            this.empDetailsContainer.Focus();
        }

        protected void refreshForm(object sender, EventArgs e)
        {
            string pathname = string.Empty;
            if (String.IsNullOrEmpty(Request.QueryString["empId"]))
                pathname = $"~/HR/EmployeeDetails?mode={ Request.QueryString["mode"]}";
            else
                pathname = $"~/HR/EmployeeDetails?mode={Request.QueryString["mode"]}&empId={Request.QueryString["empId"]}";
            Response.Redirect(pathname);
        }

        protected void clearErrors()
        {
            //SUCCESSES
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

            //EMPLOYMENT RECORD
            invalidStartDateValidationMsgPanel.Style.Add("display", "none");
            invalidEndDateValidationMsgPanel.Style.Add("display", "none");
            dateComparisonValidationMsgPanel.Style.Add("display", "none");
            startDateIsWeekendPanel.Style.Add("display", "none");
            endDateIsWeekendPanel.Style.Add("display", "none");
            duplicateRecordPanel.Style.Add("display", "none");

            // END EMPLOYMENT RECORD
            invalidEndDatePanel.Style.Add("display", "none");
            emptyEndDatePanel.Style.Add("display", "none");
            endDateBeforeStartDatePanel.Style.Add("display", "none");
            actualEndDateIsWeekendPanel.Style.Add("display", "none");
        }

        protected void populatePage(string empId)
        {
            // This function gets all employment record(s) and populates the employment record gridview

            /*  
             *  The following sql command executed initializes the isChanged field as 0 to give each field being populated in the gridview a status of 'not deleted'
            *   Employment records are loaded first in order to check the employment type such that the current HR viewing the page can be authorized
            */
            DataTable dataTable = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
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
                                FORMAT(ep.actual_end_date, 'd/MM/yyyy') actual_end_date
                                
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

            // the following code ensures that the current HR can view and edit the information presented based on the employee's employment type
            if(dataTable != null && dataTable.Rows.Count > 0)
            {
                // the first row in the dataTable is used because the data is ordered by start date such that the first record will show the most recent employment record. This
                // will therefore contain the most recent and accurate employment type: Contract or Public Service. Using this, the permissions are used for authorization
                string empType = dataTable.Rows[0].ItemArray[(int)add_emp_records_columns.employment_type].ToString();
                if (!permissions.Contains("hr1_permissions"))
                {
                    // the HR 2, HR 3 must have permissions to view data for the same employment type as for the employee who submitted the application
                    //check if hr can view applications from the relevant employment type 
                    if ((empType == "Contract" && !permissions.Contains("contract_permissions")) || (empType == "Public Service" && !permissions.Contains("public_officer_permissions")))
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
                            this.previousRoles = new List<string>();
                            if (supervisorCheck.Checked)
                                this.previousRoles.Add("sup");
                            if (hr1Check.Checked)
                                this.previousRoles.Add("hr1");
                            if (hr2Check.Checked)
                                this.previousRoles.Add("hr2");
                            if (hr3Check.Checked)
                                this.previousRoles.Add("hr3");
                            if (contractCheck.Checked)
                                this.previousRoles.Add("hr_contract");
                            if (publicServiceCheck.Checked)
                                this.previousRoles.Add("hr_public_officer");


                            ViewState["previousRoles"] = this.previousRoles;
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
                            this.previousLeaveBalances = new Dictionary<string, string>();
                            this.previousLeaveBalances["vacation"] = vacationLeaveInput.Text;
                            this.previousLeaveBalances["personal"] = personalLeaveInput.Text;
                            this.previousLeaveBalances["casual"] = casualLeaveInput.Text;
                            this.previousLeaveBalances["sick"] = sickLeaveInput.Text;
                            this.previousLeaveBalances["bereavement"] = bereavementLeaveInput.Text;
                            this.previousLeaveBalances["maternity"] = maternityLeaveInput.Text;
                            this.previousLeaveBalances["pre_retirement"] = preRetirementLeaveInput.Text;

                            ViewState["previousLeaveBalances"] = this.previousLeaveBalances;
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
             *      Since the previous roles held by the employee is stored in this.previousRoles, the current roles being submitted to edit are used to find out which roles were deleted. As such,
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
            Boolean isRolesEditSuccessful, isLeaveEditSuccessful, isEmpRecordEditSuccessful, isEmpRecordDeleteSuccessful, isEmpRecordInsertSuccessful, isEmpRecordEditEndDateSuccessful;
            isRolesEditSuccessful = isLeaveEditSuccessful = isEmpRecordEditSuccessful = isEmpRecordDeleteSuccessful = isEmpRecordInsertSuccessful = isEmpRecordEditEndDateSuccessful = true;

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
                foreach (string role in this.previousRoles)
                {
                    if (!authorizations.Contains(role))
                        deletedRoles.Add(role);
                }

                if (deletedRoles.Count > 0)
                {
                    //delete roles which were previously assigned to employee but which are now being removed
                    foreach(string roleToDelete in deletedRoles)
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
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            string sql = $@"
                                    INSERT INTO [dbo].[auditlog] ([acting_employee_id], [acting_employee_name], [affected_employee_id], [affected_employee_name], [action], [created_at])
                                    VALUES ( 
                                        @ActingEmployeeId, 
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @ActingEmployeeId), 
                                        @AffectedEmployeeId,
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @AffectedEmployeeId), 
                                        @Action, 
                                        @CreatedAt);
                                ";
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@ActingEmployeeId", Session["emp_id"].ToString());
                                command.Parameters.AddWithValue("@AffectedEmployeeId", empId);

                                /*
                                 * Add info about edits made to employee such as:
                                 * Employee ID
                                 * Roles deleted
                                 * */


                                string action = $"Edited roles; Roles Deleted= {String.Join(", ", deletedRoles.ToArray())}";
                                command.Parameters.AddWithValue("@Action", action);

                                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM-dd-yyyy h:mm tt"));
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception logic
                        throw ex;
                    }
                }

                // previous roles holds the employee's previous roles. The following code looks at the previous roles and checks which roles were added to the employee
                // As such, addedRoles contains the list of all roles added to the employee that were not previously there
                List<string> addedRoles = new List<string>();
                foreach (string role in authorizations)
                {
                    if (!this.previousRoles.Contains(role))
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
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            string sql = $@"
                                    INSERT INTO [dbo].[auditlog] ([acting_employee_id], [acting_employee_name], [affected_employee_id], [affected_employee_name], [action], [created_at])
                                    VALUES ( 
                                        @ActingEmployeeId, 
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @ActingEmployeeId), 
                                        @AffectedEmployeeId,
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @AffectedEmployeeId), 
                                        @Action, 
                                        @CreatedAt);
                                ";
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@ActingEmployeeId", Session["emp_id"].ToString());
                                command.Parameters.AddWithValue("@AffectedEmployeeId", empId);

                                /*
                                 * Add info about edits made to employee such as:
                                 * Employee ID
                                 * Roles added
                                 * */
                                string action = $"Edited roles; Roles Added= {String.Join(", ", addedRoles.ToArray())}";
                                command.Parameters.AddWithValue("@Action", action);

                                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM-dd-yyyy h:mm tt"));
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception logic
                        throw ex;
                    }
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
            foreach (KeyValuePair<string, string> entry in this.previousLeaveBalances)
            {
                if (currentLeaveBalances[entry.Key] != this.previousLeaveBalances[entry.Key])
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
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = $@"
                                    INSERT INTO [dbo].[auditlog] ([acting_employee_id], [acting_employee_name], [affected_employee_id], [affected_employee_name], [action], [created_at])
                                    VALUES ( 
                                        @ActingEmployeeId, 
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @ActingEmployeeId), 
                                        @AffectedEmployeeId,
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @AffectedEmployeeId), 
                                        @Action, 
                                        @CreatedAt);
                                ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@ActingEmployeeId", Session["emp_id"].ToString());
                            command.Parameters.AddWithValue("@AffectedEmployeeId", empId);

                            /*
                             * Add info about edits made to employee such as:
                             * Employee ID
                             * Leave Balance edited and the value which it was changed to
                             * */
                            string action = $"Edited leave balances; {String.Join(", ", editedLeaveBalances.Select(lb => lb.Key + "=" + lb.Value).ToArray())}";
                            command.Parameters.AddWithValue("@Action", action);

                            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM-dd-yyyy h:mm tt"));
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //exception logic
                    throw ex;
                }

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

            if (!this.isTableEmpty())
            {
                // a list to contain the ids of deleted employment records. This list is used to create audit logs for deletion of records
                List<string> deletedRecords = new List<string>();

                //delete the records with an 'isChanged' field of '1'
                foreach (DataRow dr in dt.Rows)
                {
                    // once the row does not represent a newly added row and the 'isChanged' field is '1' (representing a deleted row)
                    if (dr.ItemArray[(int)add_emp_records_columns.record_id].ToString() != "-1" && dr.ItemArray[(int)add_emp_records_columns.isChanged].ToString() == "1")
                    {
                        try
                        {
                            string sql = $@"
                                DELETE FROM [dbo].[employeeposition]
                                WHERE employee_id = '{empId}' AND id = '{dr.ItemArray[(int)add_emp_records_columns.record_id]}'
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
                            deletedRecords.Add(dr.ItemArray[(int)add_emp_records_columns.record_id].ToString());
                    }
                }

                // add audit log for deleted records
                if(deletedRecords.Count > 0)
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            string sql = $@"
                            INSERT INTO [dbo].[auditlog] ([acting_employee_id], [acting_employee_name], [affected_employee_id], [affected_employee_name], [action], [created_at])
                            VALUES ( 
                                @ActingEmployeeId, 
                                (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @ActingEmployeeId), 
                                @AffectedEmployeeId,
                                (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @AffectedEmployeeId), 
                                @Action, 
                                @CreatedAt);
                        ";
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@ActingEmployeeId", Session["emp_id"].ToString());
                                command.Parameters.AddWithValue("@AffectedEmployeeId", empId);

                                /*
                                    * Add info about edits made to employee such as:
                                    * Employee ID
                                    * ID of Employment Records deleted
                                    * */
                                string action = $"Deleted employment records; {String.Join(", ", deletedRecords.Select(lb =>  "id =" + lb).ToArray())}";
                                command.Parameters.AddWithValue("@Action", action);

                                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM-dd-yyyy h:mm tt"));
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception logic
                        throw ex;
                    }
                }

                // a list to contain the ids of added employment records. This list is used to create audit logs for addition of records
                List<string> addedRecords = new List<string>();

                //add new records data from gridview
                foreach (DataRow dr in dt.Rows)
                {

                    // only if the record id is '-1' and isChanged is '0' (representing a undeleted row) or isChanged is '2' (representing an edited row) then insert it
                    if (dr.ItemArray[(int)add_emp_records_columns.record_id].ToString() == "-1" && dr.ItemArray[(int)add_emp_records_columns.isChanged].ToString() != "-1")
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
                                    command.Parameters.AddWithValue("@EmploymentType", dr.ItemArray[(int)add_emp_records_columns.employment_type]);
                                    command.Parameters.AddWithValue("@DeptId", dr.ItemArray[(int)add_emp_records_columns.dept_id]);
                                    command.Parameters.AddWithValue("@PositionId", dr.ItemArray[(int)add_emp_records_columns.pos_id]);
                                    command.Parameters.AddWithValue("@StartDate", dr.ItemArray[(int)add_emp_records_columns.start_date]);
                                    command.Parameters.AddWithValue("@ExpectedEndDate", dr.ItemArray[(int)add_emp_records_columns.expected_end_date]);

                                    string new_record_id = command.ExecuteScalar().ToString();
                                    isEmpRecordInsertSuccessful = !String.IsNullOrWhiteSpace(new_record_id);

                                    
                                    if (isEmpRecordInsertSuccessful)
                                    {
                                        addedRecords.Add(new_record_id); // adding id of inserted record in added records list
                                        dr.SetField<string>((int)add_emp_records_columns.record_id, new_record_id); // add new id of inserted record into datatable
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
                if(addedRecords.Count > 0)
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            string sql = $@"
                            INSERT INTO [dbo].[auditlog] ([acting_employee_id], [acting_employee_name], [affected_employee_id], [affected_employee_name], [action], [created_at])
                            VALUES ( 
                                @ActingEmployeeId, 
                                (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @ActingEmployeeId), 
                                @AffectedEmployeeId,
                                (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @AffectedEmployeeId), 
                                @Action, 
                                @CreatedAt);
                        ";
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@ActingEmployeeId", Session["emp_id"].ToString());
                                command.Parameters.AddWithValue("@AffectedEmployeeId", empId);

                                /*
                                    * Add info about edits made to employee such as:
                                    * Employee ID
                                    * ID of Employment Records added
                                    * */
                                string action = $"Added employment records; {String.Join(", ", addedRecords.Select(lb => "id =" + lb).ToArray())}";
                                command.Parameters.AddWithValue("@Action", action);

                                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM-dd-yyyy h:mm tt"));
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception logic
                        throw ex;
                    }
                }

                List<string> editedRecords = new List<string>();
                //edit records data from gridview
                foreach (DataRow dr in dt.Rows)
                {

                    // only if the record id is not '-1' and isChanged is '2' (representing a edited row) then update it
                    if (dr.ItemArray[(int)add_emp_records_columns.record_id].ToString() != "-1" && dr.ItemArray[(int)add_emp_records_columns.isChanged].ToString() == "2")
                    {
                        // edit record
                        try
                        {
                            string sql = $@"
                                UPDATE [dbo].[employeeposition]
                                SET actual_end_date = @ActualEndDate
                                WHERE id= @RecordId;
                            ";

                            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                            {
                                connection.Open();
                                using (SqlCommand command = new SqlCommand(sql, connection))
                                {
                                    command.Parameters.AddWithValue("@ActualEndDate", DateTime.ParseExact(dr.ItemArray[(int)add_emp_records_columns.actual_end_date].ToString(), "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy"));
                                    command.Parameters.AddWithValue("@RecordId", dr.ItemArray[(int)add_emp_records_columns.record_id]);

                                    int rowsAffected = command.ExecuteNonQuery();
                                    isEmpRecordEditEndDateSuccessful = rowsAffected > 0;

                                    if (isEmpRecordEditEndDateSuccessful)
                                        editedRecords.Add($"id= {dr.ItemArray[(int)add_emp_records_columns.record_id].ToString()}, actual_end_date= '{dr.ItemArray[(int)add_emp_records_columns.actual_end_date].ToString()}'");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // exception logic
                            isEmpRecordEditEndDateSuccessful = false;
                        }
                    }
                }

                // add audit log for edited records
                if (editedRecords.Count > 0)
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            string sql = $@"
                            INSERT INTO [dbo].[auditlog] ([acting_employee_id], [acting_employee_name], [affected_employee_id], [affected_employee_name], [action], [created_at])
                            VALUES ( 
                                @ActingEmployeeId, 
                                (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @ActingEmployeeId), 
                                @AffectedEmployeeId,
                                (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @AffectedEmployeeId), 
                                @Action, 
                                @CreatedAt);
                        ";
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@ActingEmployeeId", Session["emp_id"].ToString());
                                command.Parameters.AddWithValue("@AffectedEmployeeId", empId);

                                /*
                                    * Add info about edits made to employee such as:
                                    * Employee ID
                                    * ID of Employment Records edited and the end date added
                                    * */
                                string action = $"Edited employment records; {String.Join("; ", editedRecords.ToArray())}";
                                command.Parameters.AddWithValue("@Action", action);

                                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM-dd-yyyy h:mm tt"));
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception logic
                        throw ex;
                    }
                }

                if (deletedRecords.Count > 0 || addedRecords.Count > 0 || editedRecords.Count > 0)
                    isEmpRecordChanged = true;
            } else
            {
                noEmploymentRecordEnteredErrorPanel.Style.Add("display", "inline-block");
                isEmpRecordInsertSuccessful = isEmpRecordDeleteSuccessful = false;
            }                      
            isEmpRecordEditSuccessful = isEmpRecordDeleteSuccessful && isEmpRecordInsertSuccessful && isEmpRecordEditEndDateSuccessful;

            // END EMPLOYMENT RECORDS--------------------------------------------------------------------------------------------------------------------------

            // reset page so it is ready for new edits
            populatePage(empId);

            // USER FEEDBACK--------------------------------------------------------------------------------------------------------------------------
            if(!isRolesChanged && !isLeaveBalancesChanged && !isEmpRecordChanged)
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
                        if (!isEmpRecordInsertSuccessful && !isEmpRecordDeleteSuccessful && !isEmpRecordEditEndDateSuccessful)
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
                            if (!isEmpRecordEditEndDateSuccessful)
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

        protected void returnToPreviousBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/HR/AllEmployees.aspx");
        }

        protected Boolean isTableEmpty()
        {
            if(GridView1.Rows.Count > 0)
            {
                foreach (TableRow row in GridView1.Rows)
                {
                    if (!row.CssClass.Contains("hidden"))
                        return false;
                }
            }
            return true;
            
        }

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

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //this function executes after data is bound for each row

            int i;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

                // highlight new records
                i = GetColumnIndexByName(e.Row, "record_id");
                if (e.Row.Cells[i].Text.ToString() == "-1" && this.isEditMode) // row is a new record if record_id = -1
                    e.Row.CssClass = "new-record";

                //hide end employment record button if actual_end_date is already populated
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
            // it checks to see if all rows are hidden which means to the user that all records are deleted
            // if the above condition is true then it hides the header row of the gridview
            if(GridView1.HeaderRow != null)
                GridView1.HeaderRow.Visible = !isTableEmpty();

        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if(e.CommandName == "endEmpRecord")
            {
                // reset form
                clearErrors();
                txtEmpRecordEndDate.Text = string.Empty;

                // store data about row which must be edited
                int index = Convert.ToInt32(e.CommandArgument);
                Session["startDate"] = GridView1.Rows[index].Cells[GetColumnIndexByName(GridView1.Rows[index],"start_date")].Text.ToString();
                Session["empRecordRowIndex"] = index;

                // show modal
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#cancelEmpRecordModal').modal('show');", true);
            }
                
        }

        protected void submitEndEmpRecordBtn_Click(object sender, EventArgs e)
        {
            clearErrors();
            Boolean isValidated = true;
            DateTime end = DateTime.MinValue;

            // validate dates
            if (!String.IsNullOrEmpty(txtEmpRecordEndDate.Text))
            {

                // validate end date is a date
                if (!DateTime.TryParseExact(txtEmpRecordEndDate.Text, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out end))
                {
                    invalidEndDatePanel.Style.Add("display", "inline-block");
                    isValidated = false;
                }else {
                    // end date is valid

                    // ensure end date is not weekend
                    if (end.DayOfWeek == DayOfWeek.Saturday || end.DayOfWeek == DayOfWeek.Sunday)
                    {
                        actualEndDateIsWeekendPanel.Style.Add("display", "inline-block");
                        isValidated = false;
                    }

                    // compare dates to ensure end date is not before start date
                    string startDate = string.Empty;
                    if (Session["startDate"] != null)
                        startDate = Session["startDate"].ToString();
                    if (!String.IsNullOrEmpty(startDate))
                    {
                        if (DateTime.Compare(DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture), end) > 0)
                        {
                            invalidEndDatePanel.Style.Add("display", "inline-block");
                            endDateBeforeStartDatePanel.Style.Add("display", "inline-block");
                            isValidated = false;
                        }
                    } else
                        isValidated = false;
                }
            }
            else
            {
                emptyEndDatePanel.Style.Add("display", "inline-block");
                isValidated = false;
            }
                

            if(isValidated && Session["empRecordRowIndex"] != null)
            {
                // edit datatable and rebind gridview

                /** Since there exists a frontend datatable which contains only records which are not deleted, there may be a discrepancy between the indexes in the frontend datatable
                 *  and the backend datatable. As such the following code is necessary in order to find the suitable index in the backend datatable such as to edit the actual_end_date value
                 * 
                 * */
                int indexInFrontEndDataTable = Convert.ToInt32(Session["empRecordRowIndex"]);
                DataTable frontEndDt = ViewState["Gridview1_frontend_dataSource"] as DataTable;
                DataRow rowToFindInBackEnd = frontEndDt.Rows[indexInFrontEndDataTable];


                if (ViewState["Gridview1_dataSource"] != null)
                {
                    DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;

                    int indexInDt = getIndexOfRelevantRowInBackendDataTable(rowToFindInBackEnd);
                    dt.Rows[indexInDt].SetField<string>((int)add_emp_records_columns.isChanged, "2");
                    dt.Rows[indexInDt].SetField<string>((int)add_emp_records_columns.actual_end_date, end.ToString("d/MM/yyyy"));
                    ViewState["Gridview1_dataSource"] = dt;
                    this.bindGridview();

                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#cancelEmpRecordModal').modal('hide');", true);
                }

                Session["startDate"] = null;
                Session["empRecordRowIndex"] = null;
            }

            

            
        }
    }
}