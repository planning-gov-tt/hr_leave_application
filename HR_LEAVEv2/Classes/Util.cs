using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace HR_LEAVEv2.Classes
{
    public class Util
    {
        public class EmailDetails
        {
            public string employee_id { get; set; }
            public string employee_name { get; set; }
            public string date_submitted { get; set; }
            public string start_date { get; set; }
            public string end_date { get; set; }
            public string expected_end_date { get; set; }
            public string days_taken { get; set; }
            public string type_of_leave { get; set; }
            public string qualified { get; set; }
            public string supervisor_id { get; set; }
            public string supervisor_name { get; set; }
            public string comment { get; set; }
            public string recipient { get; set; }
            public string subject { get; set; }
        }



        // _______________________________________________________________________________________________________________________________________________
        // DATE METHODS
        public DateTime getCurrentDate()
        {
            return DateTime.Now;
            //return DateTime.ParseExact("28/09/2020 8:00 am", "dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture);
            //return DateTime.ParseExact("02/01/2022 8:00 am", "dd/MM/yyyy h:mm tt", CultureInfo.InvariantCulture);
        }

        public DateTime getCurrentDateToday()
        {
            return DateTime.Today;
            //return DateTime.ParseExact("28/09/2020", "dd/MM/yyyy", CultureInfo.InvariantCulture);
            //return DateTime.ParseExact("02/01/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        public int getNumYearsBetween(DateTime start, DateTime end)
        {

            if(DateTime.Compare(start, end) < 0)
            {
                int literalYearsInBetween = end.Year - start.Year;
                if (start.Month > end.Month || (start.Month == end.Month && start.Day > end.Day))
                    literalYearsInBetween--;
                return literalYearsInBetween;
                
            }
            return 0;
            
        }

        public IEnumerable<DateTime> EachCalendarDay(DateTime startDate, DateTime endDate)
        {
            for (var date = startDate.Date; date.Date <= endDate.Date; date = date.AddDays(1))
                yield
            return date;
        }

        public List<string> getHolidaysBetween(DateTime start, DateTime end)
        {
            // returns a List<string> containing the names of holidays which fall in between the leave period specified

            Dictionary<string, DateTime> publicHolidays = new Dictionary<string, DateTime>() {
                    { "New Years", new DateTime(getCurrentDate().Year, 1, 1) },
                    { "New Years Day", new DateTime(getCurrentDate().Year + 1, 1, 1) },
                    { "Shouter Baptist Day", new DateTime(getCurrentDate().Year, 3, 30) },
                    { "Good Friday", new DateTime(getCurrentDate().Year, 4, 10) },
                    { "Easter Monday", new DateTime(getCurrentDate().Year, 4, 13) },
                    { "Indian Arrival Day", new DateTime(getCurrentDate().Year, 5, 30) },
                    { "Corpus Christi", new DateTime(getCurrentDate().Year, 6, 11) },
                    { "Labour Day", new DateTime(getCurrentDate().Year, 6, 19) },
                    { "Emancipation Day", new DateTime(getCurrentDate().Year, 8, 1) },
                    { "Independence Day", new DateTime(getCurrentDate().Year, 8, 31) },
                    { "Republic Day", new DateTime(getCurrentDate().Year, 9, 24) },
                    { "Christmas Day", new DateTime(getCurrentDate().Year, 12, 25) },
                    { "Boxing Day", new DateTime(getCurrentDate().Year, 12, 26) },
                };

            List<string> holidaysInBetween = new List<string>();
            foreach (DateTime day in EachCalendarDay(start, end))
            {
                foreach (KeyValuePair<string, DateTime> holiday in publicHolidays)
                {
                    if (DateTime.Compare(day, holiday.Value) == 0)
                        holidaysInBetween.Add(holiday.Key);
                }
            }

            return holidaysInBetween;
        }

        public DateTime getDateFromDataRow(DataRow dr, string columnName)
        {
            DateTime date;
            try
            {
                date = dr.Field<DateTime>(columnName);
            }
            catch (Exception e)
            {
                date = DateTime.MinValue;
            }

            return date;
        }
        // _______________________________________________________________________________________________________________________________________________

        // _______________________________________________________________________________________________________________________________________________
        // LEAVE TYPE METHODS
        public Dictionary<string, string> getLeaveTypeMapping()
        {
            // returns a dictionary with the leave type (type_id) as the Key and the column name representing the leave balance as the value
            // dictionary only contains leave types that have corresponding balances

            Dictionary<string, string> leaveBalanceColumnName = new Dictionary<string, string>();


            leaveBalanceColumnName.Add("Bereavement", "[bereavement]");
            leaveBalanceColumnName.Add("Casual", "[casual]");
            leaveBalanceColumnName.Add("Maternity", "[maternity]");
            leaveBalanceColumnName.Add("Paternity", "[paternity]");
            leaveBalanceColumnName.Add("Personal", "[personal]");
            leaveBalanceColumnName.Add("Pre-retirement", "[pre_retirement]");
            leaveBalanceColumnName.Add("Sick", "[sick]");
            leaveBalanceColumnName.Add("Vacation", "[vacation]");
            return leaveBalanceColumnName;
        }

        public bool isLeaveTypeWithoutBalance(string leaveType)
        {
            // returns a boolean representing whether the leave type has a corresponding balance or not

            Dictionary<string, string> leaveMappings= getLeaveTypeMapping();
            return !leaveMappings.ContainsKey(leaveType);
        }

        public List<string> getListOfAllLeaveTypes(List<string> permissions = null)
        {
            // returns a List<string> of all the leave types in the DB or leave types specific to a certain employment type Contract or Public service

            List<string> leaveTypes = new List<string>();

            if(permissions == null)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = $@"
                        SELECT [type_id] FROM [dbo].[leavetype]
                    ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    leaveTypes.Add(reader["type_id"].ToString());
                                }
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
            else
            {
                // assumes permissions is a list containing either 'Contract' or 'Public Service'
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = $@"
                        SELECT DISTINCT [leave_type] FROM [dbo].[emptypeleavetype] WHERE [employment_type] IN ({String.Join(", ", permissions.ToArray())})
                    ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    leaveTypes.Add(reader["leave_type"].ToString());
                                }
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
           
            return leaveTypes;
        }

        // _______________________________________________________________________________________________________________________________________________

        // _______________________________________________________________________________________________________________________________________________
        // NOTIFICATIONS METHODS
        public string resetNumNotifications(string employee_id)
        {
            /* What this function does:
             *     This function is used to reset the num_notifications Label control on the Master.aspx page. It returns the current number of notifications unread 
             *     for the employee associated with the employee_id passed
             *     
             * Where it is used:
             *     This function is used anywhere notifications are created: Employee applied for leave, Supervisor recommends/not recommends LA, HR approves/ not approves LA
             *     as well as in Notifications.cs when a user reads/ unreads or clears notifications
             * 
             * */

            // set number of notifications
            string count = string.Empty;
            try
            {
                string sql = $@"
                        SELECT COUNT([is_read]) AS 'num_notifs' FROM [dbo].[notifications] where [is_read] = 'No' AND [employee_id] = '{employee_id}';
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

            return count;
        }
        // _______________________________________________________________________________________________________________________________________________


        // _______________________________________________________________________________________________________________________________________________
        // AUDITING METHODS
        public Boolean addAuditLog(string actingEmployeeId, string affectedEmployeeId, string action)
        {
            string hostName = getLocalHostName();
            string ipv6Address = getIpv6Address(hostName);
            // add audit log
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    string sql = $@"
                                    INSERT INTO [dbo].[auditlog] ([machine_name], [ipv6_address], [acting_employee_id], [acting_employee_name], [affected_employee_id], [affected_employee_name], [action], [created_at])
                                    VALUES ( 
                                        '{hostName}',
                                        '{ipv6Address}',
                                        @ActingEmployeeId, 
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @ActingEmployeeId), 
                                        @AffectedEmployeeId,
                                        (SELECT first_name + ' ' + last_name FROM dbo.employee WHERE employee_id = @AffectedEmployeeId), 
                                        @Action, 
                                        @CreatedAt);
                                ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@ActingEmployeeId", actingEmployeeId);
                        command.Parameters.AddWithValue("@AffectedEmployeeId", affectedEmployeeId);
                        command.Parameters.AddWithValue("@Action", action);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM-dd-yyyy h:mm tt"));
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public string getLocalHostName()
        {
            return Dns.GetHostName();
        }

        public string getIpv6Address(string hostName)
        {
            return Dns.GetHostEntry(hostName).AddressList[0].ToString();
        }
        // _______________________________________________________________________________________________________________________________________________


        // _______________________________________________________________________________________________________________________________________________
        // EMAIL METHODS

        public MailMessage getNewMailMessage(string subject, List<string> recipients)
        {
            MailMessage message = new MailMessage();
            message.IsBodyHtml = true;
            message.Subject = subject;
            message.From = new MailAddress("hr.leave@planning.gov.tt");
            //foreach(string recipient in recipients)
            //{
            //    message.To.Add(new MailAddress(recipient));
            //}

            message.To.Add(new MailAddress("Tristan.Sankar@planning.gov.tt"));
            return message;
        }

        public Boolean sendMail(MailMessage message)
        {
            SmtpClient smtp = new SmtpClient();
            smtp.Port = 25;
            smtp.Host = "10.240.32.231"; //for PLANNING host 
            //smtp.EnableSsl = true;
            //smtp.UseDefaultCredentials = false;
            //smtp.Credentials = new NetworkCredential("hr.leave@planning.gov.tt", "p@ssword1");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

            try
            {
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        
        // returns mail message containing body of email reminding employee to submit leave
        public MailMessage getAlertEmpToSubmitLeave(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, new List<string>() { details.recipient });
            msg.Body = $@"
                <div style='margin-bottom:15px;'>
                    DO NOT REPLY<br/>
                    <br/>

                    This is a reminder to submit a leave application for the period of {details.start_date} to {details.end_date}. Apply for leave applications 
                    by clicking <a href='http://webtest/hrls/Employee/ApplyForLeave.aspx'>here</a>.
                <div>
                <div>
                    <br/>
                    Regards,<br/>
                        {details.supervisor_name}
                </div>
            ";

            return msg;
        }

        /*HR view: recommended LA
         * Get the email body for the email sent to HR when a LA is recommended by an employee's supervisor */
        public MailMessage getHRViewLeaveApplicationRecommended(EmailDetails details)
        {
            List<string> recipients = new List<string>(details.recipient.Split(';'));
            MailMessage msg = getNewMailMessage(details.subject, recipients);
            msg.Body = $@"
                    <style>
                        #leaveDetails{{
                            font-family: arial, sans-serif;
                            border-collapse: collapse;
                            width: 100%;
                        }}

                        #leaveDetails td,#leaveDetails th {{
                            border: 1px solid #dddddd;
                            text-align: left;
                            padding: 8px;
                        }}
                    </style>
                    <div style='margin-bottom:15px;'>
                        DO NOT REPLY<br/>
                        <br/>
                        {details.supervisor_name} recommended a leave application for approval. Details about the application can be found below: <br/>

                        <table id='leaveDetails'>
                            <tr>
                                <th> Date Submitted </th>
                                <th> Supervisor Name </th>
                                <th> Employee Name </th>
                                <th> Start Date </th>
                                <th> End Date </th>
                                <th> Days Taken </th>
                                <th> Leave Type </th>
                                <th> Status </th>
                                <th> Qualified </th>
                            </tr>
                            <tr>
                                <td>
                                    {details.date_submitted}
                                </td>
                                <td>
                                    {details.supervisor_name}
                                </td>
                                <td>
                                    {details.employee_name}
                                </td>
                                <td>
                                    {details.start_date}
                                </td>
                                <td>
                                    {details.end_date}
                                </td>
                                <td>
                                    {details.days_taken}
                                </td>
                                <td>
                                    {details.type_of_leave}
                                </td>
                                <td>
                                    Recommended
                                </td>
                                <td>
                                    {details.qualified}
                                </td>
                            </tr>
                        </table>
                        <br/>
                    </div>
                    <div>
                        Check the status of employees' leave applications under HR Actions > Leave Applications or click <a href='http://webtest/hrls/HR/AllEmployeeLeaveApplications.aspx'>here</a>. Contact IT for any further information. <br/> 
                        <br/>
                        Regards,<br/>
                            HR
                    </div>


                ";
            return msg;
        }

        /* supervisor view: LA approval undone
        *  Get the email body for the email sent to a supervisor when their empoyee's LA approval is undone */
        public MailMessage getSupervisorViewLeaveApprovalUndone(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, new List<string>() { details.recipient });
            msg.Body = $@"
                            <style>
                                #leaveDetails{{
                                    font-family: arial, sans-serif;
                                    border-collapse: collapse;
                                    width: 100%;
                                }}

                                #leaveDetails td,#leaveDetails th {{
                                    border: 1px solid #dddddd;
                                    text-align: left;
                                    padding: 8px;
                                }}
                            </style>
                            <div style='margin-bottom:15px;'>
                                DO NOT REPLY<br/>
                                <br/>
                                The approval for the leave application of {details.employee_name} was undone by HR. Details about the application can be found below: <br/>

                                <table id='leaveDetails'>
                                    <tr>
                                        <th> Date Submitted </th>
                                        <th> Employee Name </th>
                                        <th> Start Date </th>
                                        <th> End Date </th>
                                        <th> Days Taken </th>
                                        <th> Leave Type </th>
                                        <th> Status </th>
                                        <th> Qualified </th>
                                    </tr>
                                    <tr>
                                        <td>
                                            {details.date_submitted}
                                        </td>
                                        <td>
                                            {details.employee_name}
                                        </td>
                                        <td>
                                            {details.start_date}
                                        </td>
                                        <td>
                                            {details.end_date}
                                        </td>
                                        <td>
                                            {details.days_taken}
                                        </td>
                                        <td>
                                            {details.type_of_leave}
                                        </td>
                                        <td>
                                            Recommended
                                        </td>
                                        <td>
                                            {details.qualified}
                                        </td>
                                    </tr>
                                </table>
                                <br/>
                            </div>
                            <div>
                                Check the status of your leave applications under My Account > View Leave Logs or click <a href='http://webtest/hrls/Employee/MyAccount.aspx'>here</a>. Contact HR for any further information. <br/> 
                                <br/>
                                Regards,<br/>
                                    HR
                            </div>


                        ";
            return msg;
        }

        /* supervisor view: approved LA
        * Get the email body for the email sent to a supervisor when their employee's LA is approved */
        public MailMessage getSupervisorViewLeaveApplicationApproved(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, new List<string>() { details.recipient });
            msg.Body = $@"
                        <style>
                            #leaveDetails{{
                                font-family: arial, sans-serif;
                                border-collapse: collapse;
                                width: 100%;
                            }}

                            #leaveDetails td,#leaveDetails th {{
                                border: 1px solid #dddddd;
                                text-align: left;
                                padding: 8px;
                            }}
                        </style>
                        <div style='margin-bottom:15px;'>
                            DO NOT REPLY<br/>
                            <br/>
                            The leave application made by {details.employee_name} was approved. Details about the application can be found below: <br/>

                            <table id='leaveDetails'>
                                <tr>
                                    <th> Date Submitted </th>
                                    <th> Employee Name </th>
                                    <th> Start Date </th>
                                    <th> End Date </th>
                                    <th> Days Taken </th>
                                    <th> Leave Type </th>
                                    <th> Status </th>
                                    <th> Qualified </th>
                                </tr>
                                <tr>
                                    <td>
                                        {details.date_submitted}
                                    </td>
                                    <td>
                                        {details.employee_name}
                                    </td>
                                    <td>
                                        {details.start_date}
                                    </td>
                                    <td>
                                        {details.end_date}
                                    </td>
                                    <td>
                                        {details.days_taken}
                                    </td>
                                    <td>
                                        {details.type_of_leave}
                                    </td>
                                    <td>
                                        Approved
                                    </td>
                                    <td>
                                        {details.qualified}
                                    </td>
                                </tr>
                            </table>
                            <br/>
                        </div>
                        <div>
                            Check the status of employees' leave applications under Supervisor Actions > Leave Applications or click <a href='http://webtest/hrls/Supervisor/MyEmployeeLeaveApplications.aspx'>here</a>. Contact HR for any further information. <br/> 
                            <br/>
                            Regards,<br/>
                                HR
                        </div>


                    ";
            return msg;
        }

        /* supervisor view: not approved LA
         * Get the email body for the email sent to a supervisor when their employee's LA is not approved */
        public MailMessage getSupervisorViewLeaveApplicationNotApproved(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, new List<string>() { details.recipient });
            msg.Body = $@"
                    <style>
                        #leaveDetails{{
                            font-family: arial, sans-serif;
                            border-collapse: collapse;
                            width: 100%;
                        }}

                        #leaveDetails td,#leaveDetails th {{
                            border: 1px solid #dddddd;
                            text-align: left;
                            padding: 8px;
                        }}

                    </style>
                    <div style='margin-bottom:15px;'>
                        DO NOT REPLY<br/>
                        <br/>
                        The leave application of {details.employee_name} was not approved by HR. Details about the application can be found below: <br/>
                        {details.comment}
                        <table id='leaveDetails'>
                            <tr>
                                <th> Date Submitted </th>
                                <th> Employee Name </th>
                                <th> Start Date </th>
                                <th> End Date </th>
                                <th> Days Taken </th>
                                <th> Leave Type </th>
                                <th> Status </th>
                                <th> Qualified </th>
                            </tr>
                            <tr>
                                <td>
                                    {details.date_submitted}
                                </td>
                                <td>
                                    {details.employee_name}
                                </td>
                                <td>
                                    {details.start_date}
                                </td>
                                <td>
                                    {details.end_date}
                                </td>
                                <td>
                                    {details.days_taken}
                                </td>
                                <td>
                                    {details.type_of_leave}
                                </td>
                                <td>
                                    Not Approved
                                </td>
                                <td>
                                    {details.qualified}
                                </td>
                            </tr>
                        </table>
                        <br/>
                    </div>
                    <div>
                        Check the status of employees' leave applications under Supervisor Actions > Leave Applications or click <a href='http://webtest/hrls/Supervisor/MyEmployeeLeaveApplications.aspx'>here</a>. Contact HR for any further information. <br/> 
                        <br/>
                        Regards,<br/>
                            HR
                    </div>


                ";
            return msg;
        }

        /* supervisor view: submitted LA
         * Get the email body for the email sent to a supervisor when an employee submits a LA */
        public MailMessage getSupervisorViewEmployeeSubmittedLeaveApplication(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, new List<string>() { details.recipient });
            msg.Body = $@"
                            <style>
                                #leaveDetails{{
                                  font-family: arial, sans-serif;
                                  border-collapse: collapse;
                                  width: 100%;
                                }}

                                #leaveDetails td,#leaveDetails th {{
                                  border: 1px solid #dddddd;
                                  text-align: left;
                                  padding: 8px;
                                }}
                            </style>
                            <div style='margin-bottom:15px;'>
                                DO NOT REPLY <br/>
                                <br/>
                                {details.employee_name} submitted a leave application. Details about the application can be found below: <br/>
                                
                                <table id='leaveDetails'>
                                    <tr>
                                        <th> Date Submitted </th>
                                        <th> Employee Name </th>
                                        <th> Start Date </th>
                                        <th> End Date </th>
                                        <th> Days Taken </th>
                                        <th> Leave Type </th>
                                        <th> Status </th>
                                    </tr>
                                    <tr>
                                        <td>
                                            {details.date_submitted}
                                        </td>
                                        <td>
                                            {details.employee_name}
                                        </td>
                                        <td>
                                            {details.start_date}
                                        </td>
                                        <td>
                                            {details.end_date}
                                        </td>
                                        <td>
                                            {details.days_taken}
                                        </td>
                                        <td>
                                            {details.type_of_leave}
                                        </td>
                                        <td>
                                            Pending
                                        </td>
                                    </tr>
                                </table>
                                <br/>
                            </div>
                            <div>
                                Check the status of your employees' leave applications under Supervisor Actions > Leave Applications or click <a href='http://webtest/hrls/Supervisor/MyEmployeeLeaveApplications'>here</a>. Contact HR for any further information. <br/> 
                                <br/>
                                Regards,<br/>
                                    HR
                            </div>


                        ";
            return msg;
        }

        /* employee view: LA approval undone
        *  Get the email body for the email sent to an employee when their LA approval is undone */
        public MailMessage getEmployeeViewLeaveApprovalUndone(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, new List<string>() { details.recipient });
            msg.Body = $@"
                            <style>
                                #leaveDetails{{
                                    font-family: arial, sans-serif;
                                    border-collapse: collapse;
                                    width: 100%;
                                }}

                                #leaveDetails td,#leaveDetails th {{
                                    border: 1px solid #dddddd;
                                    text-align: left;
                                    padding: 8px;
                                }}
                            </style>
                            <div style='margin-bottom:15px;'>
                                DO NOT REPLY<br/>
                                <br/>
                                Your leave application approval was undone by HR. Details about the application can be found below: <br/>

                                <table id='leaveDetails'>
                                    <tr>
                                        <th> Date Submitted </th>
                                        <th> Supervisor Name </th>
                                        <th> Start Date </th>
                                        <th> End Date </th>
                                        <th> Days Taken </th>
                                        <th> Leave Type </th>
                                        <th> Status </th>
                                        <th> Qualified </th>
                                    </tr>
                                    <tr>
                                        <td>
                                            {details.date_submitted}
                                        </td>
                                        <td>
                                            {details.supervisor_name}
                                        </td>
                                        <td>
                                            {details.start_date}
                                        </td>
                                        <td>
                                            {details.end_date}
                                        </td>
                                        <td>
                                            {details.days_taken}
                                        </td>
                                        <td>
                                            {details.type_of_leave}
                                        </td>
                                        <td>
                                            Recommended
                                        </td>
                                        <td>
                                            {details.qualified}
                                        </td>
                                    </tr>
                                </table>
                                <br/>
                            </div>
                            <div>
                                Check the status of your leave applications under My Account > View Leave Logs or click <a href='http://webtest/hrls/Employee/MyAccount.aspx'>here</a>. Contact HR for any further information. <br/> 
                                <br/>
                                Regards,<br/>
                                    HR
                            </div>


                        ";
            return msg;
        }

        /* employee view: approved LA
        *  Get the email body for the email sent to an employee when their LA is approved */
        public MailMessage getEmployeeViewLeaveApplicationApproved(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, new List<string>() { details.recipient });
            msg.Body = $@"
                            <style>
                                #leaveDetails{{
                                    font-family: arial, sans-serif;
                                    border-collapse: collapse;
                                    width: 100%;
                                }}

                                #leaveDetails td,#leaveDetails th {{
                                    border: 1px solid #dddddd;
                                    text-align: left;
                                    padding: 8px;
                                }}
                            </style>
                            <div style='margin-bottom:15px;'>
                                DO NOT REPLY<br/>
                                <br/>
                                Your leave application was approved by HR. Details about the application can be found below: <br/>

                                <table id='leaveDetails'>
                                    <tr>
                                        <th> Date Submitted </th>
                                        <th> Supervisor Name </th>
                                        <th> Start Date </th>
                                        <th> End Date </th>
                                        <th> Days Taken </th>
                                        <th> Leave Type </th>
                                        <th> Status </th>
                                        <th> Qualified </th>
                                    </tr>
                                    <tr>
                                        <td>
                                            {details.date_submitted}
                                        </td>
                                        <td>
                                            {details.supervisor_name}
                                        </td>
                                        <td>
                                            {details.start_date}
                                        </td>
                                        <td>
                                            {details.end_date}
                                        </td>
                                        <td>
                                            {details.days_taken}
                                        </td>
                                        <td>
                                            {details.type_of_leave}
                                        </td>
                                        <td>
                                            Approved
                                        </td>
                                        <td>
                                            {details.qualified}
                                        </td>
                                    </tr>
                                </table>
                                <br/>
                            </div>
                            <div>
                                Check the status of your leave applications under My Account > View Leave Logs or click <a href='http://webtest/hrls/Employee/MyAccount.aspx'>here</a>. Contact HR for any further information. <br/> 
                                <br/>
                                Regards,<br/>
                                    HR
                            </div>


                        ";
            return msg;
        }

        /* employee view: recommended LA
         * Get the email body for the email sent to an employee when a LA is recommended by their supervisor */
        public MailMessage getEmployeeViewLeaveApplicationRecommended(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, new List<string>() { details.recipient });
            msg.Body = $@"
                    <style>
                        #leaveDetails{{
                            font-family: arial, sans-serif;
                            border-collapse: collapse;
                            width: 100%;
                        }}

                        #leaveDetails td,#leaveDetails th {{
                            border: 1px solid #dddddd;
                            text-align: left;
                            padding: 8px;
                        }}
                    </style>
                    <div style='margin-bottom:15px;'>
                        DO NOT REPLY<br/>
                        <br/>
                        Your leave application was recommended by your supervisor, {details.supervisor_name}. Details about the application can be found below: <br/>

                        <table id='leaveDetails'>
                            <tr>
                                <th> Date Submitted </th>
                                <th> Supervisor Name </th>
                                <th> Start Date </th>
                                <th> End Date </th>
                                <th> Days Taken </th>
                                <th> Leave Type </th>
                                <th> Status </th>
                                <th> Qualified </th>
                            </tr>
                            <tr>
                                <td>
                                    {details.date_submitted}
                                </td>
                                <td>
                                    {details.supervisor_name}
                                </td>
                                <td>
                                    {details.start_date}
                                </td>
                                <td>
                                    {details.end_date}
                                </td>
                                <td>
                                    {details.days_taken}
                                </td>
                                <td>
                                    {details.type_of_leave}
                                </td>
                                <td>
                                    Recommended
                                </td>
                                <td>
                                    {details.qualified}
                                </td>
                            </tr>
                        </table>
                        <br/>
                    </div>
                    <div>
                        Check the status of your leave applications under My Account > View Leave Logs or click <a href='http://webtest/hrls/Employee/MyAccount.aspx'>here</a>. Contact HR for any further information. <br/> 
                        <br/>
                        Regards,<br/>
                            HR
                    </div>


                ";
            return msg;
        }

        /* employee view: not recommended LA
         * Get the email body for the email sent to an employee when a LA is not recommended by their supervisor */
        public MailMessage getEmployeeViewLeaveApplicationNotRecommended(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, new List<string>() { details.recipient });
            msg.Body = $@"
                    <style>
                        #leaveDetails{{
                            font-family: arial, sans-serif;
                            border-collapse: collapse;
                            width: 100%;
                        }}

                        #leaveDetails td,#leaveDetails th {{
                            border: 1px solid #dddddd;
                            text-align: left;
                            padding: 8px;
                        }}

                    </style>
                    <div style='margin-bottom:15px;'>
                        DO NOT REPLY<br/>
                        <br/>
                        Your leave application was not recommended by your supervisor, {details.supervisor_name}. Details about the application can be found below: <br/>
                        {details.comment}
                        <table id='leaveDetails'>
                            <tr>
                                <th> Date Submitted </th>
                                <th> Supervisor Name </th>
                                <th> Start Date </th>
                                <th> End Date </th>
                                <th> Days Taken </th>
                                <th> Leave Type </th>
                                <th> Status </th>
                                <th> Qualified </th>
                            </tr>
                            <tr>
                                <td>
                                    {details.date_submitted}
                                </td>
                                <td>
                                    {details.supervisor_name}
                                </td>
                                <td>
                                    {details.start_date}
                                </td>
                                <td>
                                    {details.end_date}
                                </td>
                                <td>
                                    {details.days_taken}
                                </td>
                                <td>
                                    {details.type_of_leave}
                                </td>
                                <td>
                                    Not Recommended
                                </td>
                                <td>
                                    {details.qualified}
                                </td>
                            </tr>
                        </table>
                        <br/>
                    </div>
                    <div>
                        Check the status of your leave applications under My Account > View Leave Logs or click <a href='http://webtest/hrls/Employee/MyAccount.aspx'>here</a>. Contact HR for any further information. <br/> 
                        <br/>
                        Regards,<br/>
                            HR
                    </div>
            ";
            return msg;
        }

        /* employee view: not approved LA
         * Get the email body for the email sent to an employee when their LA is not approved */
        public MailMessage getEmployeeViewLeaveApplicationNotApproved(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, new List<string>() { details.recipient });
            msg.Body = $@"
                    <style>
                        #leaveDetails{{
                            font-family: arial, sans-serif;
                            border-collapse: collapse;
                            width: 100%;
                        }}

                        #leaveDetails td,#leaveDetails th {{
                            border: 1px solid #dddddd;
                            text-align: left;
                            padding: 8px;
                        }}
                    </style>
                    <div style='margin-bottom:15px;'>
                        DO NOT REPLY<br/>
                        <br/>
                        Your leave application was not approved by HR. Details about the application can be found below: <br/>
                        {details.comment}
                        <table id='leaveDetails'>
                            <tr>
                                <th> Date Submitted </th>
                                <th> Supervisor Name </th>
                                <th> Start Date </th>
                                <th> End Date </th>
                                <th> Days Taken </th>
                                <th> Leave Type </th>
                                <th> Status </th>
                                <th> Qualified </th>
                            </tr>
                            <tr>
                                <td>
                                    {details.date_submitted}
                                </td>
                                <td>
                                    {details.supervisor_name}
                                </td>
                                <td>
                                    {details.start_date}
                                </td>
                                <td>
                                    {details.end_date}
                                </td>
                                <td>
                                    {details.days_taken}
                                </td>
                                <td>
                                    {details.type_of_leave}
                                </td>
                                <td>
                                    Not Approved
                                </td>
                                <td>
                                    {details.qualified}
                                </td>
                            </tr>
                        </table>
                        <br/>
                    </div>
                    <div>
                        Check the status of your leave applications under My Account > View Leave Logs or click <a href='http://webtest/hrls/Employee/MyAccount.aspx'>here</a>. Contact HR for any further information. <br/> 
                        <br/>
                        Regards,<br/>
                            HR
                    </div>


                ";
            return msg;
        }

        /* employee view: submitted LA
         * Get the email body for the email sent to an employee when they submit a LA */
        public MailMessage getEmployeeViewEmployeeSubmittedLeaveApplication(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, new List<string>() { details.recipient });
            msg.Body = $@"
                            <style>
                                #leaveDetails{{
                                  font-family: arial, sans-serif;
                                  border-collapse: collapse;
                                  width: 100%;
                                }}

                                #leaveDetails td,#leaveDetails th {{
                                  border: 1px solid #dddddd;
                                  text-align: left;
                                  padding: 8px;
                                }}
                            </style>
                            <div style='margin-bottom:15px;'>
                                DO NOT REPLY <br/>
                                <br/>
                                You submitted a leave application. Details about the application can be found below: <br/>
                                <table id='leaveDetails'>
                                    <tr>
                                        <th> Date Submitted </th>
                                        <th> Supervisor Name </th>
                                        <th> Start Date </th>
                                        <th> End Date </th>
                                        <th> Days Taken </th>
                                        <th> Leave Type </th>
                                        <th> Status </th>
                                    </tr>
                                    <tr>
                                        <td>
                                            {details.date_submitted}
                                        </td>
                                        <td>
                                            {details.supervisor_name}
                                        </td>
                                        <td>
                                            {details.start_date}
                                        </td>
                                        <td>
                                            {details.end_date}
                                        </td>
                                        <td>
                                            {details.days_taken}
                                        </td>
                                        <td>
                                            {details.type_of_leave}
                                        </td>
                                        <td>
                                            Pending
                                        </td>
                                    </tr>
                                </table>
                                <br/>
                            </div>
                            <div>
                                Check the status of your leave applications under My Account > View Leave Logs or click <a href='http://webtest/hrls/Employee/MyAccount.aspx'>here</a>. Contact HR for any further information. <br/> 
                                <br/>
                                Regards,<br/>
                                    HR
                            </div>


                        ";
            return msg;
        }

        public MailMessage getReminderThatEmployeeContractIsEndingSoon(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, new List<string>() { details.recipient });
            msg.Body = $@"
                <div style='margin-bottom:15px;'>
                    DO NOT REPLY<br/>
                    <br/>

                    This is a reminder that your contract is expected to be ending soon on {details.expected_end_date}. Contact HR for more information.
                <div>
                <div>
                    <br/>
                    Regards,<br/>
                        HR
                </div>
            ";

            return msg;
        }
        // _______________________________________________________________________________________________________________________________________________


        // _______________________________________________________________________________________________________________________________________________
        // STRING METHODS
        public string sanitizeStringForAsciiCharacters(string str)
        {
            // sanitize search strings for any ASCII characters that may cause trouble 
            // single quote ('), double quote ("), open bracket and close bracket

            // single quote
            str = str.Replace("&#39;", "'");

            //double quote
            str = str.Replace("&#34;", "\"");

            //open bracket
            str = str.Replace("&#40;", "(");

            //close bracket
            str = str.Replace("&#41;", ")");

            return str;

        }
        public Boolean isNullOrEmpty(string s)
        {
            return String.IsNullOrEmpty(s) || String.IsNullOrWhiteSpace(s) || s == "&nbsp;";
        }
        // _______________________________________________________________________________________________________________________________________________


        // _______________________________________________________________________________________________________________________________________________
        // QUALIFIED METHOD
        public string getSqlForCalculatingQualifiedField(string leaveTypeToBeCompared, string empId)
        {
            // returns the sql representing the IIF statement that determines whether a LA is qualified or not based on the applied for leave type and the corresponding leave balance

            // get all leave types
            List<string> leaveTypes = getListOfAllLeaveTypes();
            Dictionary<string, string> leaveTypeMappings = getLeaveTypeMapping();
            List<string> conditionalList = new List<string>();

            foreach (string leaveType in leaveTypes)
            {
                // if leave type has no balance then it is automatically qualified
                if (isLeaveTypeWithoutBalance(leaveType))
                    conditionalList.Add($"('{leaveTypeToBeCompared}' = '{leaveType}')");

                // check if the number of days applied for is less than the number of days available in the corresponding leave balance
                else
                    conditionalList.Add($"('{leaveTypeToBeCompared}' = '{leaveType}' AND @DaysTaken <= e.{leaveTypeMappings[leaveType]})");
            }

            return $@"
                    (
                        SELECT IIF(({String.Join(" OR ", conditionalList.ToArray())}), 'Yes', 'No')
                        FROM [dbo].[employee] e
                        WHERE e.employee_id = {empId}
                    )
                ";
        }
        // _______________________________________________________________________________________________________________________________________________
    }
}