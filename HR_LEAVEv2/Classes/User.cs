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
            get { return HttpContext.Current.Session["permissions"] != null ? (List<string>)HttpContext.Current.Session["permissions"] : null; }
            set { HttpContext.Current.Session["permissions"] = value; }
        }
    }
}