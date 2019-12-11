using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using System.Web;

namespace HR_LEAVEv2
{
    public class Auth
    {

        public string getUserInfoFromActiveDirectory(string email)
        {

            string filter = string.Format("(proxyaddresses=SMTP:{0})", email);

            using (DirectoryEntry gc = new DirectoryEntry("LDAP:"))
            {
                foreach (DirectoryEntry z in gc.Children)
                {
                    using (DirectoryEntry root = z)
                    {
                        using (DirectorySearcher searcher = new DirectorySearcher(root, filter, new string[] { "Name" }))
                        {
                            searcher.ReferralChasing = ReferralChasingOption.All;
                            SearchResult result = searcher.FindOne();
                            if(result != null)
                            {
                                IEnumerator en = result.Properties["name"].GetEnumerator();
                                en.MoveNext();
                                return en.Current != null ? en.Current.ToString() : null;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public string getEmailOfSignedInUserFromActiveDirectory()
        {
            // create domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);

            // find the user
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, WindowsIdentity.GetCurrent().Name);

            if (user != null)
            {
                return user.EmailAddress;
            }
            return null;
        }

        public string getUserEmployeeId(string email)
        {
            if (email.Length <= 0)
                return "-1";
            string result = "-1";
            string sql = $@"select [employee_id] 
                           from [dbo].employee
                           where [email] = '{email}'";
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["dbConnectionString"].ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result = reader["employee_id"].ToString();
                            }
                        }
                    }
                }
            } catch(Exception ex)
            {
                //exception logic
            }
   

            return result;
        }

        public List<string> getUserPermissions(string id)
        {
            string userEmpId = id;

            List<string> permissions = new List<string>();
            string sql = $@"select distinct [permission_id]
                            from [dbo].[employeerole] er
                            left join [dbo].[rolepermission] rp
                            on er.role_id = rp.role_id
                            where [employee_id]= {userEmpId}";

            try
            {
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
            } catch (Exception ex)
            {
                //exception logic
            }
            

            return permissions.Count > 0 ? permissions : null;
        }
    }
}