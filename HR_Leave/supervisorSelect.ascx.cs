using System;

namespace HR_Leave
{
    public partial class supervisorSelect : System.Web.UI.UserControl
    {
        public string selectedSupId= "-1";
        protected void Page_Load(object sender, EventArgs e)
        {
            //if (ViewState["selectedSupId"] != null)
            //    this.selectedSupId = ViewState["selectedSupId"].ToString();
        }

        protected void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
      {
           this.selectedSupId= ComboBox1.SelectedValue;
            ViewState["selectedSupId"] = this.selectedSupId;
        }
    }
}