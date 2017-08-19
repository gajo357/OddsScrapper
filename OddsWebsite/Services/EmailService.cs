using OddsWebsite.Models;
using System.Net;
using System.Net.Mail;

namespace OddsWebsite.Services
{
    public class EmailService : IEmailService
    {
        public void SendEmail(string name, string email, string subject, string body)
        {
        }

        private const string _fromMail = "gajo357@gmail.com";

        public void SendEmail(ContactInfoModel contactInfo)
        {
            if (contactInfo == null)
                return;

            if (contactInfo.Email != _fromMail)
                return;

            if (contactInfo.Name != "petarpan")
                return;

            var fromAddress = new MailAddress(_fromMail, "me");
            var toAddress = new MailAddress(_fromMail, "me");
            const string fromPassword = "fromPassword";
            const string subject = "Subject";
            const string body = "Body";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body })
            {
                smtp.Send(message);
            }
        }
    }
}
