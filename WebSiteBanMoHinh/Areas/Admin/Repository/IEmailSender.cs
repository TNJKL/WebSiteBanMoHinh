namespace WebSiteBanMoHinh.Areas.Admin.Repository
{
    public interface IEmailSender
    {
        //Task SendEmailAsync(string email, string subject, string message);
        Task SendEmailAsync(string email, string subject, string message, bool isHtml = false);
    }
}
