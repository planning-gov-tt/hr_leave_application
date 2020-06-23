using HR_LEAVEv2.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.HR
{
    public partial class AllEmployees : System.Web.UI.Page
    {
        User user = new User();

        // used to retrieve employee details for display in modal
        protected class EmpDetails
        {
            public char isCompleteRecord { get; set; }
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

        private string typeOfEmpToView
        {
            get { return Session["viewForAllEmployees"] != null ? Session["viewForAllEmployees"].ToString() : null; }
            set { Session["viewForAllEmployees"] = value; }
        }

        private Boolean viewActive
        {
            get { return typeOfEmpToView != null ? typeOfEmpToView == "Active" : true; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (user.permissions == null || (user.permissions != null && !(user.permissions.Contains("hr1_permissions") || user.permissions.Contains("hr2_permissions" ) || user.permissions.Contains("hr3_permissions"))))
                Response.Redirect("~/AccessDenied.aspx");

            if (!IsPostBack)
            {
                // persists option for which type of employees to view, active or inactive
                if (typeOfEmpToView != null)
                {
                    employeeStatusDropDown.SelectedValue = typeOfEmpToView;
                    employeesDataPager.SetPageProperties(0, 8, false);
                }

                bindListView();
            }
        }

        // LISTVIEW METHODS _________________________________________________________________________________________
        protected void bindListView()
        {
            try
            {
                string sql;
                string isActive = "IN", activeLabel = "Active", bootstrapClass = "label-success";
                if (user.permissions.Contains("hr1_permissions"))
                {

                    if(viewActive)
                    {
                        isActive = "IN";
                        activeLabel = "Active";
                        bootstrapClass = "label-success";
                    }
                    else
                    {
                        isActive = "NOT IN";
                        activeLabel = "Inactive";
                        bootstrapClass = "label-danger";
                    }
                        

                    // HR 1
                    sql = $@"
                        SELECT
                            DISTINCT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email, '{activeLabel}' as isActive, 'label {bootstrapClass}' as bootstrapClass
                            FROM dbo.employee e

                            WHERE e.employee_id <> {user.currUserId} AND e.employee_id {isActive} 
                            (
                                select ep.employee_id
                                from dbo.employeeposition ep
                                where ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date)
                                group by ep.employee_id
                                having count(*) > 0
                            ) 
                    ";
                    
                }
                else
                {
                    List<string> emp_type = new List<string>();
                    // HR 2 and HR 3
                    if (user.permissions.Contains("contract_permissions"))
                        emp_type.Add("'Contract'");
                    if (user.permissions.Contains("public_officer_permissions"))
                        emp_type.Add("'Public Service'");

                    if (viewActive)
                    {
                        isActive = "IN";
                        activeLabel = "Active";
                        bootstrapClass = "label-success";
                    }
                    else
                    {
                        isActive = "NOT IN";
                        activeLabel = "Inactive";
                        bootstrapClass = "label-danger";
                    }

                    // the following sql ensures that out of all an employees employment records, only the most recent one (the most recently ended) is considered for if the employee can be accessed
                    // by the current HR.

                    /* Partition and row_number is used to separate the data by employee id since employeeposition table can have multiple records from the same employee. After separating the data by employee, the employment 
                     * records are ordered by actual end date, effectively creating a sorted list with the most recent record at row number = 1. This will allow HR to be able to view the relevant employees 
                     * (Contract or Public Service) regardless of whether they are active or not.
                     */
                    sql = $@"
                        SELECT *
                        FROM(
                            SELECT
                                ROW_NUMBER() OVER(PARTITION BY ep.employee_id ORDER BY ISNULL(ep.actual_end_date, CAST('1/1/9999' AS DATE)) DESC) as RowNum,
                                e.employee_id, 
                                e.ihris_id, 
                                e.first_name + ' ' + e.last_name as 'Name', 
                                e.email, 
                                ep.employment_type,
                                '{activeLabel}' as isActive, 
                                'label {bootstrapClass}' as bootstrapClass,

                                -- check to ensure HR is active
	                            (SELECT IIF(start_date <= GETDATE() AND (actual_end_date IS NULL OR GETDATE() <= actual_end_date), 'Yes', 'No')
		                            FROM (
			                            SELECT ROW_NUMBER() OVER(PARTITION BY hr_ep.employee_id ORDER BY ISNULL(hr_ep.actual_end_date, CAST('1/1/9999' AS DATE)) DESC) as RowNum, hr_ep.start_date, hr_ep.actual_end_date
			                            FROM dbo.employeeposition hr_ep
			                            WHERE hr_ep.employee_id = '{user.currUserId}' 
		                            ) HR_INFO
		                          WHERE RowNum = 1) as 'is_hr_active'

                                FROM dbo.employee e

                                JOIN employeeposition ep
                                ON ep.employee_id = e.employee_id

                                WHERE ep.employee_id <> '{user.currUserId}' AND ep.employee_id {isActive}
                                (
                                    select ep.employee_id
                                    from dbo.employeeposition ep
                                    where ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date)
                                    group by ep.employee_id
                                    having count(*) > 0
                                ) 
                        ) Employees

                        WHERE RowNum = 1 AND employment_type IN ({String.Join(", ", emp_type.ToArray())}) AND Employees.is_hr_active = 'Yes';
                    ";
                }

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

                string sql;
                string isActive = "IN", activeLabel = "Active", bootstrapClass = "label-success";
                if (user.permissions.Contains("hr1_permissions"))
                {
                    // HR 1

                    if (viewActive)
                    {
                        isActive = "IN";
                        activeLabel = "Active";
                        bootstrapClass = "label-success";
                    }
                    else
                    {
                        isActive = "NOT IN";
                        activeLabel = "Inactive";
                        bootstrapClass = "label-danger";
                    }


                    // HR 1
                    sql = $@"
                        SELECT
                            DISTINCT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email, '{activeLabel}' as isActive, 'label {bootstrapClass}' as bootstrapClass
                            FROM dbo.employee e

                            WHERE e.employee_id <> {user.currUserId} 
                                AND ((e.employee_id LIKE '@SearchString') OR (e.ihris_id LIKE @SearchString) OR (e.first_name LIKE @SearchString) OR (e.last_name LIKE @SearchString) OR (e.email LIKE @SearchString)) 
                                AND e.employee_id {isActive} 
                                (
                                    select ep.employee_id
                                    from dbo.employeeposition ep
                                    where ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date)
                                    group by ep.employee_id
                                    having count(*) > 0
                                ) 
                    ";
                }
                else
                {
                    List<string> emp_type = new List<string>();
                    // HR 2 and HR 3
                    if (user.permissions.Contains("contract_permissions"))
                        emp_type.Add("Contract");
                    if (user.permissions.Contains("public_officer_permissions"))
                        emp_type.Add("Public Service");

                    if (viewActive)
                    {
                        isActive = "IN";
                        activeLabel = "Active";
                        bootstrapClass = "label-success";
                    }
                    else
                    {
                        isActive = "NOT IN";
                        activeLabel = "Inactive";
                        bootstrapClass = "label-danger";
                    }

                    sql = $@"
                        SELECT
                            DISTINCT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email , '{activeLabel}' as isActive, 'label {bootstrapClass}' as bootstrapClass
                            FROM dbo.employee e
                            JOIN employeeposition ep
                            ON ep.employee_id = e.employee_id
                            WHERE e.employee_id <> {user.currUserId} 
                                AND ep.employment_type IN ({String.Join(", ", emp_type.ToArray())})
                                AND ((e.employee_id LIKE '@SearchString') OR (e.ihris_id LIKE @SearchString) OR (e.first_name LIKE @SearchString) OR (e.last_name LIKE @SearchString) OR (e.email LIKE @SearchString)) 
                                AND e.employee_id {isActive} 
                                (
                                    select ep.employee_id
                                    from dbo.employeeposition ep
                                    where ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date)
                                    group by ep.employee_id
                                    having count(*) > 0
                                ) 
                    ";
                }

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


        // ADD NEW EMPLOYEE _________________________________________________________________________________________
        protected void newEmployeeBtn_Click(object sender, EventArgs e)
        {
            // redirects to create new employee page
            Response.Redirect("~/HR/EmployeeDetails.aspx?mode=create");
        }
        // END ADD NEW EMPLOYEE _________________________________________________________________________________________


        // CHANGE EMPLOYEES TO VIEW (INACTIVE OR ACTIVE)_________________________________________________________________________________________
        protected void employeeStatusDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            // change the type of employees to view, Active or Inactive
            typeOfEmpToView = employeeStatusDropDown.SelectedValue;
            employeesDataPager.SetPageProperties(0, 8, false);
            bindListView();
        }
        // END CHANGE EMPLOYEES TO VIEW (INACTIVE OR ACTIVE)_________________________________________________________________________________________


        // EMPLOYEE DETAILS METHODS_________________________________________________________________________________________
        protected EmpDetails getEmpDetails(string emp_id)
        {
            /* returns EmpDetails object containing all the employee details. Returns data including Employee Position and Employee Employment type but if the data is not available
             * then by default, only basic employee info is returned 
            */
            Util util = new Util();
            EmpDetails empDetails = null;

            if (viewActive)
            {
                try
                {
                    string sql = $@"
                    SELECT TOP 1 e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email, e.vacation, e.personal, e.casual, e.sick, ep.employment_type, ep.start_date, p.pos_name
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
                                            position = reader["pos_name"].ToString(),
                                            isCompleteRecord = '1'
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
            }
            
            // loads basic employee info including all leave balances if employee is inactive
            if (empDetails == null)
            {
                try
                {
                    string sql = $@"
                        SELECT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email, e.vacation, e.personal, e.casual, e.sick
                        FROM [dbo].[employee] e
                        WHERE e.employee_id = {emp_id};
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
                                        isCompleteRecord = '0'
                                    };

                                    //display sick
                                    string leaveBalancesDetailsStr = $@"
                                            <div>
                                                 <h4 style = 'display: inline'>Sick Leave Balance:</h4>
                                                 <span> {empDetails.sick} </span>
                                            </div>
                                        ";

                                    //display personal
                                    leaveBalancesDetailsStr += $@"
                                            <div>
                                                <h4 style='display: inline'>Personal Leave Balance:</h4>
                                                <span>{empDetails.personal}</span>
                                            </div>
                                            ";
                                       
                                    // display vacation 
                                    leaveBalancesDetailsStr += $@"
                                            <div>
                                                <h4 style='display: inline'>Vacation Leave Balance:</h4>
                                                <span>{empDetails.vacation}</span>
                                            </div>
                                            ";
                                       
                                    // display casual
                                    leaveBalancesDetailsStr += $@"
                                            <div>
                                                <h4 style='display: inline'>Casual Leave Balance:</h4>
                                                <span>{empDetails.casual}</span>
                                            </div>
                                            ";

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
            }

            return empDetails;
        }
        protected void openDetailsBtn_Click(object sender, EventArgs e)
        {
            // populate modal and show employee details

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
                if (details.isCompleteRecord == '1')
                {
                    errorPanel.Visible = false;
                    positionDetails.Visible = true;
                    empPositionDetails.InnerText = details.position;
                    empTypeDetails.InnerText = details.employment_type;
                }
                else if (details.isCompleteRecord == '0')
                {
                    errorPanel.Visible = true;
                    positionDetails.Visible = false;
                }
            }
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#empDetailsModal').modal({'show':true});", true);
        }
        // END EMPLOYEE DETAILS METHODS_________________________________________________________________________________________


        // OPEN LEAVE LOGS_________________________________________________________________________________________
        protected void openLeaveLogsBtn_Click(object sender, EventArgs e)
        {
            // opens employee's leave logs

            LinkButton lb = sender as LinkButton;
            string empEmail = lb.Attributes["empEmail"].ToString();
            Response.Redirect($"~/HR/AllEmployeeLeaveApplications?empEmail={empEmail}&returnUrl={HttpContext.Current.Request.Url.PathAndQuery}");
        }
        // END OPEN LEAVE LOGS_________________________________________________________________________________________


    }
}