using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Configuration;
using System.Data.SqlClient;

namespace HR_LEAVEv2.UserControls
{
    public partial class LeaveCountUserControlBS4 : System.Web.UI.UserControl
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                string CS = ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(CS))
                {
                    SqlCommand cmd = new SqlCommand(
                        $@"
                            SELECT 
                                [sick], [vacation], [personal]
                            FROM 
                                [dbo].[employee]
                            WHERE
                                [employee_id] = '{Session["emp_id"]}'
                            ;
                        ", con);
                    con.Open();
                    SqlDataReader rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                    {
                        //h2Sick.Attributes["data-to"] = rdr["sick"].ToString();
                        //h2Vacation.Attributes["data-to"] = rdr["vacation"].ToString();
                        //h2Personal.Attributes["data-to"] = rdr["personal"].ToString();
                        ViewState["sick"] = rdr["sick"].ToString();
                        ViewState["vacation"] = rdr["vacation"].ToString();
                        ViewState["personal"] = rdr["personal"].ToString();
                    }
                }
            }
        }

    }
}