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
        User user = new User();

        //private Dictionary<string, List<string>> empPositionValidationMsgs
        //{
        //    get { return ViewState["empPositionValidationMsgs"] != null ? ViewState["empPositionValidationMsgs"] as Dictionary<string, List<string>> : null; }
        //    set { ViewState["empPositionValidationMsgs"] = value; }
        //}

        private string uploadedFileName
        {
            get { return Session["uploadedFile"] != null ? Session["uploadedFile"].ToString() : null; }
            set { Session["uploadedFile"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!(user.permissions.Contains("hr1_permissions") || user.permissions.Contains("hr2_permissions") || user.permissions.Contains("hr3_permissions")))
                Response.Redirect("~/AccessDenied.aspx");

            if (!IsPostBack)
            {
                deleteUploadedFile();
            }
                

            // hide clear file button if no file is uploaded
            clearFileBtn.Visible = uploadedFileName != null;

            if(uploadedFileName != null)
            {
                uploadedFile.Text = $"Uploaded file: <strong>{uploadedFileName}</strong>";
                uploadedFile.Visible = true;
            }
        }

        protected void clearValidationMessages()
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
            actualEndDateOnWeekendPanel.Style.Add("display", "none");

            multipleActiveRecordsExcelPanel.Style.Add("display", "none");
            clashingRecordsExcelPanel.Style.Add("display", "none");
            noSubstantiveRecordExcelPanel.Style.Add("display", "none");
            actingStartDateBeforeSubExcelPanel.Style.Add("display", "none");

            multipleActiveRecordsDbPanel.Style.Add("display", "none");
            clashingRecordsDbPanel.Style.Add("display", "none");
            noSubstantiveRecordDbPanel.Style.Add("display", "none");
            actingStartDateBeforeSubDbPanel.Style.Add("display", "none");

            wrongTablePanel.Style.Add("display", "none");
            invalidAnnualOrMaximumVacationLeaveAmtPanel.Style.Add("display", "none");
        }

        protected void resetUploadDataPage()
        {

            clearValidationMessages();
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
                    uploadedFileName = Path.GetFileName(fileToBeUploaded.FileName);
                    try
                    {
                        Directory.CreateDirectory("C:/ProgramData/HRLS/temp");
                    } catch(Exception exc)
                    {
                        throw exc;
                    }
                    
                    FileUpload1.SaveAs(Path.Combine("C:/ProgramData/HRLS/temp", FileUpload1.FileName));
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
            if (uploadedFileName != null && File.Exists(Path.Combine("C:/ProgramData/HRLS/temp", uploadedFileName)))
                File.Delete(Path.Combine("C:/ProgramData/HRLS/temp", uploadedFileName));
            uploadedFileName = null;
            clearFileBtn.Visible = uploadedFile.Visible = false;
            FileUpload1.Dispose();
        }

        protected void clearFileBtn_Click(object sender, EventArgs e)
        {
            deleteUploadedFile();
        }

        protected Boolean validateDates(DateTime start, DateTime end, Boolean isEndDateExpected, string row, Dictionary<string, List<string>> empPositionValidationMsgs)
        {
            Boolean isValidated = true;

            // ensure start date is not a weekend
            if (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday)
            {
                empPositionValidationMsgs["startDateIsWeekend"].Add(row);
                isValidated = false;
            }

            if(end != DateTime.MinValue)
            {
                // compare dates to ensure end date is not before start date
                if (DateTime.Compare(start, end) > 0)
                {
                    if (isEndDateExpected)
                    {
                        empPositionValidationMsgs["dateComparisonExpected"].Add(row);
                    }
                    else
                    {
                        empPositionValidationMsgs["dateComparisonActual"].Add(row);
                    }

                    isValidated = false;
                }

                // ensure end date is not weekend
                if (end.DayOfWeek == DayOfWeek.Saturday || end.DayOfWeek == DayOfWeek.Sunday)
                {
                    if (isEndDateExpected)
                    {
                        empPositionValidationMsgs["expectedEndDateIsWeekend"].Add(row);
                    }
                    else
                    {
                        empPositionValidationMsgs["actualEndDateIsWeekend"].Add(row);
                    }
                    isValidated = false;
                }
            }

            return isValidated;
        }

        protected Boolean isRecordActive(DateTime proposedStartDate, DateTime proposedEndDate)
        {
            // returns a Boolean which represents whether the proposed actual end date passed will make a record active or inactive. The date passed is assumed to be validated

            if (proposedStartDate == DateTime.MinValue)
                return false;

            // check if start date of record is a day in the future, meaning the record is currently inactive
            if (DateTime.Compare(proposedStartDate, util.getCurrentDate()) > 0)
                return false;

            // if the passed end date is empty then the record is automaticaly Active
            if (proposedEndDate == DateTime.MinValue)
                return true;
            else
                // if today is before the passed actual end date then the record is active and otherwise, inactive
                return (DateTime.Compare(util.getCurrentDate(), proposedEndDate) < 0);      
        }

        protected Boolean isRecordValid(DataTable TableData, string employeeId, string id, string proposedRecordType ,DateTime proposedSD, DateTime proposedAED, string row, Dictionary<string, List<string>> empPositionValidationMsgs)
        {
            // returns a Boolean representing whether the proposed start date and proposed end date passed is valid in terms of the rest of existing employment position records. 
            // This method checks the other records to see if any other active records exist in order to validate the record. 

            if (proposedSD == DateTime.MinValue)
                return false;

            if (TableData != null)
            {
                int numActiveRows = 0, numActiveSubsRecords = 0;

                // state of proposed record
                bool isProposedRecordActive = isRecordActive(proposedSD, proposedAED);


                // check all rows in employeeposition except the row with the id passed as a parameter to this method
                foreach (DataRow dr in TableData.Rows)
                {
                    if (dr.Field<string>("employee_id") == employeeId && dr.Field<int>("id").ToString() != id)
                    {
                        DateTime tableDataRowStartDate = util.getDateFromDataRow(dr, "start_date");

                        DateTime tableDataRowEndDate = util.getDateFromDataRow(dr, "actual_end_date");

                        string isTableDataRowSubstantiveOrActing = dr.Field<Boolean>("is_substantive_or_acting") ? "Substantive" : "Acting";

                        // check how much substantive records available and do not account for the record being proposed if it is acting 
                        if (isTableDataRowSubstantiveOrActing == "Substantive" && isRecordActive(tableDataRowStartDate, tableDataRowEndDate))
                            numActiveSubsRecords++;

                        if (isTableDataRowSubstantiveOrActing == proposedRecordType)
                        {
                            // check if current record being evaluated is active
                            if (isRecordActive(tableDataRowStartDate, tableDataRowEndDate))
                                numActiveRows++;

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
                                    if(!empPositionValidationMsgs["clashingRecords"].Contains(row))
                                        empPositionValidationMsgs["clashingRecords"].Add(row);
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
                                    if (!empPositionValidationMsgs["multipleActiveRecords"].Contains(row))
                                        empPositionValidationMsgs["multipleActiveRecords"].Add(row);
                                    return false;
                                }


                                if (isProposedStartDateInRowPeriod || isProposedEndDateInRowPeriod)
                                {
                                    if (!empPositionValidationMsgs["clashingRecords"].Contains(row))
                                        empPositionValidationMsgs["clashingRecords"].Add(row);
                                    return false;
                                }

                            }
                        }
                        else if (isTableDataRowSubstantiveOrActing != proposedRecordType)
                        {

                            // check that start date of acting record is not before start date of current active substantive record
                            if (proposedRecordType == "Acting" && isRecordActive(tableDataRowStartDate, tableDataRowEndDate) && DateTime.Compare(proposedSD, tableDataRowStartDate) < 0)
                            {
                                if (!empPositionValidationMsgs["actingStartDateBeforeSub"].Contains(row))
                                    empPositionValidationMsgs["actingStartDateBeforeSub"].Add(row);
                                return false;
                            }

                            //// check that subst cannot become inactive when there is an active acting record
                            //if (proposedRecordType == "Substantive" && !isRecordActive(proposedSD, proposedAED) && isRecordActive(tableDataRowStartDate, tableDataRowEndDate))
                            //{
                            //    if (!empPositionValidationMsgs["noSubstantiveRecord"].Contains(row))
                            //        empPositionValidationMsgs["noSubstantiveRecord"].Add(row);
                            //    return false;
                            //}


                        }

                       
                    }

                }
                if (isProposedRecordActive)
                    numActiveRows++;

                if (numActiveRows <= 1)
                {
                    if (numActiveSubsRecords == 0 && proposedRecordType == "Acting")
                    {
                        if (!empPositionValidationMsgs["noSubstantiveRecord"].Contains(row))
                            empPositionValidationMsgs["noSubstantiveRecord"].Add(row);
                        return false;
                    }
                    return true;
                }
                else if (numActiveRows > 1)
                {
                    if (!empPositionValidationMsgs["multipleActiveRecords"].Contains(row))
                        empPositionValidationMsgs["multipleActiveRecords"].Add(row);
                    return false;
                }
                    
            }

            return true;

        }

        protected void populateErrorMessages(Dictionary<string, List<string>> msgs)
        {
            // check errors and show appropriate messages
            foreach (KeyValuePair<string, List<string>> kvp in msgs)
            {
                if (kvp.Value.Count > 0)
                {
                    switch (kvp.Key)
                    {
                        case "startDateInvalid":
                            invalidStartDateTxt.InnerText = $"Start date on row(s) {String.Join(", ", kvp.Value.ToArray())} is/are not valid";
                            invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");
                            break;
                        case "expectedEndDateInvalid":
                            invalidExpectedEndDateTxt.InnerText = $"Expected end date on row(s) {String.Join(", ", kvp.Value.ToArray())} is/are not valid";
                            invalidExpectedEndDatePanel.Style.Add("display", "inline-block");
                            break;
                        case "actualEndDateInvalid":
                            invalidActualEndDateTxt.InnerText = $"Actual end date on row(s) {String.Join(", ", kvp.Value.ToArray())} is/are not valid";
                            invalidActualEndDatePanel.Style.Add("display", "inline-block");
                            break;
                        case "dateComparisonExpected":
                            dateComparisonExpectedTxt.InnerText = $"Expected end date on row(s) {String.Join(", ", kvp.Value.ToArray())} cannot precede their corresponding start date";
                            dateComparisonExpectedValidationMsgPanel.Style.Add("display", "inline-block");
                            break;
                        case "dateComparisonActual":
                            dateComparisionActualTxt.InnerText = $"Actual end date on row(s) {String.Join(", ", kvp.Value.ToArray())} cannot precede their corresponding start date";
                            dateComparisonActualValidationMsgPanel.Style.Add("display", "inline-block");
                            break;
                        case "startDateIsWeekend":
                            startDateIsWeekendTxt.InnerText = $"Start date on row(s) {String.Join(", ", kvp.Value.ToArray())} is/are on the weekend";
                            startDateIsWeekendPanel.Style.Add("display", "inline-block");
                            break;
                        case "expectedEndDateIsWeekend":
                            expectedEndDateIsWeekendTxt.InnerText = $"Expected end date on row(s) {String.Join(", ", kvp.Value.ToArray())} is/are on the weekend";
                            expectedEndDateIsWeekendPanel.Style.Add("display", "inline-block");
                            break;
                        case "actualEndDateIsWeekend":
                            actualEndDateIsWeekendTxt.InnerText = $"Actual end date on row(s) {String.Join(", ", kvp.Value.ToArray())} is/are on the weekend";
                            actualEndDateOnWeekendPanel.Style.Add("display", "inline-block");
                            break;
                        case "invalidAnnualAndMaxVacationAmt":
                            invalidAnnualOrMaximumVacationLeaveAmtTxt.InnerText = $"Employment records being inserted on row(s) {String.Join(", ", kvp.Value.ToArray())} would result in annual amount of vacation leave being more than maximum accumulated vacation leave";
                            invalidAnnualOrMaximumVacationLeaveAmtPanel.Style.Add("display", "inline-block");
                            break;
                        case "clashingRecords":
                            clashingRecordsDbTxt.InnerText = $"Employment records being inserted on row(s) {String.Join(", ", kvp.Value.ToArray())} clash/clashes with another employment record of the same type for their corresponding employee";
                            clashingRecordsDbPanel.Style.Add("display", "inline-block");
                            break;
                        case "multipleActiveRecords":
                            multipleActiveRecordsDbTxt.InnerText = $"Employment records being inserted on row(s) {String.Join(", ", kvp.Value.ToArray())} would result in multiple active records of the same type for their corresponding employee";
                            multipleActiveRecordsDbPanel.Style.Add("display", "inline-block");
                            break;
                        case "noSubstantiveRecord":
                            noSubstantiveRecordDbTxt.InnerText = $"On row(s) {String.Join(", ", kvp.Value.ToArray())} at least one (1) active substantive employment record in order to have active acting records";
                            noSubstantiveRecordDbPanel.Style.Add("display", "inline-block");
                            break;
                        case "actingStartDateBeforeSub":
                            actingStartDateBeforeSubDbTxt.InnerText = $"Employment records being inserted on row(s) {String.Join(", ", kvp.Value.ToArray())} would result in the start date of the acting record being before the start date of the active substantive record";
                            actingStartDateBeforeSubDbPanel.Style.Add("display", "inline-block");
                            break;
                    }
                }
            }

        }

        protected void uploadDataBtn_Click(object sender, EventArgs e)
        {
            clearValidationMessages();
            Boolean isUploadSuccessful = false;
            if (uploadedFileName != null)
            {
                string filePath = Path.Combine("C:/ProgramData/HRLS/temp", uploadedFileName);
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
                                            dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                                            i++;
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
                    // only checks and validates data for columnns that are present in the excel file. If a column which is necessary but is not included in the file then the error will be thrown and caught when 
                    // trying to upload the data to the db (the last try catch)
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
                                        Boolean isDateValid = DateTime.TryParseExact(dr.ItemArray[colIndex].ToString(), "d/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out formattedDate);

                                        if (isDateValid && formattedDate != DateTime.MinValue)
                                        {
                                            dr.SetField(colIndex, formattedDate.ToString());
                                        }
                                        else
                                        {
                                            colNamesWithDateTimeTypeError.Add(metaDataDr.ItemArray[0].ToString());
                                            typeErrorTxt.InnerText = $"Data on row {rowIndex + 1} in {String.Join(", ", colNamesWithDateTimeTypeError.ToArray())} is supposed to be a date formatted as d/mm/yyyy (eg. 12/04/2020 or 1/02/2020)";
                                            typeErrorPanel.Style.Add("display", "inline-block");
                                        }

                                        isDataValid = isDataValid && isDateValid;
                                    }

                                    // check if data exceeds max length
                                    if(metaDataDr.ItemArray[2].ToString() != "-1")
                                    {
                                        // length of data exceeds max length
                                        if (dr.ItemArray[colIndex].ToString().Length > Convert.ToInt32(metaDataDr.ItemArray[2]))
                                        {
                                            dr.SetField(colIndex, dr.ItemArray[colIndex].ToString().Substring(0, Convert.ToInt32(metaDataDr.ItemArray[2])));
                                            //dr.ItemArray[colIndex] = dr.ItemArray[colIndex].ToString().Substring(0, Convert.ToInt32(metaDataDr.ItemArray[2]));
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
                                            dr.SetField(colIndex, DBNull.Value);
                                            //dr.ItemArray[colIndex] = DBNull.Value;
                                    }

                                }
                            }
                        }
                        
                    }

                    if (isDataValid && tableSelectDdl.SelectedValue == "employeeposition")
                    {
                        // reset empPositionValidationMsgs
                        Dictionary<string, List<string>> empPositionDateAndVacationValidationMsgs = new Dictionary<string, List<string>>()
                        {
                            {"startDateInvalid", new List<string>() },
                            {"expectedEndDateInvalid", new List<string>() },
                            {"actualEndDateInvalid", new List<string>() },
                            {"dateComparisonExpected", new List<string>() },
                            {"dateComparisonActual", new List<string>() },
                            {"startDateIsWeekend", new List<string>() },
                            {"expectedEndDateIsWeekend", new List<string>() },
                            {"actualEndDateIsWeekend", new List<string>() },
                            {"invalidAnnualAndMaxVacationAmt", new List<string>() }
                        };

                        Dictionary<string, List<string>> empPositionDbValidationMsgs = new Dictionary<string, List<string>>()
                        {
                            {"clashingRecords", new List<string>() },
                            {"multipleActiveRecords", new List<string>() },
                            {"noSubstantiveRecord", new List<string>() },
                            {"actingStartDateBeforeSub", new List<string>() }
                        };

                        Dictionary<string, List<string>> empPositionExcelValidationMsgs = new Dictionary<string, List<string>>()
                        {
                            {"clashingRecords", new List<string>() },
                            {"multipleActiveRecords", new List<string>() },
                            {"noSubstantiveRecord", new List<string>() },
                            {"actingStartDateBeforeSub", new List<string>() }
                        };
                        // check employment records for consistency for employeeposition table
                        DataTable employmentRecordsDt = new DataTable();
                        try
                        {
                            // empPositionValidationMsgs is initialized with column names already
                            Boolean areDatesValid = false, areRecordsValid = false, areVacationAmtsValid = false;

                            // temp has all fields identical to the employeeposition table 
                            DataTable temp = dt.Copy();
                            temp.Columns.Add("id", typeof(int));
                            temp.Columns.Add("has_received_notif_about_end_of_contract", typeof(Boolean));
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                temp.Rows[i].SetField<int>("id", -(i+1));
                            }

                            // used to check whcih records are validated in order to add back to data table being used to check for errors
                            DataTable validatedRecords = temp.Clone();

                            for (int rowIndex = 0; rowIndex < temp.Rows.Count; rowIndex++)
                            {
                                DataRow dr = temp.Rows[rowIndex];
                                employmentRecordsDt = new DataTable();
                                

                                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                                {
                                    connection.Open();
                                    string sql = $@"
                                        SELECT * FROM dbo.employeeposition 
                                        WHERE employee_id = {dr.Field<string>("employee_id")};
                                    ";
                                    using (SqlCommand command = new SqlCommand(sql, connection))
                                    {
                                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                                        {
                                            adapter.Fill(employmentRecordsDt);

                                        }
                                    }
                                }
                                areDatesValid = areRecordsValid = areVacationAmtsValid = false;

                                DateTime start = DateTime.MinValue,
                                         expectedEnd = DateTime.MinValue,
                                         actualEnd = DateTime.MinValue;
                                // check validity of dates 
                                if (dt.Columns.Contains("start_date") && dt.Columns.Contains("expected_end_date"))
                                {
                                    start = Convert.ToDateTime(dr.Field<string>("start_date"));
                                    expectedEnd = Convert.ToDateTime(dr.Field<string>("expected_end_date"));
                                    areDatesValid = validateDates(start, expectedEnd, true, (rowIndex + 1).ToString(), empPositionDateAndVacationValidationMsgs);
                                }


                                // validate start date and actual end date
                                if (dt.Columns.Contains("start_date") && dt.Columns.Contains("actual_end_date"))
                                {
                                    start = Convert.ToDateTime(dr.Field<string>("start_date"));
                                    if (!util.isNullOrEmpty(dr.Field<string>("actual_end_date")))
                                        actualEnd = Convert.ToDateTime(dr.Field<string>("actual_end_date"));
                                    areDatesValid = areDatesValid && validateDates(start, actualEnd, false, (rowIndex + 1).ToString(), empPositionDateAndVacationValidationMsgs);
                                }

                                // validate record to ensure more than one active record is not added
                                // populates empPositionValidationMsgs
                                if (areDatesValid)
                                {
                                    foreach(DataRow row in validatedRecords.Rows)
                                    {
                                        // if row is validated and is from the employee currently being checked 
                                        if (row.Field<string>("employee_id") == dr.Field<string>("employee_id"))
                                            employmentRecordsDt.ImportRow(row);
                                    }
                                    
                                    // ensure records are valid given records in db
                                    string isSubstantiveOrActing = dr.Field<string>("is_substantive_or_acting").ToLower() == "true" ? "Substantive" : "Acting";
                                    areRecordsValid = isRecordValid(employmentRecordsDt, dr.Field<string>("employee_id"), (dr.Field<int>("id")).ToString(), isSubstantiveOrActing, start, actualEnd, (rowIndex + 1).ToString(), empPositionDbValidationMsgs);

                                    if(areRecordsValid)
                                        validatedRecords.ImportRow(dr);
                                    // ensure records are valid with other records in excel sheet

                                }

                                areVacationAmtsValid = Convert.ToInt32(dr.Field<string>("annual_vacation_amt")) < Convert.ToInt32(dr.Field<string>("max_vacation_accumulation"));
                                if (!areVacationAmtsValid)
                                    empPositionDateAndVacationValidationMsgs["invalidAnnualAndMaxVacationAmt"].Add((rowIndex + 1).ToString());


                            }
                            isDataValid = validatedRecords.Rows.Count == dt.Rows.Count && areVacationAmtsValid;

                            if (!isDataValid)
                            {
                                populateErrorMessages(empPositionDateAndVacationValidationMsgs);
                                populateErrorMessages(empPositionDbValidationMsgs);
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            //exception logic
                            isDataValid = false;
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
                            if (ex.Message.Contains("The given ColumnMapping does not match up with any column in the source or destination."))
                                wrongTablePanel.Style.Add("display", "inline-block");
                            //exception logic
                            isUploadSuccessful = false;
                        }
                    }
                    else
                        isUploadSuccessful = false;

                    
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
            clearValidationMessages();
            uploadDataBtnPanel.Visible = tableSelectDdl.SelectedValue != "-";
        }
    }
}