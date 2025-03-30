using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebSiteBanMoHinh.Helpers;
using WebSiteBanMoHinh.Repository;
using WebSiteBanMoHinh.ViewModels;

namespace WebSiteBanMoHinh.Controllers
{
    [Authorize(Roles = "CSKH")]
    public class MessagesController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly DataContext _context;
        private readonly ICurrentUserService _currentUserService;
        public MessagesController(IMessageService messageService, DataContext context, ICurrentUserService currentUserService)
        {
            _messageService = messageService;
            _context = context;
            _currentUserService = currentUserService;
        }
        //BẢN GỐC LỖI THÌ BACKUP
        //public async Task<IActionResult> Index()
        //{
        //    var users = await _messageService.GetUsers();
        //    return View(users);
        //}

        //BẢN NHÁP ĐỂ TEST LỖI THÌ LẤY BẢN GỐC Ở TRÊN
        public async Task<IActionResult> Index()
        {
            var users = await _messageService.GetUsers();
            // Sắp xếp danh sách user theo LastMessageDate giảm dần (tin nhắn mới nhất đứng đầu)
            var sortedUsers = users.OrderByDescending(u => u.LastMessageDate).ToList();
            return View(sortedUsers);
        }

        public async Task<IActionResult> Chat(string selectedUserId)
        {
            var chatViewModel = await _messageService.GetMessages(selectedUserId);
            return View(chatViewModel);
        }

        //public async Task<IActionResult> Messages(string selectedUserId)
        //{
        //    // Lấy danh sách người dùng
        //    var users = await _messageService.GetUsers();

        //    // Lấy dữ liệu chat (nếu có selectedUserId)
        //    ChatViewModel chat = null;
        //    if (!string.IsNullOrEmpty(selectedUserId))
        //    {
        //        chat = await _messageService.GetMessages(selectedUserId);
        //    }

        //    // Gộp dữ liệu vào CombinedMessagesViewModel
        //    var model = new CombinedMessagesViewModel
        //    {
        //        Users = users,
        //        Chat = chat
        //    };

        //    return View(model);
        //}

        public async Task<IActionResult> Messages(string selectedUserId)
        {
            var users = await _messageService.GetUsers();
            var currentUserId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(selectedUserId))
            {
                var roles = await _context.UserRoles
                    .Where(ur => ur.UserId == currentUserId)
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .ToListAsync();

                if (roles.Contains("CUSTOMER"))
                {
                    var cskhUser = users.FirstOrDefault(u => u.UserName.Contains("CSKH"));
                    selectedUserId = cskhUser?.Id;
                }
                else if (roles.Contains("CSKH"))
                {
                    selectedUserId = users.FirstOrDefault()?.Id;
                }
            }

            ChatViewModel chat = null;
            if (!string.IsNullOrEmpty(selectedUserId))
            {
                chat = await _messageService.GetMessages(selectedUserId);
                // Đặt ViewData cho CSKH
                if (User.IsInRole("CSKH"))
                {
                    var selectedUser = await _context.Users.FindAsync(selectedUserId);
                    ViewData["SelectedUserId"] = selectedUserId;
                    ViewData["SelectedUserName"] = selectedUser?.UserName ?? "Khách hàng";
                }
            }
            else
            {
                chat = new ChatViewModel
                {
                    CurrentUserId = currentUserId,
                    ReceiverId = null,
                    ReceiverUserName = "Chưa chọn người nhận",
                    Messages = new List<UserMessagesListViewModel>()
                };
            }

            var model = new CombinedMessagesViewModel
            {
                Users = users,
                Chat = chat
            };

            return View(model);
        }


    }
}
