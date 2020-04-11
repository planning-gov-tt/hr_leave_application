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

        public Boolean isReturnToPreviousBtnVisible = false;
        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> permissions = (List<string>)Session["permissions"];
            if (permissions == null || !permissions.Contains("sup_permissions"))
                Response.Redirect("~/AccessDenied.aspx");

            if (!IsPostBack)
            {
                // check if filters should be preset
                if (Request.QueryString.HasKeys())
                {
                    // show return to previous button
                    isReturnToPreviousBtnVisible = true;
                }
            }
        }

        protected void returnToPreviousBtn_Click(object sender, EventArgs e)
        {
            // returns to wherever is specified in the query string, returnUrl
            Response.Redirect(Request.QueryString["returnUrl"]);
        }
    }
}