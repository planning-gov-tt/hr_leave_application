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

                    // FIXME: Change to filter and hardcoded
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

                // if gridview is empty then show message update panel
                emptyGridViewMsgPanel.Style.Add("display", "none");
                if (GridView.Rows.Count == 0)
                {
                    emptyGridViewMsgPanel.Style.Add("display", "inline-block");
                }
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

        // changing button view/visibility - row by row
        protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //int buttonColumnIndex = e.Row.Cells.Count - 1;
            //int undoColumnIndex = buttonColumnIndex - 1;

            // init all undos to invisible
            // e.Row.Cells[undoColumnIndex].Visible = false;

            // remove border on last column
            // e.Row.Cells[buttonColumnIndex].Style.Add("BORDER", "0px");

            // get start date
            //DateTime startDate = new DateTime();
            //if (e.Row.RowType == DataControlRowType.DataRow) // makes sure it is not a header row, only data row
            //{
            //    int index = GetColumnIndexByName(e.Row, "start_date");
            //    startDate = DateTime.Parse(e.Row.Cells[index].Text);
            //}

            // get leave status for that row
            string leaveStatus = null;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int statusIndex = GetColumnIndexByName(e.Row, "status");
                leaveStatus = e.Row.Cells[statusIndex].Text.ToString();
            }

            // if employee view
            if (gridViewType == "emp")
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    // get cancel leave request button 
                    Button btnCancelLeave = (Button)e.Row.FindControl("btnCancelLeave");

                    // employees are only allowed to CANCEL requests if they are still pending
                    // employees cannot edit leave requests
                    if (leaveStatus == "Pending")
                    {
                        btnCancelLeave.Visible = true;
                    }
                    else
                    {
                        btnCancelLeave.Visible = false;
                    }
                }                
            }

            // if supervisor view
            else if (gridViewType == "sup")
            {
                // setting template firel coumn to visible
                // e.Row.Cells[buttonColumnIndex].Visible = true;

                // get supervisor buttons

                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    Button btnNotRecommended = (Button)e.Row.FindControl("btnNotRecommended");
                    Button btnRecommended = (Button)e.Row.FindControl("btnRecommended");

                    // only show buttons if HR has not acted or pending
                    // else do not show buttons
                    if (leaveStatus == "Pending" || leaveStatus == "Recommended" || leaveStatus == "Not Recommended")
                    {
                        btnNotRecommended.Visible = btnRecommended.Visible = true;
                    }
                    else
                    {
                        btnNotRecommended.Visible = btnRecommended.Visible = false;
                    }
                }
            }

            // if employee view OR hr view
            else if (gridViewType == "hr")
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    // get HR buttons
                    Button btnNotApproved = (Button)e.Row.FindControl("btnNotApproved");
                    Button btnApproved = (Button)e.Row.FindControl("btnApproved");
                    Button btnEditLeaveRequest = (Button)e.Row.FindControl("btnEditLeaveRequest");
                    Button btnUndoApprove = (Button)e.Row.FindControl("btnUndoApprove");

                    // if recommended or not approved then show all buttons except undo
                    if (leaveStatus == "Recommended" || leaveStatus == "Not Approved")
                    {
                        btnUndoApprove.Visible = false;
                        btnNotApproved.Visible = btnApproved.Visible = btnEditLeaveRequest.Visible = true;
                    }

                    // if approved then ONLY show undo button
                    else if (leaveStatus == "Approved" )
                    {
                        btnUndoApprove.Visible = true;
                        btnNotApproved.Visible = btnApproved.Visible = btnEditLeaveRequest.Visible = false;
                    }
                }

            }
        }


        protected void GridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Sql command object
            SqlCommand sqlCommand = new SqlCommand();

            // get command type
            string commandName = e.CommandName;

            // get row index in which button was clicked
            int index = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = GridView.Rows[index];

            // get transaciton_id associated with row
            int transaction_id = Convert.ToInt32(GridView.DataKeys[index].Values["transaction_id"]);

            string status = "";

            string updateStatement = $@"
                UPDATE 
                    [dbo].[leavetransaction] 
            ";

            string whereStatement = $@"
                WHERE 
                    [transaction_id] = {transaction_id};
            ";

            string setStatement = "";

            // if employee view
            if(gridViewType == "emp")
            {
                // if cancel button clicked then set leave status to cancelled
                if (commandName == "cancelLeave")
                {
                    status = "Cancelled";
                    setStatement = $@"
                        SET
                            [status]='{status}' 
                    ";

                    //sqlCommand.CommandText = $@"
                    //    UPDATE 
                    //        [dbo].[leavetransaction] 
                    //    SET 
                    //        [status]='{status}'                             
                    //    WHERE 
                    //        [transaction_id] = {transaction_id};
                    //";
                }
                sqlCommand.CommandText = updateStatement + setStatement + whereStatement;
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
                setStatement = $@"
                    SET
                        [status]='{status}',
                        [supervisor_edit_date] = CURRENT_TIMESTAMP 
                ";

                //sqlCommand.CommandText = $@"
                //    UPDATE 
                //        [dbo].[leavetransaction] 
                //    SET 
                //        [status]='{status}', 
                //        [supervisor_edit_date] = CURRENT_TIMESTAMP 
                //    WHERE 
                //        [transaction_id] = {transaction_id};
                //";
                sqlCommand.CommandText = updateStatement + setStatement + whereStatement;
            }
            else if (gridViewType == "hr")
            {          

                if (commandName == "notApproved")
                {
                    // update status to NotApproved
                    status = "Not Approved";
                    setStatement = $@"
                    SET
                        [status]='{status}',
                        [hr_manager_id] = '{Session["emp_id"]}', 
                        [hr_manager_edit_date] = CURRENT_TIMESTAMP
                    ";
                    sqlCommand.CommandText = updateStatement + setStatement + whereStatement;
                }

                else if (commandName == "editLeaveRequest")
                {
                    // redirect to apply for leave form and prepopulate...
                    return;
                }

                else // approve (subtract) or undo (add)
                {
                    // get employee id start date and end dates, leave type from gridview row
                    string employee_id = Convert.ToString(GridView.DataKeys[index].Values["employee_id"]);
                    DateTime startDate = new DateTime();
                    DateTime endDate = new DateTime();
                    string leaveType = "";
                    if (row.RowType == DataControlRowType.DataRow) // makes sure it is not a header row, only data row
                    {
                        int indexStartDate = GetColumnIndexByName(row, "start_date");
                        startDate = DateTime.Parse(row.Cells[indexStartDate].Text);

                        int indexEndDate = GetColumnIndexByName(row, "end_date");
                        endDate = DateTime.Parse(row.Cells[indexEndDate].Text);

                        int indexLeavetype = GetColumnIndexByName(row, "leave_type");
                        leaveType = (row.Cells[indexLeavetype].Text).ToString();
                    }

                    // get difference
                    int difference = (endDate - startDate).Days + 1;

                    // check leave type to match with balance type
                    Dictionary<string, string> leaveBalanceColumnName = new Dictionary<string, string>();
                    leaveBalanceColumnName.Add("Bereavement", "[bereavement]");
                    leaveBalanceColumnName.Add("Casual", "[casual]");
                    leaveBalanceColumnName.Add("Maternity", "[maternity]");
                    leaveBalanceColumnName.Add("Personal", "[personal]");
                    leaveBalanceColumnName.Add("Pre-retirement", "[pre_retirement]");
                    leaveBalanceColumnName.Add("Sick", "[sick]");
                    leaveBalanceColumnName.Add("Vacation", "[vacation]");

                    // TODO: check if in good standing (if employee has enough leave)
                    // TODO: how does HR cater for this???
                    string operation = "";
                    if (commandName == "approved")
                    {
                        // update status to Approved
                        status = "Approved";

                        // Subtract Leave from balance
                        operation = " - ";
                    }

                    // TODO: undo
                    else if (commandName == "undoApprove")
                    {
                        // reset status to Recommended
                        status = "Recommended";

                        // Add leave back to balance
                        operation = " + ";
                    }
                    string sql = $@"
                        BEGIN TRANSACTION;

                        -- updating the leave status
                        UPDATE 
                            [dbo].[leavetransaction]
                        SET
                            [status]='{status}', 
                            [hr_manager_id] = '{Session["emp_id"]}', 
                            [hr_manager_edit_date] = CURRENT_TIMESTAMP 
                        WHERE 
                            [transaction_id] = {transaction_id};

                        -- updating employee leave balance
                        UPDATE 
                            [dbo].[employee]
                        SET
                            {leaveBalanceColumnName[leaveType]} = {leaveBalanceColumnName[leaveType]} {operation} {difference}
                        WHERE
                            [employee_id] = '{employee_id}'

                        COMMIT;
                    ";

                    sqlCommand.CommandText = sql;
                }
            }

            // execute sql
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.ExecuteNonQuery();
                BindGridView();
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

        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            return;
        }

        protected void ddlType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}