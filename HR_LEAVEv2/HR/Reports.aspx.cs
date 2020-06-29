using ClosedXML.Excel;
using HR_LEAVEv2.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace HR_LEAVEv2.HR
{
    public partial class Reports : System.Web.UI.Page
    {
        Util util = new Util();
        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> permissions = (List<string>)Session["permissions"];
            if (!(permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions") || permissions.Contains("hr3_permissions")))
                Response.Redirect("~/AccessDenied.aspx");

            errorPanel.Style.Add("display", "none");
        }

        protected void getIhrisReportBtn_Click(object sender, EventArgs e)
        {
            if(!Directory.Exists("C:/ProgramData/HRLS/temp"))
                Directory.CreateDirectory("C:/ProgramData/HRLS/temp");

            // once save is successful
            Boolean isSaveSuccessful = false;
            string fileName = $"{util.getCurrentDate().ToShortDateString().Replace("/", "_")}_EmpData.xlsx";
            try
            {
                using (XLWorkbook workBook = new XLWorkbook())
                {
                    //Read the first Sheet from Excel file.
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Employee ID", typeof(int));
                    dt.Columns.Add("Name", typeof(string));
                    dt.Columns.Add("Subs Post", typeof(string));
                    dt.Columns.Add("Acting Post", typeof(string));
                    dt.Columns.Add("Subs Vacation Leave Earned", typeof(int));
                    dt.Columns.Add("Acting Vacation Leave Earned", typeof(int));
                    dt.Columns.Add("Casual Leave (7/14 Days)", typeof(int));
                    dt.Columns.Add("Sick Leave (14 Days)", typeof(int));
                    dt.Columns.Add("Comments", typeof(string));

                    // read data from db and populate data table
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = $@"
                            SELECT e.employee_id, 
                                   e.first_name + ' ' + e.last_name as name, 
                                   p.pos_name as subs_post, 
                                   '-' as acting_post,
	                               e.vacation as subs_vacation, 
                                   0 as acting_vacation, 
                                   e.casual, 
                                   e.sick, 
                                   NULL as comments
                            FROM dbo.employee e
                            LEFT JOIN dbo.employeeposition ep ON ep.employee_id = e.employee_id
                            LEFT JOIN dbo.position p ON p.pos_id = ep.position_id
                            WHERE ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date)
                            ORDER BY name ASC;
                        ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    dt.Rows.Add(
                                        reader["employee_id"],
                                        reader["name"],
                                        reader["subs_post"],
                                        reader["acting_post"],
                                        reader["subs_vacation"],
                                        reader["acting_vacation"],
                                        reader["casual"],
                                        reader["sick"],
                                        reader["comments"]
                                    );
                                }
                            }
                        }
                    }

                    var worksheet = workBook.Worksheets.Add(dt, "Employee Leave Data");
                    worksheet.Columns("A", "I").AdjustToContents();
                    workBook.SaveAs($"C:/ProgramData/HRLS/temp/{fileName}");
                    isSaveSuccessful = true;
                }
            }
            catch (Exception exc)
            {
                isSaveSuccessful = false;
            }

            if (isSaveSuccessful)
            {
                // show file to download
                fileToDownloadLabel.Text = fileName;
                filesToDownloadPanel.Visible = true;
            }
            else
            {
                filesToDownloadPanel.Visible = false;
                errorPanel.Style.Add("display", "inline-block");
            }
        }

        protected void btnDownloadFiles_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] result = File.ReadAllBytes($"C:/ProgramData/HRLS/temp/{fileToDownloadLabel.Text}");
                Response.Clear();
                Response.AddHeader("Cache-Control", "no-cache, must-revalidate, post-check=0, pre-check=0");
                Response.AddHeader("Pragma", "no-cache");
                Response.AddHeader("Content-Description", "File Download");
                Response.AddHeader("Content-Type", "application/force-download");
                Response.AddHeader("Content-Transfer-Encoding", "binary\n");
                Response.AddHeader("content-disposition", "attachment;filename=" + fileToDownloadLabel.Text);
                Response.BinaryWrite(result);
                Response.Flush();
                Response.Close();
            }
            catch (System.Threading.ThreadAbortException exf)
            {
                //do nothing
                return;
            }
            catch (Exception ex)
            {
                //exception logic
                throw ex;
            }
        }
    }
}