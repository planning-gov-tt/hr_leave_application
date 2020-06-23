using HR_LEAVEv2.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.Supervisor
{
    public partial class MyEmployees : System.Web.UI.Page
    {
        Util util = new Util();
        User user = new User();

        // used to retrieve employee details for display in modal
        protected class EmpDetails
        {
            public string emp_id { get; set; }
            public string ihris_id { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public string vacation { get; set; }
            public string personal { get; set; }
            public string casual { get; set; }
            public string sick { get; set; }
            public string leave_balances { get; set; }
            public DateTime start_date { get; set; }
            public string employment_type { get; set; }
            public string position { get; set; }
        };

        private string empEmailToAlert
        {
            get { return ViewState["empEmailToAlert"] != null ? ViewState["empEmailToAlert"].ToString() : null; }
            set { ViewState["empEmailToAlert"] = value; }
        }

        private string empIdToAlert
        {
            get { return ViewState["empIdToAlert"] != null ? ViewState["empIdToAlert"].ToString() : null; }
            set { ViewState["empIdToAlert"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!user.permissions.Contains("sup_permissions"))
                Response.Redirect("~/AccessDenied.aspx");

            if (!IsPostBack)
            {
                bindListView();
            }
        }

        // LISTVIEW METHODS _________________________________________________________________________________________
        protected void bindListView()
        {
            try
            {
                // the following sql gets all employees that are supervised by the current supervisor and that are currently active
                string sql = $@"
                    SELECT *
                    FROM (
	                    SELECT
	                    e.employee_id, 
	                    e.ihris_id, 
	                    e.first_name + ' ' + e.last_name as 'Name', 
	                    e.email,
	                    (
		                    SELECT IIF(start_date <= GETDATE() AND (actual_end_date IS NULL OR GETDATE() <= actual_end_date), 'Yes', 'No')
		                    FROM (
			                    SELECT ROW_NUMBER() OVER(PARTITION BY sup_ep.employee_id ORDER BY ISNULL(sup_ep.actual_end_date, CAST('1/1/9999' AS DATE)) DESC) as RowNum, sup_ep.actual_end_date, sup_ep.start_date
			                    FROM dbo.employeeposition sup_ep
			                    WHERE sup_ep.employee_id = '{user.currUserId}' 
		                    ) SUP_INFO
		                    WHERE RowNum = 1
	                    ) as 'is_sup_active'

	                    FROM dbo.employee e
                    ) Employees

                    JOIN [dbo].assignment a
                    ON Employees.employee_id = a.supervisee_id

                    WHERE Employees.is_sup_active = 'Yes' AND a.supervisor_id = '{user.currUserId}' AND Employees.employee_id IN
                    (
                        select ep.employee_id
                        from dbo.employeeposition ep
                        where ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date)
                        group by ep.employee_id
                        having count(*) > 0
                    ) 
                    ";
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        SqlDataAdapter ad = new SqlDataAdapter(command);

                        DataTable dt = new DataTable();
                        ad.Fill(dt);

                        employeesListView.DataSource = dt;
                        employeesListView.DataBind();

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void employeesListView_PagePropertiesChanging(object sender, PagePropertiesChangingEventArgs e)
        {
            // set current page startindex,max rows and rebind to false  
            employeesDataPager.SetPageProperties(e.StartRowIndex, e.MaximumRows, false);
            // Rebind the employeesListView  
            bindListView();
        }
        // END LISTVIEW METHODS _________________________________________________________________________________________

        
        // SEARCH METHODS _________________________________________________________________________________________
        protected void searchForEmployee(string searchStr)
        {
            try
            {
                string sql = $@"
                        SELECT
                            DISTINCT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email
                            FROM dbo.employee e
                            JOIN [dbo].assignment a
                            ON e.employee_id = a.supervisee_id
                            WHERE a.supervisor_id = {user.currUserId} 
                                AND ((e.employee_id LIKE '@SearchString') OR (e.ihris_id LIKE @SearchString) OR (e.first_name LIKE @SearchString) OR (e.last_name LIKE @SearchString) OR (e.email LIKE @SearchString))
                                AND e.employee_id IN
                                (
                                    select ep.employee_id
                                    from dbo.employeeposition ep
                                    where ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date)
                                    group by ep.employee_id
                                    having count(*) > 0
                                ) 
                    ";

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@SearchString", "%" + searchStr + "%");
                        SqlDataAdapter ad = new SqlDataAdapter(command);

                        DataTable dt = new DataTable();
                        ad.Fill(dt);
                        employeesListView.DataSource = dt;
                        employeesListView.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void searchBtn_Click(object sender, EventArgs e)
        {
            searchForEmployee(searchTxtbox.Text);
        }

        protected void searchTxtbox_TextChanged(object sender, EventArgs e)
        {
            searchForEmployee(searchTxtbox.Text);
        }
        // END SEARCH METHODS _________________________________________________________________________________________


        // OPEN DETAILS METHODS _________________________________________________________________________________________
        protected EmpDetails getEmpDetails(string emp_id)
        {
            /* returns EmpDetails object containing all the employee details. Since only active employees are shown, the top employment record will correspond to the most recent and relevant info
            */
            EmpDetails empDetails = null;
            Util util = new Util();
            try
            {
                string sql = $@"
                        SELECT TOP 1 e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email,e.vacation,e.personal, e.casual, e.sick, ep.employment_type, ep.start_date, p.pos_name
                        FROM [dbo].[employee] e

                        RIGHT JOIN [dbo].employeeposition ep
                        ON e.employee_id = ep.employee_id

                        INNER JOIN [dbo].position p
                        ON ep.position_id = p.pos_id

                        WHERE e.employee_id = {emp_id}
                        ORDER BY ISNULL(ep.actual_end_date, CAST('1/1/9999' AS DATE)) DESC;
                    ";

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    empDetails = new EmpDetails
                                    {
                                        emp_id = reader["employee_id"].ToString(),
                                        ihris_id = reader["ihris_id"].ToString(),
                                        name = reader["name"].ToString(),
                                        email = reader["email"].ToString(),
                                        vacation = reader["vacation"].ToString(),
                                        personal = reader["personal"].ToString(),
                                        casual = reader["casual"].ToString(),
                                        sick = reader["sick"].ToString(),
                                        employment_type = reader["employment_type"].ToString(),
                                        start_date = (DateTime)reader["start_date"],
                                        position = reader["pos_name"].ToString()
                                    };
                                }

                                string leaveBalancesDetailsStr = $@"
                                <div>
                                     <h4 style = 'display: inline'>Sick Leave Balance:</h4>
                                     <span> {empDetails.sick} </span>
                                </div>
                            ";
                                // set appropriate leave balance details based on employment type
                                if (empDetails.employment_type == "Contract")
                                {
                                    // is employee in first 11 months of contract?
                                    DateTime startDate = empDetails.start_date;
                                    DateTime elevenMonthsFromStartDate = startDate.AddMonths(11);

                                    if (DateTime.Compare(util.getCurrentDateToday(), elevenMonthsFromStartDate) < 0)
                                        // display personal 
                                        leaveBalancesDetailsStr += $@"
                                        <div>
                                            <h4 style='display: inline'>Personal Leave Balance:</h4>
                                            <span>{empDetails.personal}</span>
                                        </div>
                                     ";
                                    else
                                    {
                                        // display vacation 
                                        leaveBalancesDetailsStr += $@"
                                        <div>
                                            <h4 style='display: inline'>Vacation Leave Balance:</h4>
                                            <span>{empDetails.vacation}</span>
                                        </div>
                                     ";
                                    }

                                }
                                else if (empDetails.employment_type == "Public Service")
                                {
                                    // public service employees- show casual and vacation
                                    leaveBalancesDetailsStr += $@"
                                        <div>
                                            <h4 style='display: inline'>Casual Leave Balance:</h4>
                                            <span>{empDetails.casual}</span>
                                        </div>
                                        <div>
                                            <h4 style='display: inline'>Vacation Leave Balance:</h4>
                                            <span>{empDetails.vacation}</span>
                                        </div>
                                     ";
                                }

                                empDetails.leave_balances = leaveBalancesDetailsStr;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return empDetails;
        }
        protected void openDetailsBtn_Click(object sender, EventArgs e)
        {
            LinkButton lb = sender as LinkButton;
            string empId = lb.Attributes["emp_id"].ToString();

            // get details
            EmpDetails details = getEmpDetails(empId);

            if (details != null)
            {
                empNameDetails.InnerText = details.name;
                empIdDetails.InnerText = details.emp_id;
                ihrisIdDetails.InnerText = details.ihris_id;
                emailDetails.InnerText = details.email;
                leaveBalancesDetails.InnerHtml = details.leave_balances;
                empPositionDetails.InnerText = details.position;
                empTypeDetails.InnerText = details.employment_type;
            }
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#empDetailsModal').modal({'show':true});", true);
        }
        // END OPEN DETAILS METHODS _________________________________________________________________________________________


        // OPEN LEAVE LOGS _________________________________________________________________________________________
        protected void openLeaveLogsBtn_ServerClick(object sender, EventArgs e)
        {
            // opens employee's leave logs

            LinkButton lb = sender as LinkButton;
            string empEmail = lb.Attributes["empEmail"].ToString();
            Response.Redirect($"~/Supervisor/MyEmployeeLeaveApplications?empEmail={empEmail}&returnUrl={HttpContext.Current.Request.Url.PathAndQuery}");
        }
        // END OPEN LEAVE LOGS _________________________________________________________________________________________


        // REMIND EMPLOYEE TO SUBMIT LEAVE METHODS _________________________________________________________________________________________
        protected void clearSubmitAlertValidationErrors()
        {
            invalidStartDatePanel.Style.Add("display", "none");
            startDateIsWeekendPanel.Style.Add("display", "none");
            startDateIsHoliday.Style.Add("display", "none");
            invalidEndDatePanel.Style.Add("display", "none");
            endDateIsWeekendPanel.Style.Add("display", "none");
            endDateIsHoliday.Style.Add("display", "none");
            dateComparisonPanel.Style.Add("display", "none");
            successfulAlert.Style.Add("display", "none");
            unsuccessfulAlert.Style.Add("display", "none");
            unsuccessfulEmailAlertPanel.Style.Add("display", "none");
        }

        protected Boolean validateDates(string startDate, string endDate)
        {
            DateTime start, end;
            start = end = DateTime.MinValue;
            Boolean isValidated = true;
            List<string> holidaysInBetween = new List<string>();

            // validate start date is a date
            if (!DateTime.TryParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start))
            {
                invalidStartDatePanel.Style.Add("display", "inline-block");
                isValidated = false;
            }
            else
            {
                // ensure start date is not a holiday
                holidaysInBetween = util.getHolidaysBetween(start, start);
                if (holidaysInBetween.Count > 0)
                {
                    startDateIsHolidayTxt.InnerText = $"Start date cannot be on {holidaysInBetween.ElementAt(0)}";
                    startDateIsHoliday.Style.Add("display", "inline-block");
                    isValidated = false;
                }
               
                // ensure start date is not a weekend
                if (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday)
                {
                    startDateIsWeekendPanel.Style.Add("display", "inline-block");
                    isValidated = false;
                }
            }

            // validate end date is a date
            if (!DateTime.TryParseExact(endDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out end))
            {
                invalidEndDatePanel.Style.Add("display", "inline-block");
                isValidated = false;
            }
            else
            {
                // ensure end date is not a holiday
                holidaysInBetween = util.getHolidaysBetween(end, end);
                if (holidaysInBetween.Count > 0)
                {
                    endDateIsHolidayTxt.InnerText = $"End date cannot be on {holidaysInBetween.ElementAt(0)}";
                    endDateIsHoliday.Style.Add("display", "inline-block");
                    isValidated = false;
                }

                // ensure end date is not weekend
                if (end.DayOfWeek == DayOfWeek.Saturday || end.DayOfWeek == DayOfWeek.Sunday)
                {
                    endDateIsWeekendPanel.Style.Add("display", "inline-block");
                    isValidated = false;
                }

            }

            if (start != DateTime.MinValue && end != DateTime.MinValue)
            {
                // compare dates to ensure end date is not before start date
                if (DateTime.Compare(start, end) > 0)
                {
                    dateComparisonPanel.Style.Add("display", "inline-block");
                    isValidated = false;
                }
            }
            
            return isValidated;
        }

        protected void datesEntered(object sender, EventArgs e)
        {
            clearSubmitAlertValidationErrors();
            validateDates(txtFrom.Text, txtTo.Text);
        }

        
        protected void submitLeaveAlertBtn_Click(object sender, EventArgs e)
        {
            // shows modal where actual dates for employee to be reminded are entered

            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#submitLeaveAlertModal').modal({'show':true, 'backdrop':'static', 'keyboard':false});", true);
            LinkButton lb = sender as LinkButton;
            empEmailToAlert = lb.Attributes["empEmail"].ToString();
            empIdToAlert = lb.Attributes["empId"].ToString();

            clearSubmitAlertValidationErrors();
            txtFrom.Text = txtTo.Text = string.Empty;
        }

        protected void closeSubmitLeaveAlertModal(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#submitLeaveAlertModal').modal('hide');", true);
        }

        protected void submitAlertToSubmitLeaveBtn_Click(object sender, EventArgs e)
        {
            string startDate = txtFrom.Text,
                   endDate = txtTo.Text;

            if (empEmailToAlert != null && empIdToAlert != null && validateDates(startDate, endDate))
            {
                Boolean isAlertSentSuccessful = false;

                // send inhouse alert via notif
                string notif_header = $"Leave Application Requested",
                       notification = $"This is a reminder from {user.currUserName} to submit a leave application for the period of {startDate} to {endDate}";
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();

                        string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', '{notification}', 'No', '{empIdToAlert}', '{util.getCurrentDate()}');
                            ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            int rowsAffected = command.ExecuteNonQuery();
                            isAlertSentSuccessful = rowsAffected > 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                // send mail message
                MailMessage message = util.getAlertEmpToSubmitLeave(
                    new Util.EmailDetails
                    {
                        supervisor_name = user.currUserName,
                        start_date = startDate,
                        end_date = endDate,
                        subject = $"Reminder to Submit Leave Application",
                        recipient = empEmailToAlert
                    }
                );

                if (!util.sendMail(message))
                {
                    unsuccessfulEmailAlertPanel.Style.Add("display", "inline-block");
                }

                if (isAlertSentSuccessful)
                    successfulAlert.Style.Add("display", "inline-block");
                else
                    unsuccessfulAlert.Style.Add("display", "inline-block");
            }
            else
                unsuccessfulAlert.Style.Add("display", "inline-block");
        }
        // END REMIND EMPLOYEE TO SUBMIT LEAVE METHODS _________________________________________________________________________________________


    }
}