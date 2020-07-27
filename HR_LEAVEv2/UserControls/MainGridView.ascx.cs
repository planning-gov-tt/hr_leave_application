using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;

using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using HR_LEAVEv2.Classes;
using System.Web.UI;

namespace HR_LEAVEv2.UserControls
{
    public partial class MainGridView : System.Web.UI.UserControl
    {
        Util util = new Util();
        User user = new User();

        // used to identify which data and action buttons to show to user
        public string gridViewType { get; set; } // "emp", "sup", "hr"

        //SQL date formatting
        //    FORMAT(getdate(), 'dd-MM-yy') as date

        // general select that is used in constructing the relevant query based on who is viewing the gridview
        private string select = @"
            SELECT
                lt.transaction_id transaction_id,
                lt.created_at date_submitted,		
                              
                e.employee_id employee_id,
                e.last_name + ', ' + LEFT(e.first_name, 1) + '.' AS employee_name,
                IIF(relevant_ep.employment_type IS NOT NULL AND dbo.isRecordActive(relevant_ep.id) = 1, 'Yes', 'No') isLAfromActiveUser,

                lt.leave_type leave_type,
                lt.start_date,
                lt.end_date,
                lt.days_taken,
                lt.qualified,

                s.employee_id supervisor_id,
                s.last_name + ', ' + LEFT(s.first_name, 1) + '.' AS supervisor_name,
                lt.supervisor_edit_date supervisor_edit_date,

                hr.employee_id hr_manager_id,
                LEFT(hr.first_name, 1) + '. ' + hr.last_name AS hr_manager_name,
                lt.hr_manager_edit_date hr_manager_edit_date,
    
                lt.status status              
            ";

        // general from
        private string from = @"
            FROM 
                [dbo].[leavetransaction] lt 
                INNER JOIN [dbo].[employee] e ON e.employee_id = lt.employee_id
                INNER JOIN [dbo].[employee] s ON s.employee_id = lt.supervisor_id
                LEFT JOIN [dbo].[employee] hr ON hr.employee_id = lt.hr_manager_id 
                LEFT JOIN [dbo].employeeposition relevant_ep ON relevant_ep.id = lt.employee_position_id
            ";

        private string connectionString = ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString;

        // VARIABLES TO CHANGE VIEWSTATE VARIABLES 

        // sort direction toggles between two values: DESC and ASC
        private string SortDirection
        {
            get { return ViewState["SortDirection"] != null ? ViewState["SortDirection"].ToString() : "ASC"; } // default to ASC if ViewState is null
            set { ViewState["SortDirection"] = value; }
        }

        // persistent sort expression for after the custom button commands run
        private string SortExpression
        {
            get { return ViewState["SortExpression"] != null ? ViewState["SortExpression"].ToString() : null; } // could be null - null if no previous sort specified
            set { ViewState["SortExpression"] = value; }
        }

        // filter variables used to create where clause
        private string SubmittedFrom
        {
            get { return ViewState["SubmittedFrom"] != null ? ViewState["SubmittedFrom"].ToString() : null; } 
            set { ViewState["SubmittedFrom"] = value; }
        }

        private string SubmittedTo
        {
            get { return ViewState["SubmittedTo"] != null ? ViewState["SubmittedTo"].ToString() : null; }
            set { ViewState["SubmittedTo"] = value; }
        }

        private string StartDate
        {
            get { return ViewState["StartDate"] != null ? ViewState["StartDate"].ToString() : null; }
            set { ViewState["StartDate"] = value; }
        }

        private string EndDate
        {
            get { return ViewState["EndDate"] != null ? ViewState["EndDate"].ToString() : null; }
            set { ViewState["EndDate"] = value; }
        }

        private string SupervisorName_ID
        {
            get { return ViewState["SupervisorName_ID"] != null ? ViewState["SupervisorName_ID"].ToString() : null; }
            set { ViewState["SupervisorName_ID"] = value; }
        }

        private string EmployeeName_ID_Email
        {
            get { return ViewState["EmployeeName_ID_Email"] != null ? ViewState["EmployeeName_ID_Email"].ToString() : null; }
            set { ViewState["EmployeeName_ID_Email"] = value; }
        }

        private string LeaveType
        {
            get { return ViewState["LeaveType"] != null ? ViewState["LeaveType"].ToString() : null; }
            set { ViewState["LeaveType"] = value; }
        }

        private string Status
        {
            get { return ViewState["Status"] != null ? ViewState["Status"].ToString() : null; }
            set { ViewState["Status"] = value; }
        }

        private string Qualified
        {
            get { return ViewState["Qualified"] != null ? ViewState["Qualified"].ToString() : null; }
            set { ViewState["Qualified"] = value; }
        }

        private string LAfromActiveInactiveEmp
        {
            get { return ViewState["LAfromActiveInactiveEmp"] != null ? ViewState["LAfromActiveInactiveEmp"].ToString() : null; }
            set { ViewState["LAfromActiveInactiveEmp"] = value; }
        }

        private Dictionary<string, string> notApprovedRow
        {
            get { return ViewState["notApprovedRow"] != null ? (Dictionary<string, string>)ViewState["notApprovedRow"]: null; }
            set { ViewState["notApprovedRow"] = value; }
        }

        private Dictionary<string, string> notRecommendedRow
        {
            get { return ViewState["notRecommendedRow"] != null ? (Dictionary<string, string>)ViewState["notRecommendedRow"] : null; }
            set { ViewState["notRecommendedRow"] = value; }
        }


        protected void Page_Load(object sender, EventArgs e)
        {

            // 3 types of gridviews perspectives possible
            if (this.gridViewType == "emp")
            {
                // show all transactions submitted by employee_id

                // hide employee column
                GridView.Columns[2].Visible = false;
                divTbEmployee.Visible = false;
            }
            else if (this.gridViewType == "sup")
            {
                // show ALL transactions submitted TO employee_id (sup) for recommedation
                // Filter 
                // show "Recommended" and "Not Recommended", "Date Change Requested" 

                // Hide Supervisor Columnn
                GridView.Columns[1].Visible = false;
                divTbSupervisor.Visible = false;
            }

            if (!IsPostBack)
            {
                // default sort on date submitted DESC
                this.SortExpression = "date_submitted";
                this.SortDirection = "DESC";

                // check if filters should be preset
                if (Request.QueryString.HasKeys())
                {
                    string empEmail = string.Empty;

                    if (!util.isNullOrEmpty(Request.QueryString["empEmail"]))
                        empEmail = Request.QueryString["empEmail"];

                    // set filter
                    EmployeeName_ID_Email = empEmail;
                    tbEmployee.Text = empEmail;
                }
                             

                this.BindGridView();
            }
        }

        // GRIDVIEW METHODS
        private void BindGridView()
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand sqlCommand = new SqlCommand();

