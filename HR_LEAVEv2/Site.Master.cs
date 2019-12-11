using System;
using System.Collections.Generic;
using System.Web.UI;

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
                //Session["emp_email"] = auth.activeDirectorySearch();
                Session["emp_email"] = "melanie.noel@planning.gov.tt";
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
        }
    }
}