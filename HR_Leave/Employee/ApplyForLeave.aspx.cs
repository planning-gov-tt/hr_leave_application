using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HR_Leave
{
    public partial class ApplyForLeave : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["permissions"] == null)
                Response.Redirect("~/403AccessDenied.aspx");
        }

        protected Boolean validateDates(string startDate, string endDate)
        {
            // validate dates
            DateTime start = Convert.ToDateTime(startDate);
            DateTime end = Convert.ToDateTime(endDate);

            int value = DateTime.Compare(start, end);

            return value <= 0;
        }

        protected void submitLeaveApplication_ServerClick(object sender, EventArgs e)
        {
            /* data to be submitted
             * 1. Employee id
             * 2. Leave type
             * 3. Start date
             * 4. End date
             * 5. Supervisor id
             * 6. Comments
             */

            string empId, leaveType, startDate, endDate, supId, comments;
            empId = Session["emp_id"].ToString();
            leaveType = typeOfLeave.SelectedValue;
            startDate = txtFrom.Text.ToString();
            endDate = txtTo.Text.ToString();
            supId = supervisor_select.selectedSupId;
            comments = txtComments.Value.Length > 0 ? txtComments.Value.ToString(): null;

            // validate form values
            Boolean isValidated = validateDates(startDate, endDate);
            if (isValidated)
            {
                try
                {
                    string sql = $@"
                        INSERT INTO [dbo].[leavetransaction]
                           ([employee_id]
                           ,[leave_type]
                           ,[start_date]
                           ,[end_date]
                           ,[supervisor_id]
                           ,[status]
                           ,[comments])
                        VALUES
                           ( '{empId}'
                            ,'{leaveType}'
                            ,'{startDate}'
                            ,'{endDate}'
                            ,'{supId}'
                            ,'Pending'
                            ,@Comments
                            );
                    ";

                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                if(comments != null)
                                    command.Parameters.AddWithValue("@Comments", comments);
                                else
                                    command.Parameters.AddWithValue("@Comments", DBNull.Value);
                                int rowsAffected = command.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    submitButtonPanel.Style.Add("display", "none");

                                    successMsg.InnerText = "Application successfully submitted";
                                    successMsgPanel.Style.Add("display", "inline-block");
                                }
                            }
                        }
                } catch(Exception ex)
                {
                    validationMsg.InnerText = ex.Message;
                    validationMsgPanel.Style.Add("display", "inline-block");
                }

            } else
            {
                validationMsg.InnerText = "Start date cannot be before end date";
                validationMsgPanel.Style.Add("display", "inline-block");
            }
        }

        protected void refreshForm(object sender, EventArgs e)
        {
            Response.Redirect("~/Employee/ApplyForLeave.aspx");
        }
    }
}