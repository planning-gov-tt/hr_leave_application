using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HR_Leave
{
    public partial class supervisorSelect : System.Web.UI.UserControl
    {
        public string selectedSupId= "-1";
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
      {
           this.selectedSupId= ComboBox1.SelectedValue;
        }
    }
}