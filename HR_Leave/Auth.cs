using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;

namespace HR_Leave
{
    public class Auth
    {
        protected string getCurrentUserActiveDirectoryEmail()
        {
            return UserPrincipal.Current.EmailAddress;
        }

        protected string getUserEmployeeId()
        {
            string result = "-1";
            string sql = $@"select [employee_id] 
                           from [HRLeaveTestDb].[dbo].employee
                           where [email] = '{this.getCurrentUserActiveDirectoryEmail()}'";

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    //SqlParameter emailParam = new SqlParameter("@EMP_EMAIL", SqlDbType.NVarChar);
                    //command.Parameters.Add(emailParam);
                    //command.Parameters["@EMP_EMAIL"].Value = this.getCurrentUserActiveDirectoryEmail();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result = reader["employee_id"].ToString();
                        }
                    }
                }
            }

            return result;
        }

        public List<string> getUserPermissions()
        {
            string userEmpId = this.getUserEmployeeId();

            List<string> permissions = new List<string>();
            string sql = $@"select [permission_id]
                            from [HRLeaveTestDb].[dbo].[employeerole] er
                            left join [HRLeaveTestDb].[dbo].[rolepermission] rp
                            on er.role_id = rp.role_id
                            where [employee_id]= {userEmpId}";

            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissions.Add(reader["permission_id"].ToString());
                        }
                    }
                }
            }

            return permissions;
        }
    }
}