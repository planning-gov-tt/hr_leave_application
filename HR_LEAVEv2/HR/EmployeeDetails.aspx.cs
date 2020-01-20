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
                if (Request.QueryString.HasKeys())
                {
                    string mode = Request.QueryString["mode"];
                    string empId = Request.QueryString["empId"];

                    if (mode == "edit")
                    {
                        populatePage(empId);
                        this.adjustPageForEditMode();
                    }
                        
                    else
                        this.adjustPageForCreateMode();
                } 
            }
                
        }

        protected void adjustPageForEditMode()
        {
            //title
            editModeTitle.Visible = true;
            createModeTitle.Visible = false;

            //employee name
            empNamePanel.Visible = true;

            //basic info like id, ihris id, email
            empBasicInfoPanel.Visible = false;

            //authorizations
            //if hr 1 then authorizations can be edited
            authorizationLevelPanel.Visible = permissions.Contains("hr1_permissions");

            //add emp record
            addEmpRecordForm.Visible = false;
            showFormBtn.Visible = true;

            //submit
            submitFullFormPanel.Visible = false;
            editFormPanel.Visible = true;
        }

        protected void adjustPageForCreateMode()
        {
            //title
            editModeTitle.Visible = false;
            createModeTitle.Visible = true;

            //employee name
            empNamePanel.Visible = false;

            //basic info like id, ihris id, email
            empBasicInfoPanel.Visible = true;

            //authorization
            authorizationLevelPanel.Visible = true;

            //add emp record
            addEmpRecordForm.Visible = false;
            showFormBtn.Visible = true;

            //submit
            submitFullFormPanel.Visible = true;
            editFormPanel.Visible = false;

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
            addEmpRecordForm.Visible = true;
            showFormBtn.Visible = false;
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
                    dt.Columns.Add("record_id", typeof(string));
                    dt.Columns.Add("employment_type", typeof(string));
                    dt.Columns.Add("dept_id", typeof(string));
                    dt.Columns.Add("dept_name", typeof(string));
                    dt.Columns.Add("pos_id", typeof(string));
                    dt.Columns.Add("pos_name", typeof(string));
                    dt.Columns.Add("start_date", typeof(string));
                    dt.Columns.Add("expected_end_date", typeof(string));
                    dt.Columns.Add("isDeleted", typeof(string));
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
      
                //record_id, employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date, isDeleted
                dt.Rows.Add(-1,emp_type, dept_id, dept_name, position_id, position_name, startDate, expected_end_date.ToString("MM/dd/yyyy"),"0");
    
                ViewState["Gridview1_dataSource"] = dt;

                this.bindGridview();

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
            addEmpRecordForm.Visible = false;
            showFormBtn.Visible = true;
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
                dt.Rows[e.RowIndex].SetField<string>(8, "1");
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

                // get data from leave 
                sick_leave = String.IsNullOrEmpty(sickLeaveInput.Text) ? "0" : sickLeaveInput.Text;
                personal_leave = String.IsNullOrEmpty(personalLeaveInput.Text) ? "0" : personalLeaveInput.Text;
                casual_leave = String.IsNullOrEmpty(casualLeaveInput.Text) ? "0" : casualLeaveInput.Text;
                vacation_leave = String.IsNullOrEmpty(vacationLeaveInput.Text) ? "0" : vacationLeaveInput.Text;
                bereavement_leave = String.IsNullOrEmpty(bereavementLeaveInput.Text) ? "0" : bereavementLeaveInput.Text;
                maternity_leave = String.IsNullOrEmpty(maternityLeaveInput.Text) ? "0" : maternityLeaveInput.Text;
                pre_retirement_leave = String.IsNullOrEmpty(preRetirementLeaveInput.Text) ? "0" : preRetirementLeaveInput.Text;

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
                            * Columns : record_id, employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date, isDeleted
                            * */
                        foreach (DataRow dr in dt.Rows)
                        {
                            // if row is not deleted
                            if(dr.ItemArray[8].ToString() == "0")
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
                                            command.Parameters.AddWithValue("@PositionId", dr.ItemArray[4]);
                                            command.Parameters.AddWithValue("@StartDate", dr.ItemArray[6]);
                                            command.Parameters.AddWithValue("@ExpectedEndDate", dr.ItemArray[7]);
                                            command.Parameters.AddWithValue("@EmploymentType", dr.ItemArray[1]);
                                            command.Parameters.AddWithValue("@DeptId", dr.ItemArray[2]);

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
                }
            }
            else
            {
                if (String.IsNullOrEmpty(nameFromAd))
                    emailNotFoundErrorPanel.Style.Add("display", "inline-block");
                if (dt == null)
                    noEmploymentRecordEnteredErrorPanel.Style.Add("display", "inline-block");
                isInsertSuccessful = false;
            }

            if (!isInsertSuccessful)
                fullFormErrorPanel.Style.Add("display", "inline-block");

            //scroll to top of page
            Page.MaintainScrollPositionOnPostBack = false;
            this.empDetailsContainer.Focus();
        }

        protected void refreshForm(object sender, EventArgs e)
        {
            string pathname = string.Empty;
            if (String.IsNullOrEmpty(Request.QueryString["empId"]))
                pathname = $"~/HR/EmployeeDetails?mode={ Request.QueryString["mode"]}";
            else
                pathname = $"~/HR/EmployeeDetails?mode={Request.QueryString["mode"]}&empId={Request.QueryString["empId"]}";
            Response.Redirect(pathname);
        }

        protected void populatePage(string empId)
        {

            // get roles and populate checkboxes
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    string sql = $@"
                        SELECT 
                            er.role_id 
                           
                        FROM [dbo].[employeerole] er
                        WHERE er.employee_id = {empId};
                    ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                supervisorCheck.Checked = reader["role_id"].ToString() == "sup";
                                hr1Check.Checked = reader["role_id"].ToString() == "hr1";
                                hr2Check.Checked = reader["role_id"].ToString() == "hr2";
                                hr3Check.Checked = reader["role_id"].ToString() == "hr3";

                                contractCheck.Checked = reader["role_id"].ToString() == "hr_contract";
                                publicServiceCheck.Checked = reader["role_id"].ToString() == "hr_contract";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //exception logic
                Console.WriteLine(ex.Message.ToString());
            }

            // get leave balances and populate textboxes
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    string sql = $@"
                        SELECT [vacation]
                                ,[personal]
                                ,[casual]
                                ,[sick]
                                ,[bereavement]
                                ,[maternity]
                                ,[pre_retirement],
                                [first_name] + ' ' + [last_name] as 'employee_name'
                            FROM [dbo].[employee]
                            WHERE employee_id = {empId};
                    ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                vacationLeaveInput.Text = reader["vacation"].ToString();
                                personalLeaveInput.Text = reader["personal"].ToString();
                                casualLeaveInput.Text = reader["casual"].ToString();
                                sickLeaveInput.Text = reader["sick"].ToString();
                                bereavementLeaveInput.Text = reader["bereavement"].ToString();
                                maternityLeaveInput.Text = reader["maternity"].ToString();
                                preRetirementLeaveInput.Text = reader["pre_retirement"].ToString();

                                empNameHeader.InnerText = reader["employee_name"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //exception logic
                Console.WriteLine(ex.Message.ToString());
            }

            // get employment record and populate gridview
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    string sql = $@"
                            SELECT 
                                ep.id record_id,
                                ep.employment_type,
                                ep.dept_id,
                                d.dept_name,
                                ep.position_id pos_id,
                                p.pos_name,
                                FORMAT(ep.start_date, 'MM/dd/yyyy') start_date, 
                                FORMAT(ep.expected_end_date, 'MM/dd/yyyy') expected_end_date,
                                '0' as isDeleted
                                
                            FROM [dbo].[employeeposition] ep

                            LEFT JOIN [dbo].[department] d
                            ON d.dept_id = ep.dept_id
    
                            LEFT JOIN [dbo].[position] p
                            ON p.pos_id = ep.position_id

                            WHERE employee_id = {empId};
                    ";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable(); 
                        sqlDataAdapter.Fill(dataTable);

                        ViewState["Gridview1_dataSource"] = dataTable;
                        this.bindGridview();
                    }
                }
            }
            catch (Exception ex)
            {
                //exception logic
                Console.WriteLine(ex.Message.ToString());
            }

        }

        protected void editBtn_Click(object sender, EventArgs e)
        {
            // get data from every field and submit
            string empId = Request.QueryString["empId"];

            string sick_leave, personal_leave, casual_leave, vacation_leave, bereavement_leave, maternity_leave, pre_retirement_leave;
            List<string> authorizations = new List<string>();
            DataTable dt = ViewState["Gridview1_dataSource"] as DataTable;

            Boolean isRolesEditSuccessful, isLeaveEditSuccessful, isEmpRecordEditSuccessful;
            isRolesEditSuccessful = isLeaveEditSuccessful = isEmpRecordEditSuccessful = true;

            // edit roles
            if (authorizationLevelPanel.Visible)
            {
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

                //delete all roles attached to employee except 'emp'
                try
                {
                    string sql = $@"
                    DELETE FROM [dbo].[employeerole]
                    WHERE employee_id = {empId} AND role_id <> 'emp';
                ";

                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            int rowsAffected = command.ExecuteNonQuery();
                            isRolesEditSuccessful = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // exception logic
                    isRolesEditSuccessful = false;
                }

                // add roles back to employee 
                if (authorizations.Count > 0)
                {
                    foreach (string role in authorizations)
                    {
                        try
                        {
                            string sql = $@"
                            INSERT INTO [dbo].[employeerole]
                                ([employee_id], [role_id])
                            VALUES
                                (@EmployeeId, @RoleId);
                        ";

                            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                            {
                                connection.Open();
                                using (SqlCommand command = new SqlCommand(sql, connection))
                                {
                                    command.Parameters.AddWithValue("@EmployeeId", empId);
                                    command.Parameters.AddWithValue("@RoleId", role);

                                    int rowsAffected = command.ExecuteNonQuery();
                                    if (rowsAffected > 0)
                                    {
                                        isRolesEditSuccessful = true;
                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // exception logic
                            isRolesEditSuccessful = false;
                            break;
                        }
                    }
                }
                else
                    isRolesEditSuccessful = true;
                   
            }

            // edit Leave balances

            if (isRolesEditSuccessful == true || !authorizationLevelPanel.Visible)
            {
                // get data from leave 
                sick_leave = String.IsNullOrEmpty(sickLeaveInput.Text) ? "0" : sickLeaveInput.Text;
                personal_leave = String.IsNullOrEmpty(personalLeaveInput.Text) ? "0" : personalLeaveInput.Text;
                casual_leave = String.IsNullOrEmpty(casualLeaveInput.Text) ? "0" : casualLeaveInput.Text;
                vacation_leave = String.IsNullOrEmpty(vacationLeaveInput.Text) ? "0" : vacationLeaveInput.Text;
                bereavement_leave = String.IsNullOrEmpty(bereavementLeaveInput.Text) ? "0" : bereavementLeaveInput.Text;
                maternity_leave = String.IsNullOrEmpty(maternityLeaveInput.Text) ? "0" : maternityLeaveInput.Text;
                pre_retirement_leave = String.IsNullOrEmpty(preRetirementLeaveInput.Text) ? "0" : preRetirementLeaveInput.Text;

                // insert all data 
                try
                {
                    string sql = $@"
                    UPDATE [dbo].[employee]
                    SET 
                            vacation = @Vacation
                        ,personal = @Personal
                        ,casual = @Casual
                        ,sick = @Sick
                        ,bereavement = @Bereavement
                        ,maternity = @Maternity
                        ,pre_retirement = @PreRetirement
                    WHERE employee_id = {empId};
                ";

                    using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@Vacation", vacation_leave);
                            command.Parameters.AddWithValue("@Personal", personal_leave);
                            command.Parameters.AddWithValue("@Casual", casual_leave);
                            command.Parameters.AddWithValue("@Sick", sick_leave);
                            command.Parameters.AddWithValue("@Bereavement", bereavement_leave);
                            command.Parameters.AddWithValue("@Maternity", maternity_leave);
                            command.Parameters.AddWithValue("@PreRetirement", pre_retirement_leave);

                            int rowsAffected = command.ExecuteNonQuery();
                            isLeaveEditSuccessful = true;

                        }
                    }
                }
                catch (Exception ex)
                {
                    // exception logic
                    isLeaveEditSuccessful = false;
                }


                // edit Emp Records
                if (isLeaveEditSuccessful)
                {

                    //delete deleted records 


                    //add new records data from gridview
                    /**
                        * adds data from datatable which is formatted in the folowing manner:
                        * 
                        * Columns : record_id, employment_type, dept_id, dept_name, pos_id, pos_name, start_date, expected_end_date
                        * */

                    foreach (DataRow dr in dt.Rows)
                    {
                        if(dr.ItemArray[0].ToString() == "-1")
                        {
                            // if the row represents a new record
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
                                        command.Parameters.AddWithValue("@EmployeeId", empId);
                                        command.Parameters.AddWithValue("@PositionId", dr.ItemArray[4]);
                                        command.Parameters.AddWithValue("@StartDate", dr.ItemArray[6]);
                                        command.Parameters.AddWithValue("@ExpectedEndDate", dr.ItemArray[7]);
                                        command.Parameters.AddWithValue("@EmploymentType", dr.ItemArray[1]);
                                        command.Parameters.AddWithValue("@DeptId", dr.ItemArray[2]);

                                        int rowsAffected = command.ExecuteNonQuery();
                                        if (rowsAffected > 0)
                                        {
                                            isEmpRecordEditSuccessful = true;
                                        }

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                // exception logic
                                isEmpRecordEditSuccessful = false;
                            }
                        }
                             
                    }
                }
            }


            // show success/error messages
            if(isRolesEditSuccessful && isLeaveEditSuccessful && isEmpRecordEditSuccessful)
                editFullSuccessPanel.Style.Add("display", "inline-block");
            else
            {
                if (isRolesEditSuccessful)
                    editRolesSuccessPanel.Style.Add("display", "inline-block");
                if (isLeaveEditSuccessful)
                    editLeaveSuccessPanel.Style.Add("display", "inline-block");
                if (isEmpRecordEditSuccessful)
                    editEmpRecordSuccessPanel.Style.Add("display", "inline-block");

                if(!isRolesEditSuccessful && !isLeaveEditSuccessful && !isEmpRecordEditSuccessful)
                    editEmpErrorPanel.Style.Add("display", "inline-block");
            }

            //scroll to top of page
            Page.MaintainScrollPositionOnPostBack = false;
            this.empDetailsContainer.Focus();
        }

        protected void returnToPreviousBtn_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/HR/AllEmployees.aspx");
        }

        int GetColumnIndexByName(GridViewRow row, string columnName)
        {
            int columnIndex = 0;
            foreach (DataControlFieldCell cell in row.Cells)
            {
                if (cell.ContainingField is BoundField)
                    if (((BoundField)cell.ContainingField).DataField.Equals(columnName))
                        break;
                columnIndex++; // keep adding 1 while we don't have the correct name
            }
            return columnIndex;
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            Boolean isDeleted;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //TODO:find way to hide row when isDeleted is 1

                //int cellIndex = GetColumnIndexByName(e.Row, "isDeleted");
            //    string rowtext = ((TextBox)(GridView1.Rows[e.Row.RowIndex].Cells[8].Controls[0].Controls[0])).Text;
            //    //string rowText = e.Row.Cells[9].Text.ToString();
            //    isDeleted = rowtext == "1";
            //    if (isDeleted)
            //        e.Row.Visible = false;
            //}
        }
    }
}