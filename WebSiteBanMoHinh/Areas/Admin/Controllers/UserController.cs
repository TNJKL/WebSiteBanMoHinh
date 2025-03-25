using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Repository;

namespace WebSiteBanMoHinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="ADMIN")]
    public class UserController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(DataContext context, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dataContext = context;
            _userManager = userManager;
            _roleManager = roleManager;

        }
        [HttpGet]
        //public async Task<IActionResult> Index()
        //{
        //    var usersWithRoles = await (from u in _dataContext.Users
        //                                join ur in _dataContext.UserRoles on u.Id equals ur.UserId
        //                                join r in _dataContext.Roles on ur.RoleId equals r.Id
        //                                select new { User = u, RoleName = r.Name })
        //                                .ToListAsync();
        //    return View(usersWithRoles);
        //}
        public async Task<IActionResult> Index()
        {
            var allUsers = await (from u in _dataContext.Users
                                  join ur in _dataContext.UserRoles on u.Id equals ur.UserId into userRoles
                                  from ur in userRoles.DefaultIfEmpty()
                                  join r in _dataContext.Roles on ur.RoleId equals r.Id into roles
                                  from r in roles.DefaultIfEmpty()
                                  select new
                                  {
                                      User = u,
                                      RoleName = r != null ? r.Name : "No Role"
                                  })
                                 .ToListAsync();
            return View(allUsers);
        }
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new AppUserModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AppUserModel user)
        {
            if (ModelState.IsValid)
            {
                AppUserModel newUser = new AppUserModel { UserName = user.UserName, Email = user.Email , PhoneNumber = user.PhoneNumber , PasswordHash = user.PasswordHash , RoleId = user.RoleId};
                var createUserResult = await _userManager.CreateAsync(newUser, newUser.PasswordHash);
                if (createUserResult.Succeeded)
                {
                    var createUser = await _userManager.FindByEmailAsync(user.Email); //tìm user dựa vào email
                    var userId = createUser.Id; // lấy user Id
                    var role = _roleManager.FindByIdAsync(user.RoleId); //lấy RoleId
                                                                        //gán quyền

                    var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Result.Name);

                    if (!addToRoleResult.Succeeded)
                    {
                        foreach (var error in createUserResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }

                    return RedirectToAction("Index", "User");
                }
                else
                {

                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(user);
                }

            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            //var roles = await _roleManager.Roles.ToListAsync();
            //ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);

        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound();
            }
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                return View("Error");
            }
            else
            {
                TempData["success"] = "Đã xóa user thành công";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
                var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(string id, AppUserModel user)
        //{
        //    var existingUser = await _userManager.FindByIdAsync(id);
        //    if(existingUser == null)
        //    {
        //        return NotFound();
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        existingUser.UserName = user.UserName;
        //        existingUser.Email = user.Email;
        //        existingUser.PhoneNumber = user.PhoneNumber;
        //        existingUser.RoleId = user.RoleId;

        //        var updateUserResult = await _userManager.UpdateAsync(existingUser);
        //        if (updateUserResult.Succeeded)
        //        {
        //            return RedirectToAction("Index", "User");
        //        }
        //        else
        //        {
        //            AddIdentityErrors(updateUserResult);
        //            return View(existingUser);
        //        }
        //    }
        //    var roles = await _roleManager.Roles.ToListAsync();
        //    ViewBag.Roles = new SelectList(roles, "Id", "Name");


        //    TempData["error"] = "Model validation failed";
        //    var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
        //    string errorMessage = string.Join("\n", errors);
        //    return View(existingUser);
        //}

        public async Task<IActionResult> Edit(string id, AppUserModel user)
        {
            var existingUser = await _userManager.FindByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Cập nhật thông tin cơ bản của user
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleId = user.RoleId; // Vẫn giữ lại nếu bạn cần lưu trường này

                var updateUserResult = await _userManager.UpdateAsync(existingUser);
                if (updateUserResult.Succeeded)
                {
                    // Xử lý cập nhật role

                    // 1. Lấy tất cả role hiện tại của user
                    var currentRoles = await _userManager.GetRolesAsync(existingUser);

                    // 2. Xóa tất cả role hiện tại
                    await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);

                    // 3. Thêm role mới nếu có
                    if (!string.IsNullOrEmpty(user.RoleId))
                    {
                        // Lấy tên role từ ID
                        var selectedRole = await _roleManager.FindByIdAsync(user.RoleId);
                        if (selectedRole != null)
                        {
                            await _userManager.AddToRoleAsync(existingUser, selectedRole.Name);
                        }
                    }

                    return RedirectToAction("Index", "User");
                }
                else
                {
                    AddIdentityErrors(updateUserResult);
                    return View(existingUser);
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            TempData["error"] = "Model validation failed";
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            string errorMessage = string.Join("\n", errors);

            return View(existingUser);
        }
        private void AddIdentityErrors(IdentityResult identityResult)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

    }
}
