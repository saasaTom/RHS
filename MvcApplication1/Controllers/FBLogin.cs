using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace RobynHandMadeSoap.Controllers
{
    
    public class FBLogin : Controller
    {
        public Dictionary<String, String> FaceBookInfo = new Dictionary<string, string>(1);
        public Dictionary<String, String> Login()
        {
            return FaceBookInfo;
        }
        private RobynsHandMadeSoapEntities _db { get; set; }
        /*
         * Handles the Flow from FB Authorization redirect:
         * 1) Total Success: FB returns a code variable for us to use and request auth tokens, this is done by the FBLogin controller
         *     -> Redirect to FBLogin controller to retriece auth token.
         * 2) FB returns an error for some reason, we currently quietly redirect back to Home.
         *    -> Redirect to Home
         * 3) Some suspicious activity by trying to access this controller directly....should check STATE param
         *    -> Redirect to FBLogin controller to start login process
         */
        [AllowAnonymous]
        public ActionResult FBLoginAuthenticated(string state = "", string code = "", string error_reason = "", string error = "", string error_description = "", string authToken = "")
        {
            
            //We have been authenticated by FB and re-directed to this page again.
            //Request.QueryString["key"]
            //string fbGraphURL = "https://graph.facebook.com/me?access_token=";
            if (code.Length == 0)
            {
                //We have not been logged in, or there is an ERROR
                if (error.Length > 0)
                {
                    //There is an Error. We should handle this better, but not for now
                    RedirectToAction("Index", "Home");
                }
                //Otherwise head back to FBLogin to start the control flow properly
                RedirectToAction("FBLogin", "News");
            }


            return RedirectToAction("FBLogin", "News", new { code = code });
        }


        public bool FBGetAndSaveToken(string fbAccessTokenURL = "", string code = "", bool testing = false)
        {
            //Otherwise FB has sent as an Access Code to obtain a token if we wish.
            string fbAuthResult;
            string fbAccessToken = "";
            string graph_url;
            string jsonResult;
            string FBName = code; //Default FBName to code as this is set to TESTER when testing. 

            JavaScriptSerializer js = new JavaScriptSerializer();
            var graphResults = js.Deserialize<dynamic>("");
            //
            if (!testing)
            {
                using (var client = new WebClient())
                {
                    fbAuthResult = client.DownloadString(fbAccessTokenURL);
                    NameValueCollection fbAuthResultList = HttpUtility.ParseQueryString(fbAuthResult);
                    //access_token=USER_ACCESS_TOKEN&expires=NUMBER_OF_SECONDS_UNTIL_TOKEN_EXPIRES
                    fbAccessToken = fbAuthResultList["access_token"];

                    graph_url = "https://graph.facebook.com/me?access_token=" + fbAccessToken;
                    jsonResult = client.DownloadString(graph_url);

                    //NameValueCollection authStuff = js.Deserialize<NameValueCollection>(jsonResult);
                    graphResults = js.Deserialize<dynamic>(jsonResult);

                    // echo("Hello " . $user->name);
                    //FormsAuthentication.SetAuthCookie(graphResults["name"], createPersistentCookie: true);
                    FBName = graphResults["name"];
                }
            }

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                                 1,
                                 FBName,
                                 DateTime.Now,
                                 DateTime.Now.AddYears(10),
                                 true,
                                 "Can Edit Pages",
                                 FormsAuthentication.FormsCookiePath);

            // Encrypt the ticket.
            string encTicket = FormsAuthentication.Encrypt(ticket);

            // Create the cookie.
            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));

            Access_History ah = new Access_History()
            {
                Attempted_URL = FormsAuthentication.GetRedirectUrl(graphResults["name"], true),
                CookiePath = FormsAuthentication.FormsCookiePath,
                CookieSet = "Y",
                Date = DateTime.Now,
                OACode = code,
                OAToken = fbAccessToken,
                ResultSF = "SUCCESS",
                Stage = "FINAL"

            };
            _db.Access_History.AddObject(ah);
            _db.SaveChanges();

            //Redirect News PAGE
            //FormsAuthentication.GetRedirectUrl(graphResults["name"], true);
            return true;
        }

        [AllowAnonymous]
        public ActionResult FBLoginBegin(string code = "")
        {
            string myAppID;
            string myAppSecret;
            string statePass;
            string siteURL;
            bool testing = false;
            string loginRedirectUrl;
            loginRedirectUrl = Request.QueryString["ReturnUrl"].TrimStart('/') ?? "Home";



            if (Request.Url.Host == "localhost")
            {
                myAppID = "139156632894288";
                myAppSecret = "60a2df951d5523160c6b31cf06431aab";
                statePass = "xxYYZZxx";
                siteURL = Request.Url.Scheme + "://" + Request.Url.Host + (Request.Url.Port == null ? "" : ":" + Request.Url.Port.ToString());
                testing = true;
                code = "TESTER";
                /* echo("Hello " . $user->name);
                //FormsAuthentication.SetAuthCookie(graphResults["name"], createPersistentCookie: true);
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                  1,
                  "test",
                  DateTime.Now,
                  DateTime.Now.AddYears(10),
                  true,
                  "Can Edit Pages",
                  FormsAuthentication.FormsCookiePath);

                // Encrypt the ticket.
                //string encTicket = FormsAuthentication.Encrypt(ticket);

                // Create the cookie.
                //Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
                //return RedirectToAction("Index", loginRedirectUrl);
                 * */

            }
            else
            {
                myAppID = "252466691535585";
                myAppSecret = "e50a7f0d2bdf2886863a54737e5343df";
                statePass = "xxYYZZxx";
                siteURL = Request.Url.Scheme + "://" + Request.Url.Host;
            }

            string redirect_uri = siteURL + Url.Action("FBLoginAuthenticated", "News");
            //If Code is non null then we SHOULD be logged in correctly (Or testing) (User could still pass in crap code, but we will still try validate code against FB by getting Access Token!
            if (code.Length > 0)
            {
                string fbAccessTokenURL = "https://graph.facebook.com/oauth/access_token?client_id=" + myAppID +
                                "&redirect_uri=" + redirect_uri +
                                "&client_secret=" + myAppSecret +
                                "&code=" + code;
                if (!FBGetAndSaveToken(fbAccessTokenURL, code, testing)) loginRedirectUrl = "Home";

                return RedirectToAction("Index", loginRedirectUrl);
            }
            else //We have to authorize the user via FB
            {
                string fbLogin = "https://www.facebook.com/dialog/oauth?" +
                                "client_id=" + myAppID +
                                "&redirect_uri=" + redirect_uri +
                                "&scope=" + "manage_pages,user_about_me" +
                                "&state=" + statePass;
                return Redirect(fbLogin);
            }

        }
    }
}
