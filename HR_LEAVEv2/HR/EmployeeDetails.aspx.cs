using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HR_LEAVEv2.HR
{
    public partial class EmployeeDetails : System.Web.UI.Page
    {
        List<string> permissions = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            permissions = (List<string>)Session["permissions"];
            if (permissions == null || !(permissions.Contains("hr1_permissions") || permissions.Contains("hr2_permissions") || permissions.Contains("hr3_permissions")))
                Response.Redirect("~/AccessDenied.aspx");
            if (!this.IsPostBack)
            {
                ViewState["Gridview1_dataSource"] = null;
                this.bindGridview();
            }
                
        }

        protected Boolean validateDates(string startDate, string endDate)
        {

            invalidStartDateValidationMsgPanel.Style.Add("display", "none");
            invalidEndDateValidationMsgPanel.Style.Add("display", "none");
            dateComparisonValidationMsgPanel.Style.Add("display", "none");


            DateTime start, end;
            start = end = DateTime.MinValue;
            Boolean isValidated = true;

            // validate start date is a date
            try
            {
                start = Convert.ToDateTime(startDate);
            }
            catch (FormatException fe)
            {
                invalidStartDateValidationMsgPanel.Style.Add("display", "inline-block");
                isValidated = false;
            }

            if (!String.IsNullOrEmpty(endDate))
            {
                // validate end date is a date
                try
                {
                    end = Convert.ToDateTime(endDate);
                }
                catch (FormatException fe)
                {
                    invalidEndDateValidationMsgPanel.Style.Add("display", "inline-block");
                    isValidated = false;
                }

                if (isValidated)
                {

                    // compare dates to ensure end date is not before start date
                    if (DateTime.Compare(start, end) > 0)
                    {
                        invalidEndDateValidationMsgPanel.Style.Add("display", "inline-block");
                        dateComparisonValidationMsgPanel.Style.Add("display", "inline-block");
                        isValidated = false;
                    }
                }

            }

            return isValidated;
        }

     
        protected void showFormBtn_Click(object sender, EventArgs e)
        {
            addEmpRecordForm.Style.Add("display", "inline-block");
            showFormBtn.Style.Add("display", "none");
        }

        protected void addNewRecordBtn_Click(object sender, EventArgs e)
        {
            /* data to be submitted
             * 1. position
             * 2. Start date
             * 3. End date
             * 4. employment type
             * 5. dept
             */

            string  position_id, position_name, startDate, endDate, emp_type, dept_id, dept_name;
            emp_type = empTypeList.SelectedValue;
            startDate = txtStartDate.Text.ToString();
            endDate = txtEndDate.Text.ToString();
            position_id = positionList.SelectedValue;
            position_name = positionList.SelectedItem.Text;
            dept_id = deptList.SelectedValue;
            dept_name = deptList.SelectedItem.Text;

            // validate form values
            Boolean isValidated = validateDates(startDate, endDate);
            if (isValidated)
            {

                DataTable dt;
                if (ViewState["Gridview1_dataSource"] == null)
                {
                    dt = new DataTable();
                    dt.Columns.Add("employment_type", typeof(string));
                    dt.Columns.Add("dept_id", typeof(string));
                    dt.Columns.Add("dept_name", typeof(string));
                    dt.Columns.Add("pos_id", typeof(string));
                    dt.Columns.Add("pos_name", typeof(string));
                    dt.Columns.Add("start_date", typeof(string));
                    dt.Columns.Add("expected_end_date", typeof(string));
                } else
                    dt = ViewState["Gridview1_dataSource"] as DataTable;

                DateTime expected_end_date = DateTime.MinValue;
                if (String.IsNullOrEmpty(endDate))
                {
                    // no expected end date
                    if (emp_type == "Contract")
                    {
                        // if contract worker then add expected end date that is 3 years from start date
                        expected_end_date = Convert.ToDateTime(startDate);
                        expected_end_date = expected_end_date.AddYears(3);
                    }
                }
                else
                    expected_end_date = Convert.ToDateTime(endDate);
      
                //employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date
                dt.Rows.Add(emp_type, dept_id, dept_name, position_id, position_name, startDate, expected_end_date.ToString("MM/dd/yyyy"));
    
                ViewState["Gridview1_dataSource"] = dt;

                GridView1.DataSource = dt;
                GridView1.DataBind();

            }
            //this.resetAddEmploymentRecordFormFields();
        }

        protected void resetAddEmploymentRecordFormFields()
        {
            txtStartDate.Text = String.Empty;
            txtEndDate.Text = String.Empty;
            empTypeList.SelectedIndex = 0;
            deptList.SelectedIndex = 0;
            positionList.SelectedIndex = 0;
        }

        protected void cancelNewRecordBtn_Click(object sender, EventArgs e)
        {
            //reset fields
            this.resetAddEmploymentRecordFormFields();
            addEmpRecordForm.Style.Add("display", "none");
            showFormBtn.Style.Add("display", "inline-block");
        }

        protected void bindGridview()
        {
            DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;
            if(dt != null)
            {
                GridView1.DataSource = dt;
                GridView1.DataBind();
            }
        }

        protected void GridView1_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridView1.PageIndex = e.NewPageIndex;
            this.bindGridview();
        }

        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;
            if(dt != null)
            {
                dt.Rows[e.RowIndex].Delete();
                ViewState["Gridview1_dataSource"] = dt;
            }
            this.bindGridview();
        }

        protected void submitBtn_Click(object sender, EventArgs e)
        {
            fullFormErrorPanel.Style.Add("display", "none");
            emailNotFoundErrorPanel.Style.Add("display", "none");
            noEmploymentRecordEnteredErrorPanel.Style.Add("display", "none");
            fullFormSubmitSuccessPanel.Style.Add("display", "none");

            // get data from every field and submit

            string emp_id, ihris_id, email, firstname, lastname, sick_leave, personal_leave, casual_leave, vacation_leave, bereavement_leave, maternity_leave, pre_retirement_leave;
            List<string> authorizations = new List<string>() { "emp" };
            DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;

            emp_id = employeeIdInput.Text;
            ihris_id = ihrisNumInput.Text;
            email = adEmailInput.Text;

            // get first name, last name and username from active directory
            Auth auth = new Auth();
            string nameFromAd = auth.getUserInfoFromActiveDirectory(email);

            Boolean isInsertSuccessful = false;

            if (!String.IsNullOrEmpty(nameFromAd) && dt !=null )
            {
                // set first and last name
                string[] name = nameFromAd.Split(' ');
                firstname = name[0];
                lastname = name[1];

                //set username
                string username = $"PLANNING\\ {firstname} {lastname}";

                // get data from leave 
                sick_leave = String.IsNullOrEmpty(sickLeaveInput.Text) ? "0" : sickLeaveInput.Text;
                personal_leave = String.IsNullOrEmpty(personalLeaveInput.Text) ? "0" : personalLeaveInput.Text;
                casual_leave = String.IsNullOrEmpty(casualLeaveInput.Text) ? "0" : casualLeaveInput.Text;
                vacation_leave = String.IsNullOrEmpty(vacationLeaveInput.Text) ? "0" : vacationLeaveInput.Text;
                bereavement_leave = String.IsNullOrEmpty(bereavementLeaveInput.Text) ? "0" : bereavementLeaveInput.Text;
                maternity_leave = String.IsNullOrEmpty(maternityLeaveInput.Text) ? "0" : maternityLeaveInput.Text;
                pre_retirement_leave = String.IsNullOrEmpty(preRetirementLeaveInput.Text) ? "0" : preRetirementLeaveInput.Text;

                // get data from checkboxes and set authorizations
                if (supervisorCheck.Checked)
                    authorizations.Add("sup");
                if (hr1Check.Checked)
                    authorizations.Add("hr1");
                if (hr2Check.Checked)
                    authorizations.Add("hr2");
                if (hr3Check.Checked)
                    authorizations.Add("hr3");

                if (hr2Check.Checked || hr3Check.Checked)
                {
                    if (contractCheck.Checked)
                        authorizations.Add("hr_contract");
                    if (publicServiceCheck.Checked)
                        authorizations.Add("hr_public_officer");
                }


                // insert all data 
                try
                {
                    string sql = $@"
                        INSERT INTO [dbo].[employee]
                           ([employee_id]
                              ,[ihris_id]
                              ,[username]
                              ,[first_name]
                              ,[last_name]
                              ,[email]
                              ,[vacation]
                              ,[personal]
                              ,[casual]
                              ,[sick]
                              ,[bereavement]
                              ,[maternity]
                              ,[pre_retirement])
                        VALUES
                           ( @EmployeeId
                            ,@IhrisId
                            ,@Username
                            ,@FirstName
                            ,@LastName
                            ,@Email
                            ,@Vacation
                            ,@Personal
                            ,@Casual
                            ,@Sick
                            ,@Bereavement
                            ,@Maternity
                            ,@PreRetirement
                            );
                    ";

                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@EmployeeId", emp_id);
                            command.Parameters.AddWithValue("@IhrisId", ihris_id);
                            command.Parameters.AddWithValue("@Username", username);
                            command.Parameters.AddWithValue("@FirstName", firstname);
                            command.Parameters.AddWithValue("@LastName", lastname);
                            command.Parameters.AddWithValue("@Email", email);
                            command.Parameters.AddWithValue("@Vacation", vacation_leave);
                            command.Parameters.AddWithValue("@Personal", personal_leave);
                            command.Parameters.AddWithValue("@Casual", casual_leave);
                            command.Parameters.AddWithValue("@Sick", sick_leave);
                            command.Parameters.AddWithValue("@Bereavement", bereavement_leave);
                            command.Parameters.AddWithValue("@Maternity", maternity_leave);
                            command.Parameters.AddWithValue("@PreRetirement", pre_retirement_leave);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                isInsertSuccessful = true;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    // exception logic
                    isInsertSuccessful = false;
                }

                if (isInsertSuccessful)
                {
                    isInsertSuccessful = false;
                    // add roles to employee
                    foreach (string role in authorizations)
                    {
                        try
                        {
                            string sql = $@"
                            INSERT INTO [dbo].[employeerole]
                               ([employee_id]
                                  ,[role_id])
                            VALUES
                               ( @EmployeeId
                                ,@RoleId
                                );
                        ";

                            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                            {
                                connection.Open();
                                using (SqlCommand command = new SqlCommand(sql, connection))
                                {
                                    command.Parameters.AddWithValue("@EmployeeId", emp_id);
                                    command.Parameters.AddWithValue("@RoleId", role);

                                    int rowsAffected = command.ExecuteNonQuery();
                                    if (rowsAffected > 0)
                                    {
                                        isInsertSuccessful = true;
                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // exception logic
                            isInsertSuccessful = false;
                            break;
                        }
                    }
                    if (isInsertSuccessful)
                    {
                        isInsertSuccessful = false;
                        //get data from gridview
                        /**
                            * gets data from datatable which is formatted in the folowing manner:
                            * 
                            * Columns : employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date
                            * */
                        foreach (DataRow dr in dt.Rows)
                        {
                            try
                            {
                                string sql = $@"
                                    INSERT INTO [dbo].[employeeposition]
                                        ([employee_id]
                                            ,[position_id]
                                            ,[start_date]
                                            ,[expected_end_date]
                                            ,[employment_type]
                                            ,[dept_id])
                                    VALUES
                                        ( @EmployeeId
                                        ,@PositionId
                                        ,@StartDate
                                        ,@ExpectedEndDate
                                        ,@EmploymentType
                                        ,@DeptId
                                        );
                                ";

                                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                                {
                                    connection.Open();
                                    using (SqlCommand command = new SqlCommand(sql, connection))
                                    {
                                        command.Parameters.AddWithValue("@EmployeeId", emp_id);
                                        command.Parameters.AddWithValue("@PositionId", dr.ItemArray[3]);
                                        command.Parameters.AddWithValue("@StartDate", dr.ItemArray[5]);
                                        command.Parameters.AddWithValue("@ExpectedEndDate", dr.ItemArray[6]);
                                        command.Parameters.AddWithValue("@EmploymentType", dr.ItemArray[0]);
                                        command.Parameters.AddWithValue("@DeptId", dr.ItemArray[1]);

                                        int rowsAffected = command.ExecuteNonQuery();
                                        if (rowsAffected > 0)
                                        {
                                            isInsertSuccessful = true;
                                            fullFormSubmitSuccessPanel.Style.Add("display", "inline-block");
                                        }

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // exception logic
                                isInsertSuccessful = false;
                            }
                        }
                    }
                }

                if (isInsertSuccessful)
                {
                    // add audit log
                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                        {
                            connection.Open();
                            string sql = $@"
                                    INSERT INTO [dbo].[auditlog]  ([employee_id],[action],[created_at])
                                    VALUES ( @EmployeeId, @Action, @CreatedAt);
                                ";
                            using (SqlCommand command = new SqlCommand(sql, connection))
                            {
                                command.Parameters.AddWithValue("@EmployeeId", Session["emp_id"].ToString());

                                /*
                                 * Add info about new employee created such as:
                                 * Employee ID
                                 * Leave balances: Vacation, Sick, Personal, Casual
                                 * Permissions
                                 * */
                                string action = $"Created new Employee; ID= {emp_id}; Vacation:{vacation_leave}; Sick:{sick_leave}; Personal:{personal_leave}; Casual:{casual_leave}; Permissions: {String.Join(",", authorizations.ToArray())}";
                                command.Parameters.AddWithValue("@Action", action);
                                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("MM-dd-yyyy hh:mm tt"));
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //exception logic
                        Console.WriteLine(ex.Message.ToString());
                    }
                }
                else
                    fullFormErrorPanel.Style.Add("display", "inline-block");

            }
            else
            {
                if (String.IsNullOrEmpty(nameFromAd))
                    emailNotFoundErrorPanel.Style.Add("display", "inline-block");
                if (dt == null)
                    noEmploymentRecordEnteredErrorPanel.Style.Add("display", "inline-block");
            } 

            //scroll to top of page
            Page.MaintainScrollPositionOnPostBack = false;
            this.empDetailsContainer.Focus();
        }

        protected void refreshForm(object sender, EventArgs e)
        {
            Response.Redirect("~/HR/EmployeeDetails?view=create");
        }
    }
}