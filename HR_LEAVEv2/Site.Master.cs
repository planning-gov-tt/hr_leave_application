using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Configuration;
using System.Data.SqlClient;
using HR_LEAVEv2.Classes;
using System.Data;

namespace HR_LEAVEv2
{
    public partial class SiteMaster : MasterPage
    {
        enum acc_enum
        {
            leaveType = 0,
            accValue = 1,
            accType = 2,
            numDays = 3,
            annualVacationAmt = 4,
            maxVacationAccumulation = 5,
            can_accumulate_past_max = 6
        };

        Boolean isStartOfNewContractYear = false,
                       isStartOfNewYear = false;

        int currentNumYearsWorked = -1;
        Util util = new Util();
        User user = new User();


        protected void Page_Init(object sender, EventArgs e)
        {
            // sets employee email and employee id in Session. This is used for identification of the current user throughout the application

            Auth auth = new Auth();

            // store employee's email in Session variable once it is empty, ie. the first time the Master page is loaded
            if (user.currUserEmail == null)
                user.currUserEmail = auth.getEmailOfSignedInUserFromActiveDirectory();

            // if Session["emp_email"] is not null after getting call to auth.getEmailOfSignedInUserFromActiveDirectory() or 
            // if the user info has been gotten already then set username of current user
            if (user.currUserEmail != null)
            {
                string username = auth.getUserInfoFromActiveDirectory(user.currUserEmail);
                username = util.isNullOrEmpty(username) ? "User not in Active Directory" : username;
                user.currUserName = username;
            }else
                user.currUserName = "Anon";

            // store employee's id in Session
            if (user.currUserId == null && user.currUserEmail != null)
                user.currUserId = auth.getUserEmployeeId(user.currUserEmail);

            // store employee's permissions in Session
            if (Session["permissions"] == null && user.currUserId != null)
                Session["permissions"] = auth.getUserPermissions(user.currUserId);


            DateTime startDate = DateTime.MinValue;
            int yearsWorked = -1;
            string empType = string.Empty;

            currentNumYearsWorked = -1;
            // get years worked
            string sql = $@"
                        SELECT ep.start_date as startDate, ep.years_worked as yearsWorked, ep.employment_type as employmentType
                        FROM dbo.employeeposition ep
                        WHERE ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date) AND ep.employee_id = {user.currUserId};
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
                            startDate = Convert.ToDateTime(reader["startDate"]);
                            yearsWorked = Convert.ToInt32(reader["yearsWorked"]);
                            empType = reader["employmentType"].ToString();
                        }
                    }
                }
            }



            if (startDate != DateTime.MinValue && yearsWorked != -1 && !util.isNullOrEmpty(empType))
            {
                // used to display number of years worked in current job
                user.currUserNumYearsWorked = util.getNumYearsBetween(startDate, util.getCurrentDateToday());
                //Session["currNumYearsWorked"] = util.getNumYearsBetween(startDate, new DateTime(2021, 09, 27)); // for testing

                if (empType == "Contract")
                {
                    // get current number of years worked for their contract and compare with their current value for years worked

                    currentNumYearsWorked = util.getNumYearsBetween(startDate, util.getCurrentDateToday());
                    //currentNumYearsWorked = util.getNumYearsBetween(startDate, new DateTime(2021, 09, 27)); //for testing
                    isStartOfNewContractYear = currentNumYearsWorked > yearsWorked;
                }

                else if (empType == "Public Service")
                {
                    // get current number of years worked (number of new years passed)

                    currentNumYearsWorked = util.getCurrentDateToday().Year - startDate.Year;
                    //currentNumYearsWorked = 2021 - startDate.Year; //for testing
                    isStartOfNewYear = currentNumYearsWorked > yearsWorked;
                }


            }

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
                else
                    supervisorPanel.Style.Add("display", "none");

                // HR 1 or HR 2
                if (permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions"))
                {
                    hr1_hr2Panel.Style.Add("display", "block");
                }
                else
                    hr1_hr2Panel.Style.Add("display", "none");

                // HR 3
                if (permissions.Contains("hr3_permissions"))
                {
                    hr3Panel.Style.Add("display", "block");
                }
                else
                    hr3Panel.Style.Add("display", "none");

                // Admin
                if (permissions.Contains("admin_permissions"))
                {
                    adminPanel.Style.Add("display", "block");
                }
                else
                    adminPanel.Style.Add("display", "none");

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
                ddlSelectUser.SelectedValue = user.currUserEmail;
                // END DEVELOPMENT PURPOSES CODE --------------------------------------------------------


                // sql to be built which will either apply accumulations and then update the years worked or will simply update the years worked if no accumulations are present
                string updateSql = $@"
                                    BEGIN TRANSACTION;
                                ";

                // check for accumulations
                DataTable dt = new DataTable();
                dt.Columns.Add("leaveType", typeof(string));
                dt.Columns.Add("accValue", typeof(string));
                dt.Columns.Add("accType", typeof(string));
                dt.Columns.Add("numDays", typeof(string));
                dt.Columns.Add("annualVacationAmt", typeof(string));
                dt.Columns.Add("maxVacationAccumulation", typeof(string));

                try
                {
                    string accSql = $@"
                            SELECT a.leave_type as leaveType, a.accumulation_period_value as accValue, a.accumulation_type as accType, a.num_days as numDays, ep.annual_vacation_amt as annualVacationAmt, ep.max_vacation_accumulation as maxVacationAccumulation, ISNULL(ep.can_accumulate_past_max, 0) as can_accumulate_past_max
                            FROM [dbo].[accumulations] a
                            LEFT JOIN dbo.employeeposition ep
                            ON ep.employment_type = a.employment_type
                            WHERE ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date) AND ep.employee_id = {user.currUserId};
                        ;
                    ";

                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(accSql, connection))
                        {
                            SqlDataAdapter da = new SqlDataAdapter(command);
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                if (dt.Rows.Count > 0)
                {
                    // once employee is active and accumulations exist
                    Dictionary<string, string> leaveTypeMapping = util.getLeaveTypeMapping();

                    // apply accumulations
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        // iterate through all accumulations and apply accordingly

                        // accValue of 0- Update at the start of new contract year
                        // accValue of 1- Update at the start of the new year
                        if (dt.Rows[i].ItemArray[(int)acc_enum.accValue].ToString() == "0" || dt.Rows[i].ItemArray[(int)acc_enum.accValue].ToString() == "1")
                        {
                            if (isStartOfNewYear || isStartOfNewContractYear)
                            {
                                string numDays = dt.Rows[i].ItemArray[(int)acc_enum.leaveType].ToString() == "Vacation" ? dt.Rows[i].ItemArray[(int)acc_enum.annualVacationAmt].ToString() : dt.Rows[i].ItemArray[(int)acc_enum.numDays].ToString(),
                                    sqlLeaveTypeRef = leaveTypeMapping[dt.Rows[i].ItemArray[(int)acc_enum.leaveType].ToString()];

                                // employee has started new contract year or new year so their leave must be updated
                                if (dt.Rows[i].ItemArray[(int)acc_enum.accType].ToString() == "Replace")
                                {
                                    updateSql += $@"
                                            UPDATE [dbo].[employee]
                                            SET {sqlLeaveTypeRef} =  {numDays}
                                            WHERE employee_id = {user.currUserId};
                                            
                                    ";
                                }
                                else if (dt.Rows[i].ItemArray[(int)acc_enum.accType].ToString() == "Add")
                                {
                                    if (dt.Rows[i].ItemArray[(int)acc_enum.leaveType].ToString() == "Vacation" && !Convert.ToBoolean(dt.Rows[i].ItemArray[(int)acc_enum.can_accumulate_past_max].ToString()))
                                    {
                                        string maxVacationAmt = dt.Rows[i].ItemArray[(int)acc_enum.maxVacationAccumulation].ToString();
                                        // ensure amt does not go over max
                                        updateSql += $@"
                                            UPDATE [dbo].[employee]
                                            SET {sqlLeaveTypeRef} =  
                                                (SELECT IIF({sqlLeaveTypeRef} + {numDays} > {maxVacationAmt}, {maxVacationAmt}, {sqlLeaveTypeRef} + {numDays}) FROM dbo.employee WHERE employee_id = {user.currUserId})
                                            WHERE employee_id = {user.currUserId};
                                            
                                        ";
                                    }
                                    else
                                    {
                                        updateSql += $@"
                                            UPDATE [dbo].[employee]
                                            SET {sqlLeaveTypeRef} = (SELECT {sqlLeaveTypeRef} + {numDays} FROM dbo.employee WHERE employee_id = {user.currUserId})
                                            WHERE employee_id = {user.currUserId};
                                            
                                        ";
                                    }

                                }
                            }
                        }
                    }
                }

                if (isStartOfNewContractYear || isStartOfNewYear)
                {
                    updateSql += $@"

                        UPDATE [dbo].[employeeposition]
                        SET years_worked = {currentNumYearsWorked}
                        WHERE employee_id = {user.currUserId};
                                    
                        COMMIT;
                        ";

                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            using (SqlCommand command = new SqlCommand(updateSql, connection))
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



                // set number of notifications
                string count = string.Empty;
                try
                {
                    string sql = $@"
                        SELECT COUNT([is_read]) AS 'num_notifs' FROM [dbo].[notifications] where [is_read] = 'No' AND [employee_id] = '{user.currUserId}';
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
            user.currUserEmail = ddlSelectUser.SelectedItem.Text;
            user.currUserId = null;
            user.currUserNumYearsWorked = -1;
            user.currUserName = null;
            Session["permissions"] = null;
            Response.Redirect(Request.RawUrl);
        }
    }
}