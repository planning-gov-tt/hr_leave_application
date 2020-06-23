using HR_LEAVEv2.Classes;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace HR_LEAVEv2
{
    public partial class _Default : Page
    {
        public int numYearsWorked;
        User user = new User();

        protected void Page_Load(object sender, EventArgs e)
        {

            if (user.currUserNumYearsWorked != -1)
            {

                numYearsWorked = user.currUserNumYearsWorked;
                if (numYearsWorked > 0)
                    yearsWorkedPanel.Visible = true;
            }
            else
                yearsWorkedPanel.Visible = applyForLeaveCtaPanel.Visible = false;                
        }

        protected void applyForLeaveRedirectBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Employee/ApplyForLeave.aspx");
        }
    }
}