using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HR_Leave
{
    public partial class GridViewTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // spoofing authenticated user id
            Session["emp_id"] = "83612";
            Session["first_name"] = "Melanie";
            Session["last_name"] = "Noel";
        }
    }
}