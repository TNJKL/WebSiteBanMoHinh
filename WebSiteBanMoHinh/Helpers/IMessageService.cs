using WebSiteBanMoHinh.ViewModels;

namespace WebSiteBanMoHinh.Helpers
{
    public interface IMessageService
    {
        Task<IEnumerable<MessagesUsersListViewModel>> GetUsers();
        Task<ChatViewModel> GetMessages(string selectedUserId);
    }
}
