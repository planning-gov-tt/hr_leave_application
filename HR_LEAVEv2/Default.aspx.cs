using System;
using System.Collections.Generic;
using System.Web.UI;

namespace HR_LEAVEv2
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //if(Session["permissions"] != null)
            //{
            //    List<string> permissions = (List<string>)Session["permissions"];
            //    foreach(string permission in permissions)
            //    {
            //        Response.Write("permissions: "+ permission + "<br/>");
            //    }
            //} else
            //    Response.Write("null permission <br/>");

            //if (Session["emp_email"] != null)
            //    Response.Write("email:" + Session["emp_email"].ToString() + "<br/>");
            //else
            //    Response.Write("null email");
            //if (Session["emp_id"] != null)
            //    Response.Write("emp_id: "+Session["emp_id"].ToString() + "<br/>");
            //else
            //    Response.Write("null id");
        }
    }
}