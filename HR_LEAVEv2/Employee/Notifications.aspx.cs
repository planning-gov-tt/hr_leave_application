using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.Employee
{
    public partial class Notifications : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["permissions"] == null)
                Response.Redirect("~/AccessDenied.aspx");

            if (!IsPostBack)
            {
                bindListView();
            }
        }

        protected void resetNumNotifications()
        {
            // set number of notifications
            string count = string.Empty;
            try
            {
                string sql = $@"
                        SELECT COUNT([is_read]) AS 'num_notifs' FROM [dbo].[notifications] where [is_read] = 'No' AND [employee_id] = '{Session["emp_id"]}';
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
                                count = reader["num_notifs"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Label num_notifs = (Label)Master.FindControl("num_notifications");
            num_notifs.Text = count;
        }

        protected void bindListView()
        {
            try
            {
                string sql= $@"
                    SELECT [id],
                           [notification_header], 
                           [notification], 
                           IIF([is_read] = 'No', 'Unread', 'Read') AS status,
                           IIF([is_read] = 'No', 'label-primary', 'label-default') AS bootstrap_class,
                           [is_read], 
                           [created_at] 
                    FROM [dbo].[notifications] 
                    WHERE [employee_id] = '{Session["emp_id"].ToString()}'
                    ORDER BY [created_at] DESC;
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

            resetNumNotifications();
        }

        protected void ListView1_PagePropertiesChanging(object sender, PagePropertiesChangingEventArgs e)
        {
            // set current page startindex,max rows and rebind to false  
            DataPager1.SetPageProperties(e.StartRowIndex, e.MaximumRows, false);
            // Rebind the ListView1  
            bindListView();
        }

        protected void readBtn_Click(object sender, EventArgs e)
        {
            LinkButton btn = sender as LinkButton;
            string id = btn.Attributes["data-id"].ToString();
            try
            {
                string sql = $@"
                    UPDATE [dbo].[notifications] 
                    SET [is_read] = 'Yes'
                    WHERE [id] = '{id}';
                ";

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                        bindListView();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        protected void unreadBtn_Click(object sender, EventArgs e)
        {
            LinkButton btn = sender as LinkButton;
            string id = btn.Attributes["data-id"].ToString();
            try
            {
                string sql = $@"
                    UPDATE [dbo].[notifications] 
                    SET [is_read] = 'No'
                    WHERE [id] = '{id}';
                ";

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                        bindListView();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void deleteBtn_Click(object sender, EventArgs e)
        {
            LinkButton btn = sender as LinkButton;
            string id = btn.Attributes["data-id"].ToString();
            try
            {
                string sql = $@"
                    DELETE FROM [dbo].[notifications] 
                    WHERE [id] = '{id}';
                ";

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                        bindListView();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}