using HR_LEAVEv2.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.HR
{
    public partial class LeaveSettings : System.Web.UI.Page
    {
        Util util = new Util();
        protected void Page_Load(object sender, EventArgs e)
        {
            List<string> permissions = (List<string>)Session["permissions"];
            if (permissions == null || !(permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions")))
                Response.Redirect("~/AccessDenied.aspx");

            if (!IsPostBack)
            {
                // set leave types dropdown list
                bindLeaveTypeList();
            }

            // set accumulation period dropdown list
            bindAccPeriodList();

            bindGridview();

        }

        protected void bindGridview()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                            SELECT [id], [employment_type], [leave_type], [accumulation_period_text], [accumulation_type], [num_days] FROM [accumulations];
                        ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(dt);

                        noAccumulationsPanel.Visible = dt.Rows.Count <= 0;
                        accExistsPanel.Visible = dt.Rows.Count > 0;
                        if (dt.Rows.Count > 0)
                        {
                            GridView1.DataSource = dt;
                            GridView1.DataBind();
                        }                       
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void bindLeaveTypeList()
        {
            List<string> leaveTypesWithBalances = util.getLeaveTypeMapping().Keys.ToList();
            for (int i = 0; i < leaveTypesWithBalances.Count; i++)
            {
                leaveTypesWithBalances[i] = $"'{leaveTypesWithBalances[i]}'";
            }

            string leaveTypeListStr = $"({String.Join(", ", leaveTypesWithBalances.ToArray())})";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                            SELECT [type_id] 
                            FROM [dbo].[leavetype] 
                            WHERE [type_id] IN {leaveTypeListStr};
                        ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(dt);

                        leaveTypeList.DataTextField = leaveTypeList.DataValueField = "type_id";
                        leaveTypeList.DataSource = dt;
                        leaveTypeList.DataBind();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected void bindAccPeriodList()
        {
            DataTable items = new DataTable();
            items.Columns.Add("acc_period", typeof(string));
            items.Columns.Add("value", typeof(string));
            if (empTypeList.SelectedIndex == -1 || (empTypeList.SelectedItem != null && empTypeList.SelectedItem.Text == "Contract"))
                items.Rows.Add("At the start of a new contract year", "0");
            else
            {
                if (empTypeList.SelectedItem != null && empTypeList.SelectedItem.Text == "Public Service")
                    items.Rows.Add("At the start of a new calendar year", "1");
            }

            accumulationPeriodList.DataSource = items;
            accumulationPeriodList.DataTextField = "acc_period";
            accumulationPeriodList.DataValueField = "value";
            accumulationPeriodList.DataBind();
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            bindGridview();
        }

        protected void resetAddAccumulationForm()
        {
            empTypeList.DataBind();
            bindLeaveTypeList();
            bindAccPeriodList();
            accumulationTypeList.SelectedIndex = 0;
            numDaysForAccumulation.Text = string.Empty;

            //saveAccumulation.Visible = true;
            //succesfulInsertionOfAccPanel.Style.Add("display", "none");
            //errorInsertingAccPanel.Style.Add("display", "none");
        }

        protected void addAccumulationBtn_Click(object sender, EventArgs e)
        {
            resetAddAccumulationForm();
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#addAccumulationModal').modal({'show':true, 'backdrop':'static', 'keyboard':false});", true);
        }

        protected void saveAccumulation_Click(object sender, EventArgs e)
        {
            // save accumulation to DB

            string empType = empTypeList.SelectedItem == null ? empTypeList.Items[0].Text : empTypeList.SelectedItem.Text,
                   leaveType = leaveTypeList.SelectedItem == null ? leaveTypeList.Items[0].Text : leaveTypeList.SelectedItem.Text,
                   accText = accumulationPeriodList.SelectedItem == null ? accumulationPeriodList.Items[0].Text : accumulationPeriodList.SelectedItem.Text,
                   accValue = accumulationPeriodList.SelectedItem == null ? accumulationPeriodList.Items[0].Value : accumulationPeriodList.SelectedItem.Value,
                   accType = accumulationTypeList.SelectedItem == null ? accumulationTypeList.Items[0].Value : accumulationTypeList.SelectedItem.Value,
                   numDays = numDaysForAccumulation.Text;

            Boolean isInsertSuccessful = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                            INSERT INTO [dbo].[accumulations]([employment_type], [leave_type], [accumulation_period_text], [accumulation_period_value],[accumulation_type], [num_days])
                            VALUES ('{empType}', '{leaveType}', '{accText}', '{accValue}', '{accType}', {numDays});
                        ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        isInsertSuccessful = rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                isInsertSuccessful = false;
            }

            if (isInsertSuccessful)
            {
                // show success message
                //succesfulInsertionOfAccPanel.Style.Add("display", "inline-block");
                //saveAccumulation.Visible = false;
                bindGridview();
            }
            //else
            //{
            //    // show error message
            //    errorInsertingAccPanel.Style.Add("display", "inline-block");
            //}
        }

        protected void closeAddAccumulationBtn_Click(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "none", "$('#addAccumulationModal').modal('hide');", true);
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            
            DataTable dt = GridView1.DataSource as DataTable;

            // delete from db
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();

                    string sql = $@"
                            DELETE FROM [dbo].[accumulations] WHERE id = {dt.Rows[e.RowIndex].ItemArray[0]};
                        ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            this.bindGridview();
        }
    }
}