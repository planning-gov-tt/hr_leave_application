using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;


namespace HR_LEAVEv2.Employee
{
    public partial class ApplyForLeave : System.Web.UI.Page
    {

        private class LeaveTransactionDetails
        {
            public string empId { get; set; }
            public string empType { get; set; }
            public string startDate { get; set; }
            public string endDate { get; set; }
            public string typeOfLeave { get; set; }
            public string supId { get; set; }
            public string supName { get; set; }
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

                    if(mode == "view")
                        this.adjustPageForViewMode(leaveId);
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

        protected void adjustPageForViewMode(string leaveId)
        {

            LeaveTransactionDetails ltDetails = null;
            //string empId =null, startDate=null, endDate=null, typeOfLeave=null, supName=null, empComment=null, supComment=null, hrComment=null;
            // get info and populate form controls
            try
            {
                string sql = $@"
                        SELECT lt.employee_id, ep.employment_type , FORMAT(lt.start_date, 'MM/dd/yy') start_date, FORMAT(lt.end_date, 'MM/dd/yy') end_date,lt.leave_type,lt.supervisor_id, e.first_name + ' ' + e.last_name as 'supervisor_name', lt.emp_comment, lt.sup_comment, lt.hr_comment
                        FROM [dbo].[leavetransaction] lt
                        JOIN [dbo].[employee] e
                        ON e.employee_id = lt.supervisor_id
                        LEFT JOIN [dbo].employeeposition ep
                        ON ep.employee_id = lt.employee_id
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
                                    empType = reader["employment_type"].ToString(),
                                    startDate = reader["start_date"].ToString(),
                                    endDate = reader["end_date"].ToString(),
                                    typeOfLeave = reader["leave_type"].ToString(),
                                    supId = reader["supervisor_id"].ToString(),
                                    supName = reader["supervisor_name"].ToString(),
                                    empComment = reader["emp_comment"].ToString(),
                                    supComment = reader["sup_comment"].ToString(),
                                    hrComment = reader["hr_comment"].ToString()
                                };
                                
                            }

                            // check permissions of current user
                            List<string> permissions = (List<string>)Session["permissions"];

                            // Employee
                            if(permissions.Contains("emp_permissions") && !(permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions") || permissions.Contains("hr3_permissions")) && !permissions.Contains("sup_permissions"))
                            {
                                // employee can only view their own leave applications
                                if(Session["emp_id"] == null || Session["emp_id"].ToString() != ltDetails.empId)
                                    Response.Redirect("~/AccessDenied.aspx");

                            }
                            // Supervisor
                            if (permissions.Contains("sup_permissions") && !(permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions") || permissions.Contains("hr3_permissions")))
                            {
                                if (Session["emp_id"] == null || Session["emp_id"].ToString() != ltDetails.supId)
                                    Response.Redirect("~/AccessDenied.aspx");
                            }
                            // HR 1 can see everybody's data

                            // HR 2
                            if (permissions.Contains("hr2_permissions"))
                            {
                                if (!String.IsNullOrEmpty(ltDetails.empType))
                                {
                                    if (
                                        (ltDetails.empType=="Contract" && !permissions.Contains("contract_permissions")) 
                                        ||
                                        (ltDetails.empType == "Public Service" && !permissions.Contains("public_officer_permissions"))
                                       )
                                        Response.Redirect("~/AccessDenied.aspx");
                                }
                            }

                            // HR 3
                            if(permissions.Contains("hr3_permissions"))
                                Response.Redirect("~/AccessDenied.aspx");

                            //populate form
                            txtFrom.Text = ltDetails.startDate;
                            txtTo.Text = ltDetails.endDate;
                            typeOfLeaveTxt.Text = ltDetails.typeOfLeave;
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
                Console.WriteLine(ex.Message);
            }

            
            //Title
            viewModeTitle.Visible = true;
            editModeTitle.Visible = false;
            applyModeTitle.Visible = false;

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
        }

        protected void adjustPageForApplyMode()
        {
            //Title
            viewModeTitle.Visible = false;
            editModeTitle.Visible = false;
            applyModeTitle.Visible = true;

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
                    if (DateTime.Compare(end, DateTime.Today) > 0)
                    {
                        invalidSickLeaveStartDate.Style.Add("display", "inline-block");
                        isValidated = false;
                    }
                }


                return isValidated;
            }

            return false;
        }

        protected void submitLeaveApplication_Click(object sender, EventArgs e)
        {

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
            if (isValidated)
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
                    //validationMsg.InnerText = ex.Message;
                    //validationMsgPanel.Style.Add("display", "inline-block");
                    Response.Write(ex.Message.ToString());
                }

            }
        }

        protected void refreshForm(object sender, EventArgs e)
        {
            Response.Redirect("~/Employee/ApplyForLeave.aspx");
        }
    }
}