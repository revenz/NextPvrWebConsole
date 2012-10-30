using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole
{
    public class ApiAuthorizeAttribute : System.Web.Http.AuthorizeAttribute
    {
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            bool authorized = false;
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                if (HttpContext.Current.Session["authorized"] as string == "true")
                    authorized = true;
            }
            // fall back to basic auth
            if (!authorized)
            {
            }


            if (authorized)
                base.OnAuthorization(actionContext);
            else
                base.HandleUnauthorizedRequest(actionContext);
        }

    }
}