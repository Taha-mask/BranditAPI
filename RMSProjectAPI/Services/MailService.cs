using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace RMSProjectAPI.Services
{
    public static class MailService
    {
        public static void SendEmail(string email, string subject, string body)
        {

            var client = new SmtpClient("sandbox.smtp.mailtrap.io", 2525)
            {

                Credentials = new NetworkCredential("e3f6a18cfabbfe", "30c45f2f1ee2d4"),

                EnableSsl = true

            };

            client.Send("contact@test.com", email, subject, body);

            System.Console.WriteLine("Sent");
        }
    }
}
