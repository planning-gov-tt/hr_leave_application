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
                                            dr.ItemArray[colIndex] = formattedDate;
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