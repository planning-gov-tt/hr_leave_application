using HR_LEAVEv2.Classes;
using System;
using System.Collections.Generic;

namespace HR_LEAVEv2.HR
{
    public partial class AllEmployeeLeaveApplications : System.Web.UI.Page
    {
        User user = new User();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (user.permissions == null || (user.permissions != null && !(user.permissions.Contains("hr1_permissions") || user.permissions.Contains("hr2_permissions"))))
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