﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
                string sql = $@"
                        SELECT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'Name', e.email
                        FROM [dbo].[employee] e
                        JOIN [dbo].assignment a
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
                        FROM [dbo].[employee] e
                        JOIN [dbo].assignment a
                        ON e.employee_id = a.supervisee_id
                        WHERE a.supervisor_id = {Session["emp_id"].ToString()}
                            AND
                        ((e.employee_id LIKE '@SearchString%') OR (e.ihris_id LIKE @SearchString) OR (e.first_name LIKE @SearchString) OR (e.last_name LIKE @SearchString) OR (e.email LIKE @SearchString) ); 
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

        [WebMethod]
        public static string getEmpDetails(string emp_id) 
        {
            EmpDetails empDetails = null;
            try
            {
                string sql = $@"
                        SELECT e.employee_id, e.ihris_id, e.first_name + ' ' + e.last_name as 'name', e.email, e.vacation, e.personal, e.casual, e.sick
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
                                empDetails = new EmpDetails {
                                    emp_id = reader["employee_id"].ToString(),
                                    ihris_id = reader["ihris_id"].ToString(),
                                    name = reader["name"].ToString(),
                                    email = reader["email"].ToString(),
                                    vacation = reader["vacation"].ToString(),
                                    personal = reader["personal"].ToString(),
                                    casual = reader["casual"].ToString(),
                                    sick = reader["sick"].ToString()

                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "{}";
            }

            if (empDetails != null)
                return JsonConvert.SerializeObject(empDetails);
            return "{}";
        }


    }
}