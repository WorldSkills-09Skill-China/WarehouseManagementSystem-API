using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using WarehouseManagement.EFCore;

namespace WarehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordStatesController : ControllerBase
    {
        /// <summary>
        /// 显示记录状态列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("showRecordStates")]
        public IActionResult ShowRecordStates()
        {
            var ctx = new DB();
            try
            {
                var typeInfo = ctx.ReocordStates.ToList()
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
                    Data = typeInfo
                });
            }
            catch
            {
                return Ok(new
                {
                    Success = false,
                    Message = "is null",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
        }
    }
}
