using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Configuration;
using System.Data.SqlClient;

namespace HR_LEAVEv2
{
    public partial class SiteMaster : MasterPage
    {

        protected void Page_Init(object sender, EventArgs e)
        {


            Auth auth = new Auth();
            // store employee's email in Session
            
            if (Session["emp_email"] == null)
                Session["emp_email"] = auth.getEmailOfSignedInUserFromActiveDirectory();

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
            //MailMessage message = new MailMessage();
            //message.IsBodyHtml = true; //to make message body as html 
            //message.From = new MailAddress("mopd.hr.leave@gmail.com");

            //SmtpClient smtp = new SmtpClient();
            //smtp.Port = 587;
            //smtp.Host = "smtp.gmail.com"; //for gmail host  
            //smtp.EnableSsl = true;
            //smtp.UseDefaultCredentials = false;

            ////uses an application password
            //smtp.Credentials = new NetworkCredential("mopd.hr.leave@gmail.com", "kxeainpwpvdbxnxt");
            //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

            //try
            //{
            //    message.To.Add(new MailAddress("tristan.sankar@planning.gov.tt"));
            //    message.Subject = "request";
            //    message.Body = "hr leave app email test";
            //    try
            //    {
            //        smtp.Send(message);
            //    }
            //    catch (SmtpFailedRecipientException ex)
            //    {
            //        throw ex;
            //    }

            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}

            List<string> permissions = (List<string>)Session["permissions"];

            if (permissions != null)
            {
                if (permissions.Contains("sup_permissions"))
                {
                    supervisorPanel.Style.Add("display", "block");
                }
                if (permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions"))
                {
                    hr1_hr2Panel.Style.Add("display", "block");
                }
                if (permissions.Contains("hr3_permissions"))
                {
                    hr3Panel.Style.Add("display", "block");
                }
            }

            // load drop down list
            if (!IsPostBack)
            {
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
            Session["emp_email"] = ddlSelectUser.SelectedItem.Text;
            Session["emp_id"] = Session["permissions"] = null;
            Response.Redirect(Request.RawUrl);
        }
    }
}