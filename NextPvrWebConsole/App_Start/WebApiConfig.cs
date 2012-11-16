using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Security;
using System.Net.Http;
using System.Threading;
using System.Net;
using System.Text;
using System.Security.Principal;
using System.Web.Http.Hosting;
using System.Threading.Tasks;
using System.Web.Http.Routing;

namespace NextPvrWebConsole
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            /*
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
             * */
            config.Routes.MapHttpRoute("ApiServiceRoute", "service", new { controller = "service" });
            config.Routes.MapHttpRoute("DefaultApiDelete", "api/{controller}/{oid}", new { action = "Delete", oid = RouteParameter.Optional }, new { oid = @"^\d+$", httpMethod = new HttpMethodConstraint(HttpMethod.Delete) });
            config.Routes.MapHttpRoute("DefaultApiWithOId", "api/{controller}/{oid}", new { oid = RouteParameter.Optional }, new { oid = @"^\d+$" });
            config.Routes.MapHttpRoute("DefaultApiWithActionAndOId", "api/{controller}/{action}/{oid}", new { oid = RouteParameter.Optional }, new { oid = @"^\d+$" });
            config.Routes.MapHttpRoute("DefaultApiWithAction", "api/{controller}/{action}");
            config.Routes.MapHttpRoute("DefaultApiGet", "api/{controller}", new { action = "Get" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
            config.Routes.MapHttpRoute("DefaultApiPost", "api/{controller}", new { action = "Post" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });
            config.Routes.MapHttpRoute("DefaultApiPut", "api/{controller}", new { action = "Put" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Put) });

            // first handler runs last, so set encoding/compression as first (which will be last :))
            config.MessageHandlers.Add(new Handlers.EncodingDelegateHandler());
            //config.MessageHandlers.Add(new BasicAuthMessageHandler());

            config.Filters.Add(new UnhandledExceptionFilter());


        }
    }

    public class BasicAuthMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // check if already signed in
            if (WebMatrix.WebData.WebSecurity.IsAuthenticated)
                return base.SendAsync(request, cancellationToken);

            if (request.Headers.Authorization == null || request.Headers.Authorization.Scheme != "Basic")
            {
                return
                    Task<HttpResponseMessage>.Factory.StartNew(
                        () => new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }
            var encoded = request.Headers.Authorization.Parameter;
            var encoding = Encoding.GetEncoding("iso-8859-1");
            var userPass = encoding.GetString(Convert.FromBase64String(encoded));
            int sep = userPass.IndexOf(':');
            var username = userPass.Substring(0, sep);
            var identity = new GenericIdentity(username, "Basic");
            //request.Properties.Add(HttpPropertyKeys.UserPrincipalKey, new GenericPrincipal(identity, new string[] { }));
            return base.SendAsync(request, cancellationToken);
        }

    }
}
