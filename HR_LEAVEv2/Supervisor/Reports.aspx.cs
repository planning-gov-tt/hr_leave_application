using System;
using System.Collections.Generic;

namespace HR_LEAVEv2.Supervisor
{
    public partial class Reports : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> permissions = (List<string>)Session["permissions"];
            if (permissions == null || !permissions.Contains("sup_permissions"))
                Response.Redirect("~/AccessDenied.aspx");
        }
    }
}