using HR_LEAVEv2.Classes;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.Employee
{
    public partial class Notifications : System.Web.UI.Page
    {
        User user = new User();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["permissions"] == null)
                Response.Redirect("~/AccessDenied.aspx");

            if (!IsPostBack)
                bindListView();
           
        }

        protected void resetNumNotifications()
        {
            // resets number of notifications for current user
            Util util = new Util();

            Label numNotifs = (Label)Master.FindControl("num_notifications");
            int numUnread = Convert.ToInt32(util.resetNumNotifications(user.currUserId));
            markAllAsRead.Visible = numUnread > 0;
            numNotifs.Text = numUnread.ToString();
            System.Web.UI.UpdatePanel up = (System.Web.UI.UpdatePanel)Master.FindControl("notificationsUpdatePanel");
            up.Update();
        }

        protected void bindListView()
        {
            try
            {
                string sql= $@"
                    SELECT n.[id],
                           n.[notification_header], 
                           n.[notification], 
                           IIF(n.[is_read] = 'No', 'Unread', 'Read') AS status,
                           IIF(n.[is_read] = 'No', 'label-primary', 'label-default') AS bootstrap_class,
                           n.[is_read], 
                           FORMAT(n.[created_at],  'h:mmtt MMM dd yyyy') AS created_at
                    FROM [dbo].[notifications] n
                    WHERE n.[employee_id] = '{user.currUserId}'
                    ORDER BY n.[created_at] DESC;
                ";

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        SqlDataAdapter ad = new SqlDataAdapter(command);

                        DataTable dt = new DataTable();
                        ad.Fill(dt);

                        DataRow[] numRead = dt.Select("is_read = 'Yes'");
                        markAllAsUnread.Visible = numRead.Length > 0;
                        deleteAllNotifsBtn.Visible = dt.Rows.Count > 0;

                        notificationsListView.DataSource = dt;
                        notificationsListView.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            resetNumNotifications();
        }

        protected void notificationsListView_PagePropertiesChanging(object sender, PagePropertiesChangingEventArgs e)
        {
            // set current page startindex, max rows and rebind to false  
            notificationsDataPager.SetPageProperties(e.StartRowIndex, e.MaximumRows, false);
            // Rebind the notificationsListView  
            bindListView();
        }

        protected void readBtn_Click(object sender, EventArgs e)
        {
            // sets an individual notification to read
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
            // sets an individual notification to unread

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
            // deletes an individual notification

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

        protected void deleteAllNotifsBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = $@"
                    DELETE FROM [dbo].[notifications] 
                    WHERE [employee_id] = '{user.currUserId}';
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

        protected void markAllAsRead_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = $@"
                    UPDATE [dbo].[notifications] 
                    SET [is_read] = 'Yes'
                    WHERE [employee_id] = '{user.currUserId}';
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

        protected void markAllAsUnread_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = $@"
                    UPDATE [dbo].[notifications] 
                    SET [is_read] = 'No'
                    WHERE [employee_id] = '{user.currUserId}';
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