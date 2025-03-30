using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebSiteBanMoHinh.Helpers;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Repository;


namespace WebSiteBanMoHinh.Hubs
{
    public class ChatHub : Hub
    {
        private readonly DataContext context;
        private readonly ICurrentUserService currentUserService;

        public ChatHub(DataContext context, ICurrentUserService currentUserService)
        {
            this.context = context;
            this.currentUserService = currentUserService;
        }




        //BẢN GỐC

        //public async Task SendMessage(string receiverId, string message)
        //{
        //    if (string.IsNullOrWhiteSpace(message))
        //    {
        //        throw new ArgumentException("Message cannot be empty", nameof(message));
        //    }

        //    var nowDate = DateTime.UtcNow;
        //    string senderId = currentUserService.UserId;

        //    var messageToAdd = new MessageModel()
        //    {
        //        Text = message.Trim(), // Đảm bảo không lưu khoảng trắng
        //        Date = nowDate,
        //        SenderId = senderId,
        //        ReceiverId = receiverId,
        //    };

        //    await context.Messages.AddAsync(messageToAdd);
        //    await context.SaveChangesAsync();

        //    List<string> users = new List<string> { receiverId, senderId };

        //    // Gửi tin nhắn real-time đến người nhận
        //    await Clients.Users(users).SendAsync("ReceiveMessage", message, nowDate.ToShortDateString(), nowDate.ToShortTimeString(), senderId);

        //    // 🔥 Gửi sự kiện cập nhật danh sách user real-time
        //    await Clients.User(senderId).SendAsync("UpdateUserList", senderId, message);
        //    await Clients.User(receiverId).SendAsync("UpdateUserList", senderId, message);
        //}



        //LƯU Ý NẾU MUỐN BACK UP LẠI THÌ XÓA ĐOẠN NÀY VÀ DÙNG ĐOẠN TRÊN NHÉ
        public async Task SendMessage(string receiverId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be empty", nameof(message));
            }

            var nowDate = DateTime.UtcNow;
            string senderId = currentUserService.UserId;

            var messageToAdd = new MessageModel()
            {
                Text = message.Trim(), // Đảm bảo không lưu khoảng trắng
                Date = nowDate,
                SenderId = senderId,
                ReceiverId = receiverId,
            };

            await context.Messages.AddAsync(messageToAdd);
            await context.SaveChangesAsync();

            List<string> users = new List<string> { receiverId, senderId };

            // Gửi tin nhắn real-time đến người nhận
            await Clients.Users(users).SendAsync("ReceiveMessage", message, nowDate.ToShortDateString(), nowDate.ToShortTimeString(), senderId);

            // 🔥 Gửi sự kiện cập nhật danh sách user real-time
            // Dành cho người gửi: cập nhật danh sách với đối tượng là người nhận
            await Clients.User(senderId).SendAsync("UpdateUserList", receiverId, message);

            // Dành cho người nhận: cập nhật danh sách với đối tượng là người gửi
            await Clients.User(receiverId).SendAsync("UpdateUserList", senderId, message);

        }


        //LƯU Ý NẾU MUỐN BACK UP LẠI THÌ XÓA ĐOẠN NÀY VÀ DÙNG ĐOẠN TRÊN NHÉ



        //xóa nếu lỗi
        public async Task GetChatHistory(string receiverId)
        {
            var senderId = currentUserService.UserId;
            var messages = await context.Messages
                .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                            (m.SenderId == receiverId && m.ReceiverId == senderId))
                .OrderBy(m => m.Date)
                .Select(m => new
                {
                    Text = m.Text,
                    Date = m.Date.ToShortDateString(),
                    Time = m.Date.ToShortTimeString(),
                    SenderId = m.SenderId
                })
                .ToListAsync();

            await Clients.Caller.SendAsync("LoadChatHistory", messages);
        }






    }
}
