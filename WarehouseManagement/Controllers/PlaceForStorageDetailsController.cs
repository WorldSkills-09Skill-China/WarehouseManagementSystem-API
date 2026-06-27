using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using WarehouseManagement.EFCore;

namespace WarehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaceForStorageDetailsController : ControllerBase
    {
        /// <summary>
        /// 显示所有的位置列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("showAllLocation")]
        public IActionResult ShowAllLocation(int id, int batch)
        {
            var ctx = new DB();
            try
            {
                var locationInfo = ctx.PlaceForStorageDetails
                    .Include(u => u.PlaceForStorage)
                    .Include(u => u.WarehouseRecords)
                    .ToList()
                    .Where(u => (u.WarehouseRecords.Any(b => b.ItemId == id) || id == -1) && (u.WarehouseRecords.Any(b => b.Batch == batch) || batch == -1))
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


        //[HttpPost("showStatePlace")]
        //public IActionResult ShowStatePlace([FromBody] int id)
        //{
        //    var ctx = new DB();
        //    try
        //    {
        //        var placeInfo = ctx.pla
        //    }
        //    catch
        //    {

        //    }
        //}

        



        /// <summary>
        /// 显示位置对应的图片
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("showLocationImage")]
        public IActionResult ShowLocationImage([FromBody] int id)
        {
            var ctx = new DB();
            try
            {
                var locationInfo = ctx.PlaceForStorageDetails.Include(u => u.PlaceForStorage)
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
