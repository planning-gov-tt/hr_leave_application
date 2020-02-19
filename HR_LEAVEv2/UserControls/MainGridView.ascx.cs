using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;

using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using HR_LEAVEv2.Classes;
using System.Web.UI;

namespace HR_LEAVEv2.UserControls
{
    public partial class MainGridView : System.Web.UI.UserControl
    {
        Util util = new Util();

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
                e.last_name + ', ' + LEFT(e.first_name, 1) + '.' AS employee_name,
                ep.employment_type employment_type,

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
                LEFT JOIN [dbo].employeeposition ep ON ep.employee_id = lt.employee_id AND GETDATE()>=ep.start_date AND ep.actual_end_date IS NULL
            ";

        private string connectionString = ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString;

        // VARIABLES TO CHANGE VIEWSTATE VARIABLES //
        // a simple toggle
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

        // filter sql where clause variables
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

        private string EmployeeName_ID
        {
            get { return ViewState["EmployeeName_ID"] != null ? ViewState["EmployeeName_ID"].ToString() : null; }
            set { ViewState["EmployeeName_ID"] = value; }
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
                divTbEmployee.Visible = false;
            }
            else if (this.gridViewType == "sup")
            {
                // show ALL transactions submitted TO employee_id (sup) for recommedation
                // Filter 
                // show "Recommended" and "Not Recommended", "Date Change Requested" 

                // Hide Supervisor Columnn
                GridView.Columns[1].Visible = false;
                btnSupVisible = true;
                divTbSupervisor.Visible = false;
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
                // default sort on date submitted DESC
                this.SortExpression = "date_submitted";
                this.SortDirection = "DESC";                

                this.BindGridView();
            }
        }

        private void BindGridView()
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                SqlCommand sqlCommand = new SqlCommand();
                
                string whereBindGridView = "";

                // emp gridview
                if (gridViewType == "emp")
                {
                    whereBindGridView = $@"
                        WHERE
                            e.employee_id = '{Session["emp_id"]}' 
                    ";
                }

                // sup gridview
                else if (gridViewType == "sup")
                {
                    whereBindGridView = $@"
                        WHERE
                            supervisor_id = '{Session["emp_id"]}'
                    ";
                }

                // hr gridview (most complex)
                else if (gridViewType == "hr")
                {
                    whereBindGridView = $@"
                        WHERE
                            status IN ('Not Recommended', 'Recommended', 'Approved', 'Not Approved') AND
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

                    // check if hr can see contracts, public_officers or both based on their permission pool
                    string employmentTypes = "(";
                    bool hasContract = false;
                    if (permissions.Contains("contract_permissions"))
                    {
                        employmentTypes += "'Contract'";
                        hasContract = true;
                    }
                    if (permissions.Contains("public_officer_permissions"))
                    {
                        if (hasContract)
                            employmentTypes += ",";
                        employmentTypes += "'Public Service'";
                    }
                    employmentTypes += ")";
                    whereBindGridView += $@"
                        AND employment_type IN {employmentTypes}            
                    ";

                }// end hr if


