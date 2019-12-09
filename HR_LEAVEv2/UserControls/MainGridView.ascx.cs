using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace HR_LEAVEv2.UserControls
{
    public partial class MainGridView : System.Web.UI.UserControl
    {
        public string gridViewType { get; set; } // "emp", "sup", "hr"

        public bool btnEmpVisible { get; set; }

        public bool btnSupVisible { get; set; }

        public bool btnHrVisible { get; set; }

        private List<string> permissions;

        //SQL date formatting
        //    FORMAT(getdate(), 'dd-MM-yy') as date

        // general select
        private string select = @"
            SELECT
                lt.transaction_id transaction_id,
                lt.created_at date_submitted,		
                              
                e.employee_id employee_id,
                LEFT(e.first_name, 1) + '. ' + e.last_name AS employee_name,

                lt.leave_type leave_type,
                FORMAT(lt.start_date, 'MM/dd/yy') start_date,
                FORMAT(lt.end_date, 'MM/dd/yy') end_date,

                s.employee_id supervisor_id,
                LEFT(s.first_name, 1) + '. ' + s.last_name AS supervisor_name,
                lt.supervisor_edit_date supervisor_edit_date,

                hr.employee_id hr_manager_id,
                LEFT(hr.first_name, 1) + '. ' + hr.last_name AS hr_manager_name,
                lt.hr_manager_edit_date hr_manager_edit_date,
    
                lt.status status,
                lt.comments comments,
                lt.file_path file_path              
            ";

        // general from
        private string from = @"
            FROM 
                [dbo].[leavetransaction] lt 
                INNER JOIN [dbo].[employee] e ON e.employee_id = lt.employee_id
                INNER JOIN [dbo].[employee] s ON s.employee_id = lt.supervisor_id
                LEFT JOIN [dbo].[employee] hr ON hr.employee_id = lt.hr_manager_id 
            ";

        private string connectionString = ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            
            btnEmpVisible = btnSupVisible = btnHrVisible = false;
            permissions = (List<string>)Session["permissions"];

            // 3 types of gridviews perspectives possible
            if (this.gridViewType == "emp")
            {
                // show all transactions submitted by employee_id

                // hide employee column
                btnEmpVisible = true;
                GridView.Columns[2].Visible = false;
            }
            else if (this.gridViewType == "sup")
            {
                // show ALL transactions submitted TO employee_id (sup) for recommedation
                // Filter 
                // show "Recommended" and "Not Recommended", "Date Change Requested" 

                // Hide Supervisor Columnn
                GridView.Columns[1].Visible = false;

                btnSupVisible = true;
            }
            else // hr
            {
                // show all "Recommended", "Approved", "Not Approved", "Date Change Requested"
                // show "Approved" and "Not Approved" buttons
                // Filter on "Recommended" by default
                // Apply appropriate filters
                btnHrVisible = true;
            }

            if (!IsPostBack)
            {
                BindGridView();
            }
        }

        // TODO
        // get most recent employment type


        private void BindGridView()
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {

                sqlConnection.Open();
                DataSet dataSet = new DataSet();

                string whereBindGridView = "";

                // emp gridview
                if(gridViewType == "emp")
                {
                    whereBindGridView = $@"
                        WHERE
                            e.employee_id = '{Session["emp_id"]}' 
                    ";
                }

                // sup gridview
                else if(gridViewType == "sup")
                {
                    whereBindGridView = $@"
                        WHERE
                            supervisor_id = '{Session["emp_id"]}';
                    ";
                }

                // hr gridview (most complex)
                else if(gridViewType == "hr")
                {
                    whereBindGridView = $@"
                        WHERE
                            status IN ('Recommended', 'Approved', 'Not Approved') AND
                            e.employee_id != '{Session["emp_id"]}'
                    ";

                    // check the type of leave someone can approve 
                    string leaveType = "('Personal', 'No Pay', 'Bereavement', 'Maternity', 'Pre-retirement'";
                    if (permissions.Contains("approve_sick"))
                    {
                        leaveType += ", 'Sick'";
                    }
                    if (permissions.Contains("approve_casual"))
                    {
                        leaveType += ", 'Casual'";
                    }
                    if (permissions.Contains("approve_vacation"))
                    {
                        leaveType += ", 'Vacation'";
                    }
                    leaveType += ")";
                    whereBindGridView += $@" 
                        AND leave_type IN {leaveType}
                    ";

                    // TODO
                    // check if hr can see contracts, public_officers or both based on their permission pool

                    // TODO query
                    // get employment type for most recent employee contract
                    //string mostRecentEmploymentType = GetMostRecentEmploymentType(Session["emp_id"].ToString());

                    //string employementTypes = "(";
                    //if (permissions.Contains("contract_permissions"))
                    //{
                    //    employementTypes += ", 'Contract'";
                    //}
                    //if (permissions.Contains("public_officer_permissions"))
                    //{
                    //    employementTypes += ", 'Public Service'";
                    //}
                    //employementTypes += ")";
                    //whereBindGridView += $@"
                    //    AND '{mostRecentEmploymentType}' IN {employementTypes}            
                    //";


                }

                // TODO search where

                // TODO filter where (dropdown)

                string sql = select + from + whereBindGridView;
                SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);
                                
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                sqlDataAdapter.Fill(dataSet);

                GridView.DataSource = dataSet;
                GridView.DataBind();
            }
        }

        protected void GridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            SqlCommand sqlCommand = new SqlCommand();

            // get command type
            string commandName = e.CommandName;

            // get row idnex in which button was clicked
            int index = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = GridView.Rows[index];

            // get transaciton_id associated with row
            int transaction_id = Convert.ToInt32(GridView.DataKeys[index].Values["transaction_id"]);

            string status = "";
            if(gridViewType == "emp")
            {
                return;
            }
            else if (gridViewType == "sup")
            {
              

                if (commandName == "notRecommended")
                {
                    // update status to NotRecommended
                    status = "Not Recommended";
                }
                else if (commandName == "recommended")
                {
                    // update status to recommended
                    status = "Recommended";
                }

                // if sup view then update status, sup_edit_date
                sqlCommand.CommandText = $@"
                    UPDATE 
                        [dbo].[leavetransaction] 
                    SET 
                        [status]='{status}', 
                        [supervisor_edit_date] = CURRENT_TIMESTAMP 
                    WHERE 
                        [transaction_id] = {transaction_id};
                ";

            }
            else if (gridViewType == "hr")
            {
           

                if (commandName == "notApproved")
                {
                    // update status to NotRecommended
                    status = "Not Approved";
                }
                else if (commandName == "approved")
                {
                    // update status to recommended
                    status = "Approved";
                }
                // if hr view then update status, hr_id, hr_edit_date
                sqlCommand.CommandText = $@"
                    UPDATE 
                        [dbo].[leavetransaction] 
                    SET 
                        [status]='{status}', 
                        [hr_manager_id] = '{Session["emp_id"]}', 
                        [hr_manager_edit_date] = CURRENT_TIMESTAMP 
                    WHERE 
                        [transaction_id] = {transaction_id}
                ";
            }
            else
            {
                // if emp view then update start and end dates
            }

            // execute update
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.ExecuteNonQuery();
                BindGridView();
            }

        }

        // helper function to get column index by name
        int GetColumnIndexByName(GridViewRow row, string columnName)
        {
            int columnIndex = 0;
            foreach (DataControlFieldCell cell in row.Cells)
            {
                if (cell.ContainingField is BoundField)
                    if (((BoundField)cell.ContainingField).DataField.Equals(columnName))
                        break;
                columnIndex++; // keep adding 1 while we don't have the correct name
            }
            return columnIndex;
        }

        protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            int buttonColumnIndex = e.Row.Cells.Count - 1;

            // remove border on last column
            e.Row.Cells[buttonColumnIndex].Style.Add("BORDER", "0px");


            // if employee view OR hr view
            if (gridViewType == "emp" || gridViewType == "hr")
            {
                // get start date
                DateTime startDate = new DateTime();
                if (e.Row.RowType == DataControlRowType.DataRow) // makes sure it is not a header row, only data row
                {
                    int index = GetColumnIndexByName(e.Row, "start_date");
                    startDate = DateTime.Parse(e.Row.Cells[index].Text);
                }
                
                // if leave time has started then you cannot apply for a date change
                if (DateTime.Now > startDate)
                {
                    // hide button for just this row
                    e.Row.Cells[e.Row.Cells.Count - 1].Visible = false;
                }
            }

            // if supervisor view
            else if (gridViewType == "sup")
            {
                
                e.Row.Cells[buttonColumnIndex].Visible = true;

                // get leave status for that row
                string leaveStatus = null;
                if(e.Row.RowType == DataControlRowType.DataRow)
                {
                    int statusIndex = GetColumnIndexByName(e.Row, "status");
                    leaveStatus = e.Row.Cells[statusIndex].Text.ToString();
                }
                
                // only show buttons if HR have not Approved or Not Approved the leave
                // else do not show buttons
                if (leaveStatus == "Approved" || leaveStatus == "Not Approved")
                {
                    e.Row.Cells[buttonColumnIndex].Visible = false;
                }
            }
        }

        protected void GridView_Sorting(object sender, GridViewSortEventArgs e)
        {
           
        }

        protected void GridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.GridView.PageIndex = e.NewPageIndex;
            this.BindGridView();
        }
    }
}