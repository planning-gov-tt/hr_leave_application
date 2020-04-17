using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Configuration;
using System.Data.SqlClient;
using HR_LEAVEv2.Classes;

namespace HR_LEAVEv2
{
    public partial class SiteMaster : MasterPage
    {

        protected void Page_Init(object sender, EventArgs e)
        {
            // sets employee email and employee id in Session. This is used for identification of the current user throughout the application

            Auth auth = new Auth();

            // store employee's email in Session variable once it is empty, ie. the first time the Master page is loaded
            if (Session["emp_email"] == null)
                Session["emp_email"] = auth.getEmailOfSignedInUserFromActiveDirectory();

            // if Session["emp_email"] is not null after getting call to auth.getEmailOfSignedInUserFromActiveDirectory() or 
            // if the user info has been gotten already then set username of current user
            if (Session["emp_email"] != null)
            {
                string username = auth.getUserInfoFromActiveDirectory(Session["emp_email"].ToString());
                username = string.IsNullOrEmpty(username) ? "User not in Active Directory" : username;
                Session["emp_username"] = username;
            }
                
            // store employee's id in Session
            if (Session["emp_id"] == null && Session["emp_email"] != null)
                Session["emp_id"] = auth.getUserEmployeeId(Session["emp_email"].ToString());

            // store employee's permissions in Session
            if (Session["permissions"] == null && Session["emp_id"] != null)
                Session["permissions"] = auth.getUserPermissions(Session["emp_id"].ToString());

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> permissions = (List<string>)Session["permissions"];

            if (permissions != null)
            {
                // set displays based on what role a user plays 

                // supervisor
                if (permissions.Contains("sup_permissions"))
                {
                    supervisorPanel.Style.Add("display", "block");
                }

                // HR 1 or HR 2
                if (permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions"))
                {
                    hr1_hr2Panel.Style.Add("display", "block");
                }

                // HR 3
                if (permissions.Contains("hr3_permissions"))
                {
                    hr3Panel.Style.Add("display", "block");
                }
            }

            // load drop down list
            if (!IsPostBack)
            {

                // USED FOR DEVELOPMENT PURPOSES - allows users to switch between user profiles
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand(
                        $@"
                            SELECT 
                                [email]
                            FROM 
                                [dbo].[employee];
                        ", con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();
                    ddlSelectUser.DataTextField = ddlSelectUser.DataTextField = "email";
                    ddlSelectUser.DataSource = rdr;
                    ddlSelectUser.DataBind();
                }
                ddlSelectUser.SelectedValue = Session["emp_email"].ToString();
                // END DEVELOPMENT PURPOSES CODE --------------------------------------------------------

                // check for accumulations
                List<string> leaveTypes = new List<string>(),
                       accValues = new List<string>(),
                       accTypes = new List<string>();

                DateTime startDate = DateTime.MinValue;
                int yearsWorked = -1,
                    numDays = -1;
                string employmentType = string.Empty;
                try
                {
                    string sql = $@"
                            SELECT a.leave_type, a.accumulation_period_value, a.accumulation_type, a.num_days, ep.start_date, ep.years_worked, ep.employment_type
                            FROM [dbo].[accumulations] a
                            LEFT JOIN dbo.employeeposition ep
                            ON ep.employment_type = a.employment_type
                            WHERE ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date) AND ep.employee_id = {Session["emp_id"].ToString()};
                        ;
                    ";

                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    leaveTypes.Add(reader["leave_type"].ToString());
                                    accValues.Add(reader["accumulation_period_value"].ToString());
                                    accTypes.Add(reader["accumulation_type"].ToString());
                                    startDate = Convert.ToDateTime(reader["start_date"]);
                                    yearsWorked = Convert.ToInt32(reader["years_worked"]);
                                    numDays = Convert.ToInt32(reader["num_days"]);
                                    employmentType = reader["employment_type"].ToString();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                if(startDate != DateTime.MinValue && leaveTypes.Count > 0 && accValues.Count > 0 && accTypes.Count > 0 && yearsWorked != -1 && numDays != -1 && !String.IsNullOrEmpty(employmentType))
                {
                    // once employee is active and accumulations exist
                    Util util = new Util();
                    Dictionary<string, string> leaveTypeMapping = util.getLeaveTypeMapping();

                    int currentNumYearsWorked = -1;
                    if (employmentType == "Contract")
                        // get current number of years worked for their contract and compare with their current value for years worked
                        currentNumYearsWorked = util.getNumYearsBetween(startDate, DateTime.Today);
                        //currentNumYearsWorked = util.getNumYearsBetween(startDate, new DateTime(2020, 09, 28)); //for testing
                    else if (employmentType == "Public Service")
                        // get current number of years worked (number of new years passed)
                        currentNumYearsWorked = DateTime.Today.Year - startDate.Year;
                        //currentNumYearsWorked = 2021 - startDate.Year; //for testing

                    // apply accumulations
                    for (int i = 0; i < accValues.Count; i++)
                    {
                        // iterate through all accumulations and apply accordingly

                        // accValue of 0- Update at the start of new contract year
                        if(accValues[i] == "0" || accValues[i] == "1")
                        {
                            if (currentNumYearsWorked > yearsWorked)
                            {

                                // employee has started new contract year or new year so their leave must be updated
                                string sql = $@"
                                    BEGIN TRANSACTION;
                                ";
                                if(accTypes[i] == "Replace")
                                {
                                    sql += $@"
                                            UPDATE [dbo].[employee]
                                            SET {leaveTypeMapping[leaveTypes[i]]} =  {numDays}
                                            WHERE employee_id = {Session["emp_id"].ToString()};
                                            
                                    ";
                                } else if(accTypes[i] == "Add")
                                {
                                    sql += $@"
                                            UPDATE [dbo].[employee]
                                            SET {leaveTypeMapping[leaveTypes[i]]} =  (SELECT {leaveTypeMapping[leaveTypes[i]]} + {numDays} FROM dbo.employee WHERE employee_id = {Session["emp_id"].ToString()})
                                            WHERE employee_id = {Session["emp_id"].ToString()};
                                            
                                    ";
                                }

                                sql += $@"

                                    UPDATE [dbo].[employeeposition]
                                    SET years_worked = {currentNumYearsWorked}
                                    WHERE employee_id = {Session["emp_id"].ToString()};
                                    
                                    COMMIT;
                                    ";

                                try
                                {
                                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                                    {
                                        connection.Open();
                                        using (SqlCommand command = new SqlCommand(sql, connection))
                                        {
                                            int rowsAffected = command.ExecuteNonQuery();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }


                            }

                        }
                    }       
                }

                // set number of notifications
                string count = string.Empty;
                try
                {
                    string sql = $@"
                        SELECT COUNT([is_read]) AS 'num_notifs' FROM [dbo].[notifications] where [is_read] = 'No' AND [employee_id] = '{Session["emp_id"]}';
                    ";

                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    count = reader["num_notifs"].ToString();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                num_notifications.Text = count;
            }

        }

        protected void indexChanged(object sender, EventArgs e)
        {
            // sets user data when switching between users with dropdown used in development
            Session["emp_email"] = ddlSelectUser.SelectedItem.Text;
            Session["emp_id"] = Session["permissions"] = null;
            Response.Redirect(Request.RawUrl);
        }
    }
}