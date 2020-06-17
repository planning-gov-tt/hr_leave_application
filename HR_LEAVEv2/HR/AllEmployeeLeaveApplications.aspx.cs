using System;
using System.Collections.Generic;

namespace HR_LEAVEv2.HR
{
    public partial class AllEmployeeLeaveApplications : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> permissions = (List<string>)Session["permissions"];
            if (permissions == null || !(permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions")))
                Response.Redirect("~/AccessDenied.aspx");

            // check if return btn should be shown
            returnToPreviousBtn.Visible = Request.QueryString.HasKeys();
        }

        protected void returnToPreviousBtn_Click(object sender, EventArgs e)
        {
            // returns to wherever is specified in the query string, returnUrl
            Response.Redirect(Request.QueryString["returnUrl"]);
        }
    }
}