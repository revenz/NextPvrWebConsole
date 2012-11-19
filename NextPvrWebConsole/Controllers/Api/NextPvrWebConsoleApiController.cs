using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using NextPvrWebConsole.Models;

namespace NextPvrWebConsole.Controllers.Api
{
    public class NextPvrWebConsoleApiController:ApiController
    {
        public Func<User> GetUserFunction { get; set; }

        public NextPvrWebConsoleApiController()
        {
            GetUserFunction = new Func<User>(GetUserStandard);
        }

        private User GetUserStandard() 
        {                            
            var user = NextPvrWebConsole.Models.User.GetByUsername(this.User.Identity.Name);
            if (user == null)
                throw new UnauthorizedAccessException();
            return user;
        }

        public User GetUser()
        {
            return GetUserFunction();
        }
    }
}
