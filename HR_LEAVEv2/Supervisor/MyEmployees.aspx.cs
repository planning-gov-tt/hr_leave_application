using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.Supervisor
{
    public partial class MyEmployees : System.Web.UI.Page
    {

        DataTable dt;
        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> permissions = (List<string>)Session["permissions"];
            if (permissions == null || !permissions.Contains("sup_permissions"))
                Response.Redirect("~/AccessDenied.aspx");

            if (!IsPostBack)
            {
                BindListView();
            }
        }

        protected void BindListView()
        {
            dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Mobile", typeof(string));
            dt.Columns.Add("College", typeof(string));
            dt.Rows.Add(1, "Rahul", "8505012345", "MITRC");
            dt.Rows.Add(2, "Pankaj", "8505012346", "MITRC");
            dt.Rows.Add(3, "Sandeep", "8505012347", "MITRC");
            dt.Rows.Add(4, "Sanjeev", "8505012348", "MITRC");
            dt.Rows.Add(5, "Neeraj", "8505012349", "MITRC");
            dt.AcceptChanges();
            ListView1.DataSource = dt;
            ListView1.DataBind();
        }

        protected void searchBox_TextChanged(object sender, EventArgs e)
        {

        }

        protected void ListView1_PagePropertiesChanging(object sender, PagePropertiesChangingEventArgs e)
        {
            // set current page startindex,max rows and rebind to false  
            DataPager1.SetPageProperties(e.StartRowIndex, e.MaximumRows, false);
            // Rebind the ListView1  
            BindListView();
        }
    }
}