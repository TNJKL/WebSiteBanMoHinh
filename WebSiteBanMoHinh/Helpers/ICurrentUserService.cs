using WebSiteBanMoHinh.Models;

namespace WebSiteBanMoHinh.Helpers
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        Task<AppUserModel> GetUser();
    }
}
