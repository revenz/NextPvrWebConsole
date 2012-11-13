using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class UsersController : ApiController
    {
        // GET api/tuners
        public IEnumerable<Models.User> Get()
        {
            var users = Models.User.LoadAll();
            foreach (var u in users)
                u.PasswordHash = String.Empty;
            return users;
        }

        public Models.User Post(Models.UserModel value)
        {
            bool isvalid = ModelState.IsValid;
            if (!isvalid && value.Oid > 0)
            {
                // if the errors are just passwords ignore, we dont update the password when updating a user any how
                isvalid = ModelState.Keys.Where(x => !x.ToLower().Contains("password")).Count() == 0;
            }
            if (!isvalid)
                throw new ArgumentException();
            
            var user = value.Save();
            if (user != null)
                user.PasswordHash = String.Empty;
            return user;
        }

        // DELETE api/delete/5
        public void Delete(int Oid)
        {
            var user = this.GetUser();
            if (user == null)
                throw new UnauthorizedAccessException();
            if (user.Oid == Oid)
                throw new Exception("You cannot delete yourself.");

            Models.User.Delete(Oid);
        }
    }
}
