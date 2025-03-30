using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Repository;
using WebSiteBanMoHinh.ViewModels;

namespace WebSiteBanMoHinh.Helpers
{
    public class MessageService : IMessageService
    {
        private readonly DataContext _context;
        private readonly ICurrentUserService _currentUserService;


        private readonly UserManager<AppUserModel> _userManager;



        public MessageService(DataContext context, ICurrentUserService currentUserService, UserManager<AppUserModel> userManager)
        {
            _context = context;
            _currentUserService = currentUserService;
            _userManager = userManager;
        }

        public async Task<ChatViewModel> GetMessages(string selectedUserId)
        {
            var currentUserId = _currentUserService.UserId;

            var selectedUser = await _context.Users.FirstOrDefaultAsync(i => i.Id == selectedUserId);
            var selectedUserName = "";
            if (selectedUser != null)
            {
                selectedUserName = selectedUser.UserName;
            }

            var chatViewModel = new ChatViewModel()
            {
                CurrentUserId = currentUserId,
                ReceiverId = selectedUserId,
                ReceiverUserName = selectedUserName,
            };

            var messages = await _context.Messages.Where(i => (i.SenderId == currentUserId || i.SenderId == selectedUserId) && (i.ReceiverId == currentUserId || i.ReceiverId == selectedUserId)).Select(i => new UserMessagesListViewModel()
            {
                Id = i.Id,
                Text = i.Text,
                Date = i.Date.ToShortDateString(),
                Time = i.Date.ToShortTimeString(),
                IsCurrentUserSentMessage = i.SenderId == currentUserId,
            }).ToListAsync();

            chatViewModel.Messages = messages;

            return chatViewModel;
        }


        //public async Task<IEnumerable<MessagesUsersListViewModel>> GetUsers()
        //{
        //    var currentUserId = _currentUserService.UserId;

        //    var users = await _context.Users.Where(i => i.Id != currentUserId).Select(i => new MessagesUsersListViewModel()
        //    {
        //        Id = i.Id,
        //        UserName = i.UserName,
        //        LastMessage = _context.Messages.Where(m => (m.SenderId == currentUserId || m.SenderId == i.Id) && (m.ReceiverId == currentUserId || m.ReceiverId == i.Id)).OrderByDescending(m => m.Id).Select(m => m.Text).FirstOrDefault()
        //    }).ToListAsync();

        //    return users;
        //}


        //BẢN GỐC NẾU CẦN CỨ BACKUP


        //public async Task<IEnumerable<MessagesUsersListViewModel>> GetUsers()
        //{
        //    var currentUserId = _currentUserService.UserId;
        //    var currentUser = await _userManager.FindByIdAsync(currentUserId);

        //    if (currentUser == null)
        //    {
        //        return new List<MessagesUsersListViewModel>();
        //    }

        //    // Lấy vai trò của người dùng hiện tại
        //    var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

        //    IQueryable<AppUserModel> query = _context.Users.Where(u => u.Id != currentUserId);

        //    if (currentUserRoles.Contains("CSKH"))
        //    {
        //        // Nếu là CSKH, hiển thị tất cả user có role CUSTOMER hoặc không có role
        //        query = query.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == _context.Roles.FirstOrDefault(r => r.Name == "CUSTOMER").Id)
        //                                 || !_context.UserRoles.Any(ur => ur.UserId == u.Id));
        //    }
        //    else if (currentUserRoles.Contains("CUSTOMER"))
        //    {
        //        // Nếu là CUSTOMER, chỉ hiển thị người có role CSKH
        //        query = query.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == _context.Roles.FirstOrDefault(r => r.Name == "CSKH").Id));
        //    }

        //    var users = await query.Select(u => new MessagesUsersListViewModel
        //    {
        //        Id = u.Id,
        //        UserName = u.UserName,
        //        LastMessage = _context.Messages
        //            .Where(m => (m.SenderId == currentUserId || m.SenderId == u.Id) &&
        //                        (m.ReceiverId == currentUserId || m.ReceiverId == u.Id))
        //            .OrderByDescending(m => m.Id)
        //            .Select(m => m.Text)
        //            .FirstOrDefault()
        //    }).ToListAsync();

        //    return users;
        //}

        public async Task<IEnumerable<MessagesUsersListViewModel>> GetUsers()
        {
            var currentUserId = _currentUserService.UserId;
            var currentUser = await _userManager.FindByIdAsync(currentUserId);

            if (currentUser == null)
            {
                return new List<MessagesUsersListViewModel>();
            }

            // Lấy vai trò của người dùng hiện tại
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<AppUserModel> query = _context.Users.Where(u => u.Id != currentUserId);

            if (currentUserRoles.Contains("CSKH"))
            {
                // Nếu là CSKH, hiển thị tất cả user có role CUSTOMER hoặc không có role
                query = query.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id &&
                             ur.RoleId == _context.Roles.FirstOrDefault(r => r.Name == "CUSTOMER").Id)
                             || !_context.UserRoles.Any(ur => ur.UserId == u.Id));
            }
            else if (currentUserRoles.Contains("CUSTOMER"))
            {
                // Nếu là CUSTOMER, chỉ hiển thị người có role CSKH
                query = query.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id &&
                             ur.RoleId == _context.Roles.FirstOrDefault(r => r.Name == "CSKH").Id));
            }

            var users = await query.Select(u => new MessagesUsersListViewModel
            {
                Id = u.Id,
                UserName = u.UserName,
                LastMessage = _context.Messages
                    .Where(m => (m.SenderId == currentUserId || m.SenderId == u.Id) &&
                                (m.ReceiverId == currentUserId || m.ReceiverId == u.Id))
                    .OrderByDescending(m => m.Id)
                    .Select(m => m.Text)
                    .FirstOrDefault(),
                LastMessageDate = _context.Messages
                    .Where(m => (m.SenderId == currentUserId || m.SenderId == u.Id) &&
                                (m.ReceiverId == currentUserId || m.ReceiverId == u.Id))
                    .OrderByDescending(m => m.Id)
                    .Select(m => m.Date)
                    .FirstOrDefault()
            })
            // Sắp xếp theo LastMessageDate giảm dần (tin nhắn mới nhất đứng đầu)
            .OrderByDescending(u => u.LastMessageDate)
            .ToListAsync();

            return users;
        }



    }

}

