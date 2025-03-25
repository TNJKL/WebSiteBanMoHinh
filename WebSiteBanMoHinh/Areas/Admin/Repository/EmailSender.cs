using System.Net.Mail;
using System.Net;

namespace WebSiteBanMoHinh.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        //public Task SendEmailAsync(string email, string subject, string message)
        //{
        //    var client = new SmtpClient("smtp.gmail.com", 587)
        //    {
        //        EnableSsl = true, //bật bảo mật
        //        UseDefaultCredentials = false,
        //        Credentials = new NetworkCredential("sinhngay21042004@gmail.com", "cysjkighicqmtuic")
        //    };

        //    return client.SendMailAsync(
        //        new MailMessage(from: "sinhngay21042004@gmail.com",
        //                        to: email,
        //                        subject,
        //                        message
        //                        ));
        //} 
        private readonly string smtpServer = "smtp.gmail.com";
        private readonly int smtpPort = 587;
        private readonly string smtpUser = "sinhngay21042004@gmail.com";
        private readonly string smtpPass = "cysjkighicqmtuic"; // Cẩn thận với việc lưu mật khẩu trong code

        public async Task SendEmailAsync(string email, string subject, string message, bool isHtml = false)
        {
            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUser),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = isHtml // ✅ Thêm tham số để hỗ trợ HTML
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }
    }

}

