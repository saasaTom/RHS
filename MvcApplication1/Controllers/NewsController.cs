using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Web.Security;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;

namespace RobynHandMadeSoap.Controllers
{

    [Authorize (Roles = "Administrator")]

    public class NewsController : Controller
    {
        //private string requestEntry;
        private RobynsHandMadeSoapEntities db = new RobynsHandMadeSoapEntities();
        Regex HTMLToDBMarkup = new Regex(@"<([^>]*)>");
        Regex DBMarkUpToHTML = new Regex(@"\[([/]?[pPbBeE][1rRmM]?|[/]?a[^\]]*)\]");
        

        #region Old_Members
        //
        // GET: /News/
        public ActionResult Index_Old(int start = 0, int itemsPerPage = 1, string orderBy = "Id", bool desc = true)
        {
            //if (authCode.Length > 0)
            //{
                ViewBag.Count = db.News.Count();
                ViewBag.Start = start;
                ViewBag.ItemsPerPage = itemsPerPage;
                ViewBag.OrderBy = orderBy;
                ViewBag.Desc = desc;


                //Swap code for Access Toke

                return View(db.News.AsEnumerable());
           // }
            //else{
            //    return RedirectToAction("FBLogin", "News");
            //}
        }
        
        #endregion

        public ActionResult Index()
        {
            IEnumerable<RobynHandMadeSoap.Models.NewsModel.news> newsList = db.News.OrderByDescending(xd => xd.Date).ToList().Select(x => new RobynHandMadeSoap.Models.NewsModel.news(x));
            ViewBag.Array = newsList.Select(x => x.jsArrayDate).ToArray();
            return View(newsList);
        }

        //
        // GET: /News/GridData/?start=0&itemsPerPage=20&orderBy=Id&desc=true

        public ActionResult GridData(int start = 0, int itemsPerPage = 1, string orderBy = "Date", bool desc = true)
        {
            Response.AppendHeader("X-Total-Row-Count", db.News.Count().ToString());
            ObjectQuery<News> news = db.News;
            news = news.OrderBy("it." + orderBy + (desc ? " desc" : ""));

            foreach (var item in news){
                item.Detail = DBMarkUpToHTML.Replace(HTMLToDBMarkup.Replace(item.Detail, "[$1]"), "<$1>");
            }

            //item.Detail = DBMarkUpToHTML.Replace(HTMLToDBMarkup.Replace(item.Detail, "[$1]"), "<$1>");

            return PartialView(news.Skip(start).Take(itemsPerPage));
        }

        //
        // GET: /Default5/RowData/5

        public ActionResult RowData(int id)
        {
            RobynHandMadeSoap.Models.NewsModel.news news = new RobynHandMadeSoap.Models.NewsModel.news(db.News.Single(n => n.Id == id));
            return PartialView("GridData", news );
        }

        //
        // GET: /News/Create

        public ActionResult Create(DateTime newsDate)
        {
            RobynHandMadeSoap.Models.NewsModel.news dummy = new Models.NewsModel.news();
            dummy.mockNew(newsDate);
            return PartialView("Edit", dummy);
        }

        //
        // POST: /News/Create

        [HttpPost]
        public ActionResult Create(RobynHandMadeSoap.Models.NewsModel.news news)
        {
            if (ModelState.IsValid)
            {
                News actualNews = news.toNews();
                db.News.AddObject(actualNews);
                db.SaveChanges();
                news.Id = actualNews.Id;
                return PartialView("GridData",  news );
            }

            return PartialView("Edit", news);
        }

        //
        // GET: /News/Edit/5

        public ActionResult Edit(int id)
        {
            RobynHandMadeSoap.Models.NewsModel.news news = new RobynHandMadeSoap.Models.NewsModel.news(db.News.Single(n => n.Id == id));
            return PartialView("Edit",news);
        }

        //
        // POST: /News/Edit/5

        [HttpPost]
        public ActionResult Edit(RobynHandMadeSoap.Models.NewsModel.news news)
        {
            if (ModelState.IsValid)
            {
                News dbNews = news.toNews();
                db.News.Attach(dbNews);
                db.ObjectStateManager.ChangeObjectState(dbNews, EntityState.Modified);
                db.SaveChanges();
                return PartialView("GridData",news);
            }

            return PartialView("Edit",news);
        }

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


