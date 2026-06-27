using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Writers;
using Microsoft.VisualBasic;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using WarehouseManagement.EFCore;
using System.IO;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.Reflection.Metadata.Ecma335;
using WarehouseManagement.ItemsClass;
using System.Linq;
using DocumentFormat.OpenXml.EMMA;


namespace WarehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController(IWebHostEnvironment _whe) : ControllerBase
    {
        int _Batch;
        int _ItemId;
        /// <summary>
        /// 显示物品详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("showItemDetail")]
        public IActionResult ShowItemDetail([FromBody] int id)
        {
            var ctx = new DB();
            try
            {
                var itemInfo = ctx.Items
                .Include(u => u.ItemType)
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    ItemName = u.Name,
                    ItemTypeId = u.ItemTypeId,
                    SafeCount = u.SafetyInventory,
                    ImageFileName = u.Image,
                    IsFixedAssetpublic = u.IsFixedAssets,
                    Unit = u.Unit
                }).FirstOrDefault();

                return Ok(new
                {
                    Success = true,
                    Message = "",
                    TimeStamp = DateTime.UtcNow,
                    Data = itemInfo
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "没有该数据",
                    TimeStamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
        }

        /// <summary>
        /// 显示物品列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("showItemList")]
        public IActionResult ShowItemShow()
        {
            var ctx = new DB();
            var itemInfo = ctx.Items.ToList()
                .Where(u => u.IsDelete == false)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                }).ToList();
            return Ok(new
            {
                Success = true,
                Message = "",
                TimeStamp = DateTime.UtcNow,
                Data = itemInfo
            });
        }

        //显示异常物品
        //[Route("showAbnormalItems")]
        //[HttpGet]
        //public IActionResult ShowAbnormalItems()
        //{
        //    var ctx = new DB();
        //    try
        //    {
        //        var abnormalInfo = ctx.Items
        //            .Include(u => u.WarehouseRecords)
        //            .ThenInclude(u => u.HazardRecordDetail)
        //            .Include(u => u.ItemType)
        //            .Where(u => u.ItemAndStates.Where(x => x.HazardStateId == 1).Count() > 0 && u.IsDelete == false)
        //            .Select(u => new
        //            {
        //                ItemId = u.Id,
        //                ItemName = u.Name,
        //                ItemType = u.ItemType.Name,
        //                ItemCount = u.WarehouseRecords.Where(u => u.IsDelete == false && u.RecordTypeId == 2).Sum(u => u.ItemCount) -
        //                u.WarehouseRecords.Where(u => u.IsDelete == false && u.RecordTypeId == 1).Sum(u => u.ItemCount),
        //                AbnormalCount = u.ItemAndStates.Where(x => x.HazardStateId == 1).Count(),
        //                ItemSafeCount = u.SafetyInventory,
        //            }).ToList();

        //        return Ok(new
        //        {
        //            Success = true,
        //            Message = "",
        //            Timestamp = DateTime.UtcNow,
        //            Data = abnormalInfo
        //        });
        //    }
        //    catch
        //    {
        //        return BadRequest(new
        //        {
        //            success = false,
        //            Message = "",
        //            Timestamp = DateTime.UtcNow,
        //            Data = (string)null
        //        });
        //    }
        //}

        //显示对应异常的解决方案
        //[Route("abnormalJudgmentProblem")]
        //[HttpPost]
        //public IActionResult AbnormalJudgmentProblem([FromBody] int id)
        //{
        //    var ctx = new DB();

        //    try
        //    {
        //        var abnormalInfo = ctx.ItemAndStates.Include(u => u.Item).Include(u => u.HazardRecordDetail)
        //                       .Include(u => u.Item)
        //                       .ToList()
        //                       .Where(u => u.ItemId == id && u.HazardStateId == 1)
        //                       .Select(u =>
        //                       {
        //                           var amount = (ctx.WarehouseRecords.ToList().Where(u => u.ItemId == id && u.RecordTypeId == 2).Sum(u => u.ItemCount)) -
        //                          (ctx.WarehouseRecords.ToList().Where(u => u.ItemId == id && u.RecordTypeId == 1).Sum(u => u.ItemCount));
        //                           return new
        //                           {
        //                               ItemCount = u.HazardRecordDetailId == 2 ? (u.Item.SafetyInventory - amount) + 10 : amount,
        //                               Anomaly = u.HazardRecordDetail.Hint,
        //                               Type = u.HazardRecordDetailId == 2 ? false : true
        //                           };
        //                       }).ToList();

        //        if (abnormalInfo == null)
        //        {
        //            return BadRequest(new
        //            {
        //                Success = false,
        //                Message = "",
        //                Timestamp = DateTime.UtcNow,
        //                Data = (string)null
        //            });
        //        }

        //        return Ok(new
        //        {
        //            Success = true,
        //            Message = "",
        //            Timestamp = DateTime.UtcNow,
        //            Data = abnormalInfo
        //        });
        //    }
        //    catch
        //    {
        //        return BadRequest(new
        //        {
        //            Success = false,
        //            Message = "",
        //            Timestamp = DateTime.UtcNow,
        //            Data = (string)null
        //        });
        //    }
        //}

        /// <summary>
        /// 显示所有的物品情况
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("showSearchItems")]
        public IActionResult ShowSearchItems([FromBody] SearchItems data)
        {
            var ctx = new DB();
            try
            {
                var allItemInfo = ctx.Items
                    .Include(u => u.ItemType)
                    .ToList()
                    .Where(u => u.IsDelete == false && (data.TypeId == -1 || u.ItemTypeId == data.TypeId) && (string.IsNullOrEmpty(data.ItemName) || u.Name == data.ItemName)).ToList()
                    .Select(u => new
                    {
                        u.Id,
                        ItemName = u.Name,
                        ItemType = u.ItemType.Name,
                        SafeCount = u.SafetyInventory,
                        ImageFileName = u.Image
                    }).ToList();
                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = allItemInfo
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Massage = "is null",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
        }

        /// <summary>
        /// 添加新的物品
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("addItems")]
        public IActionResult AddItems([FromBody] ItemData data)
        {
            var ctx = new DB();
            try
            {

                var folder = Path.Combine(_whe.WebRootPath, "image", "itemimage");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + ".png";
                string fullPath = Path.Combine(folder, fileName);
                if (data.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(data.ImageFile))
                    {
                        var bytes = Convert.FromBase64String(data.ImageFile);
                        System.IO.File.WriteAllBytes(fullPath, bytes);
                    }
                }

                var item = new Item
                {
                    Name = data.ItemName,
                    ItemTypeId = data.ItemTypeId,
                    SafetyInventory = data.SafeCount,
                    Image = data.ImageFile == null ? null : $"/image/itemimage/{fileName}",
                    IsFixedAssets = data.IsFixedAsset,
                    Unit = data.Unit,
                    IsDelete = false
                };
                ctx.Items.Add(item);
                ctx.SaveChanges();
                if (data.AddFixedAssets.Count > 0)
                {
                    foreach (var fixedAsset in data.AddFixedAssets)
                    {
                        ctx.FixedAssets.Add(new FixedAsset
                        {
                            Code = fixedAsset.Code,
                            ItemId = item.Id,
                            FixedAssetDetailId = fixedAsset.SpecificationId == 0 ? 1 : fixedAsset.SpecificationId,
                            IsDelete = false
                        });
                        ctx.SaveChanges();
                    }
                }
                foreach (var place in data.PlaceForStorageDetailIdChangedFixedAsset.GroupBy(a => a.Value))
                {

                    var warehouseRecord = new WarehouseRecord
                    {
                        RecordStateId = 2,
                        ItemId = item.Id,
                        ItemCount = place.Count(),
                        Note = null,
                        CreateTime = DateTime.Now,
                        EndTime = DateTime.Now.AddDays(10),
                        FinishedTime = null,
                        RecordTypeId = 2,
                        UserId = null,
                        PlaceForStorageDetailId = Convert.ToInt32(place.Key.Split('|')[1]),
                        Batch = 1,
                    };
                    ctx.WarehouseRecords.Add(warehouseRecord);
                    ctx.SaveChanges();
                    foreach (var code in place)
                    {
                        var fixedAsset = ctx.FixedAssets.ToList().FirstOrDefault(a => a.Code == code.Key);
                        fixedAsset.WarehouseRcordId = warehouseRecord.Id;
                        ctx.SaveChanges();
                        ctx.AssetHistories.Add(new AssetHistory
                        {
                            FixedAssetId = fixedAsset.Id,
                            WarehouseRecordId = warehouseRecord.Id,
                            UserId = null,
                            OperationTime = DateTime.Now,
                        });
                        ctx.SaveChanges();
                    }
                }

                foreach (var fixedAsset in data.UserChangedFixedAsset)
                {
                    var fixedAssetId = ctx.FixedAssets.FirstOrDefault(a => a.Code == fixedAsset.Key).Id;
                    ctx.AssetHistories.Add(new AssetHistory
                    {
                        FixedAssetId = fixedAssetId,
                        WarehouseRecordId = null,
                        UserId = Convert.ToInt32(fixedAsset.Value.Split('|')[1]),
                        OperationTime = DateTime.Now,
                    });
                    ctx.SaveChanges();
                }

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
                return Ok(new
                {
                    Success = false,
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow,
                    Data = ""
                });
            }
        }

        /// <summary>
        /// 更改对应的物品
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("editItems")]
        public IActionResult EditItems([FromBody] ItemData data)
        {
            var ctx = new DB();
            try
            {
                var itemInfo = ctx.Items.ToList()
                    .FirstOrDefault(u => u.Id == data.Id);
                if (itemInfo == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "fail to edit",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null
                    });
                }
                var maxBatchRecord = ctx.WarehouseRecords.ToList()
                    .Where(a => a.ItemId == data.Id).ToList()
                    .OrderBy(a => a.Batch).ToList()
                    .LastOrDefault();

                if (data.ImageFile != null)
                {
                    var folder = Path.Combine(_whe.WebRootPath, "image", "itemimage");
                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid() + ".png";
                    string fullPath = Path.Combine(folder, fileName);
                    if (!string.IsNullOrEmpty(data.ImageFile))
                    {
                        var bytes = Convert.FromBase64String(data.ImageFile);
                        System.IO.File.WriteAllBytes(fullPath, bytes);
                    }
                    itemInfo.Image = $"/image/itemimage/{fileName}";
                }
                itemInfo.Name = data.ItemName;
                itemInfo.ItemTypeId = data.ItemTypeId;
                itemInfo.SafetyInventory = data.SafeCount;
                itemInfo.Unit = data.Unit;
                ctx.SaveChanges();

                if (data.IsFixedAsset == true)
                {
                    if (data.AddFixedAssets.Count > 0)
                    {
                        foreach (var fixedAsset in data.AddFixedAssets)
                        {
                            ctx.FixedAssets.Add(new FixedAsset
                            {
                                Code = fixedAsset.Code,
                                ItemId = itemInfo.Id,
                                FixedAssetDetailId = fixedAsset.SpecificationId == 0 ? 1 : fixedAsset.SpecificationId,
                                IsDelete = false
                            });
                            ctx.SaveChanges();
                        }
                    }
                    foreach (var place in data.PlaceForStorageDetailIdChangedFixedAsset.GroupBy(a => a.Value))
                    {

                        var warehouseRecord = new WarehouseRecord
                        {
                            RecordStateId = 2,
                            ItemId = itemInfo.Id,
                            ItemCount = place.Count(),
                            Note = null,
                            CreateTime = DateTime.Now,
                            EndTime = DateTime.Now.AddDays(10),
                            FinishedTime = null,
                            RecordTypeId = 2,
                            UserId = null,
                            PlaceForStorageDetailId = Convert.ToInt32(place.Key.Split('|')[1]),
                            Batch = 1,
                        };
                        ctx.WarehouseRecords.Add(warehouseRecord);
                        ctx.SaveChanges();
                        foreach (var code in place)
                        {
                            var fixedAsset = ctx.FixedAssets.ToList().FirstOrDefault(a => a.Code == code.Key);
                            fixedAsset.WarehouseRcordId = warehouseRecord.Id;
                            ctx.SaveChanges();
                            ctx.AssetHistories.Add(new AssetHistory
                            {
                                FixedAssetId = fixedAsset.Id,
                                WarehouseRecordId = warehouseRecord.Id,
                                UserId = null,
                                OperationTime = DateTime.Now,
                            });
                            ctx.SaveChanges();
                        }
                    }

                    foreach (var fixedAsset in data.UserChangedFixedAsset)
                    {
                        var fixedAssetId = ctx.FixedAssets.FirstOrDefault(a => a.Code == fixedAsset.Key).Id;
                        ctx.AssetHistories.Add(new AssetHistory
                        {
                            FixedAssetId = fixedAssetId,
                            WarehouseRecordId = null,
                            UserId = Convert.ToInt32(fixedAsset.Value.Split('|')[1]),
                            OperationTime = DateTime.Now,
                        });
                        ctx.SaveChanges();
                    }

                    if (data.DeleteFixedAssets.Count > 0)
                    {
                        foreach (var item in data.DeleteFixedAssets)
                        {
                            var deleteInfo = ctx.FixedAssets.ToList()
                                .FirstOrDefault(u => u.Code == item.Code);
                            deleteInfo.IsDelete = true;
                        }
                        ctx.SaveChanges();
                    }
                }

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
                    Message = "fail to edit",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
        }

        /// <summary>
        /// 返回最大的固定资产的Id
        /// </summary>
        /// <returns></returns>
        [HttpGet("showLargestFixedAsset")]
        public IActionResult ShowLargestFixedAsset()
        {
            var ctx = new DB();
            try
            {
                var bestId = ctx.AssetHistories.ToList()
                    .OrderByDescending(u => u.Id)
                    .FirstOrDefault().Id;
                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = bestId
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
        /// 添加物品和记录
        /// </summary>
        /// <param name="addData"></param>
        /// <returns></returns>
        [HttpPost("AddItemsAndRecord")]
        public IActionResult AddItemsAndRecord([FromBody] AddItemData addData)
        {
            var ctx = new DB();
            try
            {
                var itemInfo = ctx.Items
                    .Include(u => u.ItemType).ToList()
                    .FirstOrDefault(u => u.Name == addData.ItemName &&
                u.ItemTypeId == addData.TypeId &&
                u.SafetyInventory == addData.SafeCount);
                if (itemInfo != null)
                {
                    var record = ctx.WarehouseRecords.Include(u => u.Item).ThenInclude(u => u.ItemType).OrderByDescending(u => u.Batch).FirstOrDefault(u => u.Item.Name == addData.ItemName);

                    int? recordBatch = 0;
                    if (record == null)
                    {
                        recordBatch = 1;
                    }
                    else
                    {
                        recordBatch = record.Batch + 1;
                    }

                    ctx.WarehouseRecords.Add(new WarehouseRecord
                    {
                        RecordTypeId = 2,
                        ItemId = itemInfo.Id,
                        ItemCount = addData.Count,
                        CreateTime = DateTime.Now,
                        EndTime = DateTime.Now,
                        RecordStateId = 2,
                        UserId = 1,
                        PlaceForStorageDetailId = addData.PlaceForStorageDetailId,
                        Batch = (int)recordBatch
                    });
                    ctx.SaveChanges();
                    _Batch = (int)recordBatch;
                    _ItemId = itemInfo.Id;
                }
                else
                {
                    var itemData = new Item
                    {
                        Name = addData.ItemName,
                        ItemTypeId = addData.TypeId,
                        SafetyInventory = addData.SafeCount,
                        IsDelete = false,
                        IsFixedAssets = addData.IsFixedAssets,
                    };
                    ctx.Items.Add(itemData);
                    ctx.SaveChanges();
                    _ItemId = itemData.Id;
                    _Batch = 1;
                }
                if (addData.IsFixedAssets)
                {

                    ctx.FixedAssets.Add(new FixedAsset
                    {
                        Code = DateTime.Now.Date.ToString("yyyyMMdd") + ctx.FixedAssets.OrderByDescending(u => u.Id).FirstOrDefault() == null ? 1.ToString("D6") : ctx.FixedAssets.OrderByDescending(u => u.Id).FirstOrDefault().Id.ToString("D6"),
                        ItemId = _ItemId,
                        IsDelete = false,
                    });
                    ctx.SaveChanges();
                }
                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = _Batch,
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = -1,
                });
            }

        }

        /// <summary>
        /// 查看这个固定资产的所有的历史记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="first"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpGet("showAssetHistory")]
        public IActionResult ShowAssetHistory(string id, DateTime first, DateTime end)
        {
            var ctx = new DB();
            try
            {
                var assetHistoryInfo = ctx.AssetHistories
                    .Include(u => u.User)
                    .Include(u => u.WarehouseRecord)
                    .ThenInclude(u => u.PlaceForStorageDetail)
                    .ThenInclude(u => u.PlaceForStorage)
                    .Include(u => u.FixedAsset)
                    .ThenInclude(u => u.Item)
                    .Include(u => u.FixedAsset)
                    .ThenInclude(u => u.WarehouseRcord).ToList()
                    .Where(u => u.FixedAsset.Code == id
                    && u.OperationTime >= first
                    && u.OperationTime <= end
                    && ((u.WarehouseRecord != null && u.WarehouseRecord.RecordStateId == 2) || u.UserId != null))
                    .Select(u => new
                    {
                        ItemName = u.FixedAsset.Item.Name,
                        Code = u.FixedAsset.Code,
                        UserName = u.UserId == null ? null : u.User.Name,
                        u.OperationTime,
                        PlaceForStorageDetail = u.WarehouseRecord.PlaceForStorageDetailId == null ? null : u.WarehouseRecord.PlaceForStorageDetail.PlaceForStorage.Name + "-" + u.WarehouseRecord.PlaceForStorageDetail.Name,
                    }).ToList();
                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = assetHistoryInfo
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null,
                });
            }
        }

        /// <summary>
        /// 查看某个固定资产
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("showFixedAsset")]
        public IActionResult ShowFixedAsset([FromBody] int id)
        {
            var ctx = new DB();
            try
            {
                var fixedAssetInfo = ctx.FixedAssets
                    .Include(u => u.Item)
                    .Include(u => u.AssetHistories)
                    .ThenInclude(u => u.WarehouseRecord)
                    .ThenInclude(u => u.PlaceForStorageDetail)
                    .ThenInclude(u => u.PlaceForStorage)
                    .Include(u => u.FixedAssetDetail)
                    .Include(u => u.WarehouseRcord)
                    .Include(u => u.AssetHistories)
                    .ThenInclude(u => u.User)
                    .ToList()
                    .Where(u => u.ItemId == id && u.IsDelete == false)
                    .Select(u =>
                    {
                        var assetHistoryInfo = u.AssetHistories.OrderByDescending(u => u.OperationTime).FirstOrDefault();
                        return new
                        {
                            Code = u.Code,
                            UserName = assetHistoryInfo == null ? null :
                            assetHistoryInfo.UserId == null ? null :
                            assetHistoryInfo.User.Name,
                            ItemName = u.Item.Name,
                            PlaceForStorageDetail = assetHistoryInfo == null ? null :
                            assetHistoryInfo.WarehouseRecord == null ? null :
                            assetHistoryInfo.WarehouseRecord.PlaceForStorageDetail.PlaceForStorage.Name + "-" +
                            assetHistoryInfo.WarehouseRecord.PlaceForStorageDetail.Name,
                            Batch = u.WarehouseRcord?.Batch,
                            Specification = u.FixedAssetDetail.Specification
                        };
                    }).ToList();

                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = fixedAssetInfo
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null,
                });
            }
        }

        /// <summary>
        /// 删除某个固定资产
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("deleteFixedAsset")]
        public IActionResult DeleteFixedAsset(int id)
        {
            var ctx = new DB();
            try
            {
                var deleteInfo = ctx.FixedAssets.ToList()
                    .FirstOrDefault(u => u.Id == id);
                if (deleteInfo == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null,
                    });
                }
                deleteInfo.IsDelete = true;
                ctx.SaveChanges();
                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null,
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null,
                });
            }
        }

        /// <summary>
        /// 删除固定资产历史记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("deleteFixAssetHistory")]
        public IActionResult DeleteFixAssetHistory(int id)
        {
            var ctx = new DB();
            try
            {
                var deleteHistoryInfo = ctx.AssetHistories.ToList()
                    .FirstOrDefault(u => u.Id == id);
                if (deleteHistoryInfo == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null,
                    });
                }
                ctx.SaveChanges();

                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null,
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null,
                });
            }
        }

        /// <summary>
        /// 删除对应的物品
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("deleteItems")]
        public IActionResult DeleteItems([FromBody] int id)
        {
            var ctx = new DB();
            try
            {
                var deleteInfo = ctx.Items.ToList()
                .FirstOrDefault(u => u.Id == id);
                if (deleteInfo == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "fail to delete",
                        Timestamp = DateTime.UtcNow,
                        Data = (string)null
                    });
                }
                deleteInfo.IsDelete = true;
                ctx.SaveChanges();
                return Ok(new
                {
                    Success = true,
                    Message = "success to delete",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "fail to delete",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }

        }

        /// <summary>
        /// 显示物品的图片
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("showItemImage")]
        public IActionResult ShowItemImage([FromBody] int id)
        {
            var ctx = new DB();
            try
            {
                var itemInfo = ctx.Items.ToList()
                     .FirstOrDefault(u => u.Id == id);
                if (itemInfo == null)
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
                        ImageName = itemInfo.Image,
                        IsFixedAsset = itemInfo.IsFixedAssets,
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
        /// 更改固定资产的持有人
        /// </summary>
        /// <param name="change"></param>
        /// <returns></returns>
        [HttpPost("changeTheUser")]
        public IActionResult ChangeTheUser([FromBody] ChangeTheUser change)
        {
            var ctx = new DB();
            try
            {
                var fixedAssets = ctx.FixedAssets.FirstOrDefault(u => u.Code == change.Code);
                ctx.AssetHistories.Add(new AssetHistory
                {
                    FixedAssetId = fixedAssets.Id,
                    WarehouseRecordId = fixedAssets.AssetHistories.OrderByDescending(u => u.OperationTime).FirstOrDefault(u => u.FixedAssetId == fixedAssets.Id).WarehouseRecordId,
                    UserId = change.userId,
                    OperationTime = DateTime.Now,
                });
                ctx.SaveChanges();

                return Ok(new
                {
                    Success = true,
                    Message = "更改成功",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "更改失败",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
        }

        /// <summary>
        /// 查找固定资产对应的物品信息
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("searchThisCodeDetail")]
        public IActionResult SearchThisCodeDetail(string code)
        {
            var ctx = new DB();
            try
            {
                var itemInfo = ctx.AssetHistories
                    .Include(u => u.FixedAsset)
                    .Include(u => u.User)
                    .Include(u => u.WarehouseRecord)
                    .ThenInclude(u => u.PlaceForStorageDetail)
                    .ThenInclude(u => u.PlaceForStorage)
                    .ToList()
                    .OrderByDescending(u => u.OperationTime)
                    .FirstOrDefault(u => u.FixedAsset.Code == code);
                if (itemInfo == null)
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
                        ItemName = itemInfo.User == null ? null : itemInfo.User.Name,
                        ItemType = itemInfo.FixedAsset.Item.ItemType,
                        PlaceForStorage = itemInfo.WarehouseRecord.PlaceForStorageDetail.PlaceForStorage.Name + "-" + itemInfo.WarehouseRecord.PlaceForStorageDetail.Name
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

        //是否未解决异常物品
        //[Route("unresolvedAbnormalItem")]
        //[HttpGet]
        //public IActionResult UnresolvedAbnormalItem()
        //{
        //    var ctx = new DB();
        //    try
        //    {
        //        var isExistingProblem = ctx.ItemAndStates.Include(u => u.HazardState)
        //            .ToList()
        //            .Any(u => u.HazardStateId == 1);
        //        if (isExistingProblem)
        //        {
        //            return Ok(new
        //            {
        //                Success = true,
        //                Message = "",
        //                Timestamp = DateTime.UtcNow,
        //                Data = false
        //            });
        //        }
        //        else
        //        {
        //            return Ok(new
        //            {
        //                Success = true,
        //                Message = "",
        //                Timestamp = DateTime.UtcNow,
        //                Data = true
        //            });
        //        }
        //    }
        //    catch
        //    {
        //        return BadRequest(new
        //        {
        //            Success = false,
        //            Message = "",
        //            Timestamp = DateTime.UtcNow,
        //            Data = (string)null
        //        });
        //    }
        //}

        /// <summary>
        /// 批量导入数据
        /// </summary>
        /// <param name="imports"></param>
        /// <returns></returns>
        [HttpPost("batchImportOfData")]
        public IActionResult BatchImportOfData([FromBody] List<ImportData> imports)
        {
            var ctx = new DB();
            try
            {
                var fixedAssetList = new List<FixedData>();
                foreach (var item in imports)
                {
                    var itemTypeId = ctx.ItemTypes.ToList().FirstOrDefault(u => u.Name == item.ItemType);
                    var placeForStorageId = ctx.PlaceForStorageDetails.Include(u => u.PlaceForStorage).ToList()
                        .FirstOrDefault(u => u.PlaceForStorage.Name + u.Name == item.PlaceForStorageDetail);

                    if (itemTypeId == null || placeForStorageId == null)
                    {
                        return BadRequest(new
                        {
                            Success = false,
                            Message = "不存在类型",
                            Timestamp = DateTime.UtcNow,
                            Data = (string)null
                        });
                    }

                    var addItemInfo = new Item
                    {
                        Name = item.ItemName,
                        ItemTypeId = itemTypeId.Id,
                        SafetyInventory = item.SafeCount,
                        IsDelete = false,
                        Unit = item.Unit,
                        IsFixedAssets = true
                    };
                    ctx.Items.Add(addItemInfo);
                    ctx.SaveChanges();

                    var warehouseRecordInfo = new WarehouseRecord
                    {
                        RecordTypeId = 2,
                        ItemId = addItemInfo.Id,
                        ItemCount = item.Count,
                        CreateTime = DateTime.Now,
                        EndTime = DateTime.Now,
                        FinishedTime = DateTime.Now,
                        PlaceForStorageDetailId = placeForStorageId.Id,
                        RecordStateId = 2,
                        UserId = 5,
                        Batch = ctx.WarehouseRecords.FirstOrDefault(u => u.ItemId == addItemInfo.Id) == null ? 1 :
                        ctx.WarehouseRecords.FirstOrDefault(u => u.ItemId == addItemInfo.Id).Batch + 1,
                    };
                    ctx.WarehouseRecords.Add(warehouseRecordInfo);
                    ctx.SaveChanges();

                    if (item.IsFixedAsset)
                    {
                        var now = DateTime.Now;
                        for (var i = 0; i <= warehouseRecordInfo.ItemCount; i++)
                        {
                            var assets = new FixedAsset
                            {
                                Code = ctx.FixedAssets.Count() <= 0 ? now.Year.ToString("D4") + now.Month + now.Day + 1.ToString("D6") :
                                now.Year.ToString("D4") + now.Month + now.Day + ctx.FixedAssets.ToList().OrderByDescending(u => u.Id).FirstOrDefault().Id.ToString("D6"),
                                ItemId = addItemInfo.Id,
                                IsDelete = false,
                                WarehouseRcordId = warehouseRecordInfo.Id,
                            };
                            ctx.FixedAssets.Add(assets);
                            ctx.SaveChanges();

                            fixedAssetList.Add(new FixedData
                            {
                                ItemName = item.ItemName,
                                Code = assets.Code,
                                Batch = (int)warehouseRecordInfo.Batch,
                                TypeName = item.ItemType,
                            });

                            ctx.AssetHistories.Add(new AssetHistory
                            {
                                FixedAssetId = assets.Id,
                                WarehouseRecordId = warehouseRecordInfo.Id,
                                UserId = null,
                                OperationTime = DateTime.UtcNow,
                            });
                            ctx.SaveChanges();
                        };

                        fixedAssetList.PrintALabel(_whe.WebRootPath);
                    }
                }

                return Ok(new
                {
                    Success = true,
                    Message = "成功添加",
                    TimeStamp = DateTime.UtcNow,
                    Data = (string)null
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
        /// 编辑物品规格
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        [HttpPost("editItemSpecification")]
        public IActionResult EditItemSpecification([FromBody] ItemsSpecification items)
        {
            var newFixedAssets = new List<String>();
            var specificationId = 0;
            var ctx = new DB();
            var specification = ctx.FixedAssetDetails.ToList().FirstOrDefault(a => a.Specification == items.Specification);
            if (specification == null)
            {
                var info = new FixedAssetDetail
                {
                    Specification = items.Specification,
                };
                ctx.FixedAssetDetails.Add(info);
                ctx.SaveChanges();
                specificationId = info.Id;
                foreach (var item in items.CodeList)
                {
                    var itemData = ctx.FixedAssets.ToList().FirstOrDefault(a => a.Code == item);
                    if (itemData != null)
                    {
                        itemData.FixedAssetDetailId = info.Id;
                    }
                    else
                    {
                        newFixedAssets.Add(item);
                    }
                }
                ctx.SaveChanges();
            }
            else
            {
                specificationId = specification.Id;
                foreach (var item in items.CodeList)
                {
                    var itemData = ctx.FixedAssets.ToList().FirstOrDefault(a => a.Code == item);
                    if (itemData != null)
                    {
                        itemData.FixedAssetDetailId = specification.Id;
                    }
                    else
                    {
                        newFixedAssets.Add(item);
                    }
                }
                ctx.SaveChanges();
            }

            return Ok(new
            {
                Success = true,
                Message = "",
                Timestamp = DateTime.UtcNow,
                Data = new
                {
                    NewFixedAssets = newFixedAssets,
                    SpecificationId = specificationId
                }
            });
        }

        /// <summary>
        /// 物品规格
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("getItemSpecification")]
        public IActionResult GetItemSpecification(string code)
        {
            var ctx = new DB();
            var specification = ctx.FixedAssetDetails.Include(a => a.FixedAssets).ToList().FirstOrDefault(a => a.FixedAssets.Any(b => b.Code == code)).Specification;
            return Ok(new
            {
                Success = true,
                Message = "",
                Timestamp = DateTime.UtcNow,
                Data = specification
            });
        }

        /// <summary>
        /// 可还批次物品
        /// </summary>
        /// <param name="count"></param>
        /// <param name="batch"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("returnBatchItem")]
        public IActionResult ReturnBatchItem(int count, int batch, int id)
        {
            var ctx = new DB();
            var batchFixedAssets = ctx.FixedAssets
                .Include(a => a.WarehouseRcord)
                .Include(a => a.AssetHistories)
                .ToList()
                .Where(a =>
                a.WarehouseRcordId != null
                && a.WarehouseRcord.Batch == batch
                && a.ItemId == id
                && !a.IsDelete
                && (a.AssetHistories != null && (a.AssetHistories.ToList().OrderBy(b => b.OperationTime).LastOrDefault().WarehouseRecord.RecordTypeId == 1
                && a.AssetHistories.ToList().OrderBy(b => b.OperationTime).LastOrDefault().UserId == null))).ToList()
                .Select(a => new
                {
                    ItemName = a.Item.Name,
                    Code = a.Code,
                    Batch = a.WarehouseRcord.Batch,
                    Detail = a.FixedAssetDetail?.Specification
                }).ToList();

            return Ok(new
            {
                Success = true,
                Message = "",
                Timestamp = DateTime.UtcNow,
                Data = batchFixedAssets
            });
        }
    }
}
