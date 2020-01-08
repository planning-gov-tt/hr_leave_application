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
            {
                Session["emp_email"] = auth.getEmailOfSignedInUserFromActiveDirectory();
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
                string CS = ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(CS))
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