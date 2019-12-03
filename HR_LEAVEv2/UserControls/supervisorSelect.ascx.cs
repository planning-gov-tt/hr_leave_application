using System;
using System.Collections.Generic;

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
                        SqlDataSource1.SelectCommand = $@"
                        select e.employee_id, e.first_name + ' ' + e.last_name as 'Supervisor Name'
                        from[HRLeaveTestDb].[dbo].[employee] e
                        left join[HRLeaveTestDb].[dbo].[employeerole] er
                        on e.employee_id = er.employee_id
                        where er.role_id = 'sup' and e.employee_id != {Session["emp_id"].ToString()}; ";
                    }
                    else
                    {
                        SqlDataSource1.SelectCommand = $@"
                        select e.employee_id, e.first_name + ' ' + e.last_name as 'Supervisor Name'
                        from[HRLeaveTestDb].[dbo].[employee] e
                        left join[HRLeaveTestDb].[dbo].[employeerole] er
                        on e.employee_id = er.employee_id
                        where er.role_id = 'sup';";
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