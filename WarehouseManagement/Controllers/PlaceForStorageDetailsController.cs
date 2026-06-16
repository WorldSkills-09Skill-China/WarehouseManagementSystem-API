using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManagement.EFCore;

namespace WarehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaceForStorageDetailsController : ControllerBase
    {
        //显示所有的位置列表
        [Route("showAllLocation")]
        [HttpGet]
        public IActionResult ShowAllLocation()
        {
            var ctx = new DB();
            try
            {
                var locationInfo = ctx.PlaceForStorageDetails.Include(u => u.PlaceForStorage)
                    .ToList()
                               .Select(u => new
                               {
                                   u.Id,
                                   Name = u.PlaceForStorage.Name + "-" + u.Name
                               }).ToList();
                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = locationInfo
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

        //显示位置对应的图片
        [Route("showLocationImage")]
        [HttpPost]
        public IActionResult ShowLocationImage([FromBody] int id)
        {
            var ctx = new DB();
            try
            {
                var locationInfo = ctx.PlaceForStorageDetails.Include(u =>u.PlaceForStorage)
                    .ToList()
                     .FirstOrDefault(u => u.Id == id);
                if (locationInfo == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "is null",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null
                    });
                }
                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = locationInfo.PlaceForStorage.Image
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
