using System;
using System.Collections.Generic;
using System.Web;

namespace HR_LEAVEv2.Classes
{
    public class User 
    {
        public string currUserEmail
        {
            get { return HttpContext.Current.Session["emp_email"] != null ? HttpContext.Current.Session["emp_email"].ToString() : null; }
            set { HttpContext.Current.Session["emp_email"] = value; }
        }

        public string currUserName
        {
            get { return HttpContext.Current.Session["emp_username"] != null ? HttpContext.Current.Session["emp_username"].ToString() : null; }
            set { HttpContext.Current.Session["emp_username"] = value; }
        }

        public string currUserId
        {
            get { return HttpContext.Current.Session["emp_id"] != null ? HttpContext.Current.Session["emp_id"].ToString() : null; }
            set { HttpContext.Current.Session["emp_id"] = value; }
        }

        public int currUserNumYearsWorked
        {
            get { return HttpContext.Current.Session["currNumYearsWorked"] != null ? Convert.ToInt32(HttpContext.Current.Session["currNumYearsWorked"]) : -1; }
            set { HttpContext.Current.Session["currNumYearsWorked"] = value; }
        }

        public List<string> permissions
        {
            get { return HttpContext.Current.Session["permissions"] != null ? (List<string>)HttpContext.Current.Session["permissions"] : new List<string>(); }
            set { HttpContext.Current.Session["permissions"] = value; }
        }

        public Boolean hasNoPermissions()
        {
            return permissions == null || (permissions != null && permissions.Count == 0);
        }

        public Boolean isAdmin()
        {
            return permissions.Contains("admin_permissions");
        }

        public Boolean isUserAllowedToViewOrEditLeaveApplication(string employeeId, string employeeType ,string supervisorId, string typeOfLeave) 
        {
            Util util = new Util();

            // if user trying to view/ edit is HR 1
            if (permissions.Contains("hr1_permissions"))
                return true;

            // if user trying to view/ edit is not in system
            if (currUserId == null)
                return false;

            // if user trying to view/ edit is the employee who submitted the application
            if (currUserId == employeeId)
                return true;

            // All combinations of HR 2, HR 3, Supervisor and Employee that did not submit the application. This means that only people with HR 2 privileges or 
            // the supervisor who the LA was submitted to can see it

            // HR 3
            // HR 3 should not have access to leave application data
            if (permissions.Contains("hr3_permissions"))
                return false;

            // HR 2
            if (permissions.Contains("hr2_permissions"))
            {
                if (!util.isNullOrEmpty(employeeType))
                {
                    // the HR 2 must have permissions to view data for the same employment type as for the employee who submitted the application
                    if (
                        (
                            //check if hr can view applications from the relevant employment type 
                            (employeeType == "Contract" && !permissions.Contains("contract_permissions"))
                            ||
                            (employeeType == "Public Service" && !permissions.Contains("public_officer_permissions"))
                        )

                        ||

                        (
                            //check if hr can view applications of the relevant leave type
                            (typeOfLeave == "Sick" && !permissions.Contains("approve_sick"))
                            ||
                            (typeOfLeave == "Vacation" && !permissions.Contains("approve_vacation"))
                            ||
                            (typeOfLeave == "Casual" && !permissions.Contains("approve_casual"))
                        )
                    )
                        return false;
                }
                else
                    return false;
            }
            else
            {
                // if emp trying to view LA is supervisor 
                if (permissions.Contains("sup_permissions"))
                {
                    //LA was not submitted to them
                    if (currUserId != supervisorId)
                        return false;
                }
                else
                {
                    // if another emp trying to view LA and they did not submit the LA
                    if (currUserId != employeeId)
                        return false;
                }

            }
            return false;
        }
    }
}