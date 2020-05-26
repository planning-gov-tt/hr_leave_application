using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using ClosedXML.Excel;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Data.SqlClient;
using HR_LEAVEv2.Classes;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.HR
{
    public partial class UploadData : System.Web.UI.Page
    {
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
        Util util = new Util();
        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> permissions = (List<string>)Session["permissions"];
            if (permissions == null || !(permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions") || permissions.Contains("hr3_permissions")))
                Response.Redirect("~/AccessDenied.aspx");

            // hide clear file button if no file is uploaded
            clearFileBtn.Visible = Session["uploadedFile"] != null;

            if(Session["uploadedFile"] != null)
            {
                uploadedFile.Text = $"Uploaded file: <strong>{Session["uploadedFile"]}</strong>";
                uploadedFile.Visible = true;
            }
        }

        protected void resetUploadDataPage()
        {
            invalidFileTypePanel.Style.Add("display", "none");
            fileUploadedTooLargePanel.Style.Add("display", "none");
            noFileUploaded.Style.Add("display", "none");
            successfulDataInsertPanel.Style.Add("display", "none");
            typeErrorPanel.Style.Add("display", "none");
            maxLengthErrorPanel.Style.Add("display", "none");
            nullableErrorPanel.Style.Add("display", "none");
            unsuccessfulInsertPanel.Style.Add("display", "none");

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

            uploadedFile.Visible = false;
            uploadDataBtnPanel.Visible = false;
            chooseTablePanel.Visible = false;
            tableSelectDdl.SelectedIndex = 0;
        }

        protected void uploadFilesBtn_Click(object sender, EventArgs e)
        {
            resetUploadDataPage();

            if (FileUpload1.HasFile)
            {
                Boolean isInvalidFileType = false,
                        isFileTooLarge = false;

                // used to check whether the files uploaded fit the size requirement specified in the web config
                HttpRuntimeSection section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
                int maxRequestLength = section != null ? section.MaxRequestLength : 4096;

                // used to check whether the file(s) uploaded are of a certain format
                List<string> allowedFileExtensions = new List<string>() { ".xlsx" };
                HttpPostedFile fileToBeUploaded = FileUpload1.PostedFile;

                isFileTooLarge = fileToBeUploaded.ContentLength > maxRequestLength;
                isInvalidFileType = !allowedFileExtensions.Contains(Path.GetExtension(fileToBeUploaded.FileName).ToString());

                if (isInvalidFileType)
                {
                    invalidFileTypePanel.Style.Add("display", "inline-block");
                }

                if (isFileTooLarge)
                    fileUploadedTooLargePanel.Style.Add("display", "inline-block");

                if (!isInvalidFileType && !isFileTooLarge)
                {
                    // add files to session so they will persist after postback
                    Session["uploadedFile"] = Path.GetFileName(fileToBeUploaded.FileName);
                    FileUpload1.SaveAs(Path.Combine(Server.MapPath("~/Assets/temp"), FileUpload1.FileName));
                    uploadedFile.Visible = true;
                    uploadedFile.Text = $"Uploaded file: <strong>{Path.GetFileName(fileToBeUploaded.FileName)}</strong>";
                    chooseTablePanel.Visible = true;
                    clearFileBtn.Visible = true;
                }
            }
            else
            {
                noFileUploaded.Style.Add("display", "inline-block");
            }
        }

        protected void deleteUploadedFile()
        {
            resetUploadDataPage();
            if (Session["uploadedFile"] != null && File.Exists(Path.Combine(Server.MapPath("~/Assets/temp"), Session["uploadedFile"].ToString())))
                File.Delete(Path.Combine(Server.MapPath("~/Assets/temp"), Session["uploadedFile"].ToString()));
            Session["uploadedFile"] = null;
            clearFileBtn.Visible = uploadedFile.Visible = false;
            FileUpload1.Dispose();
        }

        protected void clearFileBtn_Click(object sender, EventArgs e)
        {
            deleteUploadedFile();
        }

        //protected DataTable getDataTableRepresentingTable(string tableName)
        //{
        //    try
        //    {
        //        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
        //        {
        //            connection.Open();
        //            string sql = $@"
        //                    SELECT *
        //                    FROM dbo.{tableName}
        //                    WHERE 1=2
        //                ";
        //            using (SqlCommand command = new SqlCommand(sql, connection))
        //            {
        //                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
        //                {
        //                    DataTable dt = new DataTable();
        //                    adapter.Fill(dt);

        //                    return dt;

        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //exception logic
        //        return null;
        //    }

        //}
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

        protected Boolean isRecordValid(DataTable TableData, string employeeId, string id, string proposedStartDate, string proposedEndDate)
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

        protected void uploadDataBtn_Click(object sender, EventArgs e)
        {
            Boolean isUploadSuccessful = false;
            if (Session["uploadedFile"] != null)
            {
                string filePath = Path.Combine(Server.MapPath("~/Assets/temp"), Session["uploadedFile"].ToString());
                if (File.Exists(filePath))
                {
                    // get metadata about table's columns like column name, data type, maximum length(if applicable) and whether it is nullable
                    // this meta data is used to validate the data before being inserted
                    DataTable metaData = new DataTable();
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            string sql = $@"
                            SELECT COL.COLUMN_NAME, 
                                   COL.DATA_TYPE,
                                   ISNULL(COL.CHARACTER_MAXIMUM_LENGTH, -1) MAX_LENGTH,
                                   COL.IS_NULLABLE
                            FROM INFORMATION_SCHEMA.COLUMNS COL
                            WHERE COL.TABLE_NAME = '{tableSelectDdl.SelectedValue}'
                        ";
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                                {
                                    adapter.Fill(metaData);

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception logic
                        isUploadSuccessful = false;
                    }

                    // get actual data to be inserted
                    DataTable dt = new DataTable();
                    try
                    {
                        using (XLWorkbook workBook = new XLWorkbook(filePath))
                        {
                            //Read the first Sheet from Excel file.
                            IXLWorksheet workSheet = workBook.Worksheet(1);

                            bool firstRow = true;
                            foreach (IXLRow row in workSheet.Rows())
                            {
                                if (!row.IsEmpty())
                                {
                                    //Use the first row to add columns to DataTable.
                                    if (firstRow)
                                    {
                                        firstRow = false;
                                        foreach (IXLCell cell in row.Cells(1, metaData.Rows.Count))
                                        {
                                            if(!util.isNullOrEmpty(cell.Value.ToString()))
                                                dt.Columns.Add(cell.Value.ToString(), typeof(string));
                                        }
                                    }                                       
                                    else
                                    {
                                        //Add rows to DataTable.
                                        dt.Rows.Add();
                                        int i = 0;

                                        foreach (IXLCell cell in row.Cells(1, dt.Columns.Count))
                                        {
                                            //if (!String.IsNullOrEmpty(cell.Value.ToString()) && !String.IsNullOrWhiteSpace(cell.Value.ToString()))
                                            //{
                                                dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                                                i++;
                                            //}
                                        }
                                    }
                                }

                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        isUploadSuccessful = false;
                    }

                    Boolean isDataValid = true;
                    
                    // check whether data is valid based on table metadata and converts datetimes to their proper type
                    for(int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
                    {
                        List<string> colNamesWithIntTypeError = new List<string>(),
                                 colNamesWithDateTimeTypeError = new List<string>(),
                                 colNamesWithMaxLengthError = new List<string>(),
                                 colNamesWithNullableError = new List<string>();
                        DataRow dr = dt.Rows[rowIndex];
                        for (int colIndex = 0; colIndex < dt.Columns.Count; colIndex++)
                        {
                            // check metadata
                            foreach (DataRow metaDataDr in metaData.Rows)
                            {
                                if (metaDataDr.ItemArray[0].ToString() == dt.Columns[colIndex].ColumnName)
                                {
                                    // check if data is correct type
                                    if(metaDataDr.ItemArray[1].ToString() == "int" && !util.isNullOrEmpty(dr.ItemArray[colIndex].ToString()))
                                    {
                                        //expect data to be integer
                                        try
                                        {
                                            int temp = Convert.ToInt32(dr.ItemArray[colIndex].ToString());
                                        }
                                        catch(FormatException fe)
                                        {
                                            colNamesWithIntTypeError.Add(metaDataDr.ItemArray[0].ToString());
                                            typeErrorTxt.InnerText = $"Data on row {rowIndex + 1} in {String.Join(", ", colNamesWithIntTypeError.ToArray())} is supposed to be of type integer";
                                            isDataValid = false;
                                            typeErrorPanel.Style.Add("display", "inline-block");
                                        }
                                    }

                                    if (metaDataDr.ItemArray[1].ToString() == "datetime" && !util.isNullOrEmpty(dr.ItemArray[colIndex].ToString()))
                                    {
                                        //expect data to be date
                                        DateTime formattedDate = DateTime.MinValue;
                                        isDataValid = DateTime.TryParseExact(dr.ItemArray[colIndex].ToString(), "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out formattedDate);

                                        if (isDataValid && formattedDate != DateTime.MinValue)
                                        {
                                            dr.ItemArray[colIndex] = formattedDate.ToString();
                                        }
                                        else
                                        {
                                            colNamesWithDateTimeTypeError.Add(metaDataDr.ItemArray[0].ToString());
                                            typeErrorTxt.InnerText = $"Data on row {rowIndex + 1} in {String.Join(", ", colNamesWithDateTimeTypeError.ToArray())} is supposed to be a date formatted as d/mm/yyyy (eg. 12/04/2020 or 1/02/2020)";
                                            typeErrorPanel.Style.Add("display", "inline-block");
                                        }


                                    }

                                    // check if data exceeds max length
                                    if(metaDataDr.ItemArray[2].ToString() != "-1")
                                    {
                                        // length of data exceeds max length
                                        if (dr.ItemArray[colIndex].ToString().Length > Convert.ToInt32(metaDataDr.ItemArray[2]))
                                        {
                                            dr.ItemArray[colIndex] = dr.ItemArray[colIndex].ToString().Substring(0, Convert.ToInt32(metaDataDr.ItemArray[2]));
                                            colNamesWithMaxLengthError.Add(metaDataDr.ItemArray[0].ToString());
                                            maxLengthErrorTxt.InnerText = $"Data on row {rowIndex + 1} in {String.Join(", ", colNamesWithMaxLengthError.ToArray())} exceeds maximum length of {metaDataDr.ItemArray[2].ToString()} characters";
                                            maxLengthErrorPanel.Style.Add("display", "inline-block");
                                        }


                                    }

                                    // check if data is nullable
                                    if(metaDataDr.ItemArray[3].ToString() == "NO")
                                    {
                                        // must not have any blanks
                                        if (util.isNullOrEmpty(dr.ItemArray[colIndex].ToString()))
                                        {
                                            isDataValid = false;
                                            colNamesWithNullableError.Add(metaDataDr.ItemArray[0].ToString());
                                            nullableErrorTxt.InnerText = $"Data on row {rowIndex + 1} in {String.Join(", ", colNamesWithNullableError.ToArray())} must not be blank";
                                            nullableErrorPanel.Style.Add("display", "inline-block");
                                        }

                                    }
                                    else
                                    {
                                        // replace blanks with null value
                                        if (util.isNullOrEmpty(dr.ItemArray[colIndex].ToString()))
                                            dr.ItemArray[colIndex] = DBNull.Value;
                                    }

                                }
                            }
                        }
                        
                    }

                    if(tableSelectDdl.SelectedValue == "employeeposition")
                    {
                        // check employment records for consistency for employeeposition table
                        DataTable employmentRecordsDt = new DataTable();
                        try
                        {
                            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                            {
                                connection.Open();
                                string sql = $@"
                                SELECT * FROM dbo.employeeposition;
                            ";
                                using (SqlCommand command = new SqlCommand(sql, connection))
                                {
                                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                                    {
                                        adapter.Fill(employmentRecordsDt);

                                    }
                                }
                            }

                            Boolean areDatesValid = false,
                                isNewRecordValid = true;

                            for (int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
                            {
                                DataRow dr = dt.Rows[rowIndex];
                                // check validity of dates 
                                if (dt.Columns.Contains("start_date") && dt.Columns.Contains("expected_end_date"))
                                    areDatesValid = validateDates(dr.Field<string>("start_date"), dr.Field<string>("expected_end_date"), invalidExpectedEndDatePanel, expectedEndDateIsWeekendPanel, dateComparisonExpectedValidationMsgPanel);

                                // validate start date and actual end date
                                if (dt.Columns.Contains("start_date") && dt.Columns.Contains("actual_end_date"))
                                    areDatesValid = areDatesValid && validateDates(dr.Field<string>("start_date"), dr.Field<string>("actual_end_date"), invalidActualEndDatePanel, actualEndDateOnWeekend, dateComparisonActualValidationMsgPanel);

                                // validate record if employment record to ensure more than one active record is not added
                                // both start date and actual end date will already be validated in the proper form of d/MM/yyyy
                                if (areDatesValid)
                                    isNewRecordValid = isRecordValid(employmentRecordsDt, dr.Field<string>("employee_id"), "-1", dr.Field<string>("start_date"), dr.Field<string>("actual_end_date"));

                                if (!areDatesValid || !isNewRecordValid)
                                {
                                    isUploadSuccessful = isDataValid = false;
                                    break;
                                }
                                    
                            }

                        }
                        catch (Exception ex)
                        {
                            //exception logic
                            isUploadSuccessful = false;
                        }            
                    }


                    if (isDataValid)
                    {
                        // upload to db
                        try
                        {
                            using (var bulkCopy = new SqlBulkCopy(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString, SqlBulkCopyOptions.KeepIdentity))
                            {
                                foreach (DataColumn col in dt.Columns)
                                {
                                    bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                                }

                                bulkCopy.BulkCopyTimeout = 600;
                                bulkCopy.DestinationTableName = tableSelectDdl.SelectedValue;
                                bulkCopy.WriteToServer(dt);
                                isUploadSuccessful = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            //exception logic
                            isUploadSuccessful = false;
                        }
                    }

                    
                }
            }

            if (isUploadSuccessful)
            {
                // delete file from temp and show success message
                deleteUploadedFile();
                successfulDataInsertPanel.Style.Add("display", "inline-block");
            } else 
                unsuccessfulInsertPanel.Style.Add("display", "inline-block");
        }   

        protected void tableSelectDdl_SelectedIndexChanged(object sender, EventArgs e)
        {
            uploadDataBtnPanel.Visible = tableSelectDdl.SelectedValue != "-";
        }
    }
}