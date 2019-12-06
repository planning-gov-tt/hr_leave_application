using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace HR_LEAVEv2.HR
{
    public partial class EmployeeDetails : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bindGridView();
            }
        }

        protected void bindGridView()
        {
            try
            {
                string sql= $@"
                    SELECT ep.employment_type, d.dept_name, p.pos_name, CONVERT(varchar(10), ep.start_date, 103) AS start_date, CONVERT(varchar(10), ep.expected_end_date, 103) AS expected_end_date
                    FROM [dbo].employeeposition ep

                    LEFT JOIN [dbo].position p
                    ON ep.position_id = p.pos_id

                    LEFT JOIN [dbo].department d
                    ON ep.dept_id = d.dept_id

                    WHERE ep.employee_id = {employeeIdInput.Value};
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
                            GridView1.DataSource = dt;
                            GridView1.DataBind();
                        }
                        else
                            GridView1.DataSource = null;


                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}