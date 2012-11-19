using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
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
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
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
            }
            catch (Exception ex)
            {
                ViewBag.PasswordReset = false;
                ViewBag.Error = ex.Message;
            }
            return View("Login");
        }
        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

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

        public ActionResult ResetPassword(string Username, string Code)
        {
            throw new NotImplementedException();
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
