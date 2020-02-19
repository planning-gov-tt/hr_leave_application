﻿using System;
using System.Configuration;
using System.Data.SqlClient;
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
            public string days_taken { get; set; }
            public string type_of_leave { get; set; }
            public string qualified { get; set; }
            public string supervisor_id { get; set; }
            public string supervisor_name { get; set; }
            public string comment { get; set; }
            public string recipient { get; set; }
            public string subject { get; set; }
        }

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

        public MailMessage getNewMailMessage(string subject, string recipient)
        {
            MailMessage message = new MailMessage();
            message.IsBodyHtml = true;
            message.Subject = subject;
            message.From = new MailAddress("hr.leave@planning.gov.tt");
            message.To.Add(new MailAddress(recipient));
            //message.To.Add(new MailAddress("Tristan.Sankar@planning.gov.tt"));
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

        /*
         * supervisor view: approved LA
         * Get the email body for the email sent to a supervisor when their employee's LA is approved
         **/
        public MailMessage getSupervisorViewLeaveApplicationApproved(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, details.recipient);
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
                            Check the status of employees' leave applications under Supervisor Actions > Leave Applications or click <a href='http://webtest/deploy/Supervisor/MyEmployeeLeaveApplications.aspx'>here</a>. Contact HR for any further information. <br/> 
                            <br/>
                            Regards,<br/>
                                HR
                        </div>


                    ";
            return msg;
        }

        /*
         * employee view: approved LA
         * Get the email body for the email sent to an employee when their LA is approved
         **/
        public MailMessage getEmployeeViewLeaveApplicationApproved(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, details.recipient);
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
                                Check the status of your leave applications under My Account > View Leave Logs or click <a href='http://webtest/deploy/Employee/MyAccount.aspx'>here</a>. Contact HR for any further information. <br/> 
                                <br/>
                                Regards,<br/>
                                    HR
                            </div>


                        ";
            return msg;
        }

        /*
         * supervisor view: not approved LA
         * Get the email body for the email sent to a supervisor when their employee's LA is not approved
         **/
        public MailMessage getSupervisorViewLeaveApplicationNotApproved(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, details.recipient);
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
                        Check the status of employees' leave applications under Supervisor Actions > Leave Applications or click <a href='http://webtest/deploy/Supervisor/MyEmployeeLeaveApplications.aspx'>here</a>. Contact HR for any further information. <br/> 
                        <br/>
                        Regards,<br/>
                            HR
                    </div>


                ";
            return msg;
        }

        /*
         * employee view: not approved LA
         * Get the email body for the email sent to an employee when their LA is not approved
         **/
        public MailMessage getEmployeeViewLeaveApplicationNotApproved(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, details.recipient);
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
                        Check the status of your leave applications under My Account > View Leave Logs or click <a href='http://webtest/deploy/Employee/MyAccount.aspx'>here</a>. Contact HR for any further information. <br/> 
                        <br/>
                        Regards,<br/>
                            HR
                    </div>


                ";
            return msg;
        }

        /*
         * HR view: recommended LA
         * Get the email body for the email sent to HR when a LA is recommended by an employee's supervisor
         **/
        public MailMessage getHRViewLeaveApplicationRecommended(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, details.recipient);
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
                        Check the status of employees' leave applications under HR Actions > Leave Applications or click <a href='http://webtest/deploy/HR/AllEmployeeLeaveApplications.aspx'>here</a>. Contact IT for any further information. <br/> 
                        <br/>
                        Regards,<br/>
                            HR
                    </div>


                ";
            return msg;
        }

        /*
         * employee view: recommended LA
         * Get the email body for the email sent to an employee when a LA is recommended by their supervisor
         **/
        public MailMessage getEmployeeViewLeaveApplicationRecommended(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, details.recipient);
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
                        Check the status of your leave applications under My Account > View Leave Logs or click <a href='http://webtest/deploy/Employee/MyAccount.aspx'>here</a>. Contact HR for any further information. <br/> 
                        <br/>
                        Regards,<br/>
                            HR
                    </div>


                ";
            return msg;
        }

        /*
         * employee view: not recommended LA
         * Get the email body for the email sent to an employee when a LA is not recommended by their supervisor
         **/
        public MailMessage getEmployeeViewLeaveApplicationNotRecommended(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, details.recipient);
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
                        Check the status of your leave applications under My Account > View Leave Logs or click <a href='http://webtest/deploy/Employee/MyAccount.aspx'>here</a>. Contact HR for any further information. <br/> 
                        <br/>
                        Regards,<br/>
                            HR
                    </div>
            ";
            return msg;
        }

        /*
         * supervisor view: submitted LA
         * Get the email body for the email sent to a supervisor when an employee submits a LA
         **/
        public MailMessage getSupervisorViewEmployeeSubmittedLeaveApplication(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, details.recipient);
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
                                Check the status of your employees' leave applications under Supervisor Actions > Leave Applications or click <a href='http://webtest/deploy/Supervisor/MyEmployeeLeaveApplications'>here</a>. Contact HR for any further information. <br/> 
                                <br/>
                                Regards,<br/>
                                    HR
                            </div>


                        ";
            return msg;
        }

        /*
         * employee view: submitted LA
         * Get the email body for the email sent to an employee they submit a LA
         **/
        public MailMessage getEmployeeViewEmployeeSubmittedLeaveApplication(EmailDetails details)
        {
            MailMessage msg = getNewMailMessage(details.subject, details.recipient);
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
                                Check the status of your leave applications under My Account > View Leave Logs or click <a href='http://webtest/deploy/Employee/MyAccount.aspx'>here</a>. Contact HR for any further information. <br/> 
                                <br/>
                                Regards,<br/>
                                    HR
                            </div>


                        ";
            return msg;
        }


    }
}