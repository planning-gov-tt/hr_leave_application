using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.Admin
{
    public partial class Settings : System.Web.UI.Page
    {
        string selectedTable = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> permissions = (List<string>)Session["permissions"];
            if(permissions == null || !permissions.Contains("admin_permissions"))
                Response.Redirect("~/AccessDenied.aspx");

            selectedTable = DropDownList1.SelectedValue.ToString() != "-" ? DropDownList1.SelectedValue.ToString() : string.Empty;
            addPanel.Visible = !String.IsNullOrEmpty(selectedTable);

            clearFeedback();
            bindGridview();
        }


        protected void generateForm(DataTable dt)
        {
            List<string> tablesWithNoIntegerId = new List<string>() { "permission", "role", "rolepermission", "employee", "employeerole", "assignment", "leavetype", "employmenttype" };
            int startingIndex = tablesWithNoIntegerId.Contains(selectedTable) ? 0 : 1;
            for (int i = startingIndex; i < dt.Columns.Count; i++)
            {
                HtmlGenericControl tr = new HtmlGenericControl("tr");

                HtmlGenericControl tdLabel = new HtmlGenericControl("td");
                Label lb = new Label();
                lb.ID = $"label_{dt.Columns[i].ColumnName}";
                lb.Text = $"{dt.Columns[i].ColumnName}";
                lb.Style.Add("margin-right", "10px");
                tdLabel.Controls.Add(lb);

                HtmlGenericControl tdTxt = new HtmlGenericControl("td");
                TextBox tb = new TextBox();
                tb.ID = $"text_{dt.Columns[i].ColumnName}";
                tb.Width = new Unit("300px");
                tdTxt.Controls.Add(tb);

                tr.Controls.Add(tdLabel);
                tr.Controls.Add(tdTxt);

                formPlaceholder.Controls.Add(tr);

            }
        }

        protected void destroyForm()
        {
            formPlaceholder.Controls.Clear();
        }

        
        protected void bindGridview()
        {

            destroyForm();

            if (!String.IsNullOrEmpty(selectedTable))
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        string sql = $@"
                            SELECT * FROM dbo.{selectedTable};
                        ";
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            using(SqlDataAdapter adapter = new SqlDataAdapter(command))
                            {
                                DataTable dt = new DataTable();
                                adapter.Fill(dt);

                                GridView1.PageIndex = 0;
                                GridView1.DataSource = dt;
                                GridView1.DataBind();
                                if (dt.Rows.Count <= 0)
                                    noDataPanel.Style.Add("display", "inline-block");

                                // generate form controls 
                                generateForm(dt);
                                    
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //exception logic
                    throw ex;
                }
            }
            else
            {
                GridView1.DataSource = new DataTable();
                GridView1.DataBind();
                noTableSelectedPanel.Style.Add("display", "inline-block");
            }
                
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            bindGridview();
        }

        protected void clearFeedback()
        {
            noDataPanel.Style.Add("display", "none");
            noTableSelectedPanel.Style.Add("display", "none");
            deleteSuccessfulPanel.Style.Add("display", "none");
            deleteUnsuccessfulPanel.Style.Add("display", "none");
        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // if unwanted command triggers this function then do nothing
            if (e.CommandName == "Sort" || e.CommandName == "Page")
            {
                return;
            }

            // get row index in which button was clicked
            int index = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = GridView1.Rows[index];

            if(row.RowType == DataControlRowType.DataRow)
            {
                if(e.CommandName == "editRow")
                {
                } else if(e.CommandName == "deleteRow")
                {

                    Boolean isDeleteSuccessful = false;
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();

                            List<string> bridgeTables = new List<string>() { "assignment", "employeerole", "emptypeleavetype", "rolepermission" };

                            string sql = $"DELETE FROM dbo.{selectedTable} WHERE ";
                            if (bridgeTables.Contains(selectedTable))
                            {
                                // use all columns in where clause for identity
                                List<string> whereClauseComponents = new List<string>();
                                for (int i = 1; i < row.Cells.Count; i++)
                                    whereClauseComponents.Add($"{GridView1.HeaderRow.Cells[i].Text} = {row.Cells[i].Text}");

                                sql += $"{String.Join(" AND ", whereClauseComponents.ToArray())}";
                            }
                            else  
                                sql += $"{GridView1.HeaderRow.Cells[1].Text} = {row.Cells[1].Text}"; // use only first column to get identity

                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                int rowsAffected = command.ExecuteNonQuery();
                                isDeleteSuccessful = rowsAffected > 0;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception logic
                        isDeleteSuccessful = false;
                    }

                    if (isDeleteSuccessful)
                        deleteSuccessfulPanel.Style.Add("display", "inline-block"); //show delete success
                    else
                        deleteUnsuccessfulPanel.Style.Add("display", "inline-block");

                    bindGridview();

                }
            }

        }
    }
}