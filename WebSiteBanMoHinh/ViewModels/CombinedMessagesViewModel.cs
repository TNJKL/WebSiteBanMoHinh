namespace WebSiteBanMoHinh.ViewModels
{
    public class CombinedMessagesViewModel
    {
        public IEnumerable<MessagesUsersListViewModel> Users { get; set; }
        public ChatViewModel Chat { get; set; }
    }
}
