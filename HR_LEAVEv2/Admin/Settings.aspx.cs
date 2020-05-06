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


        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> permissions = (List<string>)Session["permissions"];
            if(permissions == null || !permissions.Contains("admin_permissions"))
                Response.Redirect("~/AccessDenied.aspx");

            selectedTable = DropDownList1.SelectedValue.ToString() != "-" ? DropDownList1.SelectedValue.ToString() : string.Empty;
            addPanel.Visible = !String.IsNullOrEmpty(selectedTable);
            TableMetaData = !String.IsNullOrEmpty(selectedTable) ? TableMetaData : null;

            clearFeedback();
            bindGridview();
        }

        protected void generateForm(DataTable dt)
        {

            // set header
            headerForForm.InnerText = "Create";

            List<string> tablesWithNoIntegerId = new List<string>() { "permission", "role", "rolepermission", "employee", "employeerole", "assignment", "leavetype", "employmenttype" };
            int startingIndex = tablesWithNoIntegerId.Contains(selectedTable) ? 0 : 1;
            
            
            for (int i = startingIndex; i < dt.Columns.Count; i++)
            {
                int tdListIndex = 0;
                HtmlGenericControl tr = new HtmlGenericControl("tr");
                List<HtmlGenericControl> tdList = new List<HtmlGenericControl>();
                string colName = dt.Columns[i].ColumnName;

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

                // ensure data being input is correct type
                if (TableMetaData.Rows[i].ItemArray[1].ToString() == "int")
                {
                    RegularExpressionValidator intTypeVal = new RegularExpressionValidator();
                    intTypeVal.ID = $"int_val_{colName}_{selectedTable}";
                    intTypeVal.ErrorMessage = "Input must be numerical";
                    intTypeVal.ValidationExpression = $"^\\d+$";
                    intTypeVal.ValidationGroup = "CU_validation";
                    intTypeVal.ControlToValidate = $"text_{colName}_{selectedTable}";
                    intTypeVal.Display = ValidatorDisplay.Dynamic;
                    intTypeVal.ForeColor = Color.Red;
                    //maxLengthVal.Style.Add("float", "left");
                    tdList[tdListIndex].Controls.Add(intTypeVal);
                }
                if (TableMetaData.Rows[i].ItemArray[1].ToString() == "datetime" || colName.Contains("date"))
                {
                    RegularExpressionValidator dateTypeVal = new RegularExpressionValidator();
                    dateTypeVal.ID = $"date_val_{colName}_{selectedTable}";
                    dateTypeVal.ErrorMessage = "Dates must be in form dd/mm/yyyy";
                    dateTypeVal.ValidationExpression = "^([0-9]|[0-2][0-9]|(3)[0-1])(\\/)(((0)[0-9])|((1)[0-2]))(\\/)\\d{4}$";
                    dateTypeVal.ValidationGroup = "CU_validation";
                    dateTypeVal.ControlToValidate = $"text_{colName}_{selectedTable}";
                    dateTypeVal.Display = ValidatorDisplay.Dynamic;
                    dateTypeVal.ForeColor = Color.Red;
                    //maxLengthVal.Style.Add("float", "left");
                    tdList[tdListIndex].Controls.Add(dateTypeVal);
                }                

                // if column has maximum character length
                if (TableMetaData.Rows[i].ItemArray[2].ToString() != "-1")
                {
                    
                    RegularExpressionValidator maxLengthVal = new RegularExpressionValidator();
                    maxLengthVal.ID = $"max_length_{colName}_{selectedTable}";
                    maxLengthVal.ErrorMessage = "Exceeded max data length";
                    maxLengthVal.ValidationExpression = $"^[\\s\\S]{{0,{TableMetaData.Rows[i].ItemArray[2].ToString()}}}$";
                    maxLengthVal.ValidationGroup = "CU_validation";
                    maxLengthVal.ControlToValidate = $"text_{colName}_{selectedTable}";
                    maxLengthVal.Display = ValidatorDisplay.Dynamic;
                    maxLengthVal.ForeColor = Color.Red;
                    //maxLengthVal.Style.Add("float", "left");
                    tdList[tdListIndex].Controls.Add(maxLengthVal);
                }

                // if column is nullable
                if (TableMetaData.Rows[i].ItemArray[3].ToString() == "NO")
                {
                    tdList.Add(new HtmlGenericControl("td"));
                    RequiredFieldValidator requiredVal = new RequiredFieldValidator();
                    requiredVal.ID = $"required_{colName}_{selectedTable}";
                    //requiredVal.Style.Add("float", "left");
                    requiredVal.ErrorMessage = "Required";
                    requiredVal.ValidationGroup = "CU_validation";
                    requiredVal.ControlToValidate = $"text_{colName}_{selectedTable}";
                    requiredVal.Display = ValidatorDisplay.Dynamic;
                    requiredVal.ForeColor = Color.Red;
                    tdList[tdListIndex].Controls.Add(requiredVal);
                }

                foreach(HtmlGenericControl td in tdList)
                {
                    tr.Controls.Add(td);
                }
 
                formPlaceholder.Controls.Add(tr);

                // show create Btn
                createBtn.Visible = true;

            }
        }

        protected void destroyForm()
        {
            formPlaceholder.Controls.Clear();
        }

        
        protected void bindGridview()
        {

            destroyForm();

            if (!String.IsNullOrEmpty(selectedTable))
            {

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
                            using(SqlDataAdapter adapter = new SqlDataAdapter(command))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);

                                TableData = dt;
                                GridView1.DataSource = dt;
                                GridView1.DataBind();
                                if (dt.Rows.Count <= 0)
                                    noDataPanel.Style.Add("display", "inline-block");

                                // generate form controls 
                                generateForm(dt);
                                    
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
            else
            {
                GridView1.DataSource = new DataTable();
                GridView1.DataBind();
                noTableSelectedPanel.Style.Add("display", "inline-block");
            }
                
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {

            GridView1.PageIndex = e.NewPageIndex;
            bindGridview();
        }

        protected void clearFeedback()
        {
            noDataPanel.Style.Add("display", "none");
            noTableSelectedPanel.Style.Add("display", "none");
            deleteSuccessfulPanel.Style.Add("display", "none");
            deleteUnsuccessfulPanel.Style.Add("display", "none");

            createSuccessfulPanel.Style.Add("display", "none");
            createUnsuccessfulPanel.Style.Add("display", "none");
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

            if(row.RowType == DataControlRowType.DataRow)
            {
                if(e.CommandName == "editRow")
                {
                } else if(e.CommandName == "deleteRow")
                {

                    Boolean isDeleteSuccessful = false;
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();

                            List<string> bridgeTables = new List<string>() { "assignment", "employeerole", "emptypeleavetype", "rolepermission" };
                            List<string> parameters = new List<string>();

                            string sql = $"DELETE FROM dbo.{selectedTable} WHERE ";
                            if (bridgeTables.Contains(selectedTable))
                            {
                                // use all columns in where clause for identity
                                List<string> whereClauseComponents = new List<string>();
                                for (int i = 1; i < row.Cells.Count; i++)
                                {
                                    parameters.Add($"@{GridView1.HeaderRow.Cells[i].Text.Replace("_", string.Empty)}");
                                    whereClauseComponents.Add($"{GridView1.HeaderRow.Cells[i].Text} = @{GridView1.HeaderRow.Cells[i].Text.Replace("_", string.Empty)}");
                                }
                                    
                                sql += $"{String.Join(" AND ", whereClauseComponents.ToArray())}";
                            }
                            else
                            {
                                parameters.Add($"@{GridView1.HeaderRow.Cells[1].Text.Replace("_", string.Empty)}");
                                sql += $"{GridView1.HeaderRow.Cells[1].Text} = @{GridView1.HeaderRow.Cells[1].Text.Replace("_", string.Empty)}"; // use only first column to get identity
                            }

                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                int i = 1;
                                foreach(string parameter in parameters)
                                {
                                    command.Parameters.AddWithValue(parameter, row.Cells[i].Text.ToString());
                                    i++;
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
                        deleteSuccessfulPanel.Style.Add("display", "inline-block"); //show delete success
                    else
                        deleteUnsuccessfulPanel.Style.Add("display", "inline-block");

                    bindGridview();

                }
            }

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

            if (!String.IsNullOrEmpty(endDate))
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

            Util util = new Util();

            // check if start date of record is a day in the future, meaning the record is currently inactive
            if (DateTime.Compare(DateTime.ParseExact(proposedStartDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture), util.getCurrentDate()) > 0)
                return false;

            // if the passed end date is empty then the record is automaticaly Active
            if (String.IsNullOrEmpty(proposedEndDate) || String.IsNullOrWhiteSpace(proposedEndDate) || proposedEndDate == "&nbsp;")
                return true;
            else
                // if today is before the passed actual end date then the record is active and otherwise, inactive
                return (DateTime.Compare(util.getCurrentDate(), DateTime.ParseExact(proposedEndDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)) < 0);
        }

        protected Boolean isRecordValid(string employeeId, string proposedStartDate, string proposedEndDate)
        {
            // returns a Boolean representing whether the proposed start date and proposed end date passed is valid in terms of the rest of existing records. This method checks the other records to see if
            // any other active records exist in order to validate the record. 

            if (TableData != null)
            {
                int numActiveRows = 0;

                // state of passed end date and corresponding record
                bool isProposedRecordActive = isRecordActive(proposedStartDate, proposedEndDate);

                DateTime proposedSD = DateTime.ParseExact(proposedStartDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                        proposedAED = !String.IsNullOrEmpty(proposedEndDate) ? DateTime.ParseExact(proposedEndDate, "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) : DateTime.MinValue;

                foreach (DataRow dr in TableData.Rows)
                {
                    if(dr.ItemArray[(int)empPositionColumns.employee_id].ToString() == employeeId)
                    {
                        // record being checked is active
                        // must convert start date to correct format before passing to isRecordActive
                        if (isRecordActive(Convert.ToDateTime(dr.ItemArray[(int)empPositionColumns.start_date]).ToString("d/MM/yyyy"), dr.ItemArray[(int)empPositionColumns.actual_end_date].ToString()))
                            numActiveRows++;


                        DateTime startDate = (DateTime)dr[(int)empPositionColumns.start_date];

                        DateTime endDate = !String.IsNullOrEmpty(dr[(int)empPositionColumns.actual_end_date].ToString()) ? DateTime.ParseExact(dr[(int)empPositionColumns.actual_end_date].ToString(), "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) : DateTime.MinValue;

                        // ensure that record does not overlap with another record
                        bool isProposedStartDateInRowPeriod = false, isProposedEndDateInRowPeriod = false;


                        // if record being checked has an end date
                        if (endDate != DateTime.MinValue)
                        {

                            bool isRowStartDateInProposedPeriod = false, isRowEndDateinProposedPeriod = false;
                            // proposed actual end date is not empty
                            if (proposedAED != DateTime.MinValue)
                            {
                                // check if period represented by proposed start date to proposed end date coincides with the given data row's period
                                isProposedStartDateInRowPeriod = DateTime.Compare(proposedSD, startDate) >= 0 && DateTime.Compare(proposedSD, endDate) <= 0;
                                isProposedEndDateInRowPeriod = DateTime.Compare(proposedAED, startDate) >= 0 && DateTime.Compare(proposedAED, endDate) <= 0;

                                isRowStartDateInProposedPeriod = DateTime.Compare(startDate, proposedSD) >= 0 && DateTime.Compare(startDate, proposedAED) <= 0;
                                isRowEndDateinProposedPeriod = DateTime.Compare(endDate, proposedSD) >= 0 && DateTime.Compare(endDate, proposedAED) <= 0;

                            }
                            // proposed actual end date is empty- proposed record is active
                            else
                            {
                                isProposedStartDateInRowPeriod = DateTime.Compare(proposedSD, endDate) <= 0 || DateTime.Compare(proposedSD, startDate) <= 0;
                            }

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
                                isProposedStartDateInRowPeriod = DateTime.Compare(proposedSD, startDate) >= 0;
                                isProposedEndDateInRowPeriod = DateTime.Compare(proposedAED, startDate) >= 0;
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

        protected void createBtn_Click(object sender, EventArgs e)
        {
            // create new record

            clearValidationErrors();

            // get data
            ControlCollection t=  formPlaceholder.Controls;
            List<string> tablesWithNoIntegerId = new List<string>() { "permission", "role", "rolepermission", "employee", "employeerole", "assignment", "leavetype", "employmenttype" };
            int startingIndex = tablesWithNoIntegerId.Contains(selectedTable) ? 0 : 1;

            Dictionary<string, string> data = new Dictionary<string, string>();
            for (int i = startingIndex; i < TableMetaData.Rows.Count; i++){
                data[TableMetaData.Rows[i].ItemArray[0].ToString()] = ((TextBox)formPlaceholder.FindControl($"text_{TableMetaData.Rows[i].ItemArray[0].ToString()}_{selectedTable}")).Text;
            }

            // validate dates uf they exist in form
            bool areDatesValid = true;

            // validate start date and expected end date
            if(data.ContainsKey("start_date") && data.ContainsKey("expected_end_date"))
                areDatesValid = validateDates(data["start_date"], data["expected_end_date"], invalidExpectedEndDatePanel, expectedEndDateIsWeekendPanel, dateComparisonExpectedValidationMsgPanel);
         
            // validate start date and actual end date
            if (data.ContainsKey("start_date") && data.ContainsKey("actual_end_date"))
                areDatesValid = areDatesValid && validateDates(data["start_date"], data["actual_end_date"], invalidActualEndDatePanel, actualEndDateOnWeekend, dateComparisonActualValidationMsgPanel);

            bool isNewRecordValid = true;
            // validate record if employment record to ensure more than one active record is not added
            if(selectedTable == "employeeposition")
            {
                // both start date and actual end date will be validated in the proper form of d/MM/yyyy
                isNewRecordValid = isRecordValid(data["employee_id"], data["start_date"], data["actual_end_date"]);
            }
            

            if (areDatesValid && isNewRecordValid)
            {
                Boolean isInsertSuccessful = false;
                Dictionary<string, string> parameterNames = new Dictionary<string, string>();

                foreach(string key in data.Keys)
                {
                    parameterNames[key] = $"@{key.Replace("_", string.Empty)}";
                }

                // add to db
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = $@"
                            INSERT INTO dbo.{selectedTable} ({String.Join(", ", data.Keys.ToArray())}) 
                            VALUES ({String.Join(", ", parameterNames.Values.ToArray())});
                        ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            int i = startingIndex;
                            foreach(string key in data.Keys)
                            {
                                if (!String.IsNullOrEmpty(data[key]))
                                {
                                    // convert to proper type
                                    if(TableMetaData.Rows[i].ItemArray[0].ToString() == key)
                                    {
                                        if(TableMetaData.Rows[i].ItemArray[1].ToString() == "datetime")
                                            command.Parameters.AddWithValue(parameterNames[key], DateTime.ParseExact(data[key], "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture));
                                        else
                                            command.Parameters.AddWithValue(parameterNames[key], data[key]);
                                    }
                                }
                                else
                                    command.Parameters.AddWithValue(parameterNames[key], DBNull.Value);

                                i++;
                            }
                            int rowsAffected = command.ExecuteNonQuery();
                            isInsertSuccessful = rowsAffected > 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //exception logic
                    isInsertSuccessful = false;
                }

                if (isInsertSuccessful)
                    createSuccessfulPanel.Style.Add("display", "inline-block");
                else
                    createUnsuccessfulPanel.Style.Add("display", "inline-block");

                bindGridview();
            }

        }
    }
}