﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using NextPvrWebConsole.Filters;
using NextPvrWebConsole.Models;

namespace NextPvrWebConsole.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && Membership.ValidateUser(model.UserName, model.Password))
            {
                Models.User.LoggedIn(model.UserName);
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);

                string[] roles = Roles.GetRolesForUser(model.UserName);

                if (String.IsNullOrEmpty(returnUrl) || !roles.Select(x => x.ToLower()).Contains(returnUrl.Substring(1).ToLower()))
                    returnUrl = "/" + roles[0];
                if (returnUrl.ToLower() == "/dashboard")
                    returnUrl = "/";

                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgotPassword(string Username)
        {
            try
            {
                ViewBag.PasswordReset = true;
                Models.User.SendPasswordResetRequest(Username);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public ActionResult ChangePassword(LocalPasswordModel Model)
        {
            if (!ModelState.IsValid)
                throw new ArgumentException();

            bool success = this.GetUser().ChangePassword(Model.OldPassword, Model.NewPassword);
            return Json(new { success = success });
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string Code)
        {
            try
            {
                Models.User user = Models.User.ValidateResetCode(Code);
                ViewBag.PasswordReset = true;
            }
            catch (Exception ex)
            {
                ViewBag.PasswordReset = false;
                ViewBag.Error = ex.Message;
            }
            return View();
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
        
        #endregion
    }
}
