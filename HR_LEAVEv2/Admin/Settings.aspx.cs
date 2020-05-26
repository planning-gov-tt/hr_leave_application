using HR_LEAVEv2.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.Admin
{
    public partial class Settings : System.Web.UI.Page
    {
        Util util = new Util();

        string selectedTable = string.Empty;

        enum empPositionColumns
        {
            record_id = 0,
            employee_id = 1,
            position_id = 2,
            start_date = 3,
            expected_end_date = 4,
            actual_end_date = 5,
            employment_type = 6,
            dept_id = 7,
            years_worked = 8
        };

        // holds data in the following format:
        // COLNAME, DATA TYPE, MAX AMT OF CHARS, IS NULLABLE
        // Number of rows = number of columns for the currently selected table
        private DataTable TableMetaData
        {
            get { return ViewState["tableMetaData"] != null ? ViewState["tableMetaData"] as DataTable : null; }
            set { ViewState["tableMetaData"] = value; }
        }

        private DataTable TableData
        {
            get { return ViewState["dataSource"] != null ? ViewState["dataSource"] as DataTable : null; }
            set { ViewState["dataSource"] = value; }
        }

        private String searchString
        {
            get { return ViewState["searchString"] != null ? ViewState["searchString"].ToString() : null; }
            set { ViewState["searchString"] = value; }
        }

        private Dictionary<string, string> dataBeforeEdit
        {
            get { return ViewState["dataBeforeEdit"] != null ? ViewState["dataBeforeEdit"] as Dictionary<string, string> : null; }
            set { ViewState["dataBeforeEdit"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> permissions = (List<string>)Session["permissions"];
            if(permissions == null || !permissions.Contains("admin_permissions"))
                Response.Redirect("~/AccessDenied.aspx");

            selectedTable = DropDownList1.SelectedValue.ToString() != "-" ? DropDownList1.SelectedValue.ToString() : string.Empty;
            addPanel.Visible = !util.isNullOrEmpty(selectedTable);
            TableMetaData = !util.isNullOrEmpty(selectedTable) ? TableMetaData : null;

            searchString = searchTxtbox.Text;

            clearFeedback();
            bindGridview();
        }

        /* GRIDVIEW METHODS */

        protected void bindGridview()
        {

            // once a table is selected then bind data to gridview
            if (!util.isNullOrEmpty(selectedTable))
            {

                // get data about the table's columns like data type, maximum length(if applicable) and whether it is nullable
                // this data is then used in generating the form used to create and update data
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = $@"
                            SELECT COL.COLUMN_NAME, 
                                   COL.DATA_TYPE,
                                   ISNULL(COL.CHARACTER_MAXIMUM_LENGTH, -1),
                                   COL.IS_NULLABLE
                            FROM INFORMATION_SCHEMA.COLUMNS COL
                            WHERE COL.TABLE_NAME = '{selectedTable}'
                        ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);

                                TableMetaData = dt;

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //exception logic
                    throw ex;
                }

                // get data from db for the selected table and bid
                if (TableData == null)
                {
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            string sql = $@"
                            SELECT * FROM dbo.{selectedTable};
                        ";
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                                {
                                    DataTable dt = new DataTable();
                                    adapter.Fill(dt);

                                    TableData = dt;

                                    if (dt.Rows.Count <= 0)
                                        noDataPanel.Style.Add("display", "inline-block");
                                    else
                                        searchPanel.Visible = true;

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception logic
                        throw ex;
                    }
                }

                // must destroy and recreate form on every postback
                destroyForm();
                generateForm(TableMetaData);

                //bind gridview
                GridView1.DataSource = TableData;
                GridView1.DataBind();

            }
            else
            {
                GridView1.DataSource = new DataTable();
                GridView1.DataBind();
                noTableSelectedPanel.Style.Add("display", "inline-block");
                searchPanel.Visible = false;
            }

        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {

            GridView1.PageIndex = e.NewPageIndex;
            bindGridview();
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // if unwanted command triggers this function then do nothing
            if (e.CommandName == "Sort" || e.CommandName == "Page")
            {
                return;
            }

            // get row index in which button was clicked
            int index = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = GridView1.Rows[index];

            if (row.RowType == DataControlRowType.DataRow)
            {
                if (e.CommandName == "editRow")
                {
                    Dictionary<string, string> previousData = new Dictionary<string, string>();
                    //store all old values
                    for (int i = 1; i < row.Cells.Count; i++)
                    {
                        previousData[GridView1.HeaderRow.Cells[i].Text] = row.Cells[i].Text;
                    }
                    dataBeforeEdit = previousData;

                    /* get relevant data from row by determining whether the selected table has an integer id or not. this is relevant since 
                    *  if the table does not have an integer id then the data will start at the index 2 since index 1 will be the integer id. The id
                    *  is not needed since id is not edited. Tables without an integer id can start from index 1
                    */
                    int startingIndex = (new string[] { "permission", "role", "rolepermission", "employee", "employeerole", "assignment", "leavetype", "employmenttype" }.Contains(selectedTable)) ? 1 : 2;

                    // repopulate form
                    for (int i = startingIndex; i < row.Cells.Count; i++)
                    {
                        TextBox tb = ((TextBox)formPlaceholder.FindControl($"text_{GridView1.HeaderRow.Cells[i].Text}_{selectedTable}"));

                        if (!util.isNullOrEmpty(row.Cells[i].Text))
                        {
                            // check if column type is datetime and convert data to proper format
                            if (TableMetaData.Rows[i - 1].ItemArray[1].ToString() == "datetime")
                                tb.Text = Convert.ToDateTime(row.Cells[i].Text).ToString("d/MM/yyyy");
                            else
                                tb.Text = row.Cells[i].Text;
                        }
                        else
                            tb.Text = string.Empty;

                    }

                    // setup form for edit mode
                    headerForForm.InnerText = "Edit";
                    createBtn.Visible = false;
                    EditBtn.Visible = true;
                    clearValidationErrors();

                }
                else if (e.CommandName == "deleteRow")
                {
                    string[] bridgeTables = new string[] { "assignment", "employeerole", "emptypeleavetype", "rolepermission" };
                    Boolean isDeleteSuccessful = false;
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();

                            /* used to construct the where clause based on whether the table being deleted is a bridge entity or not. Bridge entities
                               require all their elements to be included in the where to determine identity for the delete. Other tables, however,
                               only require the first element which is their primary key since only the bridge entities have a composite primary key
                            */
                            List<string> parameters = new List<string>();

                            string sql = $"DELETE FROM dbo.{selectedTable} WHERE ";

                            if (bridgeTables.Contains(selectedTable))
                            {
                                // use all columns in where clause for identity
                                List<string> whereClauseComponents = new List<string>();
                                for (int i = 1; i < GridView1.HeaderRow.Cells.Count; i++)
                                {
                                    parameters.Add($"@{GridView1.HeaderRow.Cells[i].Text.Replace("_", string.Empty)}");
                                    whereClauseComponents.Add($"{GridView1.HeaderRow.Cells[i].Text} = @{GridView1.HeaderRow.Cells[i].Text.Replace("_", string.Empty)}");
                                }

                                sql += $"{String.Join(" AND ", whereClauseComponents.ToArray())}";
                            }
                            else
                            {
                                // use only the first column in where clause. This column is the primary key of the table.
                                parameters.Add($"@{GridView1.HeaderRow.Cells[1].Text.Replace("_", string.Empty)}");
                                sql += $"{GridView1.HeaderRow.Cells[1].Text} = @{GridView1.HeaderRow.Cells[1].Text.Replace("_", string.Empty)}"; // use only first column to get identity
                            }

                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                /*
                                    starts with 1 because all parameters added start at the first column but depending on whether the selected table
                                    is a bridge entity or not, may or may not include data past just the first column.
                                */
                                int cellIndex = 1;
                                foreach (string parameter in parameters)
                                {
                                    command.Parameters.AddWithValue(parameter, row.Cells[cellIndex].Text.ToString());
                                    cellIndex++;
                                }
                                int rowsAffected = command.ExecuteNonQuery();
                                isDeleteSuccessful = rowsAffected > 0;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception logic
                        isDeleteSuccessful = false;
                    }


                    if (isDeleteSuccessful)
                    {
                        deleteSuccessfulPanel.Style.Add("display", "inline-block"); //show delete success

                        List<string> primaryKeys = new List<string>();
                        if (bridgeTables.Contains(selectedTable))
                        {
                            for (int i = 1; i < row.Cells.Count; i++)
                                primaryKeys.Add($"{row.Cells[i].Text.ToString()}");
                        }
                        else
                            primaryKeys.Add($"{row.Cells[1].Text.ToString()}");

                        // add audit log
                        util.addAuditLog(Session["emp_id"].ToString(), Session["emp_id"].ToString(), $"Admin deleted record with id = {String.Join(", ", primaryKeys.ToArray())} from {selectedTable}");
                    }
                        
                    else
                        deleteUnsuccessfulPanel.Style.Add("display", "inline-block"); // show delete failure

                    // rebind gridview based on whether search string is present and valid
                    if (searchString == null || util.isNullOrEmpty(searchString))
                    {
                        TableData = null; // null TableData reloads gridview from db
                        bindGridview();
                    }
                    else
                        searchForData(searchString); // reloads data from db based on search string

                }
            }

        }
        /* __________________________________________________________________________*/

        /* GENERATE AND DESTROY FORM FOR CREATE AND UPDATING RECORDS */
        protected void generateForm(DataTable dt)
        {

            string[] tablesWithNoIntegerId = new string[] { "permission", "role", "rolepermission", "employee", "employeerole", "assignment", "leavetype", "employmenttype" };
            int startingIndex = tablesWithNoIntegerId.Contains(selectedTable) ? 0 : 1;


            for (int rowIndex = startingIndex; rowIndex < dt.Rows.Count; rowIndex++)
            {
                // used to get index of latest td in list
                int tdListIndex = 0;

                // row where all td's are added
                HtmlGenericControl tr = new HtmlGenericControl("tr");
                
                // list of td's to add to row
                List<HtmlGenericControl> tdList = new List<HtmlGenericControl>();

                // used for generating id names and showing name of column 
                string colName = dt.Rows[rowIndex].ItemArray[0].ToString();

                tdList.Add(new HtmlGenericControl("td"));
                Label lb = new Label();
                lb.ID = $"label_{colName}_{selectedTable}";
                lb.Text = $"{colName}";
                lb.Style.Add("margin-right", "10px");
                tdList[tdListIndex].Controls.Add(lb);
                tdListIndex++;

                tdList.Add(new HtmlGenericControl("td"));
                TextBox tb = new TextBox();
                tb.ID = $"text_{colName}_{selectedTable}";
                tb.Width = new Unit("400px");

                tdList[tdListIndex].Controls.Add(tb);
                tdListIndex++;

                tdList.Add(new HtmlGenericControl("td"));

                // ensure data being input is correct type by adding the appropriate regex validator
                if (TableMetaData.Rows[rowIndex].ItemArray[1].ToString() == "int")
                {
                    RegularExpressionValidator intTypeVal = new RegularExpressionValidator();
                    intTypeVal.ID = $"int_val_{colName}_{selectedTable}";
                    intTypeVal.ErrorMessage = "Input must be numerical";
                    intTypeVal.ValidationExpression = $"^\\d+$";
                    intTypeVal.ValidationGroup = "CU_validation";
                    intTypeVal.ControlToValidate = $"text_{colName}_{selectedTable}";
                    intTypeVal.Display = ValidatorDisplay.Dynamic;
                    intTypeVal.ForeColor = Color.Red;
                    tdList[tdListIndex].Controls.Add(intTypeVal);
                }
                if (TableMetaData.Rows[rowIndex].ItemArray[1].ToString() == "datetime" || colName.Contains("date"))
                {
                    RegularExpressionValidator dateTypeVal = new RegularExpressionValidator();
                    dateTypeVal.ID = $"date_val_{colName}_{selectedTable}";
                    dateTypeVal.ErrorMessage = "Dates must be in form dd/mm/yyyy";
                    dateTypeVal.ValidationExpression = "^([0-9]|[0-2][0-9]|(3)[0-1])(\\/)(((0)[0-9])|((1)[0-2]))(\\/)\\d{4}$";
                    dateTypeVal.ValidationGroup = "CU_validation";
                    dateTypeVal.ControlToValidate = $"text_{colName}_{selectedTable}";
                    dateTypeVal.Display = ValidatorDisplay.Dynamic;
                    dateTypeVal.ForeColor = Color.Red;
                    tdList[tdListIndex].Controls.Add(dateTypeVal);
                }

                // if column has a maximum character length then add regex validator
                if (TableMetaData.Rows[rowIndex].ItemArray[2].ToString() != "-1")
                {

                    RegularExpressionValidator maxLengthVal = new RegularExpressionValidator();
                    maxLengthVal.ID = $"max_length_{colName}_{selectedTable}";
                    maxLengthVal.ErrorMessage = "Exceeded max data length";
                    maxLengthVal.ValidationExpression = $"^[\\s\\S]{{0,{TableMetaData.Rows[rowIndex].ItemArray[2].ToString()}}}$";
                    maxLengthVal.ValidationGroup = "CU_validation";
                    maxLengthVal.ControlToValidate = $"text_{colName}_{selectedTable}";
                    maxLengthVal.Display = ValidatorDisplay.Dynamic;
                    maxLengthVal.ForeColor = Color.Red;
                    tdList[tdListIndex].Controls.Add(maxLengthVal);
                }

                // if column is not nullable then add required field validator
                if (TableMetaData.Rows[rowIndex].ItemArray[3].ToString() == "NO")
                {
                    tdList.Add(new HtmlGenericControl("td"));
                    RequiredFieldValidator requiredVal = new RequiredFieldValidator();
                    requiredVal.ID = $"required_{colName}_{selectedTable}";
                    requiredVal.ErrorMessage = "Required";
                    requiredVal.ValidationGroup = "CU_validation";
                    requiredVal.ControlToValidate = $"text_{colName}_{selectedTable}";
                    requiredVal.Display = ValidatorDisplay.Dynamic;
                    requiredVal.ForeColor = Color.Red;
                    tdList[tdListIndex].Controls.Add(requiredVal);
                }

                // add td's to tr
                foreach (HtmlGenericControl td in tdList)
                {
                    tr.Controls.Add(td);
                }

                // add tr to form placeholder
                formPlaceholder.Controls.Add(tr);

                // show create Btn
                if(dataBeforeEdit == null)
                {
                    // set header
                    headerForForm.InnerText = "Create";
                    createBtn.Visible = true;
                    EditBtn.Visible = false;
                } else
                {
                    // set header
                    headerForForm.InnerText = "Edit";
                    createBtn.Visible = false;
                    EditBtn.Visible = true;
                }
                

            }
        }

        protected void destroyForm()
        {
            formPlaceholder.Controls.Clear();
        }
        /* __________________________________________________________________________*/


        /* CLEAR MESSAGES FROM SCREEN*/
        protected void clearFeedback()
        {
            noDataPanel.Style.Add("display", "none");
            noTableSelectedPanel.Style.Add("display", "none");
            deleteSuccessfulPanel.Style.Add("display", "none");
            deleteUnsuccessfulPanel.Style.Add("display", "none");

            createSuccessfulPanel.Style.Add("display", "none");
            createUnsuccessfulPanel.Style.Add("display", "none");

            editSuccessfulPanel.Style.Add("display", "none");
            editUnsuccessfulPanel.Style.Add("display", "none");
        }

        protected void clearValidationErrors()
        {
            invalidStartDateValidationMsgPanel.Style.Add("display", "none");
            invalidExpectedEndDatePanel.Style.Add("display", "none");
            dateComparisonExpectedValidationMsgPanel.Style.Add("display", "none");
            dateComparisonActualValidationMsgPanel.Style.Add("display", "none");
            startDateIsWeekendPanel.Style.Add("display", "none");
            expectedEndDateIsWeekendPanel.Style.Add("display", "none");
            invalidActualEndDatePanel.Style.Add("display", "none");
            actualEndDateOnWeekend.Style.Add("display", "none");

            multipleActiveRecordsPanel.Style.Add("display", "none");
            clashingRecordsPanel.Style.Add("display", "none");
        }
        /* __________________________________________________________________________*/

        /* VALIDATION METHODS*/
        protected Boolean validateDates(string startDate, string endDate, Panel endDateInvalidPanel, Panel endDateWeekendPanel, Panel dateComparisonPanel)
        {
            DateTime start, end;
            start = end = DateTime.MinValue;
            Boolean isValidated = true;

            // validate start date is a date
            if (!DateTime.TryParseExact(startDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out start))
            {
                invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }

            // ensure start date is not a weekend
            if (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday)
            {
                startDateIsWeekendPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }

            if (!util.isNullOrEmpty(endDate))
            {
                // validate end date is a date
                if (!DateTime.TryParseExact(endDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out end))
                {
                    endDateInvalidPanel.Style.Add("display", "inline-block");
                    isValidated = false;
                }
                else
                {
                    // compare dates to ensure end date is not before start date
                    if (DateTime.Compare(start, end) > 0)
                    {
                        dateComparisonPanel.Style.Add("display", "inline-block");
                        isValidated = false;
                    }

                    // ensure end date is not weekend
                    if (end.DayOfWeek == DayOfWeek.Saturday || end.DayOfWeek == DayOfWeek.Sunday)
                    {
                        endDateWeekendPanel.Style.Add("display", "inline-block");
                        isValidated = false;
                    }
                }
            }

            return isValidated;
        }

        protected Boolean isRecordActive(string proposedStartDate, string proposedEndDate)
        {
            // returns a Boolean which represents whether the proposed actual end date passed will make a record active or inactive. The date passed is assumed to be validated

            // check if start date of record is a day in the future, meaning the record is currently inactive
            if (DateTime.Compare(DateTime.ParseExact(proposedStartDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture), util.getCurrentDate()) > 0)
                return false;

            // if the passed end date is empty then the record is automaticaly Active
            if (util.isNullOrEmpty(proposedEndDate))
                return true;
            else
                // if today is before the passed actual end date then the record is active and otherwise, inactive
                return (DateTime.Compare(util.getCurrentDate(), DateTime.ParseExact(proposedEndDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)) < 0);
        }

        protected Boolean isRecordValid(string employeeId, string id, string proposedStartDate, string proposedEndDate)
        {
            // returns a Boolean representing whether the proposed start date and proposed end date passed is valid in terms of the rest of existing employment position records. 
            // This method checks the other records to see if any other active records exist in order to validate the record. 

            if (TableData != null)
            {
                int numActiveRows = 0;

                // state of proposed record
                bool isProposedRecordActive = isRecordActive(proposedStartDate, proposedEndDate);

                DateTime proposedSD = DateTime.ParseExact(proposedStartDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                        proposedAED = !util.isNullOrEmpty(proposedEndDate) ? DateTime.ParseExact(proposedEndDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) : DateTime.MinValue;

                // check all rows in employeeposition except the row with the id passed as a parameter to this method
                foreach (DataRow dr in TableData.Rows)
                {
                    if (dr.ItemArray[(int)empPositionColumns.employee_id].ToString() == employeeId && dr.ItemArray[(int)empPositionColumns.record_id].ToString() != id)
                    {
                        // record being checked is active
                        // must convert start date to correct format before passing to isRecordActive
                        if (isRecordActive(Convert.ToDateTime(dr.ItemArray[(int)empPositionColumns.start_date]).ToString("d/MM/yyyy"), dr.ItemArray[(int)empPositionColumns.actual_end_date].ToString()))
                            numActiveRows++;


                        DateTime tableDataRowStartDate = (DateTime)dr[(int)empPositionColumns.start_date];

                        DateTime tableDataRowEndDate = !util.isNullOrEmpty(dr[(int)empPositionColumns.actual_end_date].ToString()) ? DateTime.ParseExact(dr[(int)empPositionColumns.actual_end_date].ToString(), "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) : DateTime.MinValue;

                        // ensure that record does not overlap with another record
                        bool isProposedStartDateInRowPeriod = false, isProposedEndDateInRowPeriod = false;


                        // if record from employeeposition being checked has an end date
                        if (tableDataRowEndDate != DateTime.MinValue)
                        {

                            bool isRowStartDateInProposedPeriod = false, isRowEndDateinProposedPeriod = false;
                            // proposed actual end date is not empty
                            if (proposedAED != DateTime.MinValue)
                            {
                                // check if period represented by proposed start date to proposed end date coincides with the given data row's period
                                isProposedStartDateInRowPeriod = DateTime.Compare(proposedSD, tableDataRowStartDate) >= 0 && DateTime.Compare(proposedSD, tableDataRowEndDate) <= 0;
                                isProposedEndDateInRowPeriod = DateTime.Compare(proposedAED, tableDataRowStartDate) >= 0 && DateTime.Compare(proposedAED, tableDataRowEndDate) <= 0;

                                isRowStartDateInProposedPeriod = DateTime.Compare(tableDataRowStartDate, proposedSD) >= 0 && DateTime.Compare(tableDataRowStartDate, proposedAED) <= 0;
                                isRowEndDateinProposedPeriod = DateTime.Compare(tableDataRowEndDate, proposedSD) >= 0 && DateTime.Compare(tableDataRowEndDate, proposedAED) <= 0;

                            }
                            // proposed actual end date is empty- proposed record is active
                            else
                                isProposedStartDateInRowPeriod = DateTime.Compare(proposedSD, tableDataRowEndDate) <= 0 || DateTime.Compare(proposedSD, tableDataRowStartDate) <= 0;

                            if (isProposedStartDateInRowPeriod || isProposedEndDateInRowPeriod || isRowStartDateInProposedPeriod || isRowEndDateinProposedPeriod)
                            {
                                clashingRecordsPanel.Style.Add("display", "inline-block");
                                return false;
                            }

                        }
                        // if record being checked is active
                        else
                        {
                            // proposed actual end date is not empty
                            if (proposedAED != DateTime.MinValue)
                            {
                                // check if period represented by proposed start date is in active record's period
                                isProposedStartDateInRowPeriod = DateTime.Compare(proposedSD, tableDataRowStartDate) >= 0;
                                isProposedEndDateInRowPeriod = DateTime.Compare(proposedAED, tableDataRowStartDate) >= 0;
                            }
                            // proposed actual end date is empty - proposed record is active
                            else
                            {
                                if (multipleActiveRecordsPanel != null)
                                    multipleActiveRecordsPanel.Style.Add("display", "inline-block");
                                return false; // proposed record is invalid since record already exists that is active and proposed record is active
                            }


                            if (isProposedStartDateInRowPeriod || isProposedEndDateInRowPeriod)
                            {
                                clashingRecordsPanel.Style.Add("display", "inline-block");
                                return false;
                            }

                        }
                    }

                }
                if (isProposedRecordActive)
                    numActiveRows++;

                if (numActiveRows <= 1)
                    return true;
                else if (numActiveRows > 1)
                    multipleActiveRecordsPanel.Style.Add("display", "inline-block");
            }

            return true;

        }
        /* __________________________________________________________________________*/

        /* SEARCH METHODS*/
        protected void searchTxtbox_TextChanged(object sender, EventArgs e)
        {
            searchForData(searchString);
        }

        protected void searchBtn_Click(object sender, EventArgs e)
        {
            searchForData(searchString);
        }

        protected void searchForData(string searchStr)
        {
            clearSearchBtn.Visible = true;
            try
            {
                List<string> searchStringComparisonList = new List<string>();
                for (int i = 0; i < TableMetaData.Rows.Count; i++)
                {
                    searchStringComparisonList.Add($"({TableMetaData.Rows[i].ItemArray[0].ToString()} LIKE @SearchString)");
                }

                string sql = $@"
                    SELECT * FROM dbo.{selectedTable}
                    WHERE {String.Join(" OR ", searchStringComparisonList.ToArray())};
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

                        if (dt.Rows.Count <= 0)
                            noDataPanel.Style.Add("display", "inline-block");

                        TableData = dt;
                        dataBeforeEdit = null;
                        bindGridview();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void clearSearch()
        {
            clearSearchBtn.Visible = false;
            searchTxtbox.Text = string.Empty;
            TableData = null;
            searchString = null;
        }

        protected void clearSearchBtn_Click(object sender, EventArgs e)
        {
            clearSearch();
            dataBeforeEdit = null;
            bindGridview();
        }
        /* __________________________________________________________________________*/

        protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            clearSearch();
            dataBeforeEdit = null;
            TableData = null;
            bindGridview();
        }

        protected void resetForm_Click(object sender, EventArgs e)
        {
            clearValidationErrors();
            clearFeedback();

            dataBeforeEdit = null;
            destroyForm();
            if (TableMetaData != null)
                generateForm(TableMetaData);
            else
            {
                if (searchString != null || util.isNullOrEmpty(searchString))
                    searchForData(searchString);
                else
                    bindGridview();
            }

        }

        protected void createBtn_Click(object sender, EventArgs e)
        {
            // create new record

            clearValidationErrors();

            // get data
            ControlCollection t=  formPlaceholder.Controls;
            string[] tablesWithNoIntegerId = new string[] { "permission", "role", "rolepermission", "employee", "employeerole", "assignment", "leavetype", "employmenttype" };
            int startingIndex = tablesWithNoIntegerId.Contains(selectedTable) ? 0 : 1;

            // data will be in form: {<column name>: <data to be edited>}
            Dictionary<string, string> data = new Dictionary<string, string>();
            for (int i = startingIndex; i < TableMetaData.Rows.Count; i++){
                data[TableMetaData.Rows[i].ItemArray[0].ToString()] = ((TextBox)formPlaceholder.FindControl($"text_{TableMetaData.Rows[i].ItemArray[0].ToString()}_{selectedTable}")).Text;
            }

            // get primary keys
            string primaryKeys = (new string[] { "rolepermission", "employeerole", "assignment" }).Contains<string>(selectedTable) ? "*" : $"{TableMetaData.Rows[0].ItemArray[0].ToString()}";

            // validate dates if they exist in form
            bool areDatesValid = true;

            // validate start date and expected end date
            if(data.ContainsKey("start_date") && data.ContainsKey("expected_end_date"))
                areDatesValid = validateDates(data["start_date"], data["expected_end_date"], invalidExpectedEndDatePanel, expectedEndDateIsWeekendPanel, dateComparisonExpectedValidationMsgPanel);
         
            // validate start date and actual end date
            if (data.ContainsKey("start_date") && data.ContainsKey("actual_end_date"))
                areDatesValid = areDatesValid && validateDates(data["start_date"], data["actual_end_date"], invalidActualEndDatePanel, actualEndDateOnWeekend, dateComparisonActualValidationMsgPanel);

            bool isNewRecordValid = true;

            // validate record if employment record to ensure more than one active record is not added
            // both start date and actual end date will already be validated in the proper form of d/MM/yyyy
            if (selectedTable == "employeeposition")
                isNewRecordValid = isRecordValid(data["employee_id"],"-1", data["start_date"], data["actual_end_date"]);
            

            if (areDatesValid && isNewRecordValid)
            {
                Boolean isInsertSuccessful = false;

                // parameterize data
                Dictionary<string, string> parameterNames = new Dictionary<string, string>();
                foreach(string key in data.Keys)
                {
                    parameterNames[key] = $"@{key.Replace("_", string.Empty)}";
                }

                DataTable insertedData = new DataTable();
                // add to db
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = $@"
                            INSERT INTO dbo.{selectedTable} ({String.Join(", ", data.Keys.ToArray())}) 
                            OUTPUT INSERTED.{primaryKeys}
                            VALUES ({String.Join(", ", parameterNames.Values.ToArray())});
                        ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            int rowIndex = startingIndex;
                            foreach(string key in data.Keys)
                            {
                                if (!util.isNullOrEmpty(data[key]))
                                {
                                    // convert to proper type
                                    if(TableMetaData.Rows[rowIndex].ItemArray[0].ToString() == key)
                                    {
                                        if(TableMetaData.Rows[rowIndex].ItemArray[1].ToString() == "datetime")
                                            command.Parameters.AddWithValue(parameterNames[key], DateTime.ParseExact(data[key], "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture));
                                        else
                                            command.Parameters.AddWithValue(parameterNames[key], data[key]);
                                    }
                                }
                                else
                                    command.Parameters.AddWithValue(parameterNames[key], DBNull.Value);

                                rowIndex++;
                            }
                            using(SqlDataAdapter adapter = new SqlDataAdapter(command))
                            {
                                adapter.Fill(insertedData);
                                isInsertSuccessful = insertedData.Rows.Count > 0;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //exception logic
                    isInsertSuccessful = false;
                }

                if (isInsertSuccessful)
                {
                    createSuccessfulPanel.Style.Add("display", "inline-block");
                    List<string> idPairings = new List<string>();
                    for (int i = 0; i < insertedData.Columns.Count; i++)
                        idPairings.Add($"{insertedData.Columns[i].ColumnName} = {insertedData.Rows[0].ItemArray[i].ToString()}");
                    string action = $"Admin created record with {String.Join(", ", idPairings.ToArray())} in {selectedTable}";
                    util.addAuditLog(Session["emp_id"].ToString(), Session["emp_id"].ToString(), action);
                }            
                else
                    createUnsuccessfulPanel.Style.Add("display", "inline-block");

                clearSearch();
                TableData = null;
                bindGridview();
            }

        }

        protected void EditBtn_Click(object sender, EventArgs e)
        {
            // edit record
            clearValidationErrors();
                

            // get data
            ControlCollection t = formPlaceholder.Controls;

            string[] tablesWithNoIntegerId = new string[] { "permission", "role", "rolepermission", "employee", "employeerole", "assignment", "leavetype", "employmenttype" };
            int startingIndex = tablesWithNoIntegerId.Contains(selectedTable) ? 0 : 1;

            Dictionary<string, string> data = new Dictionary<string, string>();
            for (int i = startingIndex; i < TableMetaData.Rows.Count; i++)
                data[TableMetaData.Rows[i].ItemArray[0].ToString()] = ((TextBox)formPlaceholder.FindControl($"text_{TableMetaData.Rows[i].ItemArray[0].ToString()}_{selectedTable}")).Text;

            // validate dates uf they exist in form
            bool areDatesValid = true;

            // validate start date and expected end date
            if (data.ContainsKey("start_date") && data.ContainsKey("expected_end_date"))
                areDatesValid = validateDates(data["start_date"], data["expected_end_date"], invalidExpectedEndDatePanel, expectedEndDateIsWeekendPanel, dateComparisonExpectedValidationMsgPanel);

            // validate start date and actual end date
            if (data.ContainsKey("start_date") && data.ContainsKey("actual_end_date"))
                areDatesValid = areDatesValid && validateDates(data["start_date"], data["actual_end_date"], invalidActualEndDatePanel, actualEndDateOnWeekend, dateComparisonActualValidationMsgPanel);

            bool isEditedRecordValid = true;
            // validate record if employment record to ensure more than one active record is not added
            // both start date and actual end date will be validated in the proper form of d/MM/yyyy
            if (selectedTable == "employeeposition")
                isEditedRecordValid = isRecordValid(data["employee_id"], dataBeforeEdit["id"], data["start_date"], data["actual_end_date"]);

            if (areDatesValid && isEditedRecordValid && dataBeforeEdit != null)
            {
                Boolean isEditSuccessful = false;
                List<string> setClauseList = new List<string>(),
                             whereClauseList = new List<string>();

                Dictionary<string, string> parameterList = new Dictionary<string, string>();
                Dictionary<string, string> whereClauseParameterList = new Dictionary<string, string>();

                // populate set clause list
                foreach (string key in data.Keys)
                {
                    parameterList[key] = $"@{key.Replace("_", string.Empty)}";
                    setClauseList.Add($"{key} = {parameterList[key]}");
                }

                // populate where clause list with all relevant fields. Bridge entities must contain all fields in the where clause
                // other tables can just use the first column (id column)
                if((new string[] {"rolepermission", "employeerole", "assignment" }).Contains<string>(selectedTable))
                {
                    foreach (string key in dataBeforeEdit.Keys)
                    {
                        whereClauseParameterList[key] = $"@previousData_{key.Replace("_", string.Empty)}";
                        whereClauseList.Add($"{key} = {whereClauseParameterList[key]}");
                    }
                } else
                {
                    whereClauseParameterList[dataBeforeEdit.Keys.First()] = $"@previousData_{dataBeforeEdit.Keys.First().Replace("_", string.Empty)}";
                    whereClauseList.Add($"{dataBeforeEdit.Keys.First()} = {whereClauseParameterList[dataBeforeEdit.Keys.First()]}");
                }    

                // edit in db
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = $@"
                            UPDATE dbo.{selectedTable}
                            SET {String.Join(", ", setClauseList.ToArray())}
                            WHERE {String.Join(" AND ", whereClauseList.ToArray())}
                        ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            // set data from starting column only, which changes depending on whether selectedTable has an integer PK or not
                            int rowIndex = startingIndex;
                            foreach (string key in data.Keys)
                            {
                                if (!util.isNullOrEmpty(data[key]))
                                {
                                    // convert to proper type
                                    if (TableMetaData.Rows[rowIndex].ItemArray[0].ToString() == key)
                                    {
                                        if (TableMetaData.Rows[rowIndex].ItemArray[1].ToString() == "datetime")
                                            command.Parameters.AddWithValue(parameterList[key], DateTime.ParseExact(data[key], "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture));
                                        else
                                            command.Parameters.AddWithValue(parameterList[key], data[key]);
                                    }
                                }
                                else
                                    command.Parameters.AddWithValue(parameterList[key], DBNull.Value);

                                rowIndex++;
                            }

                            // set data for parameters
                            rowIndex = 0; // set to 0 since this will include the id column
                            foreach(string key in whereClauseParameterList.Keys)
                            {
                                if (!util.isNullOrEmpty(dataBeforeEdit[key]))
                                {
                                    // convert to proper type
                                    if (TableMetaData.Rows[rowIndex].ItemArray[0].ToString() == key)
                                    {
                                        if (TableMetaData.Rows[rowIndex].ItemArray[1].ToString() == "datetime")
                                            command.Parameters.AddWithValue(whereClauseParameterList[key], Convert.ToDateTime(dataBeforeEdit[key]));
                                        else
                                            command.Parameters.AddWithValue(whereClauseParameterList[key], dataBeforeEdit[key]);
                                    }
                                }
                                else
                                    command.Parameters.AddWithValue(whereClauseParameterList[key], DBNull.Value);

                                rowIndex++;
                            }
                            int rowsAffected = command.ExecuteNonQuery();
                            isEditSuccessful = rowsAffected > 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //exception logic
                    isEditSuccessful = false;
                }

                if (isEditSuccessful)
                {
                    editSuccessfulPanel.Style.Add("display", "inline-block");
                    // primary keys 
                    List<string> primaryKeys = new List<string>();
                    if ((new string[] { "rolepermission", "employeerole", "assignment" }).Contains<string>(selectedTable))
                    {
                        foreach (string key in dataBeforeEdit.Keys)
                            primaryKeys.Add(key);
                    }
                    else
                        primaryKeys.Add(dataBeforeEdit.Keys.First());
                    List<string> idPairings = new List<string>();
                    foreach(string key in primaryKeys)
                        idPairings.Add($"{key} = {dataBeforeEdit[key]}");
                    string action = $"Admin edited record with {String.Join(", ", idPairings.ToArray())} in {selectedTable}";
                    util.addAuditLog(Session["emp_id"].ToString(), Session["emp_id"].ToString(), action);
                }
                else
                    editUnsuccessfulPanel.Style.Add("display", "inline-block");

                dataBeforeEdit = null;
                if (searchString == null || util.isNullOrEmpty(searchString))
                {
                    TableData = null;
                    bindGridview();
                }
                else
                    searchForData(searchString);
            }
        }

        
    }
}