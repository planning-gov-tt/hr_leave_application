using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.HR
{
    public partial class AllEmployeeLeaveApplications : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> permissions = (List<string>)Session["permissions"];
            if (permissions == null || !(permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions")))
                Response.Redirect("~/AccessDenied.aspx");
        }
    }
}