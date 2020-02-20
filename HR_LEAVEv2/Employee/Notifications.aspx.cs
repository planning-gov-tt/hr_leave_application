﻿using HR_LEAVEv2.Classes;
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
            Util util = new Util();

            Label num_notifs = (Label)Master.FindControl("num_notifications");
            int numUnread = Convert.ToInt32(util.resetNumNotifications(Session["emp_id"].ToString()));
            markAllAsRead.Visible = numUnread > 0;
            num_notifs.Text = numUnread.ToString();
            System.Web.UI.UpdatePanel up = (System.Web.UI.UpdatePanel)Master.FindControl("notificationsUpdatePanel");
            up.Update();
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
                           FORMAT([created_at],  'h:mmtt MMM dd yyyy') AS created_at
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

                        DataRow[] numRead = dt.Select("is_read = 'Yes'");
                        markAllAsUnread.Visible = numRead.Length > 0;
                        deleteAllNotifsBtn.Visible = dt.Rows.Count > 0;

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

        protected void deleteAllNotifsBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = $@"
                    DELETE FROM [dbo].[notifications] 
                    WHERE [employee_id] = '{Session["emp_id"]}';
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
                    WHERE [employee_id] = '{Session["emp_id"]}';
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
                    WHERE [employee_id] = '{Session["emp_id"]}';
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