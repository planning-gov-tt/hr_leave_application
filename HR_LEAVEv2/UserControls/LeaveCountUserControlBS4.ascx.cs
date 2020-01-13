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

        private class EmployeeDetails
        {
            public string sick { get; set; }
            public string vacation { get; set; }
            public string personal { get; set; }
            public string casual { get; set; }
            public string employmentType { get; set; }
            public string start_date { get; set; }
        };
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                // get employee employment type
                //if contract
                //  display sick leave balance
                //  display personal leave balance if in first 11 months of contract
                //  display vacation leave balance if in 12th month+ of contract

                //if public service
                //  display sick leave balance
                //  display casual leave balance


                vacationPanel.Visible = false;
                personalPanel.Visible = false;
                casualPanel.Visible = false;
                sickPanel.Visible = true;

                EmployeeDetails empDetails = null;
                string CS = ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(CS))
                {
                    string getEmpDetailsSql = $@"
                            SELECT e.[sick], e.[vacation], e.[personal], e.[casual], ep.employment_type, FORMAT(ep.start_date, 'MM/dd/yy') as start_date
                            FROM [dbo].[employee] e
                            LEFT JOIN [dbo].employeeposition ep
                            ON e.employee_id = ep.employee_id AND GETDATE()>=ep.start_date AND GETDATE()<=ep.expected_end_date
                            WHERE e.[employee_id] = '{Session["emp_id"]}'
                            ;
                        ";
                    using (SqlCommand cmd = new SqlCommand(getEmpDetailsSql, con))
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {

                                empDetails = new EmployeeDetails
                                {
                                    sick = reader["sick"].ToString(),
                                    vacation = reader["vacation"].ToString(),
                                    personal = reader["personal"].ToString(),
                                    casual = reader["casual"].ToString(),
                                    employmentType = reader["employment_type"].ToString(),
                                    start_date = reader["start_date"].ToString(),
                                };
                            }
                        }     
                    }
                        

                    ViewState["sick"] = empDetails.sick;
                    ViewState["vacation"] = empDetails.vacation;
                    ViewState["personal"] = empDetails.personal;
                    ViewState["casual"] = empDetails.casual;

                    

                    if(empDetails.employmentType == "Contract")
                    {
                        /* contract employees
                         * for a new employee: 
                         *  vacation = 20 
                         *  personal = 5
                         * 
                         * During the first 11 months of contract, employee only has access to the 5 personal days. 
                         * After the 11 months, they have access to their:
                         *          vacation days - days used from personal
                        */

                        // is employee in first 11 months of contract?
                        DateTime startDate = Convert.ToDateTime(empDetails.start_date);
                        DateTime elevenMonthsFromStartDate = startDate.AddMonths(11);

                        if (DateTime.Compare(DateTime.Today, elevenMonthsFromStartDate) < 0)
                            // display personal 
                            personalPanel.Visible = true;
                        else
                        {
                            int personal = Convert.ToInt16(empDetails.personal);

                            if(personal > 0)
                            {
                                // subtract personal days used from vacation
                                int updatedVacation = Convert.ToInt16(empDetails.vacation);
                                updatedVacation = updatedVacation - (5 - personal);

                                if(updatedVacation != Convert.ToInt16(empDetails.vacation))
                                {
                                    // update vacation and personal in db
                                    try
                                    {
                                       string updateVacationSql = $@"
                                            BEGIN TRANSACTION [updateVacation]
                                            

                                            BEGIN TRY

                                                UPDATE [dbo].employee 
                                                SET vacation = '{updatedVacation}'
                                                WHERE employee_id = '{Session["emp_id"]}';

                                                UPDATE [dbo].employee 
                                                SET personal = '0'
                                                WHERE employee_id = '{Session["emp_id"]}';

                                                COMMIT TRANSACTION [updateVacation]

                                              END TRY

                                              BEGIN CATCH

                                                  ROLLBACK TRANSACTION [updateVacation]

                                              END CATCH  
                                        ";
                                        using (SqlCommand cmd = new SqlCommand(updateVacationSql, con))
                                        {
                                            int rowsAffected = cmd.ExecuteNonQuery();
                                            if(rowsAffected > 0) { 
                                                ViewState["vacation"] = updatedVacation.ToString();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        //exception logic
                                        Console.WriteLine(ex.Message.ToString());
                                    }
                                }
                            }

                            // display vacation 
                            vacationPanel.Visible = true;
                        }
                            
                    } else
                    {
                        // public service employees
                        casualPanel.Visible = true;
                        vacationPanel.Visible = true;
                    }

                }
            }
        }

    }
}