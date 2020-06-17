using HR_LEAVEv2.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Services;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.HR
{
    public partial class AllEmployees : System.Web.UI.Page
    {
        
        List<string> permissions = null;
        class EmpDetails
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

        protected void Page_Load(object sender, EventArgs e)
        {
            permissions = (List<string>)Session["permissions"];
            if (permissions == null || !(permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions" ) || permissions.Contains("hr3_permissions")))
                Response.Redirect("~/AccessDenied.aspx");

            if (!IsPostBack)
            {
                // persists option for which type of employees to view, active or inactive
                if (Session["viewForAllEmployees"] != null)
                {
                    employeeStatusDropDown.SelectedValue = Session["viewForAllEmployees"].ToString();
                    DataPager1.SetPageProperties(0, 8, false);
                }

                ViewState["viewActive"] = employeeStatusDropDown.SelectedValue == "Active";
                bindListView();
            }
        }

        protected void bindListView()
        {
            try
            {
                string sql;
                string isActive = "IN", activeLabel = "Active", bootstrapClass = "label-success";
                if (permissions.Contains("hr1_permissions"))
                {

                    if (ViewState["viewActive"] != null)
                    {
                        if(Convert.ToBoolean(ViewState["viewActive"]))
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
                            
                    }
                        

                    // HR 1
                    sql = $@"
                        SELECT
                            DISTINCT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email, '{activeLabel}' as isActive, 'label {bootstrapClass}' as bootstrapClass
                            FROM dbo.employee e

                            WHERE e.employee_id <> {Session["emp_id"].ToString()} AND e.employee_id {isActive} 
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
                    if (permissions.Contains("contract_permissions"))
                        emp_type.Add("'Contract'");
                    if (permissions.Contains("public_officer_permissions"))
                        emp_type.Add("'Public Service'");

                    if (ViewState["viewActive"] != null)
                    {
                        if (Convert.ToBoolean(ViewState["viewActive"]))
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
			                            WHERE hr_ep.employee_id = '{Session["emp_id"].ToString()}' 
		                            ) HR_INFO
		                          WHERE RowNum = 1) as 'is_hr_active'

                                FROM dbo.employee e

                                JOIN employeeposition ep
                                ON ep.employee_id = e.employee_id

                                WHERE ep.employee_id <> '{Session["emp_id"].ToString()}' AND ep.employee_id {isActive}
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

                        ListView1.DataSource = dt;
                        ListView1.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void ListView1_PagePropertiesChanging(object sender, PagePropertiesChangingEventArgs e)
        {
            // set current page startindex,max rows and rebind to false  
            DataPager1.SetPageProperties(e.StartRowIndex, e.MaximumRows, false);
            // Rebind the ListView1  
            bindListView();
        }

        protected void searchForEmployee(string searchStr)
        {
            try
            {

                string sql;
                string isActive = "IN", activeLabel = "Active", bootstrapClass = "label-success";
                if (permissions.Contains("hr1_permissions"))
                {
                    // HR 1
                    if (ViewState["viewActive"] != null)
                    {
                        if (Convert.ToBoolean(ViewState["viewActive"]))
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

                    }


                    // HR 1
                    sql = $@"
                        SELECT
                            DISTINCT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email, '{activeLabel}' as isActive, 'label {bootstrapClass}' as bootstrapClass
                            FROM dbo.employee e

                            WHERE e.employee_id <> {Session["emp_id"].ToString()} 
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
                    string emp_type = string.Empty;
                    // HR 2 and HR 3
                    if (permissions.Contains("contract_permissions"))
                        emp_type = "Contract";
                    else if (permissions.Contains("public_officer_permissions"))
                        emp_type = "Public Service";

                    if (ViewState["viewActive"] != null)
                    {
                        if (Convert.ToBoolean(ViewState["viewActive"]))
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
                    }

                    sql = $@"
                        SELECT
                            DISTINCT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email , '{activeLabel}' as isActive, 'label {bootstrapClass}' as bootstrapClass
                            FROM dbo.employee e
                            JOIN employeeposition ep
                            ON ep.employee_id = e.employee_id
                            WHERE e.employee_id <> {Session["emp_id"].ToString()} 
                                AND ep.employment_type='{emp_type}'
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
                        ListView1.DataSource = dt;
                        ListView1.DataBind();
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

        [WebMethod]
        public static string getEmpDetails(string emp_id)
        {
            /* returns JSON object containing all the employee details. Returns data including Employee Position and Employee Employment type but if the data is not available
             * then by default, only basic employee info is returned 
            */
            Util util = new Util();
            EmpDetails empDetails = null;
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
                return "ERROR";
            }

            if (empDetails == null)
            {
                try
                {
                    string sql = $@"
                        SELECT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email
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
                                        isCompleteRecord = '0'
                                    };
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return "ERROR";
                }
            }

            return JsonConvert.SerializeObject(empDetails);
        }

        protected void newEmployeeBtn_Click(object sender, EventArgs e)
        {
            // redirects to create new employee page
            Response.Redirect("~/HR/EmployeeDetails.aspx?mode=create");
        }

        protected void employeeStatusDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            // change the type of employees to view, Active or Inactive
            Session["viewForAllEmployees"] = employeeStatusDropDown.SelectedValue;
            ViewState["viewActive"] = employeeStatusDropDown.SelectedValue == "Active";
            DataPager1.SetPageProperties(0, 8, false);
            bindListView();
        }

        protected void openLeaveLogsBtn_Click(object sender, EventArgs e)
        {
            // opens employee's leave logs

            LinkButton lb = sender as LinkButton;
            string empEmail = lb.Attributes["empEmail"].ToString();
            Response.Redirect($"~/HR/AllEmployeeLeaveApplications?empEmail={empEmail}&returnUrl={HttpContext.Current.Request.Url.PathAndQuery}");
        }
    }
}