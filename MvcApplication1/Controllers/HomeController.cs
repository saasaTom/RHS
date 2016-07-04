using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Data.Objects;
using System.Net.Mail;
using System.IO;
using System.Text.RegularExpressions;

namespace RobynHandMadeSoap.Controllers
{
    public class HomeController : Controller
    {



        private RobynsHandMadeSoapEntities db = new RobynsHandMadeSoapEntities();

        public ActionResult Index()
        {
            ViewBag.Message = "Robyn's Home Made Soap";

            return View();
        }


        public ActionResult ProductPartial()
        {
            //ObjectQuery<Product> products = db.Products.Top("3");
            List<RobynHandMadeSoap.Models.ProductCategories> shopCategories = db.Product_Categories.Top("3").ToList().Select(p => Models.ProductCategories.FromEntity(p)).ToList();

            return PartialView(shopCategories);
        }


        public ActionResult NewsPartial()
        {
            //Regex DBMarkUpToHTML,HTMLToDBMarkup;
            //HTMLToDBMarkup = new Regex(@"<([^>]*)>");
            //DBMarkUpToHTML = new Regex(@"\[([/]?[pPbBeE][1rRmM]?|[/]?a[^\]]*)\]");
            IEnumerable<RobynHandMadeSoap.Models.NewsModel.news> news = db.News.OrderByDescending(xd => xd.Date).Take(3).ToList().Select(x => new RobynHandMadeSoap.Models.NewsModel.news(x));

            //Lots of work to make the stupid dates have st,rd, th on the end!!  But this
            //is because I decided to store news in a DB, with the date as a date field and not text!
            //foreach (var item in news)
            //{
            //    RobynHandMadeSoap.Models.Types.HTMLDate dateString = new Models.Types.HTMLDate(item.Date);
                //dateString.date = ;
            //    item.DateString = dateString.toOrdinal();
                //First remove ALL "<" and "> tags ro get rid of any markup!!  
                //Then replace the small set of tags <p>, <br>, <em>,<a> back!
            //    item.Detail = DBMarkUpToHTML.Replace(HTMLToDBMarkup.Replace(item.Detail, "[$1]"), "<$1>");
            //}
            

            return PartialView(news);
        }

        public ActionResult About()
        {
            ViewBag.Message = "About Robyn’s Handmade Soap";

            return View();
        }


        [HttpPost]
        public ActionResult Contact(RobynHandMadeSoap.Models.ContactModel model)
        {
            if (ModelState.IsValid && ModelState.Values.ToArray()[4].Value.AttemptedValue == "" & model.TestSP == null)
            {
                // save to database, email someone
                try
                {
                    //Test GMAIL server
                    string debug = "PROD";
                    if (debug == "TEST"){
                        model.SetupEmail("robynssoap@robynssoap.com", "robynssoappass", "smtp.gmail.com", "TEST", "thomaspbryant@gmail.com");

                        
                    }
                    //PROD BlackNight Server
                    else{
                        model.SetupEmail("info@robynshandmadesoap.com", "r0byn2012", "mail.blacknight.com", "PROD", "info@robynshandmadesoap.com");
                    }

                    model.SendEmail(true);

                    /*
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient(EmailServer);

                    //Setup Mail Message
                    mail.From = new MailAddress(EmailFrom);
                    mail.To.Add(EmailTo);
                    mail.ReplyToList.Add(model.EmailAddress);
                    mail.Subject = model.Subject;
                    mail.Body = model.Comment;

                    //Setup STMP settings
                    SmtpServer.Port = 587;
                    SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(EmailFrom, EmailPass);
                    SmtpServer.EnableSsl = true;
                    
                    //Send Mail
                    SmtpServer.Send(mail);
                    */
                    //If no errors, then show a success message on the page.
                    //model.SendStatus = "An email has been sent to info@robynshandmadesoap.com.  We will get back to you as soon as possible";

                    //Should really update DB here with email details


                    //model.SendStatus = "Success";
                }
                catch (Exception e)
                {
                  //Should really update DB here with email details to capture things
                  model.SendStatus = e.Message;
                }

                //Return to the Contacts page, with the model data loaded in
                ViewBag.SendStatus = model.SendStatus;
                return View(model);
            }
            else
            {
               return View(model);
            }
        }

        [HttpGet]
        public ActionResult Contact()
        {
            ViewBag.Message = "Contact";

            return View();
        }

        public ActionResult Ingredients()
        {
            ViewBag.Message = "Ingredients";

            return View();
        }

        //The main Gallery Page
        public ActionResult Gallery()
        {
            ViewBag.Message = "Gallery";

            return View();
        }

        //Used by Jquery GET request to return the partial view into a DIV based on the image clicked on the main gallery page
        public ActionResult GalleryImageView(string galleryType)
        {
            DirectoryInfo galleryDir = null;
            FileInfo[] files = null;

            string dirPath = @"/Images/Gallery";
            try
            {
                galleryDir = new DirectoryInfo(Server.MapPath(dirPath));

                
                files = galleryDir.GetFiles("*" + galleryType.ToLower() + "*.jpg");
            }catch
            {
                //Should really create an Error Gallery Image, or something;
                files = null;
            }

            return PartialView("GalleryImageView",files);
        }
     

        public ActionResult Feedback()
        {
            ViewBag.Message = "Feedback";

            return View();
        }

        [HttpPost]
        public ActionResult Stockists(List<RobynHandMadeSoap.Models.Stockist_Locations> model)
        {
            if (ModelState.IsValid)
            {
                RobynHandMadeSoap.Stockist st = new RobynHandMadeSoap.Stockist();
                st.Location_Id = model[0].Location_Id;
                st.Name = model[0].Stockist_List[0].Name;
                st.Effective_Date = DateTime.Now;
                UpdateModel(st);
                db.Stockists.AddObject(st);
                db.SaveChanges();
            }
            return View(model);
        }


        public ActionResult Stockists()
        {

            List<RobynHandMadeSoap.Models.Stockist_Locations> sl = new List<RobynHandMadeSoap.Models.Stockist_Locations>();

            sl = db.Stockist_Locations.ToList().Select(p => Models.Stockist_Locations.FromEntity(p)).ToList();

            ViewBag.Message = "Stockists";

            return View(sl);
        }

        public ActionResult Shop()
        {
            ViewBag.Message = "Shop";

            return View();
        }
    }
}
