using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;


namespace HR_LEAVEv2.Employee
{
    public partial class ApplyForLeave : System.Web.UI.Page
    {

        // this class is used to store the loaded data from the db in the process of populating fields on the page when accessing this page from 'view' mode
        protected class LeaveTransactionDetails
        {
            public string empId { get; set; }
            public string empName { get; set; }
            public string empType { get; set; }
            public string startDate { get; set; }
            public string endDate { get; set; }
            public string typeOfLeave { get; set; }
            public string supId { get; set; }
            public string supName { get; set; }
            public string status { get; set; }
            public string submittedOn { get; set; }
            public string empComment { get; set; }
            public string supComment { get; set; }
            public string hrComment { get; set; }
        };
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["permissions"] == null)
                    Response.Redirect("~/AccessDenied.aspx");

                if (Request.QueryString.HasKeys())
                {
                    string mode = Request.QueryString["mode"];
                    string leaveId = Request.QueryString["leaveId"];

                    // populates page and authorizes user based on their permissions, whether the leave application was submitted to them as a supervisor or various HR criteria
                    LeaveTransactionDetails ltDetails = populatePage(leaveId);
                    if (mode == "view")
                        this.adjustPageForViewMode();
                    else if (mode == "edit")
                        this.adjustPageForEditMode(ltDetails);
                } else
                {
                    // display normal leave application form
                    this.adjustPageForApplyMode();
                }        
            } else
            {
                //isPostback
                if (ViewState["supervisor_id"] != null && ViewState["supervisor_id"].ToString() != "-1")
                    supervisor_select.selectedSupId = ViewState["supervisor_id"].ToString();               
            }
        }

        protected LeaveTransactionDetails populatePage(string leaveId)
        {
            LeaveTransactionDetails ltDetails = null;

            // get info for relevant leave application based on passed leaveId and populate form controls
            try
            {
                /**
                 * This sql query gets various info necessary for repopulation of the fields as well as for the authorization process.
                 * for the authorization process: 
                 *      lt.employee_id: used to check whether the current employee trying to access the page is the same as the employee who made the application
                 *      lt.supervisor_id: used to check whether the supervisor trying to access the page is the supervisor who recommended the application
                 *      ep.employment_type: used to check whether the hr2 trying to access the page manages the relevant employment type of the employee who made the application (contract or public service)
                 *      
                 * for the repopulation:
                 *      all the other variables are used to repopulate some control on the page
                 * */
                string sql = $@"
                        SELECT 
                            lt.employee_id, 
                            emp.first_name + ' ' + emp.last_name as 'employee_name', 
                            ep.employment_type , 
                            FORMAT(lt.start_date, 'MM/dd/yyyy') start_date, 
                            FORMAT(lt.end_date, 'MM/dd/yyyy') end_date, 
                            lt.leave_type, 
                            lt.status, 
                            FORMAT(lt.created_at, 'MM/dd/yyyy HH:mm tt') as submitted_on,
                            lt.supervisor_id, 
                            e.first_name + ' ' + e.last_name as 'supervisor_name', 
                            lt.emp_comment, 
                            lt.sup_comment, 
                            lt.hr_comment
                        FROM [dbo].[leavetransaction] lt
                        JOIN [dbo].[employee] e
                        ON e.employee_id = lt.supervisor_id
                        LEFT JOIN [dbo].employeeposition ep
                        ON ep.employee_id = lt.employee_id
                        JOIN  [dbo].[employee] emp
                        ON emp.employee_id = lt.employee_id
                        WHERE lt.transaction_id = {leaveId};
                    ";

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ltDetails = new LeaveTransactionDetails
                                {
                                    empId = reader["employee_id"].ToString(),
                                    empName = reader["employee_name"].ToString(),
                                    empType = reader["employment_type"].ToString(),
                                    startDate = reader["start_date"].ToString(),
                                    endDate = reader["end_date"].ToString(),
                                    typeOfLeave = reader["leave_type"].ToString(),
                                    status = reader["status"].ToString(),
                                    submittedOn = reader["submitted_on"].ToString(),
                                    supId = reader["supervisor_id"].ToString(),
                                    supName = reader["supervisor_name"].ToString(),
                                    empComment = reader["emp_comment"].ToString(),
                                    supComment = reader["sup_comment"].ToString(),
                                    hrComment = reader["hr_comment"].ToString()
                                };

                            }

                            // check permissions of current user
                            List<string> permissions = (List<string>)Session["permissions"];

                            if (!permissions.Contains("hr1_permissions"))
                            {
                                // HR 2, HR 3, Supervisors, Employees and every combination


                                if(Session["emp_id"] == null || Session["emp_id"].ToString() != ltDetails.empId)
                                {
                                    // All combinations of HR 2, HR 3, Supervisor and Employee that did not submit the application. This means that only people with HR 2 privileges or 
                                    // the supervisor who the LA was submitted to can see it

                                    // HR 3
                                    // HR 3 should not have access to leave application data
                                    if (permissions.Contains("hr3_permissions"))
                                        Response.Redirect("~/AccessDenied.aspx");

                                    // HR 2
                                    if (permissions.Contains("hr2_permissions"))
                                    {
                                        if (!String.IsNullOrEmpty(ltDetails.empType))
                                        {
                                            // the HR 2 must have permissions to view data for the same employment type as for the employee who submitted the application
                                            if (
                                                (
                                                    //check if hr can view applications from the relevant employment type 
                                                    (ltDetails.empType == "Contract" && !permissions.Contains("contract_permissions"))
                                                    ||
                                                    (ltDetails.empType == "Public Service" && !permissions.Contains("public_officer_permissions"))
                                                )

                                                ||

                                                (
                                                    //check if hr can view applications of the relevant leave type
                                                    (ltDetails.typeOfLeave == "Sick" && !permissions.Contains("approve_sick"))
                                                    ||
                                                    (ltDetails.typeOfLeave == "Vacation" && !permissions.Contains("approve_vacation"))
                                                    ||
                                                    (ltDetails.typeOfLeave == "Casual" && !permissions.Contains("approve_casual"))
                                                )
                                               )
                                                Response.Redirect("~/AccessDenied.aspx");

                                        }
                                    }
                                    else
                                    {
                                        // if emp trying to view LA is supervisor 
                                        if (permissions.Contains("sup_permissions"))
                                        {
                                            //LA was not submitted to them
                                            if (Session["emp_id"] == null || Session["emp_id"].ToString() != ltDetails.supId)
                                                Response.Redirect("~/AccessDenied.aspx");
                                        } else
                                        {
                                            // if another emp trying to view LA and they did not submit the LA
                                            if (Session["emp_id"] == null || Session["emp_id"].ToString() != ltDetails.empId)
                                                Response.Redirect("~/AccessDenied.aspx");
                                        }
                                        
                                    }
                                    
                                } 

                            }

                            //populate form
                            empIdTxt.Text = ltDetails.empId;
                            empNameHeader.InnerText = ltDetails.empName;
                            txtFrom.Text = ltDetails.startDate;
                            txtTo.Text = ltDetails.endDate;
                            typeOfLeaveTxt.Text = ltDetails.typeOfLeave;
                            statusTxt.Text = ltDetails.status;
                            submittedOnTxt.Text = "Submitted on: " + ltDetails.submittedOn;
                            supervisorNameTxt.Text = ltDetails.supName;
                            empCommentsTxt.Value = ltDetails.empComment;
                            supCommentsTxt.Value = ltDetails.supComment;
                            hrCommentsTxt.Value = ltDetails.hrComment;

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //Return to previous
            returnToPreviousBtn.Visible = true;

            //Employee Name
            empNamePanel.Visible = true;

            //Submitted On
            submittedOnPanel.Visible = true;

            //Status
            statusPanel.Visible = true;
            statusTxt.Enabled = false;

            //File Upload
            fileUploadPanel.Visible = false;

            //Comments
            empCommentsPanel.Visible = true;
            supCommentsPanel.Visible = true;
            hrCommentsPanel.Visible = true;

            //Leave Count Panel
            leaveCountPanel.Visible = false;

            //Submit Button
            submitButtonPanel.Visible = false;


            // Start Date
            txtFrom.Enabled = false;
            fromCalendarExtender.Enabled = false;

            // End Date
            txtTo.Enabled = false;
            toCalendarExtender.Enabled = false;

            // Type of Leave
            typeOfLeaveDropdownPanel.Visible = false;
            typeOfLeavePanel.Visible = true;
            typeOfLeaveTxt.Enabled = false;

            //Supervisor Name
            supervisorSelectUserControlPanel.Visible = false;
            supervisorPanel.Visible = true;
            supervisorNameTxt.Enabled = false;

            //Comments
            empCommentsTxt.Disabled = true;
            supCommentsTxt.Disabled = true;
            hrCommentsTxt.Disabled = true;

            //comment edit
            submitCommentsPanel.Visible = false;

            return ltDetails;
        }

        protected void adjustPageForViewMode()
        {
            //Title
            viewModeTitle.Visible = true;
            editModeTitle.Visible = false;
            applyModeTitle.Visible = false;
        }

        protected void adjustPageForApplyMode()
        {
            //Return to previous
            returnToPreviousBtn.Visible = false;

            //Title
            viewModeTitle.Visible = false;
            editModeTitle.Visible = false;
            applyModeTitle.Visible = true;

            //Employee Name
            empNamePanel.Visible = false;

            //Submitted On
            submittedOnPanel.Visible = false;

            //Status
            statusPanel.Visible = false;

            //File upload
            fileUploadPanel.Visible = true;

            //Type of Leave
            typeOfLeavePanel.Visible = false;

            //Supervisor Name
            supervisorSelectUserControlPanel.Visible = true;
            supervisorPanel.Visible = false;

            //Comments
            empCommentsPanel.Visible = true;
            supCommentsPanel.Visible = false;
            hrCommentsPanel.Visible = false;

            //Submit Button
            submitButtonPanel.Visible = true;

            //Submit Comments 
            submitCommentsPanel.Visible = false;

        }

        protected void adjustPageForEditMode(LeaveTransactionDetails ltDetails)
        {
            //Title
            viewModeTitle.Visible = false;
            editModeTitle.Visible = true;
            applyModeTitle.Visible = false;

            //Comments

            //if supervisor on leave application, then they can leave a comment
            if(Session["emp_id"].ToString() == ltDetails.supId)
                supCommentsTxt.Disabled = false;

            List<string> permissions = (List<string>)Session["permissions"];

            if(permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions"))
                hrCommentsTxt.Disabled = false;

            submitCommentsPanel.Visible = true;

        }

        protected Boolean validateDates(string startDate, string endDate)
        {
            // validationMsgPanel.Style.Add("display", "none");
            dateComparisonValidationMsgPanel.Style.Add("display", "none");
            invalidStartDateValidationMsgPanel.Style.Add("display", "none");
            startDateBeforeTodayValidationMsgPanel.Style.Add("display", "none");
            invalidEndDateValidationMsgPanel.Style.Add("display", "none");
            invalidVacationStartDateMsgPanel.Style.Add("display", "none");
            invalidSickLeaveStartDate.Style.Add("display", "none");
            moreThan2DaysConsecutiveSickLeave.Style.Add("display", "none");

            DateTime start, end;
            start = end = DateTime.MinValue;
            Boolean isValidated = true;

            // validate start date is a date
            try
            {
                start = Convert.ToDateTime(startDate);
            }
            catch (FormatException fe)
            {
                invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }

            // validate end date is a date
            try
            {
                end = Convert.ToDateTime(endDate);
            }
            catch (FormatException fe)
            {
                invalidEndDateValidationMsgPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }

            if (isValidated)
            {
                //ensure start date is not a weekend
                if(start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday)
                {
                    startDateIsWeekend.Style.Add("display", "inline-block");
                    isValidated = false;
                }

                //ensure end date is not weekend
                if (end.DayOfWeek == DayOfWeek.Saturday || end.DayOfWeek == DayOfWeek.Sunday)
                {
                    endDateIsWeekend.Style.Add("display", "inline-block");
                    isValidated = false;
                }

                // ensure start date is not a day before today once not sick leave
                if (!typeOfLeave.SelectedValue.Equals("Sick") && DateTime.Compare(start, DateTime.Today) < 0)
                {
                    invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");
                    startDateBeforeTodayValidationMsgPanel.Style.Add("display", "inline-block");
                    isValidated = false;
                }

                // compare dates to ensure end date is not before start date
                if (DateTime.Compare(start, end) > 0)
                {
                    invalidEndDateValidationMsgPanel.Style.Add("display", "inline-block");
                    dateComparisonValidationMsgPanel.Style.Add("display", "inline-block");
                    isValidated = false;
                }

                // if leave type is vacation: ensure start date is at least one month from today
                if (typeOfLeave.SelectedValue.Equals("Vacation"))
                {
                    DateTime firstDateVacationCanBeTaken = DateTime.Today.AddMonths(1);

                    if(DateTime.Compare(start, firstDateVacationCanBeTaken) < 0)
                    {
                        invalidVacationStartDateMsgPanel.Style.Add("display", "inline-block");
                        isValidated = false;
                    }
                }

                // if type of leave is sick: ensure you can only apply for it retroactively
                if (typeOfLeave.SelectedValue.Equals("Sick"))
                {
                    if((end - start).Days + 1 > 2)
                    {
                        moreThan2DaysConsecutiveSickLeave.Style.Add("display", "inline-block");
                        isValidated = false;
                    }
                    
                    if (DateTime.Compare(end, DateTime.Today) > 0)
                    {
                        invalidSickLeaveStartDate.Style.Add("display", "inline-block");
                        isValidated = false;
                    }
                }   
            }

            return isValidated;
        }

        protected void submitLeaveApplication_Click(object sender, EventArgs e)
        {
            invalidSupervisor.Style.Add("display", "none");

            /* data to be submitted
             * 1. Employee id
             * 2. Leave type
             * 3. Start date
             * 4. End date
             * 5. Supervisor id
             * 6. Comments
             */

            string empId, leaveType, startDate, endDate, supId, comments;
            empId = Session["emp_id"].ToString();
            leaveType = typeOfLeave.SelectedValue;
            startDate = txtFrom.Text.ToString();
            endDate = txtTo.Text.ToString();
            supId = supervisor_select.selectedSupId;
            comments = empCommentsTxt.Value.Length > 0 ? empCommentsTxt.Value.ToString() : null;

            ViewState["supervisor_id"] = supId;

            // validate form values
            Boolean isValidated = validateDates(startDate, endDate);
            if (isValidated && supId != "-1")
            {
                try
                {
                    string sql = $@"
                        INSERT INTO [dbo].[leavetransaction]
                           ([employee_id]
                           ,[leave_type]
                           ,[start_date]
                           ,[end_date]
                           ,[supervisor_id]
                           ,[status]
                           ,[emp_comment])
                        VALUES
                           ( '{empId}'
                            ,'{leaveType}'
                            ,'{startDate}'
                            ,'{endDate}'
                            ,'{supId}'
                            ,'Pending'
                            ,@Comments
                            );
                    ";

                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            if (comments != null)
                                command.Parameters.AddWithValue("@Comments", comments);
                            else
                                command.Parameters.AddWithValue("@Comments", DBNull.Value);
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                submitButtonPanel.Style.Add("display", "none");
                                successMsgPanel.Style.Add("display", "inline-block");
                            }
                                
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            else if(supId == "-1")
            {
                invalidSupervisor.Style.Add("display", "inline-block");
            }
            
        }

        protected void refreshForm(object sender, EventArgs e)
        {
            Response.Redirect("~/Employee/ApplyForLeave.aspx");
        }

        protected void returnToPreviousBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.QueryString["returnUrl"]);
        }

        protected void submitCommentsBtn_Click(object sender, EventArgs e)
        {
            string leaveId = Request.QueryString["leaveId"];
            string supComment, hrComment;
            supComment = hrComment = string.Empty;
            if (supCommentsTxt.Disabled == false)
                supComment = supCommentsTxt.InnerText;

            if (hrCommentsTxt.Disabled == false)
                hrComment = hrCommentsTxt.InnerText;

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    string sql = $@"
                        UPDATE [dbo].leavetransaction 
                        SET sup_comment = @SupervisorComments, hr_comment = @HrComments
                        WHERE transaction_id = {leaveId};
                    ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if(supComment != string.Empty)
                            command.Parameters.AddWithValue("@SupervisorComments", supComment);
                        else
                            command.Parameters.AddWithValue("@SupervisorComments", DBNull.Value);

                        if(hrComment != string.Empty)
                            command.Parameters.AddWithValue("@HrComments", hrComment);
                        else
                            command.Parameters.AddWithValue("@HrComments", DBNull.Value);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            // show success message
                            successfulSubmitCommentsMsgPanel.Visible = true;

                            // hide submit button
                            submitCommentsBtn.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //exception logic
                throw ex;
            }

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
                        command.Parameters.AddWithValue("@AffectedEmployeeId", empIdTxt.Text);

                        // hr and supervisor comment
                        if (!String.IsNullOrEmpty(supComment) && !String.IsNullOrEmpty(hrComment))
                            command.Parameters.AddWithValue("@Action", $"Submitted supervisor and hr comment; leave_transaction_id= {leaveId}, supervisor_comment= {supComment}, hr_comment= {hrComment}");
                        else
                        {
                            // supervisor comment
                            if (!String.IsNullOrEmpty(supComment))
                                command.Parameters.AddWithValue("@Action", $"Submitted supervisor comment; leave_transaction_id= {leaveId}, supervisor_comment= {supComment}");

                            // hr comment
                            if (!String.IsNullOrEmpty(hrComment))    
                                command.Parameters.AddWithValue("@Action", $"Submitted HR comment; leave_transaction_id= {leaveId}, hr_comment= {hrComment}");
                        }
                        


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
    }
}