        public bool FBGetAndSaveToken(string fbAccessTokenURL ="",string code = "", bool testing = false)
         {
            //Otherwise FB has sent as an Access Code to obtain a token if we wish.
            string fbAuthResult;
            string fbAccessToken = "";
            string graph_url;
            string jsonResult;
            string FBName = code; //Default FBName to code as this is set to TESTER when testing. 
            string FBID = "";

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
                    FBID = graphResults["id"];
                }
            }
            String FBUserName = FBName + " ID=" + FBID;
            MembershipUser user = Membership.GetUser(FBUserName);
            if (user != null) {
                //They Exist already as a  just AUTH THEM
            }else{
                //NEWLY FB Authenticated with PAGE ADMIN.  SO create them as Site Admin.
                Membership.CreateUser(FBUserName, FBID);
                //Roles.AddUserToRole(FBUserName,"Administrator");
                FormsAuthentication.Authenticate(FBUserName, FBID);
            }

            //If FB Logged us in as Page Admin then we can modify this website too!
            if (! Roles.IsUserInRole("Administrator")) {
                Roles.AddUserToRole(FBUserName, "Administrator");
            }

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                                 1,
                                 FBUserName,
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
                Attempted_URL = Request.QueryString["ReturnUrl"] ?? Request.Params["URL"],
                CookiePath = FormsAuthentication.FormsCookiePath,
                CookieSet = "Y",
                Date = DateTime.Now,
                OACode = code,
                OAToken = fbAccessToken,
                ResultSF = "SUCCESS",
                Stage = "FINAL"

            };
            db.Access_History.AddObject(ah);
            db.SaveChanges();

            //Redirect News PAGE
            //FormsAuthentication.GetRedirectUrl(graphResults["name"], true);
            return true;
        }

        [AllowAnonymous]
        public ActionResult FBLogin(string code = "")
         {
            string myAppID;
            string myAppSecret;
            string statePass;
            string siteURL;
            bool testing = false;
            bool forceAuth = false; //Set to true to force authenticaion on DEV machine
            string loginRedirectUrl;
            loginRedirectUrl = Request.QueryString["ReturnUrl"] ?? "Home";
            //loginRedirectUrl = loginRedirectUrl ?? FormsAuthentication.GetRedirectUrl("", false);
            HttpContext.Session["RedirectTo"] = HttpContext.Session["RedirectTo"] ?? FormsAuthentication.GetRedirectUrl("", false); 


             if (Request.Url.Host == "localhost")
             {
                myAppID = "139156632894288";
                myAppSecret = "60a2df951d5523160c6b31cf06431aab";
                statePass = "xxYYZZxx";
                siteURL = Request.Url.Scheme + "://" + Request.Url.Host + (Request.Url.Port == null ? "" : ":" + Request.Url.Port.ToString());
                if (!forceAuth)
                {
                    code = "TESTER";
                    testing = true;
                }
                else
                {
                    testing = false;
                }

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

             //string redirect_uri = siteURL + Url.Action("FBLoginAuthenticated", "News", new { loginRedirectUrl = loginRedirectUrl });
             string redirect_uri = siteURL + Url.Action("FBLoginAuthenticated", "News");
            //If Code is non null then we SHOULD be logged in correctly (Or testing) (User could still pass in crap code, but we will still try validate code against FB by getting Access Token!
            if (code.Length > 0 )
            {
                string fbAccessTokenURL = "https://graph.facebook.com/oauth/access_token?client_id=" + myAppID +
                                "&redirect_uri=" + redirect_uri +
                                "&client_secret=" + myAppSecret +
                                "&code=" + code;
                if (!FBGetAndSaveToken(fbAccessTokenURL, code, testing)) HttpContext.Session["RedirectTo"] = "Home";

                return RedirectToAction("Index", HttpContext.Session["RedirectTo"].ToString().Trim('/'), new object { });
                //return RedirectToAction("Index", "Product");
            }else //We have to authorize the user via FB
            {
                string fbLogin = "https://www.facebook.com/dialog/oauth?" +
                                "client_id=" + myAppID +
                                "&redirect_uri=" + redirect_uri +
                                "&scope=" + "manage_pages,user_about_me" +
                                "&state=" + statePass;
                return Redirect(fbLogin);
            }

         }

        [AllowAnonymous]
        public ActionResult FBLoginWorks(string state = "", string code = "", string error_reason = "", string error = "", string error_description = "", string authToken = "")
        {
            
            string myAppID;
            string myAppSecret;
            string statePass;
            string siteURL;
            string loginRedirectUrl;
            loginRedirectUrl = Request.QueryString["ReturnUrl"].TrimStart('/') ?? "Home";


            //if (Request.QueryString["ReturnUrl"] != null) loginRedirectUrl = Request.QueryString["ReturnUrl"].TrimStart('/');


            if (Request.Url.Host == "localhost") 
            {
                myAppID = "139156632894288";
                myAppSecret = "60a2df951d5523160c6b31cf06431aab";
                statePass = "xxYYZZxx";
                siteURL = Request.Url.Scheme + "://" + Request.Url.Host + (Request.Url.Port == null ? "" : ":" + Request.Url.Port.ToString());
                //siteURL = "http://localhost:50290";

                // echo("Hello " . $user->name);
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
                string encTicket = FormsAuthentication.Encrypt(ticket);

                // Create the cookie.
                Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
                return RedirectToAction("Index", loginRedirectUrl);

            }else{
                myAppID = "252466691535585";
                myAppSecret = "e50a7f0d2bdf2886863a54737e5343df";
                statePass = "xxYYZZxx";
                siteURL = Request.Url.Scheme + "://" + Request.Url.Host;
                //siteURL = "http://107.22.249.93";
            }


            string redirect_uri = siteURL + Url.Action("FBLoginAuthenticated", "News");
            string permissionList = "manage_pages,user_about_me";

            //IF LOGGED IN RETURN NEWS VIEW
            //CHECK RET CODE TO SEE IF FB IS PASSING BACK TO HERE
            //IF SUCCESS GO TO NEWS
            //We have been redirected from FB

            //When beginning the OAuth process we set state to "".  Facebook login will re-direct us to this controller action again with a non zero state
            if (state.Length == 0)
            {
                string fbLogin = "https://www.facebook.com/dialog/oauth?client_id=" + myAppID + "&redirect_uri=" + redirect_uri + "&scope=" + permissionList + "&state=" + statePass + "&loginRedirectUrl=" + loginRedirectUrl;
                return Redirect(fbLogin);
            }
            else {
                //We have been authenticated by FB and re-directed to this page again.
                if (error.Length == 0)
                {
                    ViewBag.Code = code;

                    if (authToken.Length != 0)
                    {
                        return RedirectToAction("Index", "News", new { authCode = authToken, state = state, pass="Y" });
                    }
                    else
                    {
                        string fbAccessTokenURL = "https://graph.facebook.com/oauth/access_token?client_id=" + myAppID + "&redirect_uri=" + redirect_uri + "&client_secret=" + myAppSecret + "&code=" + code + "&loginRedirectUrl=" + loginRedirectUrl;
                        //string fbGraphURL = "https://graph.facebook.com/me?access_token=";

                        using (var client = new WebClient())
                        {
                            string fbAuthResult = client.DownloadString(fbAccessTokenURL);
                            NameValueCollection fbAuthResultList = HttpUtility.ParseQueryString(fbAuthResult);
                            //access_token=USER_ACCESS_TOKEN&expires=NUMBER_OF_SECONDS_UNTIL_TOKEN_EXPIRES
                            string fbAccessToken = fbAuthResultList["access_token"];
                            
                            string graph_url = "https://graph.facebook.com/me?access_token=" + fbAccessToken;
                            string jsonResult = client.DownloadString(graph_url);
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            //NameValueCollection authStuff = js.Deserialize<NameValueCollection>(jsonResult);
                            var graphResults = js.Deserialize<dynamic>(jsonResult);
                            
                            // echo("Hello " . $user->name);
                            //FormsAuthentication.SetAuthCookie(graphResults["name"], createPersistentCookie: true);
                            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                              1,
                              graphResults["name"],
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
                                Attempted_URL =FormsAuthentication.GetRedirectUrl(graphResults["name"], true) ,
                                CookiePath =FormsAuthentication.FormsCookiePath ,
                                CookieSet = "Y",
                                Date =DateTime.Now,
                                OACode = code,
                                OAToken = fbAccessToken,
                                ResultSF = "SUCCESS",
                                Stage = "FINAL"
                               
                            };
                            db.Access_History.AddObject(ah);
                            db.SaveChanges();

                            //Redirect News PAGE
                            //FormsAuthentication.GetRedirectUrl(graphResults["name"], true);
                            return RedirectToAction("Index", loginRedirectUrl == null ? "News" : loginRedirectUrl);
                        }
                    }

                }else{
                    ViewBag.Error = error_description;
                    return RedirectToAction("Index", "Home");
                }
            }


        }



        //
        // POST: /News/Delete/5

        [HttpPost]
        public void Delete(int id)
        {
            News news = db.News.Single(n => n.Id == id);
            db.News.DeleteObject(news);
            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        [HttpPost]
        public string Upload()
        {
            string virtualPath;
            try
            {
                //Get file information passed in request headers to an UploadedFile object 
                RobynHandMadeSoap.Models.UploadedFile file = RobynHandMadeSoap.Models.UploadedFile.RetrieveFileFromRequest(Request);

                //Save a copy and resized verion of the file
                virtualPath = RobynHandMadeSoap.Models.UploadedFile.SaveFile(file, Server.MapPath("~/Images/"), file.FileRelPath);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 404;
                Response.StatusDescription = ex.Message;
                return ex.Message;
            }
            return virtualPath;
        }


    }
}
