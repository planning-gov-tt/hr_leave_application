using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Web;

namespace HR_LEAVEv2
{
    public class Auth
    {

        public string getUserInfoFromActiveDirectory(string email)
        {
            /* What this function does:
             *      This function gets the username of the user associated with the email passed. It checks AD for this username which comprises of 
             *      a first name and last name in the following format: Firstname Lastname (eg. Tristan Sankar)
             *      
             *  Where is it used:
             *      1. In EmployeeDetails when adding a new employee to the system. Their username is obtained through their AD email which is captured in the 
             *         add new employee form. 
             *      2. In Master.cs, this function is used to populate the Session variable with the current employee's username (Session["emp_username"]). This is displayed to the user 
             *         as feedback so they always know who is logged in
             * */
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
            /* What this function does:
             *     This function gets the AD email of the current signed in user
             * 
             * Where it is used:
             *      This is used in Master.cs to set the email of the current user in the Session variable (Session["emp_email"]).
             * 
             * */

            // create domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);

            // find the user
            UserPrincipal user = UserPrincipal.FindByIdentity(ctx, HttpContext.Current.Request.LogonUserIdentity.Name.ToString());
            if (user != null)
            {
                return user.EmailAddress;
            }
            return null;

            //return "Tristan.Sankar@planning.gov.tt";
        }

        public string getUserEmployeeId(string email)
        {
            /* What this function does:
             *     This function gets the user's employee ID from the database based on the email passed
             * 
             * Where it is used:
             *      It is used in Master.cs to set the ID in the Session variable (Session["emp_id"]). This is used in many different pages in order to identify the user 
             *      currently navigating the system when completing queries to the DB
             * 
             * */

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
                return ex.Message.ToString();
            }
   

            return result;
        }

        public List<string> getUserPermissions(string id)
        {
            /* What this function does:
             *     This function gets all the user's permissions as defined in the DB and returns a List object containing these permissions
             * 
             * Where it is used:
             *     This is used in Master.cs to set the permissions in the Session variable (Session["permissions"]). This is also widely used in the system for authorization
             *     purposes.
             * 
             * */
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
                throw ex;
            }
            

            return permissions.Count > 0 ? permissions : null;
        }
    }
}