                //*************************************** FILTER ***************************************//
                // add sql parameters in command for user entered values that did not have validation
                string whereFilterGridView = "";
                if (!string.IsNullOrEmpty(SubmittedFrom))
                {
                    whereFilterGridView += $@"
                        AND lt.created_at >= '{DateTime.ParseExact(SubmittedFrom, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy")}'
                    ";
                }
                if (!string.IsNullOrEmpty(SubmittedTo))
                {
                    whereFilterGridView += $@"
                        AND lt.created_at <= '{DateTime.ParseExact(SubmittedTo, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy")}'
                    ";
                }
                if (!string.IsNullOrEmpty(StartDate))
                {
                    whereFilterGridView += $@"
                        AND lt.start_date >= '{DateTime.ParseExact(StartDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy")}'
                    ";
                }
                if (!string.IsNullOrEmpty(EndDate))
                {
                    whereFilterGridView += $@"
                        AND lt.end_date <= '{DateTime.ParseExact(EndDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture).ToString("MM/d/yyyy")}'
                    ";
                }
                if (!string.IsNullOrEmpty(SupervisorName_ID))
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
                if (!string.IsNullOrEmpty(EmployeeName_ID))
                {
                    whereFilterGridView += $@"
                        AND (
                            (e.first_name LIKE @EmployeeName_ID) OR
                            (e.last_name LIKE @EmployeeName_ID) OR
                            (e.employee_id LIKE @EmployeeName_ID) OR
                            (e.email LIKE @EmployeeName_ID)
                        ) 
                    ";
                    sqlCommand.Parameters.AddWithValue("@EmployeeName_ID","%" + EmployeeName_ID + "%");
                }
                if (!string.IsNullOrEmpty(LeaveType))
                {
                    whereFilterGridView += $@"
                        AND leave_type = '{LeaveType}'
                    ";
                }
                if (!string.IsNullOrEmpty(Status))
                {
                    whereFilterGridView += $@"
                        AND status = '{Status}'
                    ";
                }

