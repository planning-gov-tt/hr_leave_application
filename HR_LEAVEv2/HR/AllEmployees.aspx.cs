using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
                bindListView();
            }
        }

        protected void bindListView()
        {
            try
            {
                string sql;

                if (permissions.Contains("hr1_permissions"))
                {
                    // HR 1
                    sql = $@"
                        SELECT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email
                        FROM [dbo].[employee] e
                        WHERE e.employee_id != {Session["emp_id"].ToString()}
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

                    sql = $@"
                        SELECT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email
                        FROM [dbo].[employee] e
                        LEFT JOIN [dbo].employeeposition ep
                        ON e.employee_id = ep.employee_id
                        WHERE ep.employment_type='{emp_type}' AND GETDATE()>=ep.start_date AND GETDATE()<=ep.expected_end_date;
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
                Console.WriteLine(ex.Message);
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

                if (permissions.Contains("hr1_permissions"))
                {
                    // HR 1
                    sql = $@"
                        SELECT DISTINCT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email
                        FROM [dbo].[employee] e
                        LEFT JOIN [dbo].employeeposition ep
                        ON e.employee_id = ep.employee_id
                        WHERE
                         ((e.employee_id LIKE '@SearchString') OR (e.ihris_id LIKE @SearchString) OR (e.first_name LIKE @SearchString) OR (e.last_name LIKE @SearchString) OR (e.email LIKE @SearchString) ); 
                    ";
                }
                else
                {
                    string emp_type = string.Empty;
                    // HR 1 and HR 2
                    if (permissions.Contains("contract_permissions"))
                        emp_type = "Contract";
                    else if (permissions.Contains("public_officer_permissions"))
                        emp_type = "Public Service";

                    sql = $@"
                        SELECT DISTINCT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email
                        FROM [dbo].[employee] e
                        LEFT JOIN [dbo].employeeposition ep
                        ON e.employee_id = ep.employee_id
                        WHERE ep.employment_type='{emp_type}' AND 
                        ((e.employee_id LIKE '@SearchString') OR (e.ihris_id LIKE @SearchString) OR (e.first_name LIKE @SearchString) OR (e.last_name LIKE @SearchString) OR (e.email LIKE @SearchString) ); 

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
                Console.WriteLine(ex.Message);
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
            try
            {
                string sql = $@"
                        SELECT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email,e.vacation,e.personal, e.casual, e.sick, ep.employment_type, p.pos_name
                        FROM [dbo].[employee] e
                        RIGHT JOIN [dbo].employeeposition ep
                        ON e.employee_id = ep.employee_id

                        INNER JOIN [dbo].position p
                        ON ep.position_id = p.pos_id
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
                                    employment_type = reader["employment_type"].ToString(),
                                    position = reader["pos_name"].ToString(),
                                    isCompleteRecord = '1'   
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

            if (empDetails == null)
            {
                empDetails = null;
                try
                {
                    string sql = $@"
                        SELECT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email,e.vacation,e.personal, e.casual, e.sick
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
            Response.Redirect("~/HR/EmployeeDetails.aspx?view=create");
        }
    }
}