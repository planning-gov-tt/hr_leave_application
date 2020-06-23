using HR_LEAVEv2.Classes;
using System;

namespace HR_LEAVEv2.Employee
{
    public partial class MyAccount : System.Web.UI.Page
    {
        User user = new User();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (user.permissions == null)
                Response.Redirect("~/AccessDenied.aspx");
        }
    }
}