                if (!string.IsNullOrEmpty(Qualified))
                {
                    whereFilterGridView += $@"
                        AND qualified = '{Qualified}'
                    ";
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
                    LinkButton btnCancelLeave = (LinkButton)e.Row.FindControl("btnCancelLeave");

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
                    LinkButton btnNotRecommended = (LinkButton)e.Row.FindControl("btnNotRecommended");
                    LinkButton btnRecommended = (LinkButton)e.Row.FindControl("btnRecommended");
                    LinkButton btnEditLeaveRequest = (LinkButton)e.Row.FindControl("btnEditLeaveRequest");

                    // only show buttons if HR has not acted or pending
                    // else do not show buttons
                    if (leaveStatus == "Pending" || leaveStatus == "Recommended" || leaveStatus == "Not Recommended")
                    {
                        if(leaveStatus == "Recommended")
                            btnRecommended.Style.Add("opacity", "0.55");
                        else if(leaveStatus == "Not Recommended")
                            btnNotRecommended.Style.Add("opacity", "0.55");
                        btnNotRecommended.Visible = btnRecommended.Visible = true;
                    }
                    else
                    {
                        btnNotRecommended.Visible = btnRecommended.Visible = btnEditLeaveRequest.Visible = false;
                    }
                }
            }

            // if employee view OR hr view
            else if (gridViewType == "hr")
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    // get HR buttons
                    LinkButton btnNotApproved = (LinkButton)e.Row.FindControl("btnNotApproved");
                    LinkButton btnApproved = (LinkButton)e.Row.FindControl("btnApproved");
                    LinkButton btnEditLeaveRequest = (LinkButton)e.Row.FindControl("btnEditLeaveRequest");
                    LinkButton btnUndoApprove = (LinkButton)e.Row.FindControl("btnUndoApprove");

                    // if recommended or not approved then show all buttons except undo
                    if (leaveStatus == "Recommended" || leaveStatus == "Not Approved")
                    {

                        if(leaveStatus == "Not Approved")
                            btnNotApproved.Style.Add("opacity", "0.55");

                        btnUndoApprove.Visible = false;
                        btnNotApproved.Visible = btnApproved.Visible = btnEditLeaveRequest.Visible = true;
                    }

                    // if approved then ONLY show undo button
                    else if (leaveStatus == "Approved")
                    {

                        int startDateIndex = GetColumnIndexByName(e.Row, "start_date");
                        string startDate = e.Row.Cells[startDateIndex].Text.ToString();

                        if (!string.IsNullOrEmpty(startDate))
                        {
                            DateTime start = DateTime.MinValue;
                            try
                            {
                                //start = Convert.ToDateTime(startDate);
                                start = DateTime.ParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            } catch(FormatException fe)
                            {
                                throw fe;
                            }

                            // if today is a day before the start date of the application then show undo button
                            if (DateTime.Compare(DateTime.Today, start) < 0)
                                btnUndoApprove.Visible = true;
                            else
                                btnUndoApprove.Visible = false;
                        }
                       
                        btnNotApproved.Visible = btnApproved.Visible = btnEditLeaveRequest.Visible = false;
                    }
                    else if (leaveStatus == "Not Recommended")
                    {
                        btnUndoApprove.Visible = false;
                        btnNotApproved.Visible = btnApproved.Visible = btnEditLeaveRequest.Visible = false;
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

            if (commandName == "details")
                Response.Redirect("~/Employee/ApplyForLeave.aspx?mode=view&leaveId=" + transaction_id + "&returnUrl=" + HttpContext.Current.Request.Url.AbsolutePath);

            if (commandName == "editLeaveRequest")
            {
                // redirect to apply for leave form and prepopulate...
                Response.Redirect("~/Employee/ApplyForLeave.aspx?mode=edit&leaveId=" + transaction_id + "&returnUrl=" + HttpContext.Current.Request.Url.AbsolutePath);
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
            else if (gridViewType == "sup" || gridViewType == "hr")
            {

                if (gridViewType == "sup")
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
                                [hr_manager_id] = '{Session["emp_id"]}', 
                                [hr_manager_edit_date] = CURRENT_TIMESTAMP
                            ";
                        sqlCommand.CommandText = updateStatement + setStatement + whereStatement;
                    }
                    else // approve (subtract) or undo (add)
                    {
                        int daysTakenIndex = GetColumnIndexByName(row, "days_taken");

                        // get difference
                        int difference = Convert.ToInt32(row.Cells[daysTakenIndex].Text);

                        string leaveType = "";
                        if (row.RowType == DataControlRowType.DataRow) // makes sure it is not a header row, only data row
                        {
                            int indexLeavetype = GetColumnIndexByName(row, "leave_type");
                            leaveType = (row.Cells[indexLeavetype].Text).ToString();
                        }

                        // check leave type to match with balance type
                        Dictionary<string, string> leaveBalanceColumnName = new Dictionary<string, string>();
                        leaveBalanceColumnName.Add("Bereavement", "[bereavement]");
                        leaveBalanceColumnName.Add("Casual", "[casual]");
                        leaveBalanceColumnName.Add("Maternity", "[maternity]");
                        leaveBalanceColumnName.Add("Personal", "[personal]");
                        leaveBalanceColumnName.Add("Pre-retirement", "[pre_retirement]");
                        leaveBalanceColumnName.Add("Sick", "[sick]");
                        leaveBalanceColumnName.Add("Vacation", "[vacation]");

                        // TODO: check if in good standing AKA Qualified (if employee has enough leave)

                        // approved => subtract leave from balance
                        string operation = "";
                        if (commandName == "approved")
                        {
                            // update status to Approved
                            status = "Approved";
                            //message.Subject = "Leave Application Approved";

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
            }
            
            Boolean isQuerySuccessful;
            // execute sql
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                sqlCommand.Connection = sqlConnection;
                int rowsAffected = sqlCommand.ExecuteNonQuery();

                isQuerySuccessful = rowsAffected > 0;
                BindGridView(); // preserve the sort direction in the viewstate
            }

            // add audit logs
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
                    actingEmployeeID = affectedEmployeeId = Session["emp_id"].ToString();
                    action = "Canceled leave application";
                }

                // Recommend
                if (commandName == "recommended")
                {
                    sendRecommendedNotifications(row, employee_id, employeeEmail);

                    actingEmployeeID = Session["emp_id"].ToString();
                    affectedEmployeeId = employee_id;
                    action = "Recommended leave application";
                }

                // Not Recommend
                if (commandName == "notRecommended")
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

                    commentsTxtArea.InnerText = previousCommentsHiddenField.Value = !String.IsNullOrEmpty(previousComment) ? previousComment : string.Empty;
                    Dictionary<string, string> rowData = new Dictionary<string, string>()
                    {
                        { "date_submitted", row.Cells[GetColumnIndexByName(row, "date_submitted")].Text.ToString() },
                        { "employee_name", row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString() },
                        { "employee_id", employee_id},
                        { "supervisor_name", Session["emp_username"].ToString() },
                        { "start_date", row.Cells[GetColumnIndexByName(row, "start_date")].Text.ToString() },
                        { "end_date", row.Cells[GetColumnIndexByName(row, "end_date")].Text.ToString() },
                        { "days_taken", row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString() },
                        { "leave_type", row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString() },
                        { "status", row.Cells[GetColumnIndexByName(row, "status")].Text.ToString() },
                        { "qualified",row.Cells[GetColumnIndexByName(row, "qualified")].Text.ToString() },
                        { "isEmailSentAlready", row.Cells[GetColumnIndexByName(row, "status")].Text.ToString() == "Not Recommended" ? "Yes" : "No"},
                        { "employeeEmail", employeeEmail }
                    };
                    
                    ViewState["notRecommendedRow"] = rowData;

                    // reset user feedback panels
                    noEditsMadePanel.Visible = false;
                    addCommentSuccessPanel.Visible = false;
                    editCommentSuccessPanel.Visible = false;

                    // show modal for user to enter comment
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#commentsModal').modal({'show':true, 'backdrop':'static', 'keyboard':false});", true);

                    // populate audit log variables 
                    actingEmployeeID = Session["emp_id"].ToString();
                    affectedEmployeeId = employee_id;
                    action = "Unrecommended leave application";
                }

                // Approve
                if (commandName == "approved")
                {

                    sendApprovedNotifications(row, employee_id, GridView.DataKeys[index].Values["supervisor_id"].ToString(),employeeEmail, supervisorEmail);

                    actingEmployeeID = Session["emp_id"].ToString();
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

                    commentsTxtArea.InnerText = previousCommentsHiddenField.Value = !String.IsNullOrEmpty(previousComment) ? previousComment : string.Empty;
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

                    ViewState["notApprovedRow"] = rowData;

                    // reset user feedback panels
                    noEditsMadePanel.Visible = false;
                    addCommentSuccessPanel.Visible = false;
                    editCommentSuccessPanel.Visible = false;

                    // show modal for user to enter comment
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#commentsModal').modal({'show':true, 'backdrop':'static', 'keyboard':false});", true);

                    actingEmployeeID = Session["emp_id"].ToString();
                    affectedEmployeeId = employee_id;
                    action = "Unapproved leave application";
                }

                // Undo Approve
                if (commandName == "undoApprove")
                {
                    actingEmployeeID = Session["emp_id"].ToString();
                    affectedEmployeeId = employee_id;
                    action = "Undid approval for leave application";
                }

                resetNumNotifications();

                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = $@"
                                    INSERT INTO [dbo].[auditlog] ([acting_employee_id], [acting_employee_name], [affected_employee_id], [affected_employee_name], [action], [created_at])
                                    VALUES ( 
                                        @ActingEmployeeId, 
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @ActingEmployeeId), 
                                        @AffectedEmployeeId,
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @AffectedEmployeeId), 
                                        @Action, 
                                        @CreatedAt);
                                ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@ActingEmployeeId", actingEmployeeID);
                            command.Parameters.AddWithValue("@AffectedEmployeeId", affectedEmployeeId);

                            /*
                             * Add id of leave transaction recommended
                             * */

                            command.Parameters.AddWithValue("@Action", $"{action}; ID= {transaction_id}");

                            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM-dd-yyyy h:mm tt"));
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //exception logic
                    throw ex;
                }
            }
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

        protected Boolean validateDates(string startDate, string endDate, int type)
        {
            if (String.IsNullOrEmpty(startDate) && String.IsNullOrEmpty(endDate))
                return true;
            // type 0: submitted from, submitted to
            // type 1: start date, end date

            DateTime start, end;
            start = end = DateTime.MinValue;
            Boolean isValidated = true;

            // validate start date is a date
            if (!DateTime.TryParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start) || String.IsNullOrEmpty(startDate))
            {
                if (type == 0)
                    invalidSubmittedFromDate.Style.Add("display", "inline-block");
                else
                    invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }

            // validate end date is a date
            if (!DateTime.TryParseExact(endDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out end) || String.IsNullOrEmpty(endDate))
            {
                if (type == 0)
                    invalidSubmittedToDate.Style.Add("display", "inline-block");
                else
                    invalidEndDateValidationMsgPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }
      
            if (isValidated)
            {

                // compare dates to ensure end date is not before start date
                if (DateTime.Compare(start, end) > 0)
                {
                    if (type == 0)
                    {
                        invalidSubmittedToDate.Style.Add("display", "inline-block");
                        submittedDateComparisonValidationMsgPanel.Style.Add("display", "inline-block");
                    }
                    else{
                        invalidEndDateValidationMsgPanel.Style.Add("display", "inline-block");
                        appliedForDateComparisonValidationMsgPanel.Style.Add("display", "inline-block");
                    }

                    isValidated = false;
                }
            }
            return isValidated;
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            // check all textboxes/drop down lists and set the respective variables 
            // these variables are then used in the

            // reset all values to null throughout??

            // check if control exists first?? (because some may be hidden)
            SubmittedFrom = tbSubmittedFrom.Text.ToString();
            SubmittedTo = tbSubmittedTo.Text.ToString(); 

            StartDate = tbStartDate.Text.ToString();
            EndDate = tbEndDate.Text.ToString(); 

            // validate all dates
            invalidStartDateValidationMsgPanel.Style.Add("display", "none");
            invalidEndDateValidationMsgPanel.Style.Add("display", "none");
            appliedForDateComparisonValidationMsgPanel.Style.Add("display", "none");
            invalidSubmittedFromDate.Style.Add("display", "none");
            invalidSubmittedToDate.Style.Add("display", "none");
            submittedDateComparisonValidationMsgPanel.Style.Add("display", "none");

            bool areSubmitDatesValidated = false, areAppliedDatesValidated = false;
            areSubmitDatesValidated = validateDates(SubmittedFrom, SubmittedTo, 0);
            areAppliedDatesValidated = validateDates(StartDate, EndDate, 1);
            if (areSubmitDatesValidated && areAppliedDatesValidated)
            {
                // display appropriate message in the notification panel

                SupervisorName_ID = tbSupervisor.Text.ToString();
                EmployeeName_ID = tbEmployee.Text.ToString();

                LeaveType = ddlType.SelectedValue.ToString();
                Status = ddlStatus.SelectedValue.ToString();
                Qualified = ddlQualified.SelectedValue.ToString();

                this.BindGridView();
            }
        }

        protected void clearFilterBtn_Click(object sender, EventArgs e)
        {
            // clear all filters 
            SubmittedFrom = String.Empty;
            tbSubmittedFrom.Text = String.Empty;
            SubmittedTo = String.Empty;
            tbSubmittedTo.Text = String.Empty;


            StartDate = String.Empty;
            tbStartDate.Text = String.Empty;
            EndDate = String.Empty;
            tbEndDate.Text = String.Empty;

            invalidStartDateValidationMsgPanel.Style.Add("display", "none");
            invalidEndDateValidationMsgPanel.Style.Add("display", "none");
            appliedForDateComparisonValidationMsgPanel.Style.Add("display", "none");
            invalidSubmittedFromDate.Style.Add("display", "none");
            invalidSubmittedToDate.Style.Add("display", "none");
            submittedDateComparisonValidationMsgPanel.Style.Add("display", "none");

            SupervisorName_ID = String.Empty;
            tbSupervisor.Text = String.Empty;
            EmployeeName_ID = String.Empty;
            tbEmployee.Text = String.Empty;

            LeaveType = String.Empty;
            ddlType.SelectedIndex = 0;
            Status = String.Empty;
            ddlStatus.SelectedIndex = 0;
            Qualified = String.Empty;
            ddlQualified.SelectedIndex = 0;


            this.BindGridView();
        }

        protected void sendApprovedNotifications(GridViewRow row, string employee_id, string supervisor_id, string employeeEmail, string supervisorEmail)
        {

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

            // send in application notifications
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
                                VALUES('{notif_header}', '{notification}', 'No', '{employee_id}', '{DateTime.Now}');
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
                                VALUES('{notif_header}', '{notification}', 'No', '{supervisor_id}', '{DateTime.Now}');
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
            Dictionary<string, string> row = null;
            if (ViewState["notApprovedRow"] != null)
                row = (Dictionary<string, string>)ViewState["notApprovedRow"];

            Boolean isEmployeeEmailSent = false,
                    isSupervisorEmailSent = false;

            if (row != null && row["isEmailSentAlready"] == "No")
            {

                // send email notifications

                string emailCommentString = string.Empty;
                if (!String.IsNullOrEmpty(comment) && !String.IsNullOrWhiteSpace(comment))
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
               
                if(isEmployeeEmailSent && isSupervisorEmailSent)
                {
                    row["isEmailSentAlready"] = "Yes";
                    ViewState["notApprovedRow"] = row;
                }


                // send in application notifications

                // send notif to employee
                string notif_header = $"Leave Application Not Approved",
                       notification = $"Your leave application to {row["supervisor_name"]} for {row["days_taken"]} day(s) {row["leave_type"]} leave was not approved.";

                if (!String.IsNullOrEmpty(comment) && !String.IsNullOrWhiteSpace(comment))
                    notification += $"HR said \" {comment} \"";
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();

                        string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', @Notification , 'No', '{row["employee_id"]}', '{DateTime.Now}');
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

                if (!String.IsNullOrEmpty(comment) && !String.IsNullOrWhiteSpace(comment))
                    notification += $"HR said \"{comment}\".";

                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();

                        string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', '{notification}', 'No', '{row["supervisor_id"]}', '{DateTime.Now}');
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

        protected void sendRecommendedNotifications(GridViewRow row, string employee_id, string employeeEmail)
        {
            // send email notifications

            // send email to employee notifying them that their supervisor recommended their leave to HR
            MailMessage message = util.getEmployeeViewLeaveApplicationRecommended(
                    new Util.EmailDetails
                    {
                        supervisor_name = Session["emp_username"].ToString(),
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
            message = util.getHRViewLeaveApplicationRecommended(
                    new Util.EmailDetails
                    {
                        supervisor_name = Session["emp_username"].ToString(),
                        employee_name = row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString(),
                        date_submitted = row.Cells[GetColumnIndexByName(row, "date_submitted")].Text.ToString(),
                        start_date = row.Cells[GetColumnIndexByName(row, "start_date")].Text.ToString(),
                        end_date = row.Cells[GetColumnIndexByName(row, "end_date")].Text.ToString(),
                        days_taken = row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString(),
                        type_of_leave = row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString(),
                        qualified = row.Cells[GetColumnIndexByName(row, "qualified")].Text.ToString(),
                        subject = $"Leave Application Recommended for {row.Cells[GetColumnIndexByName(row, "employee_name")].Text.ToString()}",
                        recipient = "Tristan.Sankar@planning.gov.tt" // add all relevant emails from HR to recipient list
                    }
            );
            util.sendMail(message);

            // send in application notifications

            // send notif to employee
            string notif_header = $"Leave Application Recommended",
                   notification = $"Your leave application to {Session["emp_username"].ToString()} for {row.Cells[GetColumnIndexByName(row, "days_taken")].Text.ToString()} day(s) {row.Cells[GetColumnIndexByName(row, "leave_type")].Text.ToString()} leave was recommended";
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', '{notification}', 'No', '{employee_id}', '{DateTime.Now}');
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

            // TODO: send notifs to relevant HRs - iterate through each relevant HR id and send notifs

        }

        protected void sendNotRecommendedNotifications(string comment)

        {
            Dictionary<string, string> row = null;
            if (ViewState["notRecommendedRow"] != null)
                row = (Dictionary<string, string>)ViewState["notRecommendedRow"];

            if(row != null && row["isEmailSentAlready"] == "No")
            {
                // send email


                string emailCommentString = string.Empty;

                if (!String.IsNullOrEmpty(comment) && !String.IsNullOrWhiteSpace(comment))
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
                    ViewState["notRecommendedRow"] = row;
                }
                else
                    ViewState["notRecommendedRow"] = null;


                // send in application notifs
                // send notif to employee
                string notif_header = $"Leave Application Not Recommended",
                       notification = $"Your leave application to {Session["emp_username"].ToString()} for {row["days_taken"]} day(s) {row["leave_type"]} leave was not recommended.";

                if (!String.IsNullOrEmpty(comment) && !String.IsNullOrWhiteSpace(comment))
                    notification += $"Your supervisor said \" {comment} \"";
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();

                        string sql = $@"
                                INSERT INTO [dbo].[notifications] ([notification_header], [notification], [is_read], [employee_id], [created_at])
                                VALUES('{notif_header}', @Notification, 'No', '{row["employee_id"]}', '{DateTime.Now}');
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

        protected void resetNumNotifications()
        {
            // set number of notifications
            Label num_notifs = (Label)Page.Master.FindControl("num_notifications");
            num_notifs.Text = util.resetNumNotifications(Session["emp_id"].ToString());

            UpdatePanel up = (UpdatePanel)Page.Master.FindControl("notificationsUpdatePanel");
            up.Update();
        }

        protected void submitCommentBtn_Click(object sender, EventArgs e)
        {
            // submit comment to leave transaction 

            //get id of transaction
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
                                if (String.IsNullOrEmpty(previousComments) || String.IsNullOrWhiteSpace(previousComments))
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
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = $@"
                                    INSERT INTO [dbo].[auditlog] ([acting_employee_id], [acting_employee_name], [affected_employee_id], [affected_employee_name], [action], [created_at])
                                    VALUES ( 
                                        @ActingEmployeeId, 
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @ActingEmployeeId), 
                                        @AffectedEmployeeId,
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @AffectedEmployeeId), 
                                        @Action, 
                                        @CreatedAt);
                                ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@ActingEmployeeId", Session["emp_id"].ToString());
                            command.Parameters.AddWithValue("@AffectedEmployeeId", employeeId);

                            string supOrHr = gridViewType == "sup" ? "supervisor comment" : "hr comment";

                            command.Parameters.AddWithValue("@Action", $"Edited leave application; leave_transaction_id= {leaveId}, {supOrHr}= {comment}");
                            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM-dd-yyyy h:mm tt"));
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //exception logic
                    throw ex;
                }
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

            resetNumNotifications();
        }

        protected void closeCommentModal_Click(object sender, EventArgs e)
        {
            // hide modal
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#commentsModal').modal('hide');", true);

            string comment = String.IsNullOrEmpty(commentsTxtArea.InnerText) || String.IsNullOrWhiteSpace(commentsTxtArea.InnerText) ? string.Empty : commentsTxtArea.InnerText;
            if (gridViewType == "sup")
                sendNotRecommendedNotifications(comment);
            else if (gridViewType == "hr")
                sendNotApprovedNotifications(comment);
        }
    }
}