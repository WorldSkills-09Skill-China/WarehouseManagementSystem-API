using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.EFCore;

namespace WarehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypesController : ControllerBase
    {
        /// <summary>
        /// 显示物品类型列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("showAllType")]
        public IActionResult ShowAllType()
        {
            var ctx = new DB();
            var typeList = ctx.ItemTypes.ToList()
                .Select(u => new
                {
                    u.Id,
                    u.Name
                }).ToList();
            return Ok(new
            {
                Success = true,
                Message = "",
                Timestamp = DateTime.UtcNow,
                Data = typeList
            });
        }

        /// <summary>
        /// 显示对应类型的物品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("displayTypeItem")]
        public IActionResult DisplayTypeItem([FromBody] int id)
        {
            var ctx = new DB();
            try
            {
                var itemList = ctx.Items.ToList()
                .Where(u => u.ItemTypeId == id)
                .Select(u => new
                {
                    u.Id,
                    u.Name
                }).ToList();
                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = itemList
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = ""
                });
            }
        }
    }
}
