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
        }

        protected void resetUploadDataPage()
        {
            invalidFileTypePanel.Style.Add("display", "none");
            fileUploadedTooLargePanel.Style.Add("display", "none");
            noFileUploaded.Style.Add("display", "none");
            successfulDataInsertPanel.Style.Add("display", "none");
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
            FileUpload1.Dispose();
        }

        protected void clearFileBtn_Click(object sender, EventArgs e)
        {
            deleteUploadedFile();
        }

        protected DataTable getDataTableRepresentingTable(string tableName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    string sql = $@"
                            SELECT *
                            FROM dbo.{tableName}
                            WHERE 1=2
                        ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            return dt;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //exception logic
                return null;
            }

        }

        protected void uploadDataBtn_Click(object sender, EventArgs e)
        {
            Boolean isUploadSuccessful = false;
            if (Session["uploadedFile"] != null)
            {
                string filePath = Path.Combine(Server.MapPath("~/Assets/temp"), Session["uploadedFile"].ToString());
                if (File.Exists(filePath))
                {
                    //Create a new DataTable.
                    DataTable dt = getDataTableRepresentingTable(tableSelectDdl.SelectedValue);
                    //DataTable dt = new DataTable();
                    //dt.Columns.Add("test_col1", typeof(int));
                    //dt.Columns.Add("test_col2", typeof(string));

                    // Excel file cannot be open when trying to read it
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

                        // asks the question: are there more rows than just the header row?
                        isUploadSuccessful = dt.Rows.Count > 1;
                    }
                    catch (Exception exc)
                    {
                        isUploadSuccessful = false;
                    }


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
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception logic
                        isUploadSuccessful = false;
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