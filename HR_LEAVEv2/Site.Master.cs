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

        };

        Boolean isStartOfNewContractYear = false,
                       isStartOfNewYear = false;

        int currentNumYearsWorked = -1;
        Util util = new Util();


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
                username = util.isNullOrEmpty(username) ? "User not in Active Directory" : username;
                Session["emp_username"] = username;
            }
                
            // store employee's id in Session
            if (Session["emp_id"] == null && Session["emp_email"] != null)
                Session["emp_id"] = auth.getUserEmployeeId(Session["emp_email"].ToString());

            // store employee's permissions in Session
            if (Session["permissions"] == null && Session["emp_id"] != null)
                Session["permissions"] = auth.getUserPermissions(Session["emp_id"].ToString());


            DateTime startDate = DateTime.MinValue;
            int yearsWorked = -1;
            string empType = string.Empty;

            currentNumYearsWorked = -1;
            isStartOfNewYear =  isStartOfNewContractYear = false;
            // get years worked
            try
            {
                string sql = $@"
                            SELECT ep.start_date as startDate, ep.years_worked as yearsWorked, ep.employment_type as employmentType
                            FROM dbo.employeeposition ep
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
                                startDate = Convert.ToDateTime(reader["startDate"]);
                                yearsWorked = Convert.ToInt32(reader["yearsWorked"]);
                                empType = reader["employmentType"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }



            if (startDate != DateTime.MinValue && yearsWorked != -1 && !util.isNullOrEmpty(empType))
            {
                Session["currNumYearsWorked"] = util.getNumYearsBetween(startDate, util.getCurrentDateToday());
                //Session["currNumYearsWorked"] = util.getNumYearsBetween(startDate, new DateTime(2022, 09, 28)); // for testing

                if (empType == "Contract")
                {
                    // get current number of years worked for their contract and compare with their current value for years worked

                    currentNumYearsWorked = util.getNumYearsBetween(startDate, util.getCurrentDateToday());
                    //currentNumYearsWorked = util.getNumYearsBetween(startDate, new DateTime(2022, 09, 28)); //for testing
                    isStartOfNewContractYear = currentNumYearsWorked > yearsWorked;
                }

                else if (empType == "Public Service")
                {
                    // get current number of years worked (number of new years passed)

                    currentNumYearsWorked = util.getCurrentDateToday().Year - startDate.Year;
                    //currentNumYearsWorked = 2022 - startDate.Year; //for testing
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
                } else
                    supervisorPanel.Style.Add("display", "none");

                // HR 1 or HR 2
                if (permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions"))
                {
                    hr1_hr2Panel.Style.Add("display", "block");
                } else
                    hr1_hr2Panel.Style.Add("display", "none");

                // HR 3
                if (permissions.Contains("hr3_permissions"))
                {
                    hr3Panel.Style.Add("display", "block");
                } else
                    hr3Panel.Style.Add("display", "none");

                // Admin
                if (permissions.Contains("admin_permissions"))
                {
                    adminPanel.Style.Add("display", "block");
                } else
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
                ddlSelectUser.SelectedValue = Session["emp_email"].ToString();
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

                try
                {
                    string accSql = $@"
                            SELECT a.leave_type as leaveType, a.accumulation_period_value as accValue, a.accumulation_type as accType, a.num_days as numDays
                            FROM [dbo].[accumulations] a
                            LEFT JOIN dbo.employeeposition ep
                            ON ep.employment_type = a.employment_type
                            WHERE ep.start_date <= GETDATE() AND (ep.actual_end_date IS NULL OR GETDATE() <= ep.actual_end_date) AND ep.employee_id = {Session["emp_id"].ToString()};
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

                if(dt.Rows.Count > 0)
                {
                    // once employee is active and accumulations exist
                    Dictionary<string, string> leaveTypeMapping = util.getLeaveTypeMapping();

                    // apply accumulations
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        // iterate through all accumulations and apply accordingly

                        // accValue of 0- Update at the start of new contract year
                        // accValue of 1- Update at the start of the new year
                        if(dt.Rows[i].ItemArray[(int)acc_enum.accValue].ToString() == "0" || dt.Rows[i].ItemArray[(int)acc_enum.accValue].ToString() == "1")
                        {
                            if (isStartOfNewYear || isStartOfNewContractYear)
                            {

                                // employee has started new contract year or new year so their leave must be updated
                                if(dt.Rows[i].ItemArray[(int)acc_enum.accType].ToString() == "Replace")
                                {
                                    updateSql += $@"
                                            UPDATE [dbo].[employee]
                                            SET {leaveTypeMapping[dt.Rows[i].ItemArray[(int)acc_enum.leaveType].ToString()]} =  {dt.Rows[i].ItemArray[(int)acc_enum.numDays].ToString()}
                                            WHERE employee_id = {Session["emp_id"].ToString()};
                                            
                                    ";
                                } else if(dt.Rows[i].ItemArray[(int)acc_enum.accType].ToString() == "Add")
                                {
                                    updateSql += $@"
                                            UPDATE [dbo].[employee]
                                            SET {leaveTypeMapping[dt.Rows[i].ItemArray[(int)acc_enum.leaveType].ToString()]} =  (SELECT {leaveTypeMapping[dt.Rows[i].ItemArray[(int)acc_enum.leaveType].ToString()]} + {dt.Rows[i].ItemArray[(int)acc_enum.numDays].ToString()} FROM dbo.employee WHERE employee_id = {Session["emp_id"].ToString()})
                                            WHERE employee_id = {Session["emp_id"].ToString()};
                                            
                                    ";
                                }
                            }
                        }
                    }       
                }

                if(isStartOfNewContractYear || isStartOfNewYear)
                {
                    updateSql += $@"

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