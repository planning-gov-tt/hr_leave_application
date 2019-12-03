using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.Supervisor
{
    public partial class MyEmployees : System.Web.UI.Page
    {

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
                string sql = $@"
                        SELECT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email
                        FROM [HRLeaveTestDb].[dbo].[employee] e
                        JOIN [HRLeaveTestDb].[dbo].assignment a
                        ON e.employee_id = a.supervisee_id
                        WHERE a.supervisor_id = {Session["emp_id"].ToString()};
                    ";

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        SqlDataAdapter ad = new SqlDataAdapter(command);

                        DataTable dt = new DataTable();
                        ad.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            ListView1.DataSource = dt;
                            ListView1.DataBind();
                        }
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
                string sql = $@"
                        SELECT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email
                        FROM [HRLeaveTestDb].[dbo].[employee] e
                        JOIN [HRLeaveTestDb].[dbo].assignment a
                        ON e.employee_id = a.supervisee_id
                        WHERE a.supervisor_id = {Session["emp_id"].ToString()}
                            AND
                        ((e.employee_id LIKE '{searchStr}%') OR (e.ihris_id LIKE '{searchStr}%') OR (e.first_name LIKE '{searchStr}%') OR (e.last_name LIKE '{searchStr}%') OR (e.email LIKE '{searchStr}%') ); 
                    ";

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        SqlDataAdapter ad = new SqlDataAdapter(command);

                        DataTable dt = new DataTable();
                        ad.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            ListView1.DataSource = dt;
                            ListView1.DataBind();
                        }
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
    }
}