                // used to build the where statement for the gridview
                string whereBindGridView = "";

                // emp gridview
                if (gridViewType == "emp")
                {
                    whereBindGridView = $@"
                        WHERE
                            e.employee_id = '{user.currUserId}' 
                    ";
                }

                // sup gridview
                else if (gridViewType == "sup")
                {
                    whereBindGridView = $@"
                        WHERE
                            status IN ('Not Recommended', 'Recommended', 'Pending') AND
                            supervisor_id = '{user.currUserId}'
                    ";

                    // ensure that the current supervisor viewing data is active
                    whereBindGridView += $@" AND dbo.getActiveRecord({user.currUserId}) IS NOT NULL ";
                }

                // hr gridview (most complex)
                else if (gridViewType == "hr")
                {
                    // adjust the statuses that can be seen by a HR here. Ensures that the HR will only see statuses that are in the
                    // list specified below
                    whereBindGridView = $@"
                        WHERE
                            status IN ('Not Recommended', 'Recommended', 'Approved', 'Not Approved') AND
                            e.employee_id != '{user.currUserId}'
                    ";

                    List<string> leaveTypes = user.getTypesOfLeaveUserCanApprove();
                    whereBindGridView += $@" 
                        AND leave_type IN ({String.Join(", ", leaveTypes.ToArray())})
                    ";

                    // check if hr can see contracts, public_officers or both based on their permission pool
                    List<string> employmentTypes = user.getSubsetsOfEmployeesUserIsAllowedToView()["employment_types"];
                    whereBindGridView += $@"
                        AND (relevant_ep.employment_type IN ({String.Join(", ", employmentTypes.ToArray())}))        
                    ";
                    
                    // ensure hr is active
                    whereBindGridView += $@" AND dbo.getActiveRecord({user.currUserId}) IS NOT NULL ";

                }// end hr if


                //*************************************** FILTER ***************************************//
                // assume all form fields are validated by the time they reach this point'

                string whereFilterGridView = string.Empty;
                if (!util.isNullOrEmpty(SubmittedFrom))
                {
                    whereFilterGridView += $@"
                        AND lt.created_at >= '{DateTime.ParseExact(SubmittedFrom, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy")}'
                    ";
                }
                if (!util.isNullOrEmpty(SubmittedTo))
                {
                    whereFilterGridView += $@"
                        AND lt.created_at < dateadd(day,1,'{DateTime.ParseExact(SubmittedTo, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy")}')
                    ";
                }
                if (!util.isNullOrEmpty(StartDate))
                {
                    whereFilterGridView += $@"
                        AND lt.start_date >= '{DateTime.ParseExact(StartDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy")}'
                    ";
                }
                if (!util.isNullOrEmpty(EndDate))
                {
                    whereFilterGridView += $@"
                        AND lt.end_date < dateadd(day,1,'{DateTime.ParseExact(EndDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy")}') 
                    ";
                }
                if (!util.isNullOrEmpty(SupervisorName_ID))
                {
                    whereFilterGridView += $@"
                        AND (
                            (s.first_name LIKE @SupervisorName_ID) OR
                            (s.last_name LIKE @SupervisorName_ID) OR
                            (s.employee_id LIKE @SupervisorName_ID) OR
                            (s.email LIKE @SupervisorName_ID)
                        ) 
                    ";
                    sqlCommand.Parameters.AddWithValue("@SupervisorName_ID", "%" + SupervisorName_ID + "%");
                }
                if (!util.isNullOrEmpty(EmployeeName_ID_Email))
                {
                    whereFilterGridView += $@"
                        AND (
                            (e.first_name LIKE @EmployeeName_ID_Email) OR
                            (e.last_name LIKE @EmployeeName_ID_Email) OR
                            (e.employee_id LIKE @EmployeeName_ID_Email) OR
                            (e.email LIKE @EmployeeName_ID_Email)
                        ) 
                    ";
                    sqlCommand.Parameters.AddWithValue("@EmployeeName_ID_Email", "%" + EmployeeName_ID_Email + "%");
                }
                if (!util.isNullOrEmpty(LeaveType))
                {
                    whereFilterGridView += $@"
                        AND leave_type = '{LeaveType}'
                    ";
                }
                if (!util.isNullOrEmpty(Status))
                {
                    whereFilterGridView += $@"
                        AND status = '{Status}'
                    ";
                }

                if (!util.isNullOrEmpty(Qualified))
                {
                    whereFilterGridView += $@"
                        AND qualified = '{Qualified}'
                    ";
                }

                if (!util.isNullOrEmpty(LAfromActiveInactiveEmp))
                {
                    if(LAfromActiveInactiveEmp == "Active")
                        whereFilterGridView += $@"
                            AND relevant_ep.employment_type IS NOT NULL AND dbo.isRecordActive(relevant_ep.id) = 1
                        ";
                    else if(LAfromActiveInactiveEmp == "Inactive")
                        whereFilterGridView += $@"
                            AND NOT (relevant_ep.employment_type IS NOT NULL AND dbo.isRecordActive(relevant_ep.id) = 1)
                        ";
                    // else if viewing both then add no where clause
                }

                

                string sql = select + from + whereBindGridView + whereFilterGridView;
                sqlCommand.CommandText = sql;
                sqlCommand.Connection = sqlConnection;

                sqlConnection.Open();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                //DataSet dataSet = new DataSet();
                DataTable dataTable = new DataTable(); // using data table vs data set to facilitate sorting
                sqlDataAdapter.Fill(dataTable);

                // if sorting specified
                if (SortExpression != null)
                {
                    DataView dataView = dataTable.AsDataView();
                    dataView.Sort = this.SortExpression + " " + this.SortDirection;
                    this.GridView.DataSource = dataView;
                }
                else // if no sorting
                {
                    GridView.DataSource = dataTable;
                }

                // bind
                GridView.DataBind();

