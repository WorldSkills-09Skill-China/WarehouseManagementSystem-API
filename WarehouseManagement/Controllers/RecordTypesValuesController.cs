using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using WarehouseManagement.EFCore;

namespace WarehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecordTypesValuesController : ControllerBase
    {
        /// <summary>
        /// 显示记录类型
        /// </summary>
        /// <returns></returns>
        [HttpGet("showRecordType")]
        public IActionResult ShowRecordType()
        {
            try
            {
                var ctx = new DB();
                var typeIngfo = ctx.RecordTypes.ToList()
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
                    Data = typeIngfo
                });
            }
            catch
            {
                return BadRequest(new
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
