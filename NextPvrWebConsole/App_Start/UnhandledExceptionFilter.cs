﻿using System;
using System.Web.Http.Filters;
//using AspNetWebApi.Controllers;
using System.Net;
using System.Linq;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Collections.Generic;
using System.Web.Http.ModelBinding;

namespace NextPvrWebConsole
{
    public class UnhandledExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            HttpStatusCode status = HttpStatusCode.InternalServerError;

            var exType = context.Exception.GetType();

            if (exType == typeof(UnauthorizedAccessException))
                status = HttpStatusCode.Unauthorized;
            else if (exType == typeof(ArgumentException))
                status = HttpStatusCode.NotFound;

            var apiError = new ApiMessageError() { message = context.Exception.Message };

            // create a new response and attach our ApiError object
            // which now gets returned on ANY exception result
            var errorResponse = context.Request.CreateResponse<ApiMessageError>(status, apiError);
            context.Response = errorResponse;

#if(DEBUG)
            Logger.ELog(context.Exception.Message + Environment.NewLine + context.Exception.StackTrace);
#else
            Logger.ELog(context.Exception.Message);
#endif

            base.OnException(context);
        }
    } 
    
    
    /// <summary>
    /// Class that represents an error returned to
    /// the client. Can be explicitly returned or
    /// as part of the UnhandledExceptionFilter
    /// </summary>
    public class ApiMessageError
    {
        public string message { get; set; }
        public bool isCallbackError { get; set; }
        public List<string> errors { get; set; }

        public ApiMessageError()
            : this(string.Empty)
        {
        }

        public ApiMessageError(string errorMessage)
        {
            isCallbackError = true;
            errors = new List<string>();
            message = errorMessage;
        }

        public ApiMessageError(ModelStateDictionary modelState)
        {
            isCallbackError = true;
            errors = new List<string>();
            message = "Model is invalid.";

            // add errors into our client error model for client
            foreach (var modelItem in modelState)
            {
                var modelError = modelItem.Value.Errors.FirstOrDefault();
                if (!string.IsNullOrEmpty(modelError.ErrorMessage))
                    errors.Add(modelItem.Key + ": " +
                                ParseModelStateErrorMessage(modelError.ErrorMessage));
                else
                    errors.Add(modelItem.Key + ": " +
                                ParseModelStateErrorMessage(modelError.Exception.Message));
            }
        }

        /// <summary>
        /// Strips off anything after period - line number etc. info
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        string ParseModelStateErrorMessage(string msg)
        {
            int period = msg.IndexOf('.');
            if (period < 0 || period > msg.Length - 1)
                return msg;

            // strip off 
            return msg.Substring(0, period);
        }
    }
}