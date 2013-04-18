﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize(Roles="System")]
    public class UsersController : NextPvrWebConsoleApiController
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
            if (value.Oid > 0) // edit
            {
                if (!Validators.Validator.IsValid(value, "Password", "ConfirmPassword"))
                    throw new ArgumentException();

                var user = value.Save();
                if (user != null)
                    user.PasswordHash = String.Empty;
                return user;
            }
            else
            {
                if (!Validators.Validator.IsValid(value)) // need to pass in properties to ignore...
                    throw new ArgumentException();

                return Models.User.CreateUser(value.Username, value.EmailAddress, value.Password, value.UserRole, value.Administrator);
            }
        }

        // DELETE api/delete/5
        public bool Delete(int Oid)
        {
            var user = this.GetUser();
            if (user == null)
                throw new UnauthorizedAccessException();
            if (user.Oid == Oid)
                throw new ArgumentException("You cannot delete yourself.");

            Models.User.Delete(Oid);
            return true;
        }
    }
}
