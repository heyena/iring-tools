using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using System.Threading;
using System.Net.Http.Headers;
using System.Text;

namespace iRINGTOOLS.Hypermedia.Modules
{
    //Custom Basic Authontication Module
    public class CBAModule : IHttpModule
    {
        public void Dispose()
        {}

        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += new EventHandler(context_AuthenticateRequest);
            context.EndRequest += new EventHandler(context_EndRequest);
        }

        void context_EndRequest(object sender, EventArgs e)
        {
            var response = HttpContext.Current.Response;
            if (response.StatusCode == 401)
                response.Headers.Add("WWW-Authenticate", "Basic realm=\"insert for realm\"");
        }

        void context_AuthenticateRequest(object sender, EventArgs e)
        {
            var request = HttpContext.Current.Request;
            var header = request.Headers["Authorization"];
            if (header != null)
            {
                var parsedValued = AuthenticationHeaderValue.Parse(header);
                if (parsedValued.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) 
                                && parsedValued.Parameter != null)
                {
                    Authenticate(parsedValued.Parameter);
                }

            }
        }

        private bool Authenticate(string credentialValues)
        {
            bool isValid = false;
            try
            {
                var credentials = Encoding.
                                    GetEncoding("iso-8859-1").
                                    GetString(Convert.FromBase64String(credentialValues));
                var values = credentials.Split(':');
                isValid = CheckUser(userName: values[0], password: values[1]);
                if (isValid)
                    SetPrincipal(new GenericPrincipal(new GenericIdentity(values[0]), null));
            }
            catch
            {
                isValid = false;
            }

            return isValid;
        }

        private bool CheckUser(string userName, string password)
        {
            return (userName == "name" && password == "password");
        }


        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
                HttpContext.Current.User = principal;
        }
    }
}