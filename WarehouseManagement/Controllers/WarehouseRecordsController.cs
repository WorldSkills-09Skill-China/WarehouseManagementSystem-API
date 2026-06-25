using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.VisualBasic;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Xml.Linq;
using WarehouseManagement.Controllers;
using WarehouseManagement.EFCore;
using WarehouseManagement.WarehouseRecordsClass;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WarehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseRecordsController : ControllerBase
    {
        /// <summary>
        /// 显示出库和入库的对应数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("showDeliveryAndStorageCount")]
        public IActionResult ShowDeliveryAndStorageCount()
        {
            try
            {
                var ctx = new DB();
                var deliveryCount = ctx.WarehouseRecords.Include(u => u.RecordType)
                    .Where(u => u.RecordTypeId == 1 && u.RecordStateId == 3).Count();

                var storageCount = ctx.WarehouseRecords.Include(u => u.RecordType)
                   .Where(u => u.RecordStateId == 2 && u.RecordStateId == 3).Count();

                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = new
                    {
                        delivery = deliveryCount,
                        storage = storageCount
                    }
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
        }

        /// <summary>
        /// 显示总的出入库记录
        /// </summary>
        /// <returns></returns>
        [HttpGet("showTotalRecord")]
        public IActionResult ShowTotalRecord()
        {
            var ctx = new DB();
            try
            {
                var totalInfo = ctx.WarehouseRecords.Include(u => u.RecordType)
                  .Include(u => u.RecordState)
                  .Include(u => u.Item)
                  .Include(u => u.User)
                  .Include(u => u.PlaceForStorageDetail)
                  .ThenInclude(u => u.PlaceForStorage)
                  .Where(u => u.RecordStateId == 3)
                  .Select(u => new
                  {
                      u.Id,
                      Type = u.RecordType.Name,
                      ItemId = u.ItemId,
                      ItemName = u.Item.Name,
                      Count = u.ItemCount,
                      Note = u.Note,
                      CreateTime = u.CreateTime,
                      EndTime = u.EndTime,
                      FinishedTime = u.FinishedTime,
                      UserId = u.UserId,
                      u.PlaceForStorageDetailId,
                      PlaceForStorageDetailName = u.PlaceForStorageDetail.PlaceForStorage.Name + "-" + u.PlaceForStorageDetail.Name,
                      RecordState = u.RecordState.Name,
                      UserName = u.UserId == null ? null : u.User.Name,
                      ItemTypeId = u.Item.ItemTypeId,
                  }).ToList();

                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = totalInfo
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

        /// <summary>
        /// 显示未完成的任务
        /// </summary>
        /// <returns></returns>
        [HttpGet("showUnFinishedTask")]
        public IActionResult ShowUnFinishedTask()
        {
            var ctx = new DB();
            var unfinishedInfo = ctx.WarehouseRecords.Include(u => u.RecordType)
                    .Include(u => u.RecordState)
                    .Include(u => u.Item)
                    .Include(u => u.PlaceForStorageDetail)
                    .Where(u => u.RecordStateId == 1 && u.RecordStateId == 3)
                    .Select(u => new
                    {
                        u.Id,
                        Type = u.RecordType.Name,
                        ItemId = u.ItemId,
                        ItemName = u.Item.Name,
                        Count = u.ItemCount,
                        Note = u.Note,
                        CreateTime = u.CreateTime,
                        EndTime = u.EndTime,
                        FinishedTime = u.FinishedTime,
                        UserId = u.UserId,
                        u.PlaceForStorageDetailId,
                        RecordState = u.RecordState.Name,
                    }).ToList();

            return Ok(new
            {
                Success = true,
                Message = "",
                Timestamp = DateTime.UtcNow,
                Data = unfinishedInfo
            });
        }

        /// <summary>
        /// 添加新的记录
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        [HttpPost("addRecord")]
        public IActionResult AddRecord([FromBody] RecordData record)
        {
            var ctx = new DB();
            if (record.RecordTypeId == 1)
            {
                if (ctx.WarehouseRecords.ToList().Totallize() < record.ItemCount)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"库存数量不足{record.ItemCount}",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null
                    });
                }
            }
            try
            {
                var addInfo = new WarehouseRecord
                {
                    RecordTypeId = record.RecordTypeId,
                    ItemId = record.ItemId,
                    ItemCount = record.ItemCount,
                    Note = record.Note,
                    CreateTime = record.CreateTime,
                    EndTime = record.EndTime,
                    RecordStateId = 1,
                    UserId = record.UserId == -1 ? null : record.UserId,
                    PlaceForStorageDetailId = record.PlaceForStorageDetailId,
                };
                ctx.WarehouseRecords.Add(addInfo);
                ctx.SaveChanges();
                if (ctx.Items.ToList().FirstOrDefault(u => u.Id == record.ItemId).IsFixedAssets == true)
                {
                    for (int i = 0; i < addInfo.ItemCount; i++)
                    {
                        var fixInfo = new FixedAsset
                        {
                            Code = DateTime.Now.Date.ToString("yyyyMMdd") + ctx.FixedAssets.OrderByDescending(u => u.Id).FirstOrDefault() == null ? 1.ToString("D6") : ctx.FixedAssets.OrderByDescending(u => u.Id).FirstOrDefault().Id.ToString("D6"),
                            ItemId = record.ItemId,
                            IsDelete = false
                        };
                        ctx.FixedAssets.Add(fixInfo);
                        ctx.SaveChanges();
                        ctx.AssetHistories.Add(new AssetHistory
                        {
                            FixedAssetId = fixInfo.Id,
                            PlaceForStorageDetailId = addInfo.PlaceForStorageDetailId,
                            UserId = record.UserId == null ? null : record.UserId,
                            Note = record.Note,
                            OperationTime = record.CreateTime,
                        });
                        ctx.SaveChanges();
                    }
                }
                return Ok(new
                {
                    Success = true,
                    Message = "添加成功",
                    Timestamp = DateTime.UtcNow,
                    Data = addInfo.Id
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "添加失败",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
        }

        /// <summary>
        /// 更改记录
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        [HttpPost("editRecord")]
        public IActionResult EditRecord([FromBody] RecordData record)
        {
            var ctx = new DB();
            try
            {
                if (ctx.WarehouseRecords.Where(u => u.RecordTypeId == 2).Sum(u => u.ItemCount) - ctx.WarehouseRecords.Where(u => u.RecordTypeId == 1).Sum(u => u.ItemCount) < record.ItemCount)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"库存数量不足{record.ItemCount}",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null
                    });
                }

                var recordInfo = ctx.WarehouseRecords.ToList()
                    .FirstOrDefault(u => u.Id == record.Id);

                if (recordInfo == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = " is null",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null
                    });
                }

                recordInfo.RecordStateId = record.RecordStateId;
                recordInfo.ItemId = record.ItemId;
                recordInfo.UserId = record.UserId == -1 ? null : record.UserId;
                recordInfo.ItemCount = record.ItemCount;
                recordInfo.Note = record.Note;
                recordInfo.CreateTime = record.CreateTime;
                recordInfo.EndTime = record.EndTime;
                recordInfo.FinishedTime = record.FinishedTime;
                recordInfo.RecordStateId = 3;
                recordInfo.PlaceForStorageDetailId = record.PlaceForStorageDetailId;
                ctx.SaveChanges();

                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
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

        /// <summary>
        /// 申请对应的记录
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        [HttpPost("applicationRecord")]
        public IActionResult ApplicationRecord([FromBody] RecordData record)
        {
            var ctx = new DB();
            if (record.RecordTypeId == 2)
            {
                try
                {
                    ctx.WarehouseRecords.Add(new WarehouseRecord
                    {
                        RecordTypeId = 2,
                        ItemId = record.ItemId,
                        ItemCount = record.ItemCount,
                        Note = record.Note,
                        CreateTime = record.CreateTime,
                        EndTime = record.EndTime,
                        RecordStateId = 4,
                        UserId = record.UserId == -1 ? null : record.UserId,
                        PlaceForStorageDetailId = record.PlaceForStorageDetailId,
                    });
                    ctx.SaveChanges();
                    return Ok(new
                    {
                        Success = true,
                        Message = "申请成功",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null
                    });
                }
                catch
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "申请失败",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null
                    });
                }
            }

            if (ctx.Items.Where(u => u.Id == record.ItemId).Count() < record.ItemCount)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"库存数量不足{record.ItemCount}",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null

                });
            }
            try
            {
                ctx.WarehouseRecords.Add(new WarehouseRecord
                {
                    RecordTypeId = 1,
                    ItemId = record.ItemId,
                    ItemCount = record.ItemCount,
                    Note = record.Note,
                    CreateTime = record.CreateTime,
                    EndTime = record.EndTime,
                    FinishedTime = record.FinishedTime,
                    RecordStateId = 4,
                    UserId = record.UserId,
                    PlaceForStorageDetailId = record.PlaceForStorageDetailId,
                });
                ctx.SaveChanges();
                return Ok(new
                {
                    Success = true,
                    Message = "申请成功",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
            catch
            {
                return Ok(new
                {
                    Success = true,
                    Message = "申请失败",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("deleteTheTask")]
        public IActionResult DeleteTheTask([FromBody] int id)
        {
            var ctx = new DB();
            var deleteInfo = ctx.WarehouseRecords.ToList()
                .FirstOrDefault(u => u.Id == id);
            deleteInfo.RecordStateId = 3;
            ctx.SaveChanges();
            return Ok(new
            {
                Success = true,
                Message = "",
                Timestamp = DateTime.UtcNow,
                Data = (string)null
            });
        }

        /// <summary>
        /// 批准任务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("approvalTask")]
        public IActionResult ApprovalTask([FromBody] int id)
        {
            var ctx = new DB();

            var approvalInfo = ctx.WarehouseRecords.ToList()
                .FirstOrDefault(u => u.Id == id);
            approvalInfo.RecordStateId = 2;

            ctx.SaveChanges();
            return Ok(new
            {
                Success = true,
                Message = "",
                Timestamp = DateTime.UtcNow,
                Data = (string)null
            });
        }

        /// <summary>
        /// 显示任务详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("showTaskDetail")]
        public IActionResult showTaskDetail([FromBody] int id)
        {
            var ctx = new DB();
            try
            {
                var taskDetailInfo = ctx.WarehouseRecords
                    .Include(u => u.RecordType)
                    .Include(u => u.Item)
                    .Include(u => u.User)
                    .Include(u => u.PlaceForStorageDetail).ToList()
                .FirstOrDefault(u => u.Id == id && u.RecordStateId == 3);
                if (taskDetailInfo == null)
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
                    Data = new
                    {
                        RecordType = taskDetailInfo.RecordType.Name,
                        Item = taskDetailInfo.Item.Name,
                        ItemCount = taskDetailInfo.ItemCount,
                        Note = taskDetailInfo.Note,
                        CreateTime = taskDetailInfo.CreateTime,
                        EndTime = taskDetailInfo.EndTime,
                        User = taskDetailInfo.User == null ? null : taskDetailInfo.User.Name,
                        PlaceForStorageDetail = taskDetailInfo.PlaceForStorageDetail.Name,
                    }
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

        /// <summary>
        /// 显示最近的50条数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("show50Record")]
        public IActionResult Show50Record()
        {
            var ctx = new DB();
            try
            {
                var taskDetailInfo = ctx.WarehouseRecords
                    .Include(u => u.RecordType)
                    .Include(u => u.Item)
                    .Include(u => u.RecordState)
                    .Include(u => u.User)
                    .Include(u => u.PlaceForStorageDetail).ToList()
                    .OrderByDescending(u => u.CreateTime)
                .Where(u => u.RecordStateId != 3)
                .Take(50)
                .Select(u => new
                {
                    u.Id,
                    Type = u.RecordType.Name,
                    Item = u.ItemId,
                    ItemName = u.Item.Name,
                    Count = u.ItemCount,
                    Note = u.Note,
                    CreateTime = u.CreateTime,
                    EndTime = u.EndTime,
                    FinishedTime = u.FinishedTime,
                    UserName = u.User == null ? null : u.User.Name,
                    PlaceForStorageDetail = u.PlaceForStorageDetail.Name,
                    RecordState = u.RecordState.Name,
                }).ToList();

                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = taskDetailInfo
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

        /// <summary>
        /// 显示查找的记录
        /// </summary>
        /// <param name="searchData"></param>
        /// <returns></returns>
        [HttpPost("showSearchRecord")]
        public IActionResult ShowSearchRecord([FromBody] SearchData searchData)
        {
            var ctx = new DB();

            var searchInfo = ctx.WarehouseRecords
                .Include(u => u.RecordType)
                .Include(u => u.Item)
                .Include(u => u.RecordState)
                .Include(u => u.User)
                .Include(u => u.PlaceForStorageDetail)
                .ThenInclude(u => u.PlaceForStorage).ToList()
                .Where(u => u.RecordStateId != 3 && searchData.UserId == 0 ? true :
                u.UserId == searchData.UserId && searchData.ItemId == 0 ? true :
                u.ItemId == searchData.ItemId && searchData.PlaceForStorageId == 0 ? true :
                u.PlaceForStorageDetailId == searchData.PlaceForStorageId && (string.IsNullOrEmpty(searchData.StartTime) ||
                 u.CreateTime >= Convert.ToDateTime(searchData.StartTime)) && (string.IsNullOrEmpty(searchData.EndTime) ||
                 u.CreateTime <= Convert.ToDateTime(searchData.EndTime))
                )
                .Select(u => new
                {
                    Type = u.RecordType.Name,
                    Item = u.Item.Name,
                    Count = u.ItemCount,
                    CreateTime = u.CreateTime,
                    EndTime = u.EndTime,
                    User = u.User == null ? null : u.User.Name,
                    PlaceForStorage = u.PlaceForStorageDetail.PlaceForStorage.Name,
                    Batch = u.Batch
                }).ToList();
            return Ok(new
            {
                Success = true,
                Message = "",
                Timestamp = DateTime.UtcNow,
                Data = searchInfo
            });


            return BadRequest(new
            {
                Success = true,
                Message = "is null",
                Timestamp = DateTime.UtcNow,
                Data = (string)null
            });
        }

        /// <summary>
        /// 显示记录详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("showRecordDetail")]
        public IActionResult ShowRecordDetail([FromBody] int id)
        {
            var ctx = new DB();
            try
            {
                var recordInfo = ctx.WarehouseRecords.Include(u => u.Item)
                   .Include(u => u.PlaceForStorageDetail)
                   .Include(u => u.User)
                   .Include(u => u.RecordState)
                   .Include(u => u.RecordType)
                   .FirstOrDefault(u => u.RecordStateId != 3 && u.Id == id);
                if (recordInfo == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null
                    });
                }
                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = new
                    {
                        Id = recordInfo.Id,
                        ItemId = recordInfo.ItemId,
                        ItemCount = recordInfo.ItemCount,
                        PlaceForStorageDetailId = recordInfo.PlaceForStorageDetailId,
                        RecordTypeId = recordInfo.RecordTypeId,
                        RecordStateId = recordInfo.RecordStateId,
                        EndTime = recordInfo.EndTime,
                        CreateTime = recordInfo.CreateTime,
                        FinishedTime = recordInfo.FinishedTime,
                        UserId = recordInfo.UserId,
                        Note = recordInfo.Note,
                    }
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
        }

        /// <summary>
        /// 显示我的未完成任务
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("showMyUnfinishedTasks")]
        public IActionResult ShowMyUnfinishedTasks([FromBody] int userId)
        {
            var ctx = new DB();
            try
            {
                var unfinishedInfo = ctx.WarehouseRecords.Include(u => u.RecordType)
                 .Include(u => u.RecordState)
                 .Include(u => u.Item)
                 .Include(u => u.PlaceForStorageDetail)
                 .Where(u => u.RecordStateId == 1 && u.RecordStateId != 3 && u.UserId == userId)
                 .Select(u => new
                 {
                     RecordId = u.Id,
                     ItemName = u.Item.Name,
                     ItemType = u.RecordType,
                     ItemCount = u.ItemCount,

                 }).ToList();

                return Ok(new
                {
                    Success = true,
                    Message = "",
                    TimeStamp = DateTime.UtcNow,
                    Data = unfinishedInfo
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "",
                    TimeStamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
        }

        /// <summary>
        /// 录入数据记录
        /// </summary>
        /// <returns></returns>
        [HttpPost("showEnterDataRecord")]
        public IActionResult ShowEnterDataRecord()
        {
            var ctx = new DB();
            try
            {
                var recordInfo = ctx.WarehouseRecords.Include(u => u.RecordType)
                 .Include(u => u.RecordState)
                 .Include(u => u.Item)
                 .Include(u => u.PlaceForStorageDetail)
                 .Where(u => u.RecordStateId != 1 && u.UserId == null && u.RecordStateId != 3)
                 .Select(u => new
                 {
                     RecordId = u.Id,
                     ItemName = u.Item.Name,
                     ItemType = u.RecordType,
                     ItemCount = u.ItemCount,

                 }).ToList();

                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = recordInfo
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
        }

        /// <summary>
        /// 显示所有的待审批任务
        /// </summary>
        /// <returns></returns>
        [HttpGet("displayAllTaskAwaitingReview")]
        public IActionResult DisplayAllTaskAwaitingReview()
        {
            var ctx = new DB();
            try
            {
                var reviewTask = ctx.WarehouseRecords
                    .Include(u => u.User)
                    .Include(u => u.PlaceForStorageDetail)
                    .ThenInclude(u => u.PlaceForStorage)
                    .Include(u => u.Item)
                    .ToList()
                    .Where(u => u.RecordStateId == 4)
                    .Select(u => new
                    {
                        u.Id,
                        Type = u.RecordType,
                        ItemName = u.Item.Name,
                        Count = u.ItemCount,
                        PlaceForStorage = u.PlaceForStorageDetail == null ? null : u.PlaceForStorageDetail.PlaceForStorage.Name + "-" + u.PlaceForStorageDetail.Name,
                        User = u.User == null ? null : u.User.Name,
                        EndTime = u.EndTime.ToString(),
                        Unit = u.Item.Unit
                    }).ToList();

                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = reviewTask
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = -1
                });
            }
        }


        //更新异常数据
        //public void UpdateAbnormalData()
        //{
        //    var ctx = new DB();
        //    try
        //    {
        //        var abnormalInfo = ctx.WarehouseRecords.Include(u => u.Item)
        //            .Include(u => u.RecordType)
        //            .Include(u => u.RecordState)
        //            .Include(u => u.User)
        //            .Include(u => u.PlaceForStorageDetail)
        //            .ThenInclude(u => u.PlaceForStorage)
        //            .Include(u => u.FixedAssets)
        //            .Where(u => u.IsDelete == false)
        //            .GroupBy(u => u.Item)
        //            .Select(u => new
        //            {
        //                u.Key,
        //                TotalCount = u.Sum(u => u.ItemCount),
        //                SafeStock = ctx.Items.ToList().FirstOrDefault(c => c.Id == u.Key.Id).SafetyInventory
        //            }).ToList();

        //        foreach (var item in abnormalInfo)
        //        {
        //            if (item.TotalCount < item.SafeStock)
        //            {
        //                ctx.ItemAndStates.Add(new ItemAndState
        //                {
        //                    ItemId = item.Key.Id,
        //                    HazardRecordDetailId = 1,
        //                    HazardStateId = 1
        //                });
        //                ctx.SaveChanges();
        //            }

        //            if (ctx.WarehouseRecords.ToList().OrderBy(u =>u.CreateTime).FirstOrDefault(u => u.ItemId == item.Key.Id).CreateTime >= DateTime.Now)
        //            {
        //                ctx.WarehouseRecords.ToList().Add(new WarehouseRecord
        //                {
        //                    //RecordState =
        //                });
        //            }
        //        }

        //    }
        //    catch
        //    {
        //        return;
        //    }
        //}
    }
}




