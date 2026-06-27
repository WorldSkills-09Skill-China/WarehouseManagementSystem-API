using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
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
                    .Where(u => u.RecordTypeId == 1).Count();

                var storageCount = ctx.WarehouseRecords.Include(u => u.RecordType)
                   .Where(u => u.RecordStateId == 2).Count();

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
                    .Where(u => u.RecordStateId == 1)
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
            var warehouseRecordFixedAssets = ctx.FixedAssets.Include(a => a.WarehouseRcord)
                   .Include(a => a.AssetHistories)
                   .ToList()
                   .Where(a =>
                   a.WarehouseRcordId != null
                   && a.WarehouseRcord.Batch == record.Batch
                   && a.ItemId == record.ItemId
                   && !a.IsDelete
                   && (a.AssetHistories != null && (a.AssetHistories.ToList().OrderBy(b => b.OperationTime).LastOrDefault()?.WarehouseRecordId != null
                   && a.AssetHistories.ToList().OrderBy(b => b.OperationTime).LastOrDefault()?.UserId == null))).ToList();
            if (record.RecordTypeId == 1 && ctx.Items.ToList().FirstOrDefault(u => u.Id == record.ItemId).IsFixedAssets == true)
            {
                if (warehouseRecordFixedAssets.Count < record.ItemCount)
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
            if (record.RecordTypeId == 1)
            {
                if (ctx.WarehouseRecords.Where(u => u.RecordTypeId == 2
                && u.PlaceForStorageDetailId == record.PlaceForStorageDetailId
                && u.Batch == u.Batch).Sum(u => u.ItemCount)
                -
                ctx.WarehouseRecords.Where(u => u.RecordTypeId == 1
                && u.PlaceForStorageDetailId == record.PlaceForStorageDetailId
                && u.Batch == u.Batch).Sum(u => u.ItemCount) < record.ItemCount)
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
                var maxBatchRecord = ctx.WarehouseRecords.ToList()
                    .Where(a => a.ItemId == record.ItemId).ToList()
                    .OrderBy(a => a.Batch).ToList()
                    .LastOrDefault();
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
                    Batch = record.Batch != -1 ? record.Batch : maxBatchRecord == null ? 1 : maxBatchRecord.Batch,
                };

                var batchFixedAssets = ctx.FixedAssets.Include(a => a.WarehouseRcord)
                   .ToList()
                   .Where(a =>
                   a.WarehouseRcordId != null
                   && a.WarehouseRcord.Batch == record.Batch
                   && a.ItemId == record.ItemId
                   && !a.IsDelete
                   && (a.AssetHistories != null && (a.AssetHistories.ToList().OrderBy(b => b.OperationTime).LastOrDefault()?.WarehouseRecordId == null
                   && a.AssetHistories.ToList().OrderBy(b => b.OperationTime).LastOrDefault()?.UserId == null))).ToList();

                ctx.WarehouseRecords.Add(addInfo);
                ctx.SaveChanges();
                if (ctx.Items.ToList().FirstOrDefault(u => u.Id == record.ItemId).IsFixedAssets == true)
                {
                    if (record.IsUserExistingItems)
                    {
                        for (int i = 0; i < record.ItemCount; i++)
                        {
                            ctx.AssetHistories.Add(new AssetHistory
                            {
                                FixedAssetId = batchFixedAssets[i].Id,
                                WarehouseRecordId = record.Id,
                                UserId = null,
                                OperationTime = DateTime.Now,
                            });
                        }
                    }
                    else if (record.RecordTypeId == 2)
                    {
                        var fixInfo = new FixedAsset
                        {
                            WarehouseRcordId = addInfo.Id,
                            Code = DateTime.Now.Date.ToString("yyyyMMdd") + ctx.FixedAssets.OrderByDescending(u => u.Id).FirstOrDefault() == null ? 1.ToString("D6") : ctx.FixedAssets.OrderByDescending(u => u.Id).FirstOrDefault().Id.ToString("D6"),
                            ItemId = record.ItemId,
                            FixedAssetDetailId = 1,
                            IsDelete = false
                        };
                        ctx.FixedAssets.Add(fixInfo);
                        ctx.SaveChanges();
                        ctx.AssetHistories.Add(new AssetHistory
                        {
                            FixedAssetId = fixInfo.Id,
                            WarehouseRecordId = addInfo.Id,
                            UserId = record.UserId == -1 ? null : record.UserId,
                            OperationTime = record.CreateTime,
                        });
                        ctx.SaveChanges();
                    }
                    else
                    {
                        for (int i = 0; i < record.ItemCount; i++)
                        {
                            ctx.AssetHistories.Add(new AssetHistory
                            {
                                FixedAssetId = warehouseRecordFixedAssets[i].Id,
                                WarehouseRecordId = addInfo.Id,
                                UserId = null,
                                OperationTime = DateTime.Now,
                            });
                        }
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
            catch (System.Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message,
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
                var warehouseRecordFixedAssets = ctx.FixedAssets.Include(a => a.WarehouseRcord)
                 .Include(a => a.AssetHistories)
                 .ToList()
                 .Where(a =>
                 a.WarehouseRcordId != null
                 && a.WarehouseRcord.Batch == record.Batch
                 && a.ItemId == record.ItemId
                 && !a.IsDelete
                 && (a.AssetHistories != null && (a.AssetHistories.ToList().OrderBy(b => b.OperationTime).LastOrDefault()?.WarehouseRecordId != null
                 && a.AssetHistories.ToList().OrderBy(b => b.OperationTime).LastOrDefault()?.UserId == null))).ToList();
                if (record.RecordTypeId == 1 && ctx.Items.ToList().FirstOrDefault(u => u.Id == record.ItemId).IsFixedAssets == true)
                {
                    if (warehouseRecordFixedAssets.Count < record.ItemCount)
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

                if (ctx.WarehouseRecords.Where(u => u.RecordTypeId == 2 && u.PlaceForStorageDetailId == record.PlaceForStorageDetailId && u.Batch == u.Batch).Sum(u => u.ItemCount) - ctx.WarehouseRecords.Where(u => u.RecordTypeId == 1 && u.PlaceForStorageDetailId == record.PlaceForStorageDetailId && u.Batch == u.Batch).Sum(u => u.ItemCount) < record.ItemCount)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"库存数量不足{record.ItemCount}",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null
                    });
                }

                var maxBatchRecord = ctx.WarehouseRecords.ToList()
                    .Where(a => a.ItemId == record.ItemId).ToList()
                    .OrderBy(a => a.Batch).ToList()
                    .LastOrDefault();

                var recordInfo = ctx.WarehouseRecords
                    .Include(a => a.AssetHistories)
                    .ToList()
                    .FirstOrDefault(u => u.Id == record.Id);

                var batchFixedAssets = ctx.FixedAssets
                    .Include(a => a.AssetHistories)
                    .Include(a => a.WarehouseRcord)
                    .ToList()
                    .Where(a =>
                    a.WarehouseRcordId != null
                    && a.WarehouseRcord.Batch == record.Batch
                    && a.ItemId == record.ItemId
                    && !a.IsDelete
                    && (a.AssetHistories == null
                    || (a.AssetHistories.ToList().OrderBy(b => b.OperationTime).LastOrDefault()?.WarehouseRecordId == null
                    && a.AssetHistories.ToList().OrderBy(b => b.OperationTime).LastOrDefault()?.UserId == null))).ToList();

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
                recordInfo.PlaceForStorageDetailId = record.PlaceForStorageDetailId;
                recordInfo.Batch = record.Batch != -1 ? record.Batch : maxBatchRecord == null ? 1 : maxBatchRecord.Batch;

                ctx.AssetHistories.RemoveRange(recordInfo.AssetHistories);
                ctx.SaveChanges();
                if (record.IsUserExistingItems)
                {
                    for (int i = 0; i < record.ItemCount; i++)
                    {
                        ctx.AssetHistories.Add(new AssetHistory
                        {
                            FixedAssetId = batchFixedAssets[i].Id,
                            WarehouseRecordId = recordInfo.Id,
                            UserId = null,
                            OperationTime = DateTime.Now,
                        });
                    }
                }
                else if (record.RecordTypeId == 2)
                {
                    var fixInfo = new FixedAsset
                    {
                        WarehouseRcordId = recordInfo.Id,
                        Code = DateTime.Now.Date.ToString("yyyyMMdd") + ctx.FixedAssets.OrderByDescending(u => u.Id).FirstOrDefault() == null ? 1.ToString("D6") : ctx.FixedAssets.OrderByDescending(u => u.Id).FirstOrDefault().Id.ToString("D6"),
                        ItemId = record.ItemId,
                        FixedAssetDetailId = 1,
                        IsDelete = false
                    };
                    ctx.FixedAssets.Add(fixInfo);
                    ctx.SaveChanges();
                    ctx.AssetHistories.Add(new AssetHistory
                    {
                        FixedAssetId = fixInfo.Id,
                        WarehouseRecordId = recordInfo.Id,
                        UserId = record.UserId == -1 ? null : record.UserId,
                        OperationTime = record.CreateTime,
                    });
                    ctx.SaveChanges();
                }
                else
                {
                    for (int i = 0; i < record.ItemCount; i++)
                    {
                        ctx.AssetHistories.Add(new AssetHistory
                        {
                            FixedAssetId = warehouseRecordFixedAssets[i].Id,
                            WarehouseRecordId = recordInfo.Id,
                            UserId = null,
                            OperationTime = DateTime.Now,
                        });
                    }
                }

                ctx.SaveChanges();
                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message,
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
                 .Where(u => u.RecordStateId == 1 && u.UserId == userId)
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
                        EndTime = u.EndTime.ToString("yyyy-MM-dd"),
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

        /// <summary>
        /// 用来批量审批
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [HttpPost("reviewBatchTask")]
        public IActionResult ReviewBatchTask([FromBody] List<ReviewBatchTask> task)
        {
            var ctx = new DB();
            try
            {
                foreach (var item in task)
                {
                    var recordInfo = ctx.WarehouseRecords.FirstOrDefault(u => u.Id == item.Id);
                    recordInfo.RecordStateId = item.stateId;
                    ctx.SaveChanges();
                }

                return Ok(new
                {
                    Success = true,
                    Message = "审批成功",
                    Timestamp = DateTime.UtcNow,
                    Data = -1
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "审批失败",
                    Timestamp = DateTime.UtcNow,
                    Data = 1
                });
            }
        }

        /// <summary>
        /// 接取任务
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("acceptTheTask")]
        public IActionResult AcceptTheTask([FromBody] AcceptUserData data)
        {
            var ctx = new DB();
            try
            {
                var recordList = ctx.WarehouseRecords.ToList()
               .FirstOrDefault(u => u.Id == data.RecordId);
                recordList.UserId = data.UserId;
                ctx.SaveChanges();
                return Ok(new
                {
                    Success = true,
                    Message = "接取成功",
                    Timestamp = DateTime.UtcNow,
                    Data = "1"
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "接取失败",
                    Timestamp = DateTime.UtcNow,
                    Data = "1"
                });
            }
        }

        /// <summary>
        /// 当前物品的所有批次
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="recordTypeId"></param>
        /// <returns></returns>
        [HttpGet("itemBatches")]
        public IActionResult ItemBatches(int itemId, int recordTypeId)
        {
            try
            {
                var ctx = new DB();
                var batches = ctx.WarehouseRecords.ToList()
                    .Where(a => a.ItemId == itemId && a.RecordTypeId != recordTypeId).ToList()
                    .GroupBy(a => a.Batch).ToList()
                    .Select(a => new
                    {
                        Id = a.Key,
                        Name = a.Key
                    }).ToList();

                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = batches
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
        /// 查询记录
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemTypeId"></param>
        /// <returns></returns>
        [HttpGet("quaryRecord")]
        public IActionResult QuaryRecord(int itemId, int itemTypeId, bool isFinishRecords)
        {
            var ctx = new DB();
            try
            {
                var recordList = ctx.WarehouseRecords
                .Include(a => a.RecordState)
                .Include(a => a.Item)
                .ThenInclude(a => a.ItemType)
                .Include(a => a.User)
                .Include(a => a.RecordType)
                .Include(a => a.PlaceForStorageDetail)
                .ThenInclude(a => a.PlaceForStorage)
                .ToList()
                .Where(a => (itemId == a.ItemId || itemId == -1)
                && (a.Item.ItemTypeId == itemTypeId || itemTypeId == -1)
                && a.RecordStateId != 3
                && (!isFinishRecords || a.RecordStateId == 1)).ToList()
                .Select(u => new
                {
                    u.Batch,
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
                    Data = recordList
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
        ///显示我未完成的任务
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        [HttpPost("showMyUnFinishedTask")]
        public IActionResult ShowMyUnFinishedTask([FromBody] int userid)
        {
            var ctx = new DB();
            try
            {
                var recordInfo = ctx.WarehouseRecords
                    .Include(u => u.Item)
                    .ThenInclude(u => u.ItemType)
                    .ToList()
                    .Where(u => u.RecordStateId == 1 && u.UserId == userid)
                    .Select(u => new
                    {
                        RecordId = u.Id,
                        ItemName = u.Item.Name,
                        ItemType = u.Item.ItemType.Name,
                        ItemCount = u.ItemCount,
                        AbnormalProblems = "无",
                        Solution = "无"
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
                    Data = "1"
                });
            }
        }

        /// <summary>
        /// 显示我的历史任务
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        [HttpPost("showMyHistoryTask")]
        public IActionResult ShowMyHistoryTask([FromBody] int userid)
        {
            var ctx = new DB();
            try
            {
                var recordInfo = ctx.WarehouseRecords
                    .Include(u => u.Item)
                    .ThenInclude(u => u.ItemType)
                    .ToList()
                    .Where(u => u.RecordStateId == 2 && u.UserId == userid)
                    .Select(u => new
                    {
                        RecordId = u.Id,
                        ItemName = u.Item.Name,
                        ItemType = u.Item.ItemType.Name,
                        ItemCount = u.ItemCount,
                        AbnormalProblems = "无",
                        Solution = "无"
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
                    Data = "1"
                });
            }
        }
    }
}




