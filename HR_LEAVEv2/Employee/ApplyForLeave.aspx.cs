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
                    supervisor_select.selectedSupId = ViewState["supervisor_id"].ToString();
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

            // validate start date is a date
            try
            {
                start = Convert.ToDateTime(startDate);
            }
            catch (FormatException fe)
            {
                invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }

            // validate end date is a date
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
                //ensure start date is not a day before today
                if (DateTime.Compare(start, DateTime.Today) < 0)
                {
                    invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");
                    startDateBeforeTodayValidationMsgPanel.Style.Add("display", "inline-block");
                    isValidated = false;
                }

                // compare dates to ensure end date is not before start date
                if (DateTime.Compare(start, end) > 0)
                {
                    invalidEndDateValidationMsgPanel.Style.Add("display", "inline-block");
                    dateComparisonValidationMsgPanel.Style.Add("display", "inline-block");
                    isValidated = false;
                }

                //if leave type is vacation: ensure start date is at least one month from today
                if (typeOfLeave.SelectedValue.Equals("Vacation"))
                {
                    DateTime firstDateVacationCanBeTaken = DateTime.Today.AddMonths(1);

                    if(DateTime.Compare(start, firstDateVacationCanBeTaken) < 0)
                    {
                        invalidVacationStartDateMsgPanel.Style.Add("display", "inline-block");
                        isValidated = false;
                    }
                }


                return isValidated;
            }

            return false;
        }

        protected void submitLeaveApplication_Click(object sender, EventArgs e)
        {
            validationMsgPanel.Style.Add("display", "none");
            dateComparisonValidationMsgPanel.Style.Add("display", "none");
            invalidStartDateValidationMsgPanel.Style.Add("display", "none");
            startDateBeforeTodayValidationMsgPanel.Style.Add("display", "none");
            invalidEndDateValidationMsgPanel.Style.Add("display", "none");
            invalidVacationStartDateMsgPanel.Style.Add("display", "none");

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
                                submitButtonPanel.Style.Add("display", "none");
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