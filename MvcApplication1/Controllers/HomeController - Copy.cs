using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Data.Objects;
using System.Net.Mail;

namespace MvcApplication1.Controllers
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
            ObjectQuery<Product> products = db.Products.Top("3");

            return PartialView(products);
        }


        public ActionResult NewsPartial()
        {
            IQueryable<News> news = db.News.OrderByDescending(xd =>xd.Date).Take(3);

            return PartialView(news);
        }

        public ActionResult About()
        {
            ViewBag.Message = "About Robyn’s Handmade Soap";

            return View();
        }


        [HttpPost]
        public ActionResult Contact(MvcApplication1.Models.ContactModel model)
        {
            if (ModelState.IsValid && ModelState.Values.ToArray()[4].Value.AttemptedValue == "" & model.TestSP == null)
            {
                // save to database, email someone
                try
                {
                    String EmailFrom;
                    String EmailPass;
                    String EmailServer;
                    String EmailType = "BLACKNIGHT";
                    String EmailTo;

                    if (EmailType == "GMAIL"){
                        EmailServer = "smtp.gmail.com";
                        EmailFrom = "NOTLIKELY@NOTLIKELY.com";
                        EmailTo = "NOTLIKELY@gmail.com";
                        EmailPass = "NOTLIKELY";

                        
                    }
                    else{                    
                        EmailServer = "mail.blacknight.com";
                        EmailFrom = "info@NOTLIKELY.com";
                        EmailTo = "info@NOTLIKELY.com";
                        EmailPass = "NOTLIKELY";
                    }

                    
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient(EmailServer);

                    mail.From = new MailAddress(EmailFrom);
                    mail.To.Add(EmailTo);
                    mail.ReplyToList.Add(model.EmailAddress);
                    mail.Subject = model.Subject;
                    mail.Body = model.Comment;

                    SmtpServer.Port = 587;
                    SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(EmailFrom, EmailPass);
                    SmtpServer.EnableSsl = true;
                    
                    SmtpServer.Send(mail);
                    model.SendStatus = "An email has been sent to info@NOTLIKELY.com.  We will get back to you as soon as possible";

                    //model.SendStatus = "Success";
                }
                catch (Exception e)
                {
                  model.SendStatus = e.Message;
                }

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

        public ActionResult Gallery()
        {
            ViewBag.Message = "Gallery";

            return View();
        }

        public ActionResult Feedback()
        {
            ViewBag.Message = "Feedback";

            return View();
        }


        public ActionResult Stockists()
        {
            ViewBag.Message = "Stockists";

            return View();
        }

        public ActionResult Shop()
        {
            ViewBag.Message = "Shop";

            return View();
        }
    }
}
