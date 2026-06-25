using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using WarehouseManagement.EFCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WarehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginData login)
        {
            var ctx = new DB();
            var loginInfo = ctx.Users.Include(u => u.Role)
                .ToList()
                .FirstOrDefault(u => u.Name == login.Name && u.Password == login.Password);
            if (loginInfo == null)
            {
                if (!ctx.Users.ToList().Any(u => u.Name == login.Name))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "用户名错误",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null,
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "密码错误",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null
                    });
                }
            }
            return Ok(new
            {
                Success = true,
                Message = "",
                Timestamp = DateTime.UtcNow,
                Data = new
                {
                    Id = loginInfo.Id,
                    RoleId = loginInfo.RoleId,
                    UserName = loginInfo.Name
                }
            });
        }

        /// <summary>
        /// 用户列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("showUserList")]
        public IActionResult ShowUserList()
        {
            var ctx = new DB();
            var userList = ctx.Users.ToList()
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                }).ToList();
            return Ok(new
            {
                Success = true,
                Message = "",
                Timestamp = DateTime.UtcNow,
                Data = userList
            });
        }
    }
}
