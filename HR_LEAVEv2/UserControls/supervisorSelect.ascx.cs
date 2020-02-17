using System;
using System.Collections.Generic;

namespace HR_LEAVEv2.UserControls
{
    public partial class supervisorSelect : System.Web.UI.UserControl
    {
        public string empId;

        protected void Page_Load(object sender, EventArgs e)
        {

            // set select for sql data source

            if (!this.IsPostBack)
            {
                Session["supervisor_id"] = String.IsNullOrEmpty(ComboBox1.SelectedValue) ? "-1" : ComboBox1.SelectedValue;       
                SqlDataSource1.SelectParameters.Add("empId", Session["emp_id"].ToString());

            }
            
        }

        protected void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["supervisor_id"] = ComboBox1.SelectedValue;
            Session["supervisor_name"] = ComboBox1.SelectedItem.Text;
        }
    }
}