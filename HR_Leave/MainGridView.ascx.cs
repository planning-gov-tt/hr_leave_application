using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace HR_Leave
{
    public partial class MainGridView : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!IsPostBack)
            {
                BindGridView();              
            }
        }

        private void BindGridView()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["dbserver"].ConnectionString;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                string commandString = @"
                    SELECT                        
	                    lt.transaction_id transaction_id,
							
	                    s.employee_id supervisor_id,
	                    LEFT(s.first_name, 1) + '. ' + s.last_name AS supervisor_name,
                                                
	                    e.employee_id employee_id,
	                    LEFT(e.first_name, 1) + '. ' + e.last_name AS employee_name,

	                    lt.leave_type leave_type,
	                    lt.start_date start_date,
	                    lt.end_date end_date,
	                    lt.created_at date_submitted,
	                    lt.state status
                    FROM 
	                    leavetransaction lt 
	                    INNER JOIN employee e ON e.employee_id = lt.employee_id
	                    INNER JOIN employee s ON s.employee_id = lt.supervisor_id
	                    LEFT JOIN employee hr ON hr.employee_id = lt.hr_manager_id;                     
                ";
                SqlCommand sqlCommand = new SqlCommand(commandString, sqlConnection);
                sqlConnection.Open();
                GridView.DataSource = sqlCommand.ExecuteReader();
                GridView.DataBind();
                sqlConnection.Close();
            }
        }
    }
}