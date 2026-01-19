using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(solutions4home.Startup))]

namespace solutions4home
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            var clientId = ConfigurationManager.AppSettings["ida:ClientId"];
            var clientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
            var redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];

            // MULTITENANT authority
            var authority = "";

            // Cookie auth
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            // OIDC
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = clientId,
                Authority = authority,
                RedirectUri = redirectUri,
                PostLogoutRedirectUri = redirectUri,
                ClientSecret = clientSecret,
                UsePkce = false, // todo

                // ============================================================
                // 🔐 AUTHORIZATION CODE FLOW ONLY
                // ============================================================
                ResponseType = "code", // OpenIdConnectResponseType.Code,
                Scope = "openid profile email offline_access",

                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true
                },

                SignInAsAuthenticationType = "Cookies",

                // ============================================================
                // 🔐 Exchange authorization code for tokens
                // ============================================================
                RedeemCode = true,

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = async context =>
                    {
                        // 🔑 Authorization Code
                        //var code = context.Code;

                        // Redeem code for tokens
                        //var tokenClient = new HttpClient();
                        //var tokenResponse = await tokenClient.PostAsync(
                        //    "",
                        //    new FormUrlEncodedContent(
                        //        new Dictionary<string, string>
                        //        {
                        //            { "client_id", clientId },
                        //            { "grant_type", "authorization_code" },
                        //            { "code", code },
                        //            { "redirect_uri", redirectUri },
                        //            { "client_secret", clientSecret}
                        //         })
                        //);

                        //var json = await tokenResponse.Content.ReadAsStringAsync();

                    },

                    SecurityTokenValidated = context =>
                    {
                        // 🔐 ID token already validated here
                        var idToken = context.ProtocolMessage.IdToken;
                        return Task.CompletedTask;
                    },

                    AuthenticationFailed = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/error");
                        return Task.CompletedTask;
                    }
                }
            });
        }
    }
}