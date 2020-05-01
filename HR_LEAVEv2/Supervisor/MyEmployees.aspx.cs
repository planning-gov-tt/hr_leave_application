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

namespace HR_LEAVEv2.Supervisor
{
    public partial class MyEmployees : System.Web.UI.Page
    {
        class EmpDetails
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

        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> permissions = (List<string>)Session["permissions"];
            if (permissions == null || !permissions.Contains("sup_permissions"))
                Response.Redirect("~/AccessDenied.aspx");

            if (!IsPostBack)
            {
                bindListView();
            }
        }

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
			                    WHERE sup_ep.employee_id = '{Session["emp_id"].ToString()}' 
		                    ) SUP_INFO
		                    WHERE RowNum = 1
	                    ) as 'is_sup_active'

	                    FROM dbo.employee e
                    ) Employees

                    JOIN [dbo].assignment a
                    ON Employees.employee_id = a.supervisee_id

                    WHERE Employees.is_sup_active = 'Yes' AND a.supervisor_id = '{Session["emp_id"].ToString()}' AND Employees.employee_id IN
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
                string sql = $@"
                        SELECT
                            DISTINCT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email
                            FROM dbo.employee e
                            JOIN [dbo].assignment a
                            ON e.employee_id = a.supervisee_id
                            WHERE a.supervisor_id = {Session["emp_id"].ToString()} 
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
                return "ERROR";
            }

            return JsonConvert.SerializeObject(empDetails);
        }

        protected void openLeaveLogsBtn_ServerClick(object sender, EventArgs e)
        {
            // opens employee's leave logs

            LinkButton lb = sender as LinkButton;
            string empEmail = lb.Attributes["empEmail"].ToString();
            Response.Redirect($"~/Supervisor/MyEmployeeLeaveApplications?empEmail={empEmail}&returnUrl={HttpContext.Current.Request.Url.PathAndQuery}");
        }
    }
}