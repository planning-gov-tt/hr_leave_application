using System;
using System.Collections.Generic;
using System.Web.UI;

namespace HR_LEAVEv2
{
    public partial class _Default : Page
    {
        public string numYearsWorked;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["currNumYearsWorked"] != null)
            {
                numYearsWorked = Session["currNumYearsWorked"].ToString();
                if (Convert.ToInt32(numYearsWorked) > 0)
                    yearsWorkedPanel.Visible = true;
            }
                
        }
    }
}