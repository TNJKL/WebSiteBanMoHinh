namespace WebSiteBanMoHinh.ViewModels
{
    public class MessagesUsersListViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string LastMessage { get; set; }
        //ko cần thì cứ xóa LastMessageDate
        public DateTime LastMessageDate { get; set; }

        //ko cần thì cứ xóa LastMessageDate
        //thêm vào 
        public bool IsRead { get; set; }
    }
}
