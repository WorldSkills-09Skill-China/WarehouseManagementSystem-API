using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagement.EFCore;

namespace WarehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypesController : ControllerBase
    {
        //显示物品类型列表
        [Route("showAllType")]
        [HttpGet]
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
    }
}
