using System;
using System.Configuration;
using System.Data.SqlClient;


namespace HR_LEAVEv2.Employee
{
    public partial class ApplyForLeave : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["permissions"] == null)
                Response.Redirect("~/AccessDenied.aspx");

            if (this.IsPostBack)
            {
                if (ViewState["supervisor_id"] != null && ViewState["supervisor_id"].ToString() != "-1")
                {
                    supervisor_select.selectedSupId = ViewState["supervisor_id"].ToString();
                }
            }
        }

        protected Boolean validateDates(string startDate, string endDate)
        {

            invalidStartDateValidationMsgPanel.Style.Add("display", "none");
            invalidEndDateValidationMsgPanel.Style.Add("display", "none");
            dateComparisonValidationMsgPanel.Style.Add("display", "none");


            DateTime start, end;
            start = end = DateTime.MinValue;
            Boolean isValidated = true;

            // validate dates
            try
            {
                start = Convert.ToDateTime(startDate);
            }
            catch (FormatException fe)
            {
                invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }
            try
            {
                end = Convert.ToDateTime(endDate);
            }
            catch (FormatException fe)
            {
                invalidEndDateValidationMsgPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }

            if (isValidated)
            {
                if (DateTime.Compare(start, end) > 0)
                {
                    dateComparisonValidationMsgPanel.Style.Add("display", "inline-block");
                    return false;
                }
                else
                    return true;
            }

            return false;
        }

        protected void submitLeaveApplication_Click(object sender, EventArgs e)
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
            comments = txtComments.Value.Length > 0 ? txtComments.Value.ToString() : null;

            ViewState["supervisor_id"] = supId;

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
                            if (comments != null)
                                command.Parameters.AddWithValue("@Comments", comments);
                            else
                                command.Parameters.AddWithValue("@Comments", DBNull.Value);
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                validationMsgPanel.Style.Add("display", "none");
                                dateComparisonValidationMsgPanel.Style.Add("display", "none");
                                invalidStartDateValidationMsgPanel.Style.Add("display", "none");
                                invalidEndDateValidationMsgPanel.Style.Add("display", "none");
                                submitButtonPanel.Style.Add("display", "none");

                                successMsg.InnerText = "Application successfully submitted";
                                successMsgPanel.Style.Add("display", "inline-block");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    validationMsg.InnerText = ex.Message;
                    validationMsgPanel.Style.Add("display", "inline-block");
                }

            }
        }

        protected void refreshForm(object sender, EventArgs e)
        {
            Response.Redirect("~/Employee/ApplyForLeave.aspx");
        }
    }
}