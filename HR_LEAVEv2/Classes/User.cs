using System;
using System.Collections.Generic;
using System.Web;

namespace HR_LEAVEv2.Classes
{
    public class User 
    {
        private Util util = new Util();

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
            return permissions.Count == 0;
        }

        public Boolean isAdmin()
        {
            return permissions.Contains("admin_permissions");
        }

        public Boolean isUserAllowedToViewOrEditLeaveApplication(string employeeId, string employeeType ,string supervisorId, string typeOfLeave, string mode) 
        {
            // if no employement type is specified
            if (util.isNullOrEmpty(employeeType))
                return false;

            // if user trying to view/ edit is HR 1
            if (permissions.Contains("hr1_permissions"))
                return true;

            // if user trying to view/ edit is not in system
            if (currUserId == null)
                return false;

            // if user trying to view is the employee who submitted the application
            if (currUserId == employeeId && mode == "view")
                return true;

            // if user who submitted the application is trying to edit the application
            if (currUserId == employeeId && mode == "edit")
                return false;

            // All combinations of HR 2, HR 3, Supervisor and Employee that did not submit the application. This means that only people with HR 2 privileges or 
            // the supervisor who the LA was submitted to can see it

            // HR 3
            // HR 3 should not have access to leave application data
            if (permissions.Contains("hr3_permissions"))
                return false;

            // HR 2
            if (permissions.Contains("hr2_permissions"))
            {
                List<string> allowedEmpTypes = getSubsetsOfEmployeesUserIsAllowedToView()["employment_types"],
                             allowedLeaveTypes = getTypesOfLeaveUserCanApprove();
                

                if (!allowedEmpTypes.Contains($"'{employeeType}'") || !allowedLeaveTypes.Contains($"'{typeOfLeave}'"))
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

            return true;
        }

        public Boolean canViewAllEmployees()
        {
            return permissions.Contains("hr1_permissions");
        }

        public Dictionary<string, List<string>> getSubsetsOfEmployeesUserIsAllowedToView()
        {
            Dictionary<string, List<string>> subsets = new Dictionary<string, List<string>>();
            subsets.Add("employment_types", new List<string>());
            subsets.Add("depts", new List<string>());

            if (permissions.Contains("contract_permissions"))
                subsets["employment_types"].Add("'Contract'");
            if (permissions.Contains("public_officer_permissions"))
                subsets["employment_types"].Add("'Public Service'");

            // add dept code here

            return subsets;
        }

        public List<string> getTypesOfLeaveUserCanApprove()
        {
            List<string> leaveTypes = util.getListOfAllLeaveTypes();
            // add single quotes around leave types
            for (int i = 0; i < leaveTypes.Count; i++)
                leaveTypes[i] = $"'{leaveTypes[i]}'";

            if (permissions.Contains("hr1_permissions"))
                return leaveTypes;

            if (!permissions.Contains("approve_sick"))
                leaveTypes.Remove("'Sick'");
            if (!permissions.Contains("approve_casual"))
                leaveTypes.Remove("'Casual'");
            if (!permissions.Contains("approve_vacation"))
                leaveTypes.Remove("'Vacation'");

            return leaveTypes;
        }

        public Boolean isUserAllowedToViewOrEditEmployeeDetails(string employeeType)
        {
            if (util.isNullOrEmpty(employeeType))
                return false;

            if (permissions.Contains("hr1_permissions"))
                return true;

            // the HR 2, HR 3 must have permissions to view data for the same employment type as for the employee who submitted the application
            Dictionary<string, List<string>> subsets = getSubsetsOfEmployeesUserIsAllowedToView();
            List<string> allowedEmpTypes = subsets["employment_types"];

            if (!allowedEmpTypes.Contains($"'{employeeType}'"))
                return false;

            // code to check by dept here
            return true;
        }
    }
}