using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.UserControls
{
    public partial class supervisorSelect : System.Web.UI.UserControl
    {
        public string selectedSupId = "-1";
        protected void Page_Load(object sender, EventArgs e)
        {

            // set select for sql data source

            if (!this.IsPostBack)
            {
                List<string> permissions = (List<string>)Session["permissions"];
                if (permissions != null)
                {
                    if (permissions.Contains("sup_permissions"))
                    {
                        SqlDataSource1.SelectParameters.Clear();
                        SqlDataSource1.SelectParameters.Add("supervisorId", Session["emp_id"].ToString());
                    }
                }
            }
            
        }

        protected void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.selectedSupId = ComboBox1.SelectedValue;
        }
    }
}