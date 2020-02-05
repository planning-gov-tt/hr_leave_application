using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI.HtmlControls;

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
            public string daysTaken { get; set; }
            public string qualified { get; set; }
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
            // get mode: apply, edit, view
            string mode = Request.QueryString.HasKeys() ? Request.QueryString["mode"] : "apply";

            if (!IsPostBack)
            {
                if (Session["permissions"] == null)
                    Response.Redirect("~/AccessDenied.aspx");

                Session["uploadedFiles"] = null;
                Session["supervisor_id"] = null;
                filesUploadedPanel.Visible = false;


                if (mode != "apply")
                {
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
            }
            else
            {
                if(mode == "apply")
                {
                    // validate dates
                    validateDates(txtFrom.Text, txtTo.Text);
                }
                    
            }
        }

        protected void clearDateErrors()
        {
            // date errors
            dateComparisonValidationMsgPanel.Style.Add("display", "none");
            invalidStartDateValidationMsgPanel.Style.Add("display", "none");
            startDateBeforeTodayValidationMsgPanel.Style.Add("display", "none");
            invalidEndDateValidationMsgPanel.Style.Add("display", "none");
            invalidVacationStartDateMsgPanel.Style.Add("display", "none");
            invalidSickLeaveStartDate.Style.Add("display", "none");
            moreThan2DaysConsecutiveSickLeave.Style.Add("display", "none");
            startDateIsWeekend.Style.Add("display", "none");
            endDateIsWeekend.Style.Add("display", "none");
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
                            FORMAT(lt.start_date, 'd/MM/yyyy') start_date, 
                            FORMAT(lt.end_date, 'd/MM/yyyy') end_date, 
                            lt.qualified,
                            lt.days_taken,
                            lt.leave_type, 
                            lt.status, 
                            FORMAT(lt.created_at, 'd/MM/yyyy h:mm tt') as submitted_on,
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
                                    qualified = reader["qualified"].ToString(),
                                    daysTaken = reader["days_taken"].ToString(),
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
                            startDateInfoTxt.Text = ltDetails.startDate;
                            endDateInfoTxt.Text = ltDetails.endDate;
                            numDaysAppliedFor.Text = ltDetails.daysTaken;

                            // store previous num days applied for in viewstate
                            ViewState["daysTaken"] = numDaysAppliedForEditTxt.Text = ltDetails.daysTaken;

                            typeOfLeaveTxt.Text = ltDetails.typeOfLeave;
                            statusTxt.Text = ltDetails.status;
                            qualifiedTxt.Text = ltDetails.qualified;
                            submittedOnTxt.Text = "Submitted on: " + ltDetails.submittedOn;
                            supervisorNameTxt.Text = ltDetails.supName;
                            empCommentsTxt.Value = ltDetails.empComment;

                            // store previous supervisor comment in viewstate
                            ViewState["supComment"] = supCommentsTxt.Value =  ltDetails.supComment;

                            // store previous HR comment in viewstate
                            ViewState["hrComment"] = hrCommentsTxt.Value = ltDetails.hrComment;

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
            startDateApplyPanel.Visible = false;
            startDateInfoPanel.Visible = true;

            // End Date
            endDateApplyPanel.Visible = false;
            endDateInfoPanel.Visible = true;

            // Type of Leave
            typeOfLeaveDropdownPanel.Visible = false;
            typeOfLeavePanel.Visible = true;

            //Supervisor Name
            supervisorSelectUserControlPanel.Visible = false;
            supervisorPanel.Visible = true;

            //Comments
            empCommentsTxt.Disabled = true;
            supCommentsTxt.Disabled = true;
            hrCommentsTxt.Disabled = true;

            //Edits
            submitEditsPanel.Visible = false;

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

            qualifiedPanel.Visible = false;

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

            //Submit Edits 
            submitEditsPanel.Visible = false;

        }

        protected void adjustPageForEditMode(LeaveTransactionDetails ltDetails)
        {
            //Title
            viewModeTitle.Visible = false;
            editModeTitle.Visible = true;
            applyModeTitle.Visible = false;

            //if supervisor on leave application, then they can leave a comment
            if(Session["emp_id"].ToString() == ltDetails.supId)
                supCommentsTxt.Disabled = false;

            List<string> permissions = (List<string>)Session["permissions"];

            if (permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions"))
            {
                hrCommentsTxt.Disabled = false;
                numDaysAppliedForEditTxt.Visible = true;
                numDaysAppliedFor.Visible = false;
            }
            else
                numDaysAppliedFor.Visible = true;
                

            submitEditsPanel.Visible = true;

        }

        protected Boolean validateDates(string startDate, string endDate)
        {
            clearDateErrors();
            DateTime start, end;
            start = end = DateTime.MinValue;
            Boolean isValidated = true;

            // validate start date is a date
            if(!DateTime.TryParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start))
            {
                invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }

            if (!DateTime.TryParseExact(endDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out end))
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

                // compare dates to ensure end date is not before start date
                if (DateTime.Compare(start, end) > 0)
                {
                    invalidEndDateValidationMsgPanel.Style.Add("display", "inline-block");
                    dateComparisonValidationMsgPanel.Style.Add("display", "inline-block");
                    isValidated = false;
                }

                if(!String.IsNullOrEmpty(typeOfLeave.SelectedValue))
                {
                    // ensure start date is not a day before today once not sick leave
                    if (!typeOfLeave.SelectedValue.Equals("Sick") && DateTime.Compare(start, DateTime.Today) < 0)
                    {
                        invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");
                        startDateBeforeTodayValidationMsgPanel.Style.Add("display", "inline-block");
                        isValidated = false;
                    }

                    // if leave type is vacation: ensure start date is at least one month from today
                    if (typeOfLeave.SelectedValue.Equals("Vacation"))
                    {
                        DateTime firstDateVacationCanBeTaken = DateTime.Today.AddMonths(1);

                        if (DateTime.Compare(start, firstDateVacationCanBeTaken) < 0)
                        {
                            invalidVacationStartDateMsgPanel.Style.Add("display", "inline-block");
                            isValidated = false;
                        }
                    }

                    // if type of leave is sick: ensure you can only apply for it retroactively
                    if (typeOfLeave.SelectedValue.Equals("Sick"))
                    {

                        if ((end - start).Days + 1 > 2)
                        {
                            List<HttpPostedFile> files = null;
                            if (Session["uploadedFiles"] != null)
                            {
                                files = (List<HttpPostedFile>)Session["uploadedFiles"];
                                if (files.Count == 0)
                                {
                                    moreThan2DaysConsecutiveSickLeave.Style.Add("display", "inline-block");
                                    isValidated = false;
                                }
                            }
                            else
                            {
                                moreThan2DaysConsecutiveSickLeave.Style.Add("display", "inline-block");
                                isValidated = false;
                            }
                        }

                        if (DateTime.Compare(end, DateTime.Today) > 0)
                        {
                            invalidSickLeaveStartDate.Style.Add("display", "inline-block");
                            isValidated = false;
                        }
                    }
                }
            }

            return isValidated;
        }

        protected void submitLeaveApplication_Click(object sender, EventArgs e)
        {
            // submit leave application errors
            invalidSupervisor.Style.Add("display", "none");
            errorInsertingFilesToDbPanel.Style.Add("display", "none");
            errorSubmittingLeaveApplicationPanel.Style.Add("display", "none");

            /* data to be submitted
             * 1. Employee id
             * 2. Leave type
             * 3. Start date
             * 4. End date
             * 5. Supervisor id
             * 6. Uploaded files
             * 7. Comments
             */

            string empId, leaveType, startDate, endDate, supId = "-1", comments;

            // employee id
            empId = Session["emp_id"].ToString();

            // type of leave
            leaveType = typeOfLeave.SelectedValue;

            // start and end date
            startDate = txtFrom.Text.ToString();
            endDate = txtTo.Text.ToString();

            // supervisor id
            if (Session["supervisor_id"] != null)
                supId = Session["supervisor_id"].ToString();

            // uploaded files
            List<HttpPostedFile> files = null;

            if (Session["uploadedFiles"] != null)
                files = (List<HttpPostedFile>)Session["uploadedFiles"];

            // employee comments
            comments = empCommentsTxt.Value.Length > 0 ? empCommentsTxt.Value.ToString() : null;

            // validate form values
            Boolean isValidated, isLeaveApplicationInsertSuccessful, isFileUploadSuccessful, areFilesUploaded;
            isLeaveApplicationInsertSuccessful = isFileUploadSuccessful = true;
            areFilesUploaded = false;
        
            isValidated = validateDates(startDate, endDate);
            if (isValidated && supId != "-1")
            {
                string transaction_id = string.Empty;

                // used to store ids of inserted files in order to add to audit log
                List<string> uploadedFilesIds = new List<string>();

                try
                {
                    string sql = $@"
                        INSERT INTO [dbo].[leavetransaction]
                           ([employee_id]
                           ,[leave_type]
                           ,[start_date]
                           ,[end_date]
                           ,[qualified]
                           ,[days_taken]
                           ,[supervisor_id]
                           ,[status]
                           ,[emp_comment])
                        OUTPUT INSERTED.transaction_id
                        VALUES
                           ( '{empId}'
                            ,'{leaveType}'
                            ,'{DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy")}'
                            ,'{DateTime.ParseExact(endDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy")}'
                            , (SELECT IIF((
				                ( '{leaveType}'= 'Sick' AND {numDaysAppliedFor.Text} < e.sick) OR
				                ('{leaveType}' = 'Casual' AND {numDaysAppliedFor.Text} < e.casual) OR
				                ('{leaveType}' = 'Vacation' AND {numDaysAppliedFor.Text} < e.vacation) OR
				                ('{leaveType}'= 'Personal' AND {numDaysAppliedFor.Text} < e.personal) OR
				                ('{leaveType}' = 'Bereavement' AND {numDaysAppliedFor.Text} < e.bereavement) OR
				                ('{leaveType}' = 'Maternity' AND {numDaysAppliedFor.Text} < e.maternity) OR
				                ('{leaveType}' = 'Pre-retirement' AND {numDaysAppliedFor.Text} < e.pre_retirement)
				                ), 'Yes', 'No')
                              FROM [dbo].[employee] e
                              WHERE e.employee_id = {empId})
                            , {numDaysAppliedFor.Text}
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
                            transaction_id = command.ExecuteScalar().ToString();
                            isLeaveApplicationInsertSuccessful = !String.IsNullOrEmpty(transaction_id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    isLeaveApplicationInsertSuccessful = false;
                }

                if (isLeaveApplicationInsertSuccessful && files != null)
                {
                    areFilesUploaded = files.Count > 0;
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
                                //fs = File.Open(uploadedFile.FileName, FileMode.Open);
                                using (BinaryReader br = new BinaryReader(fs))
                                {
                                    byte[] bytes = br.ReadBytes((Int32)fs.Length);
                                    string sql = $@"
                                    INSERT INTO [dbo].[filestorage]
                                        (
                                        [file_data]
                                        ,[file_name]
                                        ,[file_extension]
                                        ,[uploaded_on])
                                    OUTPUT INSERTED.file_id
                                    VALUES
                                        ( @FileData
                                        ,@FileName
                                        ,@FileExtension
                                        ,@UploadedOn
                                        );";
                                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                                    {
                                        connection.Open();
                                        string fileid = string.Empty;
                                        using (SqlCommand command = new SqlCommand(sql, connection))
                                        {
                                            command.Parameters.AddWithValue("@FileData", bytes);
                                            command.Parameters.AddWithValue("@FileName", file_name);
                                            command.Parameters.AddWithValue("@FileExtension", file_extension);
                                            command.Parameters.AddWithValue("@UploadedOn", DateTime.Now);
                                            fileid = command.ExecuteScalar().ToString();

                                            isFileUploadSuccessful = !String.IsNullOrEmpty(fileid);
                                        }

                                        if (isFileUploadSuccessful)
                                        {
                                            // insert record into bridge entity which associates file(s) with a given employee
                                            sql = $@"
                                                INSERT INTO [dbo].[employeefiles] ([file_id],[employee_id],[leave_transaction_id])
                                                VALUES(@FileId, @EmployeeId, @TransactionId);";
                                            using (SqlCommand command = new SqlCommand(sql, connection))
                                            {
                                                command.Parameters.AddWithValue("@FileId", fileid);
                                                command.Parameters.AddWithValue("@EmployeeId", Session["emp_id"].ToString());
                                                command.Parameters.AddWithValue("@TransactionId", transaction_id);
                                                int rowsAffected = command.ExecuteNonQuery();
                                                isFileUploadSuccessful = rowsAffected > 0;
                                            }

                                            // add file id to add to audit log
                                            uploadedFilesIds.Add(fileid);
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
                }
               

                if (isFileUploadSuccessful)
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
                                command.Parameters.AddWithValue("@AffectedEmployeeId", Session["emp_id"].ToString());

                                string fileActionString = String.Empty;
                                if (areFilesUploaded)
                                    fileActionString = $"Files uploaded: {String.Join(", ", uploadedFilesIds.Select(lb => "id= " + lb).ToArray())}";

                                command.Parameters.AddWithValue("@Action", $"Submitted leave application: leave_transaction_id= {transaction_id};{fileActionString}");
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

                    submitButtonPanel.Style.Add("display", "none");
                    successMsgPanel.Style.Add("display", "inline-block");
                }
            }

            // ERROR FEEDBACK
            if (!isFileUploadSuccessful)
                errorInsertingFilesToDbPanel.Style.Add("display", "inline-block");
            if(!isLeaveApplicationInsertSuccessful)
                errorSubmittingLeaveApplicationPanel.Style.Add("display", "inline-block");
            if (supId == "-1")
                invalidSupervisor.Style.Add("display", "inline-block");
            
        }

        protected void refreshForm(object sender, EventArgs e)
        {
            Response.Redirect("~/Employee/ApplyForLeave.aspx");
        }

        protected void returnToPreviousBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.QueryString["returnUrl"]);
        }

        protected void datesEntered(object sender, EventArgs e)
        {
            DateTime start, end;
            start = end = DateTime.MinValue;
            Boolean isStartDateFilled, isEndDateFilled;
            isStartDateFilled = DateTime.TryParseExact(txtFrom.Text, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start);
            isEndDateFilled = DateTime.TryParseExact(txtTo.Text, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out end);

            if ((typeOfLeave.SelectedIndex != 0 && validateDates(txtFrom.Text, txtTo.Text)) || (isStartDateFilled && isEndDateFilled))
                numDaysAppliedFor.Text = ((end - start).Days + 1) > 0 ? ((end - start).Days + 1).ToString() : "0";
                
        }

        protected void typeOfLeave_SelectedIndexChanged(object sender, EventArgs e)
        {
            validateDates(txtFrom.Text, txtTo.Text);
        }

        protected void uploadBtn_Click(object sender, EventArgs e)
        {
            invalidFileTypePanel.Style.Add("display", "none");
            fileUploadedTooLargePanel.Style.Add("display", "none");

            if (FileUpload1.HasFiles)
            {
                List<string> filesTooLarge = new List<string>();

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
                    if (uploadedFile.ContentLength < maxRequestLength)
                    {
                        if (allowedFileExtensions.Contains(Path.GetExtension(uploadedFile.FileName).ToString()))
                        {
                            dt.Rows.Add(Path.GetFileName(uploadedFile.FileName));
                            files.Add(uploadedFile);
                        }
                        else
                        {
                            HtmlGenericControl txt = (HtmlGenericControl)invalidFileTypePanel.FindControl("invalidFileTypeErrorTxt");
                            txt.InnerText = $"Could not upload '{Path.GetFileName(uploadedFile.FileName).ToString()}'. Invalid file type: '{Path.GetExtension(uploadedFile.FileName).ToString()}'";
                            invalidFileTypePanel.Style.Add("display", "inline-block");
                        }
                    } else
                        filesTooLarge.Add(Path.GetFileName(uploadedFile.FileName).ToString());
                        
                }

                if(filesTooLarge.Count > 0)
                {
                    HtmlGenericControl txt = (HtmlGenericControl)invalidFileTypePanel.FindControl("fileUploadTooLargeTxt");
                    txt.InnerText = $"Could not upload {String.Join(", ", filesTooLarge.Select(fileName=> "'" + fileName + "'").ToArray())}. File(s) too large";
                    fileUploadedTooLargePanel.Style.Add("display", "inline-block");
                }

                if(dt.Rows.Count > 0)
                {
                    // hide error if it was shown
                    moreThan2DaysConsecutiveSickLeave.Style.Add("display", "none");

                    // add files to session so they will persist after postback
                    Session["uploadedFiles"] = files;

                    // show files uploaded
                    filesUploadedPanel.Visible = true;

                    // populate files uploaded list
                    filesUploadedList.DataTextField = "file_name";
                    filesUploadedList.DataSource = dt;
                    filesUploadedList.DataBind();
                }  
            }
            else
                filesUploadedPanel.Visible = false;
        }

        protected void clearUploadedFiles_Click(object sender, EventArgs e)
        {
            filesUploadedPanel.Visible = false;
            FileUpload1.Dispose();
            filesUploadedList.DataSource = new DataTable();
            filesUploadedList.DataBind();

            Session["uploadedFiles"] = null;
            validateDates(txtFrom.Text, txtTo.Text);
        }

        protected void submitEditsBtn_Click(object sender, EventArgs e)
        {
            Boolean isSupCommentsChanged, isHrCommentsChanged, isDaysTakenChanged;
            isSupCommentsChanged = isHrCommentsChanged = isDaysTakenChanged = false;

            string leaveId = Request.QueryString["leaveId"];
            string supComment, hrComment, daysTaken;
            supComment = hrComment = daysTaken = string.Empty;

            if (supCommentsTxt.Disabled == false)
            {
                supComment = supCommentsTxt.InnerText;
                isSupCommentsChanged = ViewState["supComment"].ToString() != supComment;
            }


            if (hrCommentsTxt.Disabled == false)
            {
                hrComment = hrCommentsTxt.InnerText;
                isHrCommentsChanged = ViewState["hrComment"].ToString() != hrComment;
            }

            daysTaken = numDaysAppliedForEditTxt.Text;
            isDaysTakenChanged = ViewState["daysTaken"].ToString() != daysTaken;

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    string sql = $@"
                        UPDATE [dbo].leavetransaction 
                        SET 
                            days_taken = @DaysTaken, 
                            sup_comment = @SupervisorComments, 
                            hr_comment = @HrComments, 
                            qualified = (SELECT IIF((
				                ( '{typeOfLeave.SelectedValue}'= 'Sick' AND @DaysTaken < e.sick) OR
				                ('{typeOfLeave.SelectedValue}' = 'Casual' AND @DaysTaken < e.casual) OR
				                ('{typeOfLeave.SelectedValue}' = 'Vacation' AND @DaysTaken < e.vacation) OR
				                ('{typeOfLeave.SelectedValue}'= 'Personal' AND @DaysTaken < e.personal) OR
				                ('{typeOfLeave.SelectedValue}' = 'Bereavement' AND @DaysTaken < e.bereavement) OR
				                ('{typeOfLeave.SelectedValue}' = 'Maternity' AND @DaysTaken < e.maternity) OR
				                ('{typeOfLeave.SelectedValue}' = 'Pre-retirement' AND @DaysTaken < e.pre_retirement)
				                ), 'Yes', 'No')
                              FROM [dbo].[employee] e
                              WHERE e.employee_id = (SELECT employee_id FROM leavetransaction WHERE transaction_id = {leaveId}))
                        WHERE transaction_id = {leaveId};
                    ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if (supComment != string.Empty)
                            command.Parameters.AddWithValue("@SupervisorComments", supComment);
                        else
                            command.Parameters.AddWithValue("@SupervisorComments", DBNull.Value);

                        if (hrComment != string.Empty)
                            command.Parameters.AddWithValue("@HrComments", hrComment);
                        else
                            command.Parameters.AddWithValue("@HrComments", DBNull.Value);

                        command.Parameters.AddWithValue("@DaysTaken", daysTaken);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            // show success message
                            successfulSubmitEditsMsgPanel.Visible = true;

                            // hide submit button
                            submitEditsBtn.Visible = false;
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

                        List<string> actionString = new List<string>();

                        if(isSupCommentsChanged)
                            actionString.Add($" supervisor_comment= '{supComment}'");
                        if(isHrCommentsChanged)
                            actionString.Add($"hr_comment= '{hrComment}'");
                        if(isDaysTakenChanged)
                            actionString.Add($"days_taken= {daysTaken}");

                        command.Parameters.AddWithValue("@Action", $"Edited leave application; leave_transaction_id= {leaveId}, {String.Join(", ",actionString.ToArray())}");
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

        // to add limitations to what days taken can be
        //protected void numDaysAppliedForEditTxt_TextChanged(object sender, EventArgs e)
        //{
        //    string previousDaysTaken = string.Empty;
        //    if (ViewState["daysTaken"] != null)
        //    {
        //        previousDaysTaken = ViewState["daysTaken"].ToString();
        //        if(Convert.ToInt32(numDaysAppliedForEditTxt.Text) > Convert.ToInt32(previousDaysTaken)) 
        //    }
                
        //}
    }
}