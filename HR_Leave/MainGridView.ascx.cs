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
        public string gridViewType { get; set; } // "emp", "sup", "hr"

        protected void Page_Load(object sender, EventArgs e)
        {
            //3 types of gridviews possible
            if (this.gridViewType == "emp")
            {
                // show all transactions submitted by employee_id
                GridView.Columns[2].Visible = false;
            }
            else if (this.gridViewType == "sup")
            {
                // show ALL transactions submitted TO employee_id (sup) for recommedation
                // Filter 
                // show "Recommended" and "Not Recommended" buttons
                GridView.Columns[1].Visible = false;
            }
            else // hr
            {
                // show all "Recommended", "Approved", "Not Approved", "Date Change Requested"
                // show "Approved" and "Not Approved" buttons
                // Filter on "Recommended" by default
                // Apply appropriate filters
            }

            if (!IsPostBack)
            {
                BindGridView();
            }
        }

        private void BindGridView()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString;
         
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                DataSet dataSet = new DataSet();
                SqlCommand sqlCommand = new SqlCommand("dbo.gridViewGetData", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                SqlParameter type = sqlCommand.Parameters.Add("@gridViewType", SqlDbType.NVarChar);
                type.Value = this.gridViewType.ToString();

                SqlParameter currentEmployeeId = sqlCommand.Parameters.Add("@currentEmployeeId", SqlDbType.NVarChar);
                currentEmployeeId.Value = Session["emp_id"].ToString();

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);

                sqlDataAdapter.Fill(dataSet);

                GridView.DataSource = dataSet;
                GridView.DataBind();
            }            
                    
        }
    }
}