using HR_LEAVEv2.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.Employee
{
    public partial class ApplyForLeave : System.Web.UI.Page
    {
        string mode = string.Empty; // used to determine what mode the user will view the apply page in
        Util util = new Util();
        User user = new User();

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
            public string files { get; set; }
            public string files_id { get; set; }
        };

        // used to store start date of current employee's active start record
        private string ActiveRecordStartDate
        {
            get { return ViewState["activeRecordStartDate"] != null ? ViewState["activeRecordStartDate"].ToString() : null; }
            set { ViewState["activeRecordStartDate"] = value; }
        }

        private List<HttpPostedFile> uploadedFiles
        {
            get { return Session["uploadedFiles"] != null ? (List<HttpPostedFile>)Session["uploadedFiles"] : null; }
            set { Session["uploadedFiles"] = value; }
        }

        // used for determining how much days is too much days to apply for based on a user's leave balance
        const int MAX_DAYS_PAST_BALANCE = 10;

        protected void Page_Load(object sender, EventArgs e)
        {
            // mode can be apply, edit or view 
            mode = Request.QueryString.HasKeys() ? Request.QueryString["mode"] : "apply";

            if (!IsPostBack)
            {
                if (user.hasNoPermissions())
                    Response.Redirect("~/AccessDenied.aspx");

                /*~~~~~~~~~~~Files INITIALIZATION~~~~~~~~~~~~~~~*/

                // used to maintain state for files after they have been uploaded but before the employee submits the LA
                uploadedFiles = null;
                // no files currently uploaded
                filesUploadedPanel.Visible = false;

                /*~~~~~~~~~~~Files INITIALIZATION~~~~~~~~~~~~~~~*/



                /*~~~~~~~~~~~Supervisor DDL INITIALIZATION~~~~~~~~~~~~~~~*/

                // add parameters to Stored Procedure used to get all the relevant supervisors for an employee
                supervisorDataSource.SelectParameters.Add("empId", user.currUserId);

                /*~~~~~~~~~~~Supervisor DDL INITIALIZATION~~~~~~~~~~~~~~~*/



                /* 3 modes available:
                 *      1. apply
                 *      2. view
                 *      3. edit
                 **/      
                if (mode == "apply")
                {

                    /*~~~~~~~~~~~Leave type DDL INITIALIZATION~~~~~~~~~~~~~~~*/

                    // reset typeOfLeave ddl for new data
                    typeOfLeave.DataSource = null;
                    typeOfLeave.DataBind();

                    // populate typeOfLeave ddl with relevant leave types
                    try
                    {
                        string sql = $@"
                        SELECT elt.leave_type
                        FROM dbo.employee e

                        LEFT JOIN dbo.employeeposition ep
                        ON e.employee_id = ep.employee_id AND (ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date))

                        JOIN dbo.emptypeleavetype elt
                        ON elt.employment_type = ep.employment_type

                        WHERE e.employee_id = '{user.currUserId}'
                        ORDER BY elt.leave_type DESC;
                    ";

                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                DataTable dt = new DataTable();
                                SqlDataAdapter da = new SqlDataAdapter(command);
                                da.Fill(dt);

                                typeOfLeave.DataTextField = typeOfLeave.DataValueField = "leave_type";
                                typeOfLeave.DataSource = dt;
                                typeOfLeave.DataBind();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    /*~~~~~~~~~~~Leave type DDL INITIALIZATION~~~~~~~~~~~~~~~*/



                    /*~~~~~~~~~~~Active Record of Employee applying for leave POPULATION~~~~~~~~~~~~~~~*/

                    // populate active record start date to ensure employee does not submit for leave before the start of their employment
                    DateTime activeRecordStartDate = DateTime.MinValue;

                    if (ActiveRecordStartDate == null)
                    {
                        try
                        {
                            string sql = $@"
                        SELECT ep.start_date
                        FROM dbo.employeeposition ep
                        WHERE ep.employee_id = '{user.currUserId}' AND (ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date))
                    ";
                            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                            {
                                connection.Open();
                                using (SqlCommand command = new SqlCommand(sql, connection))
                                {
                                    using (SqlDataReader reader = command.ExecuteReader())
                                    {

                                        if (reader.Read())
                                        {
                                            activeRecordStartDate = (DateTime)reader["start_date"];
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                        if (activeRecordStartDate != DateTime.MinValue)
                            ActiveRecordStartDate = activeRecordStartDate != null ? activeRecordStartDate.ToString("d/MM/yyyy") : string.Empty;
                    }
                    /*~~~~~~~~~~~Active Record of Employee applying for leave POPULATION~~~~~~~~~~~~~~~*/



                    /*~~~~~~~~~~~Page in Apply mode INITIALIZATION~~~~~~~~~~~~~~~*/

                    // display normal leave application form
                    this.adjustPageForApplyMode();

                    /*~~~~~~~~~~~Page in Apply mode INITIALIZATION~~~~~~~~~~~~~~~*/

                }
                else 
                {
                    // id of leave application to either be edited or viewed
                    string leaveId = Request.QueryString["leaveId"];

                    // populates page and authorizes user based on: their permissions, whether the leave application was submitted to them as a supervisor or various HR criteria
                    LeaveTransactionDetails ltDetails = populatePage(leaveId, mode);

                    if (mode == "view")
                        this.adjustPageForViewMode();
                    else if (mode == "edit")
                        this.adjustPageForEditMode(ltDetails);
                }       
            }
            else
            {
                clearFilesErrors();
                if (mode == "apply")
                {
                    validateDates(txtFrom.Text, txtTo.Text);
                    validateLeave(typeOfLeave.SelectedValue);
                    validateSupervisor(supervisorSelect.SelectedValue != "" ? supervisorSelect.SelectedValue : "-1");
                }
                    
            }

            // hides apply for leave page and shows disclaimer if employee is inactive
            if (mode == "apply")
            {
                container.Visible = !util.isNullOrEmpty(ActiveRecordStartDate);
                employeeInactivePanel.Visible = util.isNullOrEmpty(ActiveRecordStartDate);
            }
        }

        // VALIDATION METHODS
        protected Boolean validateDates(string startDate, string endDate)
        {
            // returns a Boolean representing whether the dates entered for the start date and end date of the leave period specified are valid
            // also shows error/warning/info messages accordingly

            clearDateErrors();

            DateTime start = DateTime.MinValue, 
                     end = DateTime.MinValue;

            Boolean isValidated = true;

            // ensure start date is a valid date
            if (!DateTime.TryParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start))
            {
                invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }

            // ensure end date is a valid date
            if (!DateTime.TryParseExact(endDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out end))
            {
                invalidEndDateValidationMsgPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }

            if (isValidated)
            {
                // check if start date is a before the start of the employee's current employment record
                if(!util.isNullOrEmpty(ActiveRecordStartDate))
                {
                    // compare start dates
                    if (DateTime.Compare(start, DateTime.ParseExact(ActiveRecordStartDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)) < 0)
                    {
                        // applied for start date is before start date of active record
                        startDateIsBeforeStartOfActiveRecordTxt.InnerText = $"Start date is before start of your current employment. Start date must be a day on or after {ActiveRecordStartDate}";
                        startDateIsBeforeStartOfActiveRecord.Style.Add("display", "inline-block");
                        isValidated = false;
                    }
                } 

                // inform user that there are holidays in between the leave period applied for
                List<string> holidaysInBetween = util.getHolidaysBetween(start, end);
                if (holidaysInBetween.Count > 0)
                {
                    holidayInAppliedTimeTxt.InnerText = $"The following public holiday(s) occur during your leave period: {String.Join(", ", holidaysInBetween.ToArray())}";
                    holidayInAppliedTimePeriodPanel.Style.Add("display", "inline-block");
                }

                // ensure start date is not a holiday
                holidaysInBetween = util.getHolidaysBetween(start, start);
                if (holidaysInBetween.Count > 0)
                {
                    startDateIsHolidayTxt.InnerText = $"Start date cannot be on {holidaysInBetween.ElementAt(0)}";
                    startDateIsHoliday.Style.Add("display", "inline-block");
                    isValidated = false;
                }
                // ensure end date is not a holiday
                holidaysInBetween = util.getHolidaysBetween(end, end);
                if (holidaysInBetween.Count > 0)
                {
                    endDateIsHolidayTxt.InnerText = $"End date cannot be on {holidaysInBetween.ElementAt(0)}";
                    endDateIsHoliday.Style.Add("display", "inline-block");
                    isValidated = false;
                }

                // ensure start date is not on the weekend
                if (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday)
                {
                    startDateIsWeekend.Style.Add("display", "inline-block");
                    isValidated = false;
                }

                // ensure end date is not on the weekend
                if (end.DayOfWeek == DayOfWeek.Saturday || end.DayOfWeek == DayOfWeek.Sunday)
                {
                    endDateIsWeekend.Style.Add("display", "inline-block");
                    isValidated = false;
                }

                // ensure end date is not before start date
                if (DateTime.Compare(start, end) > 0)
                {
                    dateComparisonValidationMsgPanel.Style.Add("display", "inline-block");
                    isValidated = false;
                }

                if (!util.isNullOrEmpty(typeOfLeave.SelectedValue))
                {
                    // once not sick leave, ensure start date is not a day before today 
                    if (!typeOfLeave.SelectedValue.Equals("Sick") && DateTime.Compare(start, util.getCurrentDateToday()) < 0)
                    {
                        startDateBeforeTodayValidationMsgPanel.Style.Add("display", "inline-block");
                        isValidated = false;
                    }

                    // if leave type is vacation, ensure start date is at least one month from today
                    if (typeOfLeave.SelectedValue.Equals("Vacation"))
                    {
                        DateTime firstDateVacationCanBeTaken = util.getCurrentDateToday().AddMonths(1);

                        if (DateTime.Compare(start, firstDateVacationCanBeTaken) < 0)
                        {
                            invalidVacationStartDateMsgPanel.Style.Add("display", "inline-block");
                            isValidated = false;
                        }
                    }

                    // if type of leave is sick, ensure you can only apply for it retroactively
                    if (typeOfLeave.SelectedValue.Equals("Sick"))
                    {

                        // ensure that if employee applies for more than two days consecutive sick leave, they are warned that they must
                        // upload a file (medical)
                        if (getNumDaysBetween(start, end) > 2)
                        {
                            List<HttpPostedFile> files = null;
                            if (uploadedFiles != null)
                            {
                                files = uploadedFiles;
                                if (files.Count == 0)
                                {
                                    moreThan2DaysConsecutiveSickLeave.Style.Add("display", "inline-block");
                                    submitHardCopyOfMedicalDisclaimerPanel.Style.Add("display", "none");
                                    isValidated = false;
                                }
                            }
                            else
                            {
                                moreThan2DaysConsecutiveSickLeave.Style.Add("display", "inline-block");
                                isValidated = false;
                            }
                        }

                        // ensure sick leave ends before today (sick leave is taken retroactively)
                        if (DateTime.Compare(end, util.getCurrentDateToday()) > 0)
                        {
                            invalidSickLeaveStartDate.Style.Add("display", "inline-block");
                            isValidated = false;
                        }
                    }

                    // if type of leave is Casual then ensure employee cannot take more than 7 consecutive days leave
                    if (typeOfLeave.SelectedValue.Equals("Casual"))
                    {
                        if (getNumDaysBetween(start, end) > 7)
                        {
                            List<HttpPostedFile> files = null;
                            if (uploadedFiles != null)
                            {
                                files = uploadedFiles;
                                if (files.Count == 0)
                                {
                                    moreThan7DaysConsecutiveCasualLeave.Style.Add("display", "inline-block");
                                    isValidated = false;
                                }
                            }
                            else
                            {
                                moreThan7DaysConsecutiveCasualLeave.Style.Add("display", "inline-block");
                                isValidated = false;
                            }
                        }
                    }
                }
            }

            return isValidated;
        }

        public int getNumWeekendsBetween(DateTime start, DateTime end)
        {
            // returns an int containing the number of weekend days which fall in between the leave period specified

            int numWeekends = 0;
            foreach (DateTime day in util.EachCalendarDay(start, end))
            {
                if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday)
                    numWeekends++;
            }

            return numWeekends;
        }

        protected Boolean validateLeave(string typeOfLeaveSelected)
        {
            // returns a Boolean representing whether the type of leave selected is valid. The appropriate validation message is also constructed and 
            // shown to the user

            invalidLeaveTypePanel.Style.Add("display", "none");
            invalidLeaveTypeTxt.InnerText = string.Empty;

            string isValid = string.Empty,
                   errTxt = string.Empty;

            List<HttpPostedFile> files = uploadedFiles;
            if (!util.isNullOrEmpty(typeOfLeaveSelected))
            {
                Dictionary<string, string> leaveBalanceMappings = util.getLeaveTypeMapping();
                if (util.isLeaveTypeWithoutBalance(typeOfLeaveSelected))
                    return true;
                else
                {
                    string empType = string.Empty,
                        leaveBalanceToGet = string.Empty;

                    int leaveBalance = 0;

                    DateTime startDate = DateTime.MinValue;

                    // get employee's employment type, start date and the leave balance of the type of leave applied for
                    try
                    {
                        string sql = $@"
                        SELECT ep.employment_type, ep.start_date, e.{leaveBalanceMappings[typeOfLeaveSelected]} as 'leave_balance'
                        FROM dbo.employee e
                        JOIN dbo.employeeposition ep
                        ON e.employee_id = ep.employee_id
                        WHERE e.employee_id = '{user.currUserId}';
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
                                        empType = reader["employment_type"].ToString();
                                        startDate = Convert.ToDateTime(reader["start_date"].ToString());
                                        isValid = "Yes";
                                        leaveBalance = Convert.ToInt32(reader["leave_balance"].ToString());

                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    // check if the amt of days taken is more than that MAX_DAYS_PAST_BALANCE
                    int daysTaken = Convert.ToInt32(numDaysAppliedFor.Text);
                    if (daysTaken >= (leaveBalance + MAX_DAYS_PAST_BALANCE) && files == null && typeOfLeave.SelectedValue != "Sick")
                    {
                        isValid = "No";
                        errTxt = "Not eligible for amount of leave entered";
                        fileUploadNeededPanel.Style.Add("display", "inline-block");
                    }
                        


                    empType = util.isNullOrEmpty(empType) ? "unregistered" : empType;

                    if (isValid == "Yes" && empType == "Contract")
                    {
                        DateTime elevenMonthsFromStartDate = startDate.AddMonths(11);

                        
                        if (DateTime.Compare(util.getCurrentDateToday(), elevenMonthsFromStartDate) < 0)
                        {
                            // ensure employee cannot apply for vacation within first 11 months of Contract
                            if (typeOfLeaveSelected == "Vacation")
                            {
                                isValid = "No";
                                errTxt = "Cannot apply for Vacation leave within first 11 months of contract";
                            }
                                
                        }
                        else
                        {
                            // ensure that employee cannot apply for personal leave after first 11 months of Contract
                            if (typeOfLeaveSelected == "Personal")
                            {
                                isValid = "No";
                                errTxt = "Cannot apply for Personal leave after first 11 months of contract";
                            }
                               
                        }

                    }
                }
            }
            else
            {
                isValid = "No";
                errTxt = "Type of Leave not selected";
            }

            if (isValid == "No" || !util.isNullOrEmpty(errTxt))
            {
                invalidLeaveTypeTxt.InnerText = errTxt;
                invalidLeaveTypePanel.Style.Add("display", "inline-block");
            }

            return isValid == "Yes" && util.isNullOrEmpty(errTxt);
        }

        protected int getNumDaysBetween(DateTime start, DateTime end)
        {
            // gets the number of days being applied for and includes type of leave in this evaluation
            if(start != DateTime.MinValue && end != DateTime.MinValue)
            {
                int numDays = 0;
                // checks to see if the amt of days in between the start and the end is greater than 0, otherwise the value will be 0
                numDays = ((end - start).Days + 1) > 0 ? ((end - start).Days + 1) : 0;

                // reduces the number of days applied for if holidays fall in between the leave period applied for
                List<string> holidays = util.getHolidaysBetween(start, end);
                if (holidays.Count > 0)
                    numDays = numDays - holidays.Count;
                //numDaysAppliedFor.Text = $"{ Convert.ToInt32(numDaysAppliedFor.Text) - holidays.Count}";

                // if leave type is not no pay or sick then minus weekends from count
                if (!typeOfLeave.SelectedValue.Equals("No Pay") && !typeOfLeave.SelectedValue.Equals("Sick"))
                    numDays = numDays - getNumWeekendsBetween(start, end);

                return numDays;
            }
            return -1;
        }

        protected void populateDaysTaken()
        {
            // populates the value for the number of days applied for 

            DateTime start = DateTime.MinValue,
                    end = DateTime.MinValue;

            Boolean isStartDateFilled = DateTime.TryParseExact(txtFrom.Text, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start),
                    isEndDateFilled = DateTime.TryParseExact(txtTo.Text, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out end);

            // if type of leave has been selected (typeOfLeave.SelectedIndex == 0 when no leave type has been selected as yet) and the dates are valid
            // OR if the start and end date are filled
            if ((typeOfLeave.SelectedIndex != 0 && validateDates(txtFrom.Text, txtTo.Text)) || (isStartDateFilled && isEndDateFilled))
            {
               
                numDaysAppliedFor.Text = getNumDaysBetween(start, end).ToString();
                validateLeave(typeOfLeave.SelectedValue);
            }
        }

        protected Boolean validateSupervisor(string supId)
        {
            // returns a Boolean representing whether the supervisor is valid (has an id that is not -1)

            invalidSupervisor.Style.Add("display", "none");
            if (supId == "-1")
                invalidSupervisor.Style.Add("display", "inline-block");

            return supId != "-1";
        }
        // _______________________________________________________________________


        // CLEAR ERRORS METHODS
        protected void clearDateErrors()
        {
            // clears all errors associated with the validation of dates

            dateComparisonValidationMsgPanel.Style.Add("display", "none");

            invalidStartDateValidationMsgPanel.Style.Add("display", "none");

            startDateBeforeTodayValidationMsgPanel.Style.Add("display", "none");

            invalidEndDateValidationMsgPanel.Style.Add("display", "none");

            invalidVacationStartDateMsgPanel.Style.Add("display", "none");

            invalidSickLeaveStartDate.Style.Add("display", "none");

            moreThan2DaysConsecutiveSickLeave.Style.Add("display", "none");

            moreThan7DaysConsecutiveCasualLeave.Style.Add("display", "none");

            startDateIsWeekend.Style.Add("display", "none");

            endDateIsWeekend.Style.Add("display", "none");

            holidayInAppliedTimePeriodPanel.Style.Add("display", "none");

            startDateIsHoliday.Style.Add("display", "none");

            endDateIsHoliday.Style.Add("display", "none");

            startDateIsBeforeStartOfActiveRecord.Style.Add("display", "none");
        }

        protected void clearSubmitLeaveApplicationErrors()
        {
            // clears all errors associated with submitting a leave application
            errorInsertingFilesToDbPanel.Style.Add("display", "none");

            errorSubmittingLeaveApplicationPanel.Style.Add("display", "none");

            errorSendingEmailNotifications.Style.Add("display", "none");

            errorSendingInHouseNotifications.Style.Add("display", "none");
        }

        protected void clearFilesErrors()
        {
            // clears all errors associated with uploading/clearing/downloading files
            invalidFileTypePanel.Style.Add("display", "none");
            fileUploadedTooLargePanel.Style.Add("display", "none");
            duplicateFileNamesPanel.Style.Add("display", "none");
            fileUploadNeededPanel.Style.Add("display", "none");
        }
        //________________________________________________________________________

        
        // INITIAL PREP OF PAGE BASED ON MODE
        protected void adjustPageForViewMode()
        {
            // sets Title of page
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
            filesToDownloadPanel.Visible = false;

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

            //disclaimer about days taken
            daysTakenDisclaimerPanel.Style.Add("display", "inline-block");

        }

        protected void adjustPageForEditMode(LeaveTransactionDetails ltDetails)
        {
            // Title
            viewModeTitle.Visible = false;
            editModeTitle.Visible = true;
            applyModeTitle.Visible = false;

            // show convert to paid leave button if leave type is No Pay
            if (typeOfLeaveTxt.Text == "No Pay")
                convertToPaidLeave.Visible = true;

            // upload files
            fileUploadPanel.Visible = true;

            // if supervisor on leave application, then they can leave a comment
            if (user.currUserId == ltDetails.supId)
                supCommentsTxt.Disabled = false;

            // adjust page to allow HR to leave comments or edit the number of days applied for
            if (user.permissions.Contains("hr1_permissions") || user.permissions.Contains("hr2_permissions"))
            {
                hrCommentsTxt.Disabled = false;
                numDaysAppliedForEditTxt.Visible = true;
                numDaysAppliedFor.Visible = false;
            }
            else
                numDaysAppliedFor.Visible = true;

            // submit button
            submitEditsPanel.Visible = true;

        }
        //________________________________________________________________________

        
        // POPULATION OF DATA FIELDS ON PAGE FOR VIEW AND EDIT MODE
        protected LeaveTransactionDetails populatePage(string leaveId, string mode)
        {
            // populates the page with the relevant details 
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
                            ep.employment_type, 
                            FORMAT(lt.start_date, 'd/MM/yyyy') start_date, 
                            FORMAT(lt.end_date, 'd/MM/yyyy') end_date, 
                            lt.qualified,
                            lt.days_taken,
                            lt.leave_type, 
                            lt.status, 
                            FORMAT(lt.created_at, 'd/MM/yyyy h:mm tt') as submitted_on,
                            lt.supervisor_id, 
                            sup.first_name + ' ' + sup.last_name as 'supervisor_name', 
                            lt.emp_comment, 
                            lt.sup_comment, 
                            lt.hr_comment,
                            (SELECT 
					            STUFF
					            (
						            (SELECT ', ' + fs.file_name
						            FROM [dbo].filestorage fs 
						            LEFT JOIN [dbo].employeefiles ef ON ef.employee_id = lt.employee_id AND ef.leave_transaction_id = lt.transaction_id 
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
						            LEFT JOIN [dbo].employeefiles ef ON ef.employee_id = lt.employee_id AND ef.leave_transaction_id = lt.transaction_id 
						            WHERE fs.file_id = ef.file_id
						            FOR XML PATH(''))
						            ,1
						            ,1,
						            ''
					            )
				            ) as 'files_id'
                            
                        FROM [dbo].[leavetransaction] lt

                        JOIN [dbo].[employee] sup ON sup.employee_id = lt.supervisor_id

                        LEFT JOIN [dbo].employeeposition ep ON ep.id = lt.employee_position_id

                        JOIN [dbo].[employee] emp ON emp.employee_id = lt.employee_id

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
                                    hrComment = reader["hr_comment"].ToString(),
                                    files = reader["files"].ToString(),
                                    files_id = reader["files_id"].ToString()
                                };

                            }

                            // check permissions of current user
                            if(!user.isUserAllowedToViewOrEditLeaveApplication(ltDetails.empId, ltDetails.empType, ltDetails.supId, ltDetails.typeOfLeave, mode))
                                Response.Redirect("~/AccessDenied.aspx");

                            //populate form
                            empIdHiddenTxt.Value = ltDetails.empId;
                            empNameHeader.InnerText = ltDetails.empName;
                            startDateInfoTxt.Text = ltDetails.startDate;
                            endDateInfoTxt.Text = ltDetails.endDate;
                            numDaysAppliedFor.Text = ltDetails.daysTaken;

                            // store previous num days applied for in viewstate
                            ViewState["daysTaken"] = numDaysAppliedForEditTxt.Text = ltDetails.daysTaken;

                            ViewState["typeOfLeave"] = typeOfLeaveTxt.Text = ltDetails.typeOfLeave;

                            statusTxt.Text = ltDetails.status;
                            qualifiedTxt.Text = ltDetails.qualified;
                            submittedOnTxt.Text = "Submitted on: " + ltDetails.submittedOn;
                            supervisorNameTxt.Text = ltDetails.supName;
                            empCommentsTxt.Value = ltDetails.empComment;

                            // store previous supervisor comment in viewstate
                            ViewState["supComment"] = supCommentsTxt.Value = ltDetails.supComment;

                            // store previous HR comment in viewstate
                            ViewState["hrComment"] = hrCommentsTxt.Value = ltDetails.hrComment;

                            //populate dropdown list with file names if there are files associated with the LA
                            /* Since the file name is gotten by using a join on both the employee id and the leave transaction id then the file name will always be relevant and not
                             * a different file with the same filename
                             * 
                             * */
                            if (!util.isNullOrEmpty(ltDetails.files))
                            {
                                string[] fileNamesArr = ltDetails.files.Split(',');
                                string[] fileIdsArr = ltDetails.files_id.Split(',');
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
                            }
                            else
                                filesToDownloadPanel.Visible = false;

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
        //________________________________________________________________________

        
        // FILES METHODS
        protected void uploadBtn_Click(object sender, EventArgs e)
        {
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
                    if (uploadedFile.ContentLength < maxRequestLength)
                    {
                        if (allowedFileExtensions.Contains(Path.GetExtension(uploadedFile.FileName).ToString()))
                        {
                            if(mode == "edit")
                            {
                                // check for duplicate file name
                                if(filesToDownloadList.Items.FindByText(Path.GetFileName(uploadedFile.FileName).ToString()) != null)
                                {
                                    duplicateFiles.Add(Path.GetFileName(uploadedFile.FileName).ToString());
                                    continue;
                                }
                            }
                            dt.Rows.Add(Path.GetFileName(uploadedFile.FileName).ToString());
                            files.Add(uploadedFile);
                        }
                        else
                            invalidFiles.Add(Path.GetFileName(uploadedFile.FileName).ToString());
                    }
                    else
                    {
                        filesTooLarge.Add(Path.GetFileName(uploadedFile.FileName).ToString());

                        if (!allowedFileExtensions.Contains(Path.GetExtension(uploadedFile.FileName).ToString()))
                            invalidFiles.Add(Path.GetFileName(uploadedFile.FileName).ToString());
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

                if(duplicateFiles.Count > 0)
                {
                    HtmlGenericControl txt = (HtmlGenericControl)duplicateFileNamesPanel.FindControl("duplicateFileNameTxt");
                    txt.InnerText = $"Could not upload {String.Join(", ", duplicateFiles.Select(fileName => "'" + fileName + "'").ToArray())}. Uploaded file name(s) already exist for this leave application";
                    duplicateFileNamesPanel.Style.Add("display", "inline-block");
                }

                if (dt.Rows.Count > 0)
                {
                    clearAllFilesBtn.Visible = true;
                    // add files to session so they will persist after postback
                    uploadedFiles = files;

                    // show files uploaded
                    filesUploadedPanel.Visible = true;

                    // populate files uploaded list
                    filesUploadedListView.DataSource = dt;
                    filesUploadedListView.DataBind();

                    if (mode == "apply")
                    {
                        // hide file upload needed panel if shown
                        fileUploadNeededPanel.Style.Add("display", "none");

                        if (typeOfLeave.SelectedValue == "Sick")
                        {
                            // hide error if it was shown
                            moreThan2DaysConsecutiveSickLeave.Style.Add("display", "none");

                            // show disclaimer
                            submitHardCopyOfMedicalDisclaimerPanel.Style.Add("display", "inline-block");
                        }
                        else if(typeOfLeave.SelectedValue == "Casual")
                            moreThan7DaysConsecutiveCasualLeave.Style.Add("display", "none");

                        validateLeave(typeOfLeave.SelectedValue);
                    }
                    else if (mode == "edit")
                    {
                        filesNeededForConversionToPaidPanel.Visible = false;
                        filesToDownloadPanel.Style.Add("margin-top", "15px");
                    }

                }
                else
                    clearAllFilesBtn.Visible = false;
            }
            else
            {
                List<HttpPostedFile> files = null;

                if (uploadedFiles != null)
                    files = uploadedFiles;

                if (uploadedFiles == null || files == null || files.Count <= 0)
                    filesUploadedPanel.Visible = false;
            }

        }

        protected void clearUploadedFiles_Click(object sender, EventArgs e)
        {
            // clears uploaded files from Session storage

            filesUploadedPanel.Visible = false;
            FileUpload1.Dispose();

            clearAllFilesBtn.Visible = false;

            filesUploadedListView.DataSource = new DataTable();
            filesUploadedListView.DataBind();

            uploadedFiles = null;

            if (mode == "apply")
            {
                validateDates(txtFrom.Text, txtTo.Text);
                validateLeave(typeOfLeave.SelectedValue);
            }
            else if (mode == "edit")
            {
                filesToDownloadPanel.Style.Clear();
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

        protected void clearIndividualFileBtn_Click(object sender, EventArgs e)
        {
            // remove an individual file from List of uploaded files in Session

            LinkButton btn = sender as LinkButton;
            string file_name = btn.Attributes["data-id"].ToString();

            DataTable dt = new DataTable();
            dt.Columns.Add("file_name", typeof(string));

            // go through all files and create new datatable
            List<HttpPostedFile> files = null;
            if (uploadedFiles != null)
            {
                files = uploadedFiles;

                HttpPostedFile fileToRemove = files.SingleOrDefault<HttpPostedFile>(file => Path.GetFileName(file.FileName).ToString() == file_name);
                if (fileToRemove != null)
                    files.Remove(fileToRemove);

                if (files.Count > 0)
                {
                    foreach (HttpPostedFile file in files)
                    {
                        dt.Rows.Add(Path.GetFileName(file.FileName).ToString());
                    }
                    uploadedFiles = files;
                }
                else
                {
                    // no uploaded files left
                    clearAllFilesBtn.Visible = false;

                    uploadedFiles = null;
                    filesUploadedPanel.Visible = false;
                    if (mode == "apply")
                    {
                        validateDates(txtFrom.Text, txtTo.Text);
                        validateLeave(typeOfLeave.SelectedValue);
                    }
                    else if (mode == "edit")
                    {
                        filesToDownloadPanel.Style.Clear();
                    }
                }

                filesUploadedListView.DataSource = dt;
                filesUploadedListView.DataBind();

            }

        }
        //________________________________________________________________________


        // APPLY MODE: LA SUBMITTED METHODS
        protected void submitLeaveApplication_Click(object sender, EventArgs e)
        {
            /**
             * This function fulfills the following purposes:
             * 
             *  1. Submit Leave application
             *      An employee can submit a leave application including the following fields
             *      a. Start Date
             *      b. End Date
             *      c. Days Taken (calculated literal count of days between the start and end date inclusive, not accounting for holidays or weekends in between etc.)
             *      d. Type of leave taken
             *      e. Supervisor name
             *      f. Uploaded files- for uploading any supporting documentation
             *  2. Submit emails
             *      a. To supervisor specified in application
             *      b. To employee who submitted the application
             *  3. Audit Logs
             *      a. Adds audit logs for submitted applications
             * 
             * */


            // submit leave application errors

            // hide error messages before rechecking criteria for new/ preexisting errors
            clearSubmitLeaveApplicationErrors();

            // get values from form controls
            string empId = user.currUserId, // employee id
                leaveType = typeOfLeave.SelectedValue, // type of leave
                startDate = txtFrom.Text.ToString(), // start date
                endDate = txtTo.Text.ToString(), // end date
                supId = supervisorSelect.SelectedValue != "" ? supervisorSelect.SelectedValue : "-1", // supervisor id
                comments = empCommentsTxt.Value.Length > 0 ? empCommentsTxt.Value.ToString() : null; // employee comments

            // uploaded files
            List<HttpPostedFile> files = uploadedFiles;

            // validate form values using these booleans
            Boolean isValidated, isLeaveApplicationInsertSuccessful, isFileUploadSuccessful, areFilesUploaded;
            isLeaveApplicationInsertSuccessful = isFileUploadSuccessful = true;
            areFilesUploaded = false;

            isValidated = validateDates(startDate, endDate) && validateLeave(leaveType) && validateSupervisor(supId);
            if (isValidated)
            {
                // used to store id of leave transaction which is outputted by the INSERT statement. This is later used in the audit log
                string transaction_id = string.Empty;

                // used to store ids of inserted files in order to add to audit log
                List<string> uploadedFilesIds = new List<string>();

                try
                {
                    string sql = $@"
                        INSERT INTO [dbo].[leavetransaction]
                           ([employee_id]
                           ,[employee_position_id]
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
                            , (SELECT id FROM [dbo].[employeeposition] WHERE start_date <= GETDATE() AND (actual_end_date IS NULL OR GETDATE() <= actual_end_date) AND employee_id = '{empId}')
                            ,'{leaveType}'
                            ,'{DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy")}'
                            ,'{DateTime.ParseExact(endDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy")}'
                            , {util.getSqlForCalculatingQualifiedField(leaveType, $"'{empId}'")}
                            , @DaysTaken
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

                            command.Parameters.AddWithValue("@DaysTaken", numDaysAppliedFor.Text);
                            transaction_id = command.ExecuteScalar().ToString();
                            isLeaveApplicationInsertSuccessful = !util.isNullOrEmpty(transaction_id);
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
                                        string fileid = string.Empty;
                                        using (SqlCommand command = new SqlCommand(sql, connection))
                                        {
                                            command.Parameters.AddWithValue("@FileData", bytes);
                                            command.Parameters.AddWithValue("@FileName", file_name);
                                            command.Parameters.AddWithValue("@FileExtension", file_extension);
                                            //command.Parameters.AddWithValue("@UploadedOn", util.getCurrentDate());
                                            fileid = command.ExecuteScalar().ToString();

                                            isFileUploadSuccessful = !util.isNullOrEmpty(fileid);
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
                                                command.Parameters.AddWithValue("@EmployeeId", user.currUserId);
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


                if (isFileUploadSuccessful && isLeaveApplicationInsertSuccessful)
                {
                    // add audit log
                    string fileActionString = String.Empty;
                    if (areFilesUploaded)
                        fileActionString = $"Files uploaded: {String.Join(", ", uploadedFilesIds.Select(lb => "id= " + lb).ToArray())}";

                    string action = $"Submitted leave application: leave_transaction_id= {transaction_id};{fileActionString}";
                    util.addAuditLog(user.currUserId, user.currUserId, action);

                    // show feedback
                    submitButtonPanel.Style.Add("display", "none");
                    successMsgPanel.Style.Add("display", "inline-block");

                    // send notifs
                    sendNotifications();
                    resetNumNotifications();
                }
            }

            // ERROR FEEDBACK
            if (!isFileUploadSuccessful)
                errorInsertingFilesToDbPanel.Style.Add("display", "inline-block");
            if (!isLeaveApplicationInsertSuccessful)
                errorSubmittingLeaveApplicationPanel.Style.Add("display", "inline-block");

        }

        protected void sendNotifications()
        {
            // sends both email and in application notifs to the relevant users when an employee submits a leave application

            Boolean isEmailNotifsSentSuccessfully = false,
                    isInAppNotifsSentSuccessfully = false;

            // send email notifications

            // send email to supervisor to let them know an employee has submitted a LA
            // get supervisor's email in order to send then a message notifying them that an employee has submitted an application
            string supEmail = string.Empty;

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                            SELECT email FROM [dbo].[employee] WHERE employee_id = {supervisorSelect.SelectedValue.ToString()}
                        ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                supEmail = reader["email"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            MailMessage message = util.getSupervisorViewEmployeeSubmittedLeaveApplication(
                new Util.EmailDetails
                {
                    employee_name = user.currUserName,
                    date_submitted = util.getCurrentDate().ToString("d/MM/yyyy h:mm tt"),
                    start_date = txtFrom.Text.ToString(),
                    end_date = txtTo.Text.ToString(),
                    days_taken = numDaysAppliedFor.Text,
                    type_of_leave = typeOfLeave.SelectedValue,
                    subject = $"{user.currUserName} Submitted Leave Application",
                    recipient = supEmail
                }
                );

            isEmailNotifsSentSuccessfully = util.sendMail(message);

            // send email to employee 
            message = util.getEmployeeViewEmployeeSubmittedLeaveApplication(
                new Util.EmailDetails
                {
                    supervisor_name = supervisorSelect.SelectedItem.Text,
                    date_submitted = util.getCurrentDate().ToString("d/MM/yyyy h:mm tt"),
                    start_date = txtFrom.Text.ToString(),
                    end_date = txtTo.Text.ToString(),
                    days_taken = numDaysAppliedFor.Text,
                    type_of_leave = typeOfLeave.SelectedValue,
                    subject = "Submitted Leave Application",
                    recipient = user.currUserEmail
                }
                );
            isEmailNotifsSentSuccessfully = isEmailNotifsSentSuccessfully && util.sendMail(message);


            // send inhouse notifications
            // send employee notif
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('Submitted Leave Application',@Notification, 'No', '{user.currUserId}', '{util.getCurrentDate()}');
                            ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Notification", $"You submitted a leave application to {supervisorSelect.SelectedItem.Text} for {numDaysAppliedFor.Text} day(s) {typeOfLeave.SelectedValue} leave");
                        int rowsAffected = command.ExecuteNonQuery();
                        isInAppNotifsSentSuccessfully = rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                isInAppNotifsSentSuccessfully = false;
            }

            //send supervisor notif
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{user.currUserName} Submitted Leave Application',@Notification, 'No', '{supervisorSelect.SelectedValue.ToString()}', '{util.getCurrentDate()}');
                            ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Notification", $"{user.currUserName} submitted a leave application for {numDaysAppliedFor.Text} day(s) {typeOfLeave.SelectedValue} leave");
                        int rowsAffected = command.ExecuteNonQuery();
                        isInAppNotifsSentSuccessfully = rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                isInAppNotifsSentSuccessfully = false;
            }

            // user feedback
            if (!isEmailNotifsSentSuccessfully)
                errorSendingEmailNotifications.Style.Add("display", "inline-block");
            if (!isInAppNotifsSentSuccessfully)
                errorSendingInHouseNotifications.Style.Add("display", "inline-block");
        }

        protected void resetNumNotifications()
        {
            // resets the number of notifications for the current user
            Label num_notifs = (Label)Master.FindControl("num_notifications");
            num_notifs.Text = util.resetNumNotifications(user.currUserId);

            System.Web.UI.UpdatePanel up = (System.Web.UI.UpdatePanel)Master.FindControl("notificationsUpdatePanel");
            up.Update();
        }
        //________________________________________________________________________

        
        // EDIT MODE: LA EDITED
        protected void submitEditsBtn_Click(object sender, EventArgs e)
        {
            // allows supervisor or HR to edit fields in an employee's LA including uploaded additional files
            Boolean isSupCommentsChanged = false,
                isHrCommentsChanged = false,
                isDaysTakenChanged = false,
                isLeaveTypeChanged = false,
                areFilesUploaded = false,
                isEditSuccessful = false;

            string leaveId = Request.QueryString["leaveId"],
                   supComment = string.Empty,
                   hrComment = string.Empty,
                   daysTaken = string.Empty;

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

            isLeaveTypeChanged = ViewState["typeOfLeave"].ToString() != typeOfLeaveTxt.Text;

            daysTaken = numDaysAppliedForEditTxt.Text;
            isDaysTakenChanged = ViewState["daysTaken"].ToString() != daysTaken;

            List<HttpPostedFile> files = uploadedFiles;
            areFilesUploaded = files != null;
            List<string> uploadedFilesIds = null;

            if (isDaysTakenChanged || isSupCommentsChanged || isHrCommentsChanged || isLeaveTypeChanged || areFilesUploaded)
            {

                if (isLeaveTypeChanged && !areFilesUploaded)
                {
                    filesNeededForConversionToPaidPanel.Visible = true;
                    return;
                }

                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = string.Empty,
                            comments = string.Empty,
                            daysTakenAndQualifiedSql = string.Empty,
                            leaveTypeSql = string.Empty,
                            updateLeaveBalance = string.Empty;

                        if (!isSupCommentsChanged && isHrCommentsChanged)
                            comments = $"hr_comment = @HrComments,";
                        if (isSupCommentsChanged && !isHrCommentsChanged)
                            comments = $"sup_comment = @SupervisorComments,";
                        if (isSupCommentsChanged && isHrCommentsChanged)
                            comments = $"sup_comment = @SupervisorComments, hr_comment = @HrComments,";

                        Dictionary<string, string> leaveBalanceColumnName = util.getLeaveTypeMapping();

                        if (statusTxt.Text == "Approved" && isDaysTakenChanged && !util.isLeaveTypeWithoutBalance(typeOfLeaveTxt.Text))
                        {
                            int difference = Convert.ToInt32(daysTaken) - Convert.ToInt32(ViewState["daysTaken"].ToString());
                            updateLeaveBalance = $@"                          
                                -- updating employee leave balance
                                UPDATE 
                                    [dbo].[employee]
                                SET
                                    {leaveBalanceColumnName[typeOfLeaveTxt.Text]} = {leaveBalanceColumnName[typeOfLeaveTxt.Text]} - ({difference})
                                WHERE
                                    [employee_id] = '{empIdHiddenTxt.Value}';
                            ";
                        }

                        sql = $@"
                            BEGIN TRANSACTION;
                                UPDATE [dbo].leavetransaction 
                                SET 
                                    [hr_manager_id] = '{user.currUserId}', 
                                    [hr_manager_edit_date] = CURRENT_TIMESTAMP,
                                    {comments}
                                    leave_type = '{typeOfLeaveTxt.Text}',
                                    days_taken = @DaysTaken, 
                                    qualified = { util.getSqlForCalculatingQualifiedField(typeOfLeaveTxt.Text, $"(SELECT employee_id FROM leavetransaction WHERE transaction_id = {leaveId})")}
                                    
                                WHERE transaction_id = {leaveId};
                                
                                {updateLeaveBalance}                                

                            COMMIT;
                        ";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            if (isSupCommentsChanged)
                            {
                                if (!util.isNullOrEmpty(supComment))
                                    command.Parameters.AddWithValue("@SupervisorComments", supComment);
                                else
                                    command.Parameters.AddWithValue("@SupervisorComments", DBNull.Value);
                            }

                            if (isHrCommentsChanged)
                            {
                                if (!util.isNullOrEmpty(hrComment))
                                    command.Parameters.AddWithValue("@HrComments", hrComment);
                                else
                                    command.Parameters.AddWithValue("@HrComments", DBNull.Value);
                            }

                            command.Parameters.AddWithValue("@DaysTaken", daysTaken);

                            int rowsAffected = command.ExecuteNonQuery();
                            isEditSuccessful = rowsAffected > 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //exception logic
                    isEditSuccessful = false;
                }

                if (areFilesUploaded)
                {
                    uploadedFilesIds = new List<string>();
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
                                        string fileid = string.Empty;
                                        using (SqlCommand command = new SqlCommand(sql, connection))
                                        {
                                            command.Parameters.AddWithValue("@FileData", bytes);
                                            command.Parameters.AddWithValue("@FileName", file_name);
                                            command.Parameters.AddWithValue("@FileExtension", file_extension);
                                            //.Parameters.AddWithValue("@UploadedOn", util.getCurrentDate());
                                            fileid = command.ExecuteScalar().ToString();

                                            isEditSuccessful = isEditSuccessful && !util.isNullOrEmpty(fileid);
                                        }

                                        if (!util.isNullOrEmpty(fileid))
                                        {
                                            // insert record into bridge entity which associates file(s) with a given employee
                                            sql = $@"
                                                INSERT INTO [dbo].[employeefiles] ([file_id],[employee_id],[leave_transaction_id])
                                                VALUES(@FileId, @EmployeeId, @TransactionId);";
                                            using (SqlCommand command = new SqlCommand(sql, connection))
                                            {
                                                command.Parameters.AddWithValue("@FileId", fileid);
                                                command.Parameters.AddWithValue("@EmployeeId", empIdHiddenTxt.Value);
                                                command.Parameters.AddWithValue("@TransactionId", leaveId);
                                                int rowsAffected = command.ExecuteNonQuery();
                                                isEditSuccessful = isEditSuccessful && rowsAffected > 0;
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
                            isEditSuccessful = false;
                        }
                    }
                }
                Page.MaintainScrollPositionOnPostBack = false;
                if (isEditSuccessful)
                {
                    successfulSubmitEditsMsgPanel.Visible = true;
                    successfulSubmitEditsMsgPanel.Focus();
                }

                else
                {
                    errorEditingApplicationPanel.Visible = true;
                    errorEditingApplicationPanel.Focus();
                }
                    

                
                // add audit log
                List<string> actionString = new List<string>();

                if (isSupCommentsChanged)
                    actionString.Add($" supervisor comment= '{supComment}'");
                if (isHrCommentsChanged)
                    actionString.Add($"hr comment= '{hrComment}'");
                if (isDaysTakenChanged)
                    actionString.Add($"Days taken= {daysTaken}");
                if (isLeaveTypeChanged)
                    actionString.Add($"Leave Type changed to= {typeOfLeaveTxt.Text}");
                if (areFilesUploaded)
                    actionString.Add($"Files uploaded: {String.Join(", ", uploadedFilesIds.Select(lb => "id= " + lb).ToArray())}");

                string action = $"Edited leave application; leave transaction id= {leaveId}, {String.Join(", ", actionString.ToArray())}";

                util.addAuditLog(user.currUserId, empIdHiddenTxt.Value, action);
            }
            else
            {
                Page.MaintainScrollPositionOnPostBack = false;
                noEditsMadePanel.Visible = true;
                noEditsMadePanel.Focus();
            }

            // hide submit button
            submitEditsBtn.Visible = false;
        }
        //________________________________________________________________________

        
        // METHODS FIRED WHEN FORM INPUTS CHANGE
        protected void supervisorSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            // validates supervisor whenever the user changes the supervisor value
            validateSupervisor(supervisorSelect.SelectedValue);
        }

        protected void datesEntered(object sender, EventArgs e)
        {
            // populates the value for the number of days applied for 
            populateDaysTaken();
        }

        protected void typeOfLeave_SelectedIndexChanged(object sender, EventArgs e)
        {
            validateDates(txtFrom.Text, txtTo.Text);
            validateLeave(typeOfLeave.SelectedValue);
            populateDaysTaken();
        }
        //________________________________________________________________________

        
        protected void refreshForm(object sender, EventArgs e)
        {
            // refreshes form by redirecting back to the page
            Response.Redirect($"~{Request.Url.PathAndQuery}");
        }

        protected void returnToPreviousBtn_Click(object sender, EventArgs e)
        {
            // returns to wherever is specified in the query string, returnUrl
            string returnUrl = Request.QueryString["returnUrl"].ToString().Replace(",","&returnUrl=");
            Response.Redirect($"{returnUrl}");
        }

        protected void convertToPaidLeave_Click(object sender, EventArgs e)
        {
            List<HttpPostedFile> files = uploadedFiles;
            if(files != null)
            {
                // convert
                typeOfLeaveTxt.Text = "Paid";
            }
            else
            {
                filesNeededForConversionToPaidPanel.Visible = true;
            }
        }



        //protected void disableForm()
        //{
        //    // start date and end date
        //    txtFrom.Enabled = txtTo.Enabled = typeOfLeave.Enabled = false;

        //    // employee comments
        //    empCommentsTxt.Disabled = true;
        //}

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