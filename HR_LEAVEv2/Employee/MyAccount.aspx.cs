using System;

namespace HR_LEAVEv2.Employee
{
    public partial class MyAccount : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["permissions"] == null)
                Response.Redirect("~/AccessDenied.aspx");
        }
    }
}