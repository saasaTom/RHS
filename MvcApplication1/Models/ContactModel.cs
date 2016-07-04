using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Net.Mail;

namespace RobynHandMadeSoap.Models
{
    public class ContactModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        [Display(Name = "Comment")]
        public string Comment { get; set; }

        public string SendStatus {get; set; }

        public string TestSP { get; set; }

        private string EmailFrom;
        private string EmailPass;
        private string EmailServer;
        private string EmailType; // = "BLACKNIGHT";
        private string EmailTo;

        public void SetupEmail(string EmailFrom, string EmailPass, string EmailServer, string EmailType, string EmailTo) {
            this.EmailFrom = EmailFrom;
            this.EmailPass = EmailPass;
            this.EmailServer = EmailServer;
            this.EmailType = EmailType;
            this.EmailTo = EmailTo;
        }

        public string SendEmail (Boolean fullSend) {

            try
            {

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(EmailServer);

                //Setup Mail Message
                mail.From = new MailAddress(EmailFrom);
                mail.To.Add(EmailTo);
                mail.ReplyToList.Add(this.EmailAddress);
                mail.Subject = this.Subject;
                mail.Body = this.Comment;

                //Setup STMP settings
                SmtpServer.Port = 587;
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                SmtpServer.Credentials = new System.Net.NetworkCredential(EmailFrom, EmailPass);
                SmtpServer.EnableSsl = true;

                //Send Mail
                if (fullSend)
                {
                    SmtpServer.Send(mail);

                    //If no errors, then show a success message on the page.
                    this.SendStatus = "An email has been sent to " + EmailTo + ".  We will get back to you as soon as possible";
                }else {
                     this.SendStatus = "No email has been sent.  If this isn't the desired result please contact us";
                }

                //Should really update DB here with email details


                //model.SendStatus = "Success";
            }
            catch (Exception e)
            {
                //Should really update DB here with email details to capture things
                this.SendStatus = e.Message;
            }


            return "SUCCESS";
        }
    }
}