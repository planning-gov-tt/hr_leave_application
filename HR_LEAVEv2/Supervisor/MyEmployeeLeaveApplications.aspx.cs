using HR_LEAVEv2.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.Supervisor
{
    public partial class MyEmployeeLeaveApplications : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            User user = new User();
            if (user.permissions == null || (user.permissions != null && !user.permissions.Contains("sup_permissions")))
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