                // assume GV is not empty...if gridview is empty then show message update panel
                emptyGridViewMsgPanel.Style.Add("display", "none");
                if (GridView.Rows.Count == 0)
                    emptyGridViewMsgPanel.Style.Add("display", "inline-block");
            }
        }
 
        int GetColumnIndexByName(GridViewRow row, string columnName)
        {
            // helper function to get column index by name

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
            // changing button view/visibility - row by row
            
            // get leave status for that row
            string leaveStatus = null;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int statusIndex = GetColumnIndexByName(e.Row, "status");
                leaveStatus = e.Row.Cells[statusIndex].Text.ToString();
            }

            // hide isLAfromActiveUser column
            int isLAfromActiveUserIndex = -1;
            string isLAfromActiveUser = string.Empty;
            if (e.Row.RowType == DataControlRowType.Header || e.Row.RowType == DataControlRowType.DataRow)
            {
                isLAfromActiveUserIndex = GetColumnIndexByName(e.Row, "isLAfromActiveUser");
                e.Row.Cells[isLAfromActiveUserIndex].Style.Add("display", "none");

                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    // if LA is from inactive user- show visual feedback
                    isLAfromActiveUser = e.Row.Cells[isLAfromActiveUserIndex].Text.ToString();
                    if (isLAfromActiveUser == "No")
                        e.Row.CssClass = "inactive-employee-LA";
                    else
                        e.Row.CssClass.Replace("inactive-employee-LA", "");
                }
            }

            // if employee view
            if (gridViewType == "emp")
            {
                if (e.Row.RowType == DataControlRowType.DataRow && isLAfromActiveUser == "Yes")
                {
                    // get cancel leave request button 
                    LinkButton btnCancelLeave = (LinkButton)e.Row.FindControl("btnCancelLeave");

                    // employees are only allowed to CANCEL requests if they are still pending
                    // employees cannot edit leave requests
                    if (leaveStatus == "Pending")
                        btnCancelLeave.Visible = true;
                    else
                        btnCancelLeave.Visible = false;
                }
            }
            else
            {

                // if supervisor view
                if (gridViewType == "sup")
                {
                    // get supervisor buttons
                    if (e.Row.RowType == DataControlRowType.DataRow && isLAfromActiveUser == "Yes")
                    {
                        LinkButton btnNotRecommended = (LinkButton)e.Row.FindControl("btnNotRecommended");
                        LinkButton btnRecommended = (LinkButton)e.Row.FindControl("btnRecommended");
                        LinkButton btnEditLeaveRequest = (LinkButton)e.Row.FindControl("btnEditLeaveRequest");

                        btnEditLeaveRequest.Visible = true;

                        // only show buttons if HR has not acted or pending
                        // else do not show buttons
                        if (leaveStatus == "Pending" || leaveStatus == "Recommended" || leaveStatus == "Not Recommended")
                        {
                            // visual feedback for user on button to show whether LA is Recommended or Not Recommended
                            if (leaveStatus == "Recommended")
                                btnRecommended.Style.Add("opacity", "0.55");
                            else if (leaveStatus == "Not Recommended")
                                btnNotRecommended.Style.Add("opacity", "0.55");

                            btnNotRecommended.Visible = btnRecommended.Visible = true;
                        }
                        else
                            btnNotRecommended.Visible = btnRecommended.Visible = false;
                    }
                }

                // if hr view
                else if (gridViewType == "hr")
                {
                    if (e.Row.RowType == DataControlRowType.DataRow && isLAfromActiveUser == "Yes")
                    {
                        // get HR buttons
                        LinkButton btnNotApproved = (LinkButton)e.Row.FindControl("btnNotApproved");
                        LinkButton btnApproved = (LinkButton)e.Row.FindControl("btnApproved");
                        LinkButton btnEditLeaveRequest = (LinkButton)e.Row.FindControl("btnEditLeaveRequest");
                        LinkButton btnUndoApprove = (LinkButton)e.Row.FindControl("btnUndoApprove");

                        // if recommended or not approved then show all buttons except undo
                        if (leaveStatus == "Recommended" || leaveStatus == "Not Approved")
                        {
                            // visual feedback for Not Approved
                            if (leaveStatus == "Not Approved")
                                btnNotApproved.Style.Add("opacity", "0.55");

                            btnUndoApprove.Visible = false;
                            btnNotApproved.Visible = btnApproved.Visible = btnEditLeaveRequest.Visible = true;
                        }

                        // if approved then ONLY show undo button if conditions are right
                        else if (leaveStatus == "Approved")
                        {

                            // ensure that a HR can undo an approve if the leave period has not already started
                            int startDateIndex = GetColumnIndexByName(e.Row, "start_date");
                            string startDate = e.Row.Cells[startDateIndex].Text.ToString();

                            if (!util.isNullOrEmpty(startDate))
                            {
                                DateTime start = DateTime.MinValue;
                                try
                                {
                                    //start = Convert.ToDateTime(startDate);
                                    start = DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (FormatException fe)
                                {
                                    throw fe;
                                }

                                // if today is a day before the start date of the application then show undo button
                                if (DateTime.Compare(util.getCurrentDateToday(), start) < 0)
                                    btnUndoApprove.Visible = true;
                                else
                                    btnUndoApprove.Visible = false;
                            }

                            btnNotApproved.Visible = btnApproved.Visible = false;
                        }
                        else if (leaveStatus == "Not Recommended")
                        {
                            btnUndoApprove.Visible = false;
                            btnNotApproved.Visible = btnApproved.Visible = btnEditLeaveRequest.Visible = false;
                        }
                    }
                }
            }
            
        }

        protected void GridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // if unwanted command triggers this function then do nothing
            if (e.CommandName == "Sort" || e.CommandName == "Page")
            {
                return;
            }

            // if the command is a custom button command 
            // Sql command object created here will changed based on the command type
            SqlCommand sqlCommand = new SqlCommand();

            // get command type
            string commandName = e.CommandName;

            // get row index in which button was clicked
            int index = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = GridView.Rows[index];

            // get transaction_id associated with row
            int transaction_id = Convert.ToInt32(GridView.DataKeys[index].Values["transaction_id"]);

            // get employee_id associated with row
            string employee_id = GridView.DataKeys[index].Values["employee_id"].ToString();

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

            // view details of a LA
            if (commandName == "details")
                Response.Redirect($"~/Employee/ApplyForLeave.aspx?mode=view&leaveId={transaction_id}&returnUrl={HttpContext.Current.Request.Url.PathAndQuery}");

            // edit a LA
            if (commandName == "editLeaveRequest")
            {
                // redirect to apply for leave form and prepopulate...
                Response.Redirect($"~/Employee/ApplyForLeave.aspx?mode=edit&leaveId={transaction_id}&returnUrl={HttpContext.Current.Request.Url.PathAndQuery}");
            }
            // if employee view
            if (gridViewType == "emp")
            {
                // if cancel button clicked then set leave status to cancelled
                if (commandName == "cancelLeave")
                {
                    status = "Cancelled";
                    setStatement = $@"
                        SET
                            [status]='{status}' 
                    ";
                }
                sqlCommand.CommandText = updateStatement + setStatement + whereStatement;
            }
            else if (gridViewType == "sup")
            {
                if (commandName == "notRecommended")
                {
                    // update status to Not Recommended
                    status = "Not Recommended";
                }
                else if (commandName == "recommended")
                {
                    // update status to Recommended
                    status = "Recommended";
                }


                // if sup view then update status, sup_edit_date                    
                setStatement = $@"
                SET
                    [status]='{status}',
                    [supervisor_edit_date] = CURRENT_TIMESTAMP 
                ";

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
                            [hr_manager_id] = '{user.currUserId}', 
                            [hr_manager_edit_date] = CURRENT_TIMESTAMP
                        ";
                    sqlCommand.CommandText = updateStatement + setStatement + whereStatement;
                }
                else // approve (subtract) or undo (add)
                {
                    string leaveType = "";
                    if (row.RowType == DataControlRowType.DataRow) // makes sure it is not a header row, only data row
                    {
                        int indexLeavetype = GetColumnIndexByName(row, "leave_type");
                        leaveType = (row.Cells[indexLeavetype].Text).ToString();
                    }

                    string sql = string.Empty;

                    if (util.isLeaveTypeWithoutBalance(leaveType))
                    {
                        // if command is approve, update status to Approved
                        // if command is undo, reset status to Recommended
                        status = commandName == "approved" ? "Approved" : "Recommended";

                        sql = $@"
                            -- updating the leave status
                            UPDATE 
                                [dbo].[leavetransaction]
                            SET
                                [status]='{status}', 
                                [hr_manager_id] = '{user.currUserId}', 
                                [hr_manager_edit_date] = CURRENT_TIMESTAMP 
                            WHERE 
                                [transaction_id] = {transaction_id};
                        ";
                    }
                    else
                    {
                        int daysTakenIndex = GetColumnIndexByName(row, "days_taken");

                        // get difference
                        int difference = Convert.ToInt32(row.Cells[daysTakenIndex].Text);

                        // check leave type from Gridview to match with balance type in DB in order to update the right leave balance
                        Dictionary<string, string> leaveBalanceColumnName = util.getLeaveTypeMapping();

                        // approved => subtract leave from balance
                        string operation = "";
                        if (commandName == "approved")
                        {
                            // update status to Approved
                            status = "Approved";

                            // Subtract Leave from balance
                            operation = " - ";
                        }

                        // undo => add leave back
                        else if (commandName == "undoApprove")
                        {
                            // reset status to Recommended
                            status = "Recommended";

                            // Add leave back to balance
                            operation = " + ";
                        }

                        sql = $@"
                            BEGIN TRANSACTION;

                            -- updating the leave status
                            UPDATE 
                                [dbo].[leavetransaction]
                            SET
                                [status]='{status}', 
                                [hr_manager_id] = '{user.currUserId}', 
                                [hr_manager_edit_date] = CURRENT_TIMESTAMP 
                            WHERE 
                                [transaction_id] = {transaction_id};

                            -- updating employee leave balance
                            UPDATE 
                                [dbo].[employee]
                            SET
                                {leaveBalanceColumnName[leaveType]} = IIF({leaveBalanceColumnName[leaveType]} {operation} {difference} >= 0, {leaveBalanceColumnName[leaveType]} {operation} {difference}, 0)
                            WHERE
                                [employee_id] = '{employee_id}';

                            COMMIT;
                        ";
                    }

                    sqlCommand.CommandText = sql;
                }
            }
            Boolean isQuerySuccessful;
            // execute sql
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                int rowsAffected = sqlCommand.ExecuteNonQuery();

                isQuerySuccessful = rowsAffected > 0;
            }

            // send notifications and add audit logs
            if (isQuerySuccessful)
            {
                string actingEmployeeID, affectedEmployeeId;
                string action;

                actingEmployeeID = affectedEmployeeId = action = string.Empty;

                // get employee's email associated with leave application
                string employeeEmail = string.Empty;
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();

                        string sql = $@"
                                SELECT email FROM [dbo].[employee] WHERE employee_id = {employee_id}
                            ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    employeeEmail = reader["email"].ToString();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                // get supervisors's email associated with leave application
                string supervisorEmail = string.Empty;
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();

                        string sql = $@"
                                SELECT email FROM [dbo].[employee] WHERE employee_id = {GridView.DataKeys[index].Values["supervisor_id"].ToString()}
                            ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    supervisorEmail = reader["email"].ToString();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                // Cancel Leave
                if (commandName == "cancelLeave")
                {
                    actingEmployeeID = affectedEmployeeId = user.currUserId;
                    action = "Canceled leave application";
                }

                // Recommend
                if (commandName == "recommended")
                {
                    sendRecommendedNotifications(row, employee_id, employeeEmail);

                    actingEmployeeID = user.currUserId;
                    affectedEmployeeId = employee_id;
                    action = "Recommended leave application";
                }

                // Not Recommend
                if (commandName == "notRecommended")
                {
                    // show modal to user to input comment for why they are not recommending the application. This comment is added to the db and to the email in the onclick function of the
                    // submit comment button
                    commentModalTransactionIdHiddenField.Value = transaction_id.ToString(); // add leave transaction id to hidden field in modal to access for db insert
                    commentModalEmployeeIdHiddenField.Value = employee_id; // add leave transaction id to hidden field in modal to access later for audit log

                    // load back comment panel with comments from db, if there
                    string previousComment = string.Empty;
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            string sql = $@"
                                SELECT sup_comment FROM leavetransaction WHERE transaction_id = {transaction_id}
                            ";
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        previousComment = reader["sup_comment"].ToString();
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception logic
                        throw ex;
                    }

                    // if previous comment exists then populate textfield with it and store it in a hihdden field to check if the comment was edited
                    // in any way later
                    commentsTxtArea.InnerText = previousCommentsHiddenField.Value = !util.isNullOrEmpty(previousComment) ? previousComment : string.Empty;
                    Dictionary<string, string> rowData = new Dictionary<string, string>()
                    {
                        { "date_submitted", row.Cells[GetColumnIndexByName(row, "date_submitted")].Text.ToString() },
                        { "employee_name", row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString() },
                        { "employee_id", employee_id},
                        { "supervisor_name", user.currUserName },
                        { "start_date", row.Cells[GetColumnIndexByName(row, "start_date")].Text.ToString() },
                        { "end_date", row.Cells[GetColumnIndexByName(row, "end_date")].Text.ToString() },
                        { "days_taken", row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString() },
                        { "leave_type", row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString() },
                        { "status", row.Cells[GetColumnIndexByName(row, "status")].Text.ToString() },
                        { "qualified",row.Cells[GetColumnIndexByName(row, "qualified")].Text.ToString() },
                        { "isEmailSentAlready", row.Cells[GetColumnIndexByName(row, "status")].Text.ToString() == "Not Recommended" ? "Yes" : "No"},
                        { "employeeEmail", employeeEmail }
                    };


                    notRecommendedRow = rowData;

                    // reset user feedback panels
                    noEditsMadePanel.Visible = false;
                    addCommentSuccessPanel.Visible = false;
                    editCommentSuccessPanel.Visible = false;
                    errorSubmittingEmailMsgPanel.Visible = false;

                    // show modal for user to enter comment
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#commentsModal').modal({'show':true, 'backdrop':'static', 'keyboard':false});", true);

                    // populate audit log variables 
                    actingEmployeeID = user.currUserId;
                    affectedEmployeeId = employee_id;
                    action = "Unrecommended leave application";
                }

                // Approve
                if (commandName == "approved")
                {

                    sendApprovedNotifications(row, employee_id, GridView.DataKeys[index].Values["supervisor_id"].ToString(), employeeEmail, supervisorEmail);

                    actingEmployeeID = user.currUserId;
                    affectedEmployeeId = employee_id;
                    action = "Approved leave application";
                }

                // Not Approve
                if (commandName == "notApproved")
                {
                    // show modal to user to input comment for why they are not recommending the application. This comment is added to the db and to the email in the onclick function of the
                    // submit comment button
                    commentModalTransactionIdHiddenField.Value = transaction_id.ToString(); // add leave transaction id to hidden field in modal to access for db insert
                    commentModalEmployeeIdHiddenField.Value = employee_id;// add leave transaction id to hidden field in modal to access later for audit log

                    // load back comment panel with comments from db, if there
                    string previousComment = string.Empty;
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            string sql = $@"
                                SELECT hr_comment FROM leavetransaction WHERE transaction_id = {transaction_id}
                            ";
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        previousComment = reader["hr_comment"].ToString();
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception logic
                        throw ex;
                    }

                    // if previous comment exists then populate textfield with it and store it in a hihdden field to check if the comment was edited
                    // in any way later
                    commentsTxtArea.InnerText = previousCommentsHiddenField.Value = !util.isNullOrEmpty(previousComment) ? previousComment : string.Empty;
                    Dictionary<string, string> rowData = new Dictionary<string, string>()
                    {
                        { "date_submitted", row.Cells[GetColumnIndexByName(row, "date_submitted")].Text.ToString() },
                        { "employee_id", employee_id},
                        { "employee_name", row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString() },
                        { "employeeEmail", employeeEmail },
                        { "supervisor_name", row.Cells[GetColumnIndexByName(row, "supervisor_name")].Text.ToString() },
                        { "supervisor_id", GridView.DataKeys[index].Values["supervisor_id"].ToString()},
                        { "supervisorEmail", supervisorEmail },
                        { "start_date", row.Cells[GetColumnIndexByName(row, "start_date")].Text.ToString() },
                        { "end_date", row.Cells[GetColumnIndexByName(row, "end_date")].Text.ToString() },
                        { "days_taken", row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString() },
                        { "leave_type", row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString() },
                        { "status", row.Cells[GetColumnIndexByName(row, "status")].Text.ToString() },
                        { "qualified",row.Cells[GetColumnIndexByName(row, "qualified")].Text.ToString() },
                        { "isEmailSentAlready", row.Cells[GetColumnIndexByName(row, "status")].Text.ToString() == "Not Approved" ? "Yes" : "No"}
                    };

                    notApprovedRow = rowData;

                    // reset user feedback panels
                    noEditsMadePanel.Visible = false;
                    addCommentSuccessPanel.Visible = false;
                    editCommentSuccessPanel.Visible = false;
                    errorSubmittingEmailMsgPanel.Visible = false;

                    // show modal for user to enter comment
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#commentsModal').modal({'show':true, 'backdrop':'static', 'keyboard':false});", true);

                    actingEmployeeID = user.currUserId;
                    affectedEmployeeId = employee_id;
                    action = "Unapproved leave application";
                }

                // Undo Approve
                if (commandName == "undoApprove")
                {
                    sendApprovalUndoneNotifications(row, employee_id, GridView.DataKeys[index].Values["supervisor_id"].ToString(), employeeEmail, supervisorEmail);
                    actingEmployeeID = user.currUserId;
                    affectedEmployeeId = employee_id;
                    action = "Undid approval for leave application";
                }

                resetNumNotifications();

                string actionStr = $"{action}; ID= {transaction_id}";
                util.addAuditLog(actingEmployeeID, affectedEmployeeId, actionStr);
            }

            // show submit comment button
            submitCommentBtn.Visible = true;

            BindGridView();
        }

        protected void GridView_Sorting(object sender, GridViewSortEventArgs e)
        {
            this.SortExpression = e.SortExpression; // set new sort expression
            this.SortDirection = this.SortDirection == "ASC" ? "DESC" : "ASC"; // toggle ASC/DESC

            // TODO: add/remove/change arrow direction in column to denote which sort the column is on

            this.BindGridView();
        }

        protected void GridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.GridView.PageIndex = e.NewPageIndex;
            this.BindGridView();
        }
        // _________________________________________________________________


        // FILTER METHODS
        protected Boolean validateDates(string startDate, string endDate, Panel invalidStartDateParam, Panel invalidEndDateParam, Panel invalidComparisonParam)
        {
            // validates two dates and returns a Boolean representing whether they are valid or not

            // clear errors before validating 
            invalidStartDateParam.Style.Add("display", "none");
            invalidEndDateParam.Style.Add("display", "none");
            invalidComparisonParam.Style.Add("display", "none");

            if (util.isNullOrEmpty(startDate) && util.isNullOrEmpty(endDate))
                return true;

            DateTime start, end;
            start = end = DateTime.MinValue;
            Boolean isValidated = true;

            // validate start date is a date
            if (!DateTime.TryParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start) || util.isNullOrEmpty(startDate))
            {
                invalidStartDateParam.Style.Add("display", "inline-block");
                isValidated = false;
            }

            // validate end date is a date
            if (!DateTime.TryParseExact(endDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out end) || util.isNullOrEmpty(endDate))
            {
                invalidEndDateParam.Style.Add("display", "inline-block");
                isValidated = false;
            }

            if (isValidated)
            {

                // compare dates to ensure end date is not before start date
                if (DateTime.Compare(start, end) > 0)
                {
                    invalidEndDateParam.Style.Add("display", "inline-block");
                    invalidComparisonParam.Style.Add("display", "inline-block");
                    isValidated = false;
                }
            }
            return isValidated;
        }

        protected void clearFilterErrors()
        {
            invalidStartDateValidationMsgPanel.Style.Add("display", "none");
            invalidEndDateValidationMsgPanel.Style.Add("display", "none");
            appliedForDateComparisonValidationMsgPanel.Style.Add("display", "none");
            invalidSubmittedFromDate.Style.Add("display", "none");
            invalidSubmittedToDate.Style.Add("display", "none");
            submittedDateComparisonValidationMsgPanel.Style.Add("display", "none");
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            // check all textboxes/drop down lists and set the respective variables. These variables are checked when binding 
            // the gridview in order to filter LAs

            clearFilterErrors();

            bool areSubmitDatesValidated = false, areAppliedDatesValidated = false;
            areSubmitDatesValidated = validateDates(tbSubmittedFrom.Text.ToString(), tbSubmittedTo.Text.ToString(), invalidSubmittedFromDate, invalidSubmittedToDate, submittedDateComparisonValidationMsgPanel);
            areAppliedDatesValidated = validateDates(tbStartDate.Text.ToString(), tbEndDate.Text.ToString(), invalidStartDateValidationMsgPanel, invalidEndDateValidationMsgPanel, appliedForDateComparisonValidationMsgPanel);

            if (areSubmitDatesValidated && areAppliedDatesValidated)
            {
                // display appropriate message in the notification panel

                SubmittedFrom = tbSubmittedFrom.Text.ToString();
                SubmittedTo = tbSubmittedTo.Text.ToString();

                StartDate = tbStartDate.Text.ToString();
                EndDate = tbEndDate.Text.ToString();

                SupervisorName_ID = tbSupervisor.Text.ToString();

                EmployeeName_ID_Email = tbEmployee.Text.ToString();

                LeaveType = ddlType.SelectedValue.ToString();

                Status = ddlStatus.SelectedValue.ToString();

                Qualified = ddlQualified.SelectedValue.ToString();

                LAfromActiveInactiveEmp = ddlLAfromActiveOrInactive.SelectedValue.ToString();

                this.BindGridView();
            }
        }

        protected void clearFilterBtn_Click(object sender, EventArgs e)
        {
            // clear all filters
            clearFilterErrors();
             
            SubmittedFrom = String.Empty;
            tbSubmittedFrom.Text = String.Empty;

            SubmittedTo = String.Empty;
            tbSubmittedTo.Text = String.Empty;

            StartDate = String.Empty;
            tbStartDate.Text = String.Empty;

            EndDate = String.Empty;
            tbEndDate.Text = String.Empty;

            SupervisorName_ID = String.Empty;
            tbSupervisor.Text = String.Empty;

            EmployeeName_ID_Email = String.Empty;
            tbEmployee.Text = String.Empty;

            LeaveType = String.Empty;
            ddlType.SelectedIndex = 0;

            Status = String.Empty;
            ddlStatus.SelectedIndex = 0;

            Qualified = String.Empty;
            ddlQualified.SelectedIndex = 0;

            LAfromActiveInactiveEmp = String.Empty;
            ddlLAfromActiveOrInactive.SelectedIndex = 0;

            this.BindGridView();
        }
        // _________________________________________________________________


        // NOTIFICATIONS METHODS
        protected void sendApprovedNotifications(GridViewRow row, string employee_id, string supervisor_id, string employeeEmail, string supervisorEmail)
        {
            // sends notifs to appropriate recipients (email and in house notifs)

            // SEND EMAILS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            // send email to employee notifying them that their leave application was approved
            MailMessage message = util.getEmployeeViewLeaveApplicationApproved(
                    new Util.EmailDetails
                    {
                        supervisor_name = row.Cells[GetColumnIndexByName(row, "supervisor_name")].Text.ToString(),
                        date_submitted = row.Cells[GetColumnIndexByName(row, "date_submitted")].Text.ToString(),
                        start_date = row.Cells[GetColumnIndexByName(row, "start_date")].Text.ToString(),
                        end_date = row.Cells[GetColumnIndexByName(row, "end_date")].Text.ToString(),
                        days_taken = row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString(),
                        type_of_leave = row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString(),
                        qualified = row.Cells[GetColumnIndexByName(row, "qualified")].Text.ToString(),
                        recipient = employeeEmail,
                        subject = "Leave Application Approved"
                    }
            );
            util.sendMail(message);

            // send email to supervisor letting them know that HR approved application for employee
            message = util.getSupervisorViewLeaveApplicationApproved(
                    new Util.EmailDetails
                    {
                        employee_name = row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString(),
                        date_submitted = row.Cells[GetColumnIndexByName(row, "date_submitted")].Text.ToString(),
                        start_date = row.Cells[GetColumnIndexByName(row, "start_date")].Text.ToString(),
                        end_date = row.Cells[GetColumnIndexByName(row, "end_date")].Text.ToString(),
                        days_taken = row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString(),
                        type_of_leave = row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString(),
                        qualified = row.Cells[GetColumnIndexByName(row, "qualified")].Text.ToString(),
                        recipient = supervisorEmail,
                        subject = $"Leave Application Approved for {row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString()}"
                    }
                    );
            util.sendMail(message);

            // SEND IN APPLICATION NOTIFS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            // send notif to employee
            string notif_header = $"Leave Application Approved",
                   notification = $"Your leave application to {row.Cells[GetColumnIndexByName(row, "supervisor_name")].Text.ToString()} for {row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString()} day(s) {row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString()} leave was approved.";
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', '{notification}', 'No', '{employee_id}', '{util.getCurrentDate()}');
                            ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            // send notif to supervisor
            notif_header = $"Leave Application Approved for {row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString()}";
            notification = $"The leave application of {row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString()} for {row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString()} day(s) {row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString()} leave was approved.";
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', '{notification}', 'No', '{supervisor_id}', '{util.getCurrentDate()}');
                            ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        protected void sendNotApprovedNotifications(string comment)
        {
            // sends notifs to appropriate recipients (email and in house notifs)

            Dictionary<string, string> row = null;
            if (notApprovedRow != null)
                row = notApprovedRow;

            Boolean isEmployeeEmailSent = false,
                    isSupervisorEmailSent = false;

            if (row != null && row["isEmailSentAlready"] == "No")
            {

                // SEND EMAILS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

                string emailCommentString = string.Empty;
                if (!util.isNullOrEmpty(comment))
                    emailCommentString = $"HR has left the following comment: <blockquote style='font-size: 1.1em;'>\"{comment}\"</blockquote>";

                // send email to employee
                MailMessage message = util.getEmployeeViewLeaveApplicationNotApproved(
                    new Util.EmailDetails
                    {
                        supervisor_name = row["supervisor_name"],
                        date_submitted = row["date_submitted"],
                        start_date = row["start_date"],
                        end_date = row["end_date"],
                        days_taken = row["days_taken"],
                        type_of_leave = row["leave_type"],
                        qualified = row["qualified"],
                        comment = emailCommentString,
                        subject = "Leave Application Not Approved",
                        recipient = row["employeeEmail"]
                    }
                );
                isEmployeeEmailSent = util.sendMail(message);


                // send supervisor email
                message = util.getSupervisorViewLeaveApplicationNotApproved(
                    new Util.EmailDetails
                    {
                        employee_name = row["employee_name"],
                        date_submitted = row["date_submitted"],
                        start_date = row["start_date"],
                        end_date = row["end_date"],
                        days_taken = row["days_taken"],
                        type_of_leave = row["leave_type"],
                        qualified = row["qualified"],
                        comment = emailCommentString,
                        subject = $"Leave Application Not Approved for {row["employee_name"]}",
                        recipient = row["supervisorEmail"]
                    }
                );
                isSupervisorEmailSent = util.sendMail(message);

                if (isEmployeeEmailSent && isSupervisorEmailSent)
                {
                    row["isEmailSentAlready"] = "Yes";
                    notApprovedRow = row;
                } else
                    errorSubmittingEmailMsgPanel.Visible = true;

                // SEND IN HOUSE NOTIFS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

                // send notif to employee
                string notif_header = $"Leave Application Not Approved",
                       notification = $"Your leave application to {row["supervisor_name"]} for {row["days_taken"]} day(s) {row["leave_type"]} leave was not approved.";

                if (!util.isNullOrEmpty(comment))
                    notification += $"HR said \" {comment} \"";
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();

                        string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', @Notification , 'No', '{row["employee_id"]}', '{util.getCurrentDate()}');
                            ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@Notification", notification);
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }


                // send notif to supervisor
                notif_header = $"Leave Application Not Approved for {row["employee_name"]}";
                notification = $"The leave application of {row["employee_name"]} for {row["days_taken"]} day(s) {row["leave_type"]} leave was not approved.";

                if (!util.isNullOrEmpty(comment))
                    notification += $"HR said \" {comment} \".";

                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();

                        string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', @Notification, 'No', '{row["supervisor_id"]}', '{util.getCurrentDate()}');
                            ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@Notification", notification);
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }

        protected void sendRecommendedNotifications(GridViewRow row, string employee_id, string employeeEmail)
        {
            // sends email and in house notifs


            // SEND EMAILS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            // send email to employee notifying them that their supervisor recommended their leave to HR
            MailMessage message = util.getEmployeeViewLeaveApplicationRecommended(
                    new Util.EmailDetails
                    {
                        supervisor_name = user.currUserName,
                        date_submitted = row.Cells[GetColumnIndexByName(row, "date_submitted")].Text.ToString(),
                        start_date = row.Cells[GetColumnIndexByName(row, "start_date")].Text.ToString(),
                        end_date = row.Cells[GetColumnIndexByName(row, "end_date")].Text.ToString(),
                        days_taken = row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString(),
                        type_of_leave = row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString(),
                        qualified = row.Cells[GetColumnIndexByName(row, "qualified")].Text.ToString(),
                        subject = "Leave Application Recommended",
                        recipient = employeeEmail
                    }
            );
            util.sendMail(message);

            // send email to relevant HR officers letting them know that supervisor recommended application
            // get relevant HR officers info: id, emails
            Dictionary<string, string> hr_info = new Dictionary<string, string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    string sql = $@"
                                    SELECT hr_e.employee_id, hr_e.email 
                                    FROM
                                    (
	                                    (
		                                    -- this block is a list of all the relevant hr 2s and all hr 1s
		                                    (
			                                    -- this block gets all hr 2 personnel that deal with the same employment type as the employee in question
			                                    SELECT er.employee_id 
			                                    FROM dbo.employeerole er
			                                    WHERE er.role_id = 'hr2'

			                                    INTERSECT

			                                    SELECT er.employee_id 
			                                    FROM dbo.employeerole er

			                                    JOIN dbo.employee e 
			                                    ON e.employee_id = {employee_id}

			                                    LEFT JOIN dbo.employeeposition ep                                          
			                                    ON ep.employee_id = e.employee_id AND ep.id = dbo.getActiveRecord({employee_id})

			                                    WHERE er.role_id =  IIF(ep.employment_type = 'Contract', 'hr_contract', 'hr_public_officer')
		                                    )


		                                    UNION 

		                                    (
			                                    -- this block gets all hr 1 personnel
			                                    SELECT er.employee_id 
			                                    FROM dbo.employeerole er
			                                    WHERE er.role_id = 'hr1'
		                                    )
	                                    )

	                                    INTERSECT

	                                    (
		                                    -- this block gets all active hr personnel
		                                    SELECT er.employee_id 
		                                    FROM dbo.employeerole er

		                                    LEFT JOIN dbo.employeeposition hr_ep
		                                    ON hr_ep.employee_id = er.employee_id AND dbo.isRecordActive(hr_ep.id) = 1

		                                    WHERE hr_ep.dept_id = 2
	                                    )
                                    ) hr_ids

                                    JOIN dbo.employee hr_e
                                    ON hr_e.employee_id = hr_ids.employee_id
                                ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                hr_info[reader["employee_id"].ToString()] = reader["email"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            string[] hr_emails = new string[hr_info.Count];
            hr_info.Values.CopyTo(hr_emails, 0);

            message = util.getHRViewLeaveApplicationRecommended(
                    new Util.EmailDetails
                    {
                        supervisor_name = user.currUserName,
                        employee_name = row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString(),
                        date_submitted = row.Cells[GetColumnIndexByName(row, "date_submitted")].Text.ToString(),
                        start_date = row.Cells[GetColumnIndexByName(row, "start_date")].Text.ToString(),
                        end_date = row.Cells[GetColumnIndexByName(row, "end_date")].Text.ToString(),
                        days_taken = row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString(),
                        type_of_leave = row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString(),
                        qualified = row.Cells[GetColumnIndexByName(row, "qualified")].Text.ToString(),
                        subject = $"Leave Application Recommended for {row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString()}",
                        recipient = $"{ String.Join(";", hr_emails) }" // add all relevant emails from HR to recipient list
                    }
            );
            util.sendMail(message);

            // SEND IN HOUSE NOTIFS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            // send notif to employee
            string notif_header = $"Leave Application Recommended",
                   notification = $"Your leave application to {user.currUserName} for {row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString()} day(s) {row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString()} leave was recommended";
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', '{notification}', 'No', '{employee_id}', '{util.getCurrentDate()}');
                            ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // send notif(s) to HR
            foreach (string id in hr_info.Keys)
            {
                notif_header = $"Leave application recommended for {row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString()}";
                notification = $"{user.currUserName} recommended the leave application of {row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString()} for {row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString()} day(s) {row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString()} leave";
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();

                        string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', '{notification}', 'No', '{id}', '{util.getCurrentDate()}');
                            ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

        }

        protected void sendNotRecommendedNotifications(string comment)
        {
            // sends email and in house notifs

            Dictionary<string, string> row = null;
            if (notRecommendedRow != null)
                row = notRecommendedRow;

            if (row != null && row["isEmailSentAlready"] == "No")
            {
                // SEND EMAILS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

                string emailCommentString = string.Empty;

                if (!util.isNullOrEmpty(comment))
                    emailCommentString = $"Your supervisor left the following comment: <blockquote style='font-size:1.1em;'>\"{comment}\"</blockquote>";

                // send email to employee
                MailMessage message = util.getEmployeeViewLeaveApplicationNotRecommended(
                    new Util.EmailDetails
                    {
                        supervisor_name = row["supervisor_name"],
                        date_submitted = row["date_submitted"],
                        start_date = row["start_date"],
                        end_date = row["end_date"],
                        days_taken = row["days_taken"],
                        type_of_leave = row["leave_type"],
                        qualified = row["qualified"],
                        comment = emailCommentString,
                        subject = "Leave Application Not Recommended",
                        recipient = row["employeeEmail"]
                    }
                );

                if (util.sendMail(message))
                {
                    row["isEmailSentAlready"] = "Yes";
                    notRecommendedRow = row;
                }
                else
                {
                    notRecommendedRow = null;
                    errorSubmittingEmailMsgPanel.Visible = true;
                }
                    

                // SEND IN HOUSE NOTIFS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                
                // send notif to employee
                string notif_header = $"Leave Application Not Recommended",
                       notification = $"Your leave application to {user.currUserName} for {row["days_taken"]} day(s) {row["leave_type"]} leave was not recommended.";

                if (!util.isNullOrEmpty(comment))
                    notification += $"Your supervisor said \" {comment} \"";
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();

                        string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', @Notification, 'No', '{row["employee_id"]}', '{util.getCurrentDate()}');
                            ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@Notification", notification);
                            int rowsAffected = command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

        }

        protected void sendApprovalUndoneNotifications(GridViewRow row,string employeeId, string supervisorId, string employeeEmail, string supervisorEmail)
        {
            // sends email and in house notifs


            // SEND EMAILS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            // send email to employee notifying them that their supervisor recommended their leave to HR
            MailMessage message = util.getEmployeeViewLeaveApprovalUndone(
                    new Util.EmailDetails
                    {
                        supervisor_name = row.Cells[GetColumnIndexByName(row, "supervisor_name")].Text.ToString(),
                        date_submitted = row.Cells[GetColumnIndexByName(row, "date_submitted")].Text.ToString(),
                        start_date = row.Cells[GetColumnIndexByName(row, "start_date")].Text.ToString(),
                        end_date = row.Cells[GetColumnIndexByName(row, "end_date")].Text.ToString(),
                        days_taken = row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString(),
                        type_of_leave = row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString(),
                        qualified = row.Cells[GetColumnIndexByName(row, "qualified")].Text.ToString(),
                        subject = "Leave Application Approval Undone",
                        recipient = employeeEmail
                    }
            );
            util.sendMail(message);

            // send email to supervisor letting them know that HR undid approval for employee
            message = util.getSupervisorViewLeaveApprovalUndone(
                    new Util.EmailDetails
                    {
                        employee_name = row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString(),
                        date_submitted = row.Cells[GetColumnIndexByName(row, "date_submitted")].Text.ToString(),
                        start_date = row.Cells[GetColumnIndexByName(row, "start_date")].Text.ToString(),
                        end_date = row.Cells[GetColumnIndexByName(row, "end_date")].Text.ToString(),
                        days_taken = row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString(),
                        type_of_leave = row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString(),
                        qualified = row.Cells[GetColumnIndexByName(row, "qualified")].Text.ToString(),
                        recipient = supervisorEmail,
                        subject = $"Leave Application Approval Undone for {row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString()}"
                    }
                    );
            util.sendMail(message);

            // SEND IN APPLICATION NOTIFS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            // send notif to employee
            string notif_header = $"Leave Application Approval Undone",
                   notification = $"Your approval for the leave application to {row.Cells[GetColumnIndexByName(row, "supervisor_name")].Text.ToString()} for {row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString()} day(s) {row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString()} leave was undone.";
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', '{notification}', 'No', '{employeeId}', '{util.getCurrentDate()}');
                            ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            // send notif to supervisor
            notif_header = $"Leave Application Approval for {row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString()} was Undone";
            notification = $"The approval for the leave application of {row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString()} for {row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString()} day(s) {row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString()} leave was undone.";
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', '{notification}', 'No', '{supervisorId}', '{util.getCurrentDate()}');
                            ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void resetNumNotifications()
        {
            // set number of notifications
            Label num_notifs = (Label)Page.Master.FindControl("num_notifications");
            num_notifs.Text = util.resetNumNotifications(user.currUserId);

            UpdatePanel up = (UpdatePanel)Page.Master.FindControl("notificationsUpdatePanel");
            up.Update();
        }
        // _________________________________________________________________

        
        // COMMENT MODAL ACTIVATED WHEN AN EMPLOYEE IS NOT RECOMMENDED BY THEIR SUPERVISOR OR NOT APPROVED BY HR METHODS
        protected void submitCommentBtn_Click(object sender, EventArgs e)
        {
            // submit comment to leave transaction 

            //get id of transaction, id of employee, previous comment on LA and current comment on LA
            string leaveId = commentModalTransactionIdHiddenField.Value,
                   employeeId = commentModalEmployeeIdHiddenField.Value,
                   previousComments = previousCommentsHiddenField.Value,
                   comment = commentsTxtArea.InnerText;

            Boolean isCommentChanged = comment != previousComments;

            if (isCommentChanged)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = string.Empty;

                        if(gridViewType == "sup")
                        {
                            sql = $@"
                            UPDATE [dbo].leavetransaction 
                            SET 
                                sup_comment = @Comments
                            WHERE transaction_id = {leaveId};";
                        } else if (gridViewType == "hr")
                        {
                            sql = $@"
                            UPDATE [dbo].leavetransaction 
                            SET 
                                hr_comment = @Comments
                            WHERE transaction_id = {leaveId};";
                        }
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@Comments", comment);
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                noEditsMadePanel.Visible = false;

                                // show success message
                                if (util.isNullOrEmpty(previousComments))
                                    addCommentSuccessPanel.Visible = true;
                                else
                                    editCommentSuccessPanel.Visible = true;
                                    
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //exception logic
                    throw ex;
                }

                // add audit log
                string supOrHr = gridViewType == "sup" ? "supervisor comment" : "hr comment";
                string action = $"Edited leave application; leave_transaction_id= {leaveId}, {supOrHr}= {comment}";
                util.addAuditLog(user.currUserId, employeeId, action);
            }
            else
            {
                editCommentSuccessPanel.Visible = false;
                addCommentSuccessPanel.Visible = false;
                noEditsMadePanel.Visible = true;
            }

            commentsUpdatePanel.Update();

            if (gridViewType == "sup")
                sendNotRecommendedNotifications(comment);
            else if (gridViewType == "hr")
                sendNotApprovedNotifications(comment);

            submitCommentBtn.Visible = false;
            resetNumNotifications();
        }

        protected void closeCommentModal_Click(object sender, EventArgs e)
        {
            // hide modal
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#commentsModal').modal('hide');", true);

            string comment = util.isNullOrEmpty(commentsTxtArea.InnerText) ? string.Empty : commentsTxtArea.InnerText;
            if (gridViewType == "sup")
                sendNotRecommendedNotifications(comment);
            else if (gridViewType == "hr")
                sendNotApprovedNotifications(comment);
        }
        // _________________________________________________________________

    }
}



