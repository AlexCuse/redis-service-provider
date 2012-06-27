﻿using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web;
using System.Web.SessionState;

namespace Web2.SessionProvider {
    public class CustomSessionManager : ISessionIDManager {

        private SessionStateSection pConfig = null;


        //
        // IHttpModule Members
        //


        //
        // IHttpModule.Init
        //

        public void Init(HttpApplication app) {
            // Obtain session-state configuration settings.

            if (pConfig == null) {
                Configuration cfg =
                  WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
                pConfig = (SessionStateSection)cfg.GetSection("system.web/sessionState");
            }
        }


        //
        // IHttpModule.Dispose
        //

        public void Dispose() {
        }




        //
        // ISessionIDManager Members
        //




        //
        // ISessionIDManager.Initialize
        //

        public void Initialize() {
        }


        //
        // ISessionIDManager.InitializeRequest
        //

        public bool InitializeRequest(HttpContext context,
                                      bool suppressAutoDetectRedirect,
                                      out bool supportSessionIDReissue) {
            if (pConfig.Cookieless == HttpCookieMode.UseCookies) {
                supportSessionIDReissue = false;
                return false;
            }
            else {
                supportSessionIDReissue = true;
                return context.Response.IsRequestBeingRedirected;
            }
        }




        //
        // ISessionIDManager.GetSessionID
        //
        public string GetSessionID(HttpContext context) {
            string id = null;

            if (pConfig.Cookieless == HttpCookieMode.UseUri) {
                // Retrieve the SessionID from the URI.
            }
            else {
                id = context.Request.Cookies[pConfig.CookieName].Value;
            }

            // Verify that the retrieved SessionID is valid. If not, return null.

            if (!Validate(id))
                id = null;

            return id;
        }

        //
        // ISessionIDManager.CreateSessionID
        //

        public string CreateSessionID(HttpContext context) {
            return Guid.NewGuid().ToString();
        }

        //
        // ISessionIDManager.RemoveSessionID
        //

        public void RemoveSessionID(HttpContext context) {
            context.Response.Cookies.Remove(pConfig.CookieName);
        }


        //
        // ISessionIDManager.SaveSessionID
        //

        public void SaveSessionID(HttpContext context, string id, out bool redirected, out bool cookieAdded) {
            redirected = false;
            cookieAdded = false;

            if (pConfig.Cookieless == HttpCookieMode.UseUri) {
                // Add the SessionID to the URI. Set the redirected variable as appropriate.

                redirected = true;
                return;
            }
            else {
                context.Response.Cookies.Add(new HttpCookie(pConfig.CookieName, id));
                cookieAdded = true;
            }
        }


        //
        // ISessionIDManager.Validate
        //

        public bool Validate(string id) {
            try {
                Guid testGuid = new Guid(id);

                if (id == testGuid.ToString())
                    return true;
            }
            catch {
            }

            return false;
        }
    }
}