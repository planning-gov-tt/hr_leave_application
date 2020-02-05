using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.UserControls
{
    public partial class supervisorSelect : System.Web.UI.UserControl
    {
        public string validationGroup { get; set; }

        public string empId;

        protected void Page_Load(object sender, EventArgs e)
        {

            // set select for sql data source

            if (!this.IsPostBack)
            {
                Session["supervisor_id"] = String.IsNullOrEmpty(ComboBox1.SelectedValue) ? "-1" : ComboBox1.SelectedValue;
                List<string> permissions = (List<string>)Session["permissions"];
                if (permissions != null)
                {
                    if (permissions.Contains("sup_permissions"))
                    {
                        SqlDataSource1.SelectParameters.Clear();
                        SqlDataSource1.SelectParameters.Add("supervisorId", Session["emp_id"].ToString());
                    }
                }
                if (!String.IsNullOrEmpty(validationGroup))
                    comboBoxRequiredValidator.ValidationGroup = validationGroup;

               
                SqlDataSource1.SelectParameters.Add("empId", Session["emp_id"].ToString());

            }
            
        }

        protected void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session["supervisor_id"] = ComboBox1.SelectedValue;
        }
    }
}