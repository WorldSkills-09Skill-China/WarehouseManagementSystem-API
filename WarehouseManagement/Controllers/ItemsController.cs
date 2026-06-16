using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.OpenApi.Writers;
using System.ComponentModel.DataAnnotations;
using WarehouseManagement.EFCore;

namespace WarehouseManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        int _Batch;
        int _ItemId;
        //显示物品详情
        [Route("showItemDetail")]
        [HttpPost]
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
                    IsFixedAssetpublic = u.IsFixedAssets
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

        //显示物品列表
        [Route("showItemList")]
        [HttpGet]
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
        [Route("showAbnormalItems")]
        [HttpGet]
        public IActionResult ShowAbnormalItems()
        {
            var ctx = new DB();
            try
            {
                var abnormalInfo = ctx.Items
                    .Include(u => u.WarehouseRecords)
                    .Include(u => u.ItemAndStates)
                    .ThenInclude(u => u.HazardRecordDetail)
                    .Include(u => u.ItemType)
                    .Where(u => u.ItemAndStates.Where(x => x.HazardStateId == 1).Count() > 0 && u.IsDelete == false)
                    .Select(u => new
                    {
                        ItemId = u.Id,
                        ItemName = u.Name,
                        ItemType = u.ItemType.Name,
                        ItemCount = u.WarehouseRecords.Where(u => u.IsDelete == false && u.RecordTypeId == 2).Sum(u => u.ItemCount) -
                        u.WarehouseRecords.Where(u => u.IsDelete == false && u.RecordTypeId == 1).Sum(u => u.ItemCount),
                        AbnormalCount = u.ItemAndStates.Where(x => x.HazardStateId == 1).Count(),
                        ItemSafeCount = u.SafetyInventory,
                    }).ToList();

                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = abnormalInfo
                });
            }
            catch
            {
                return BadRequest(new
                {
                    success = false,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
        }

        //显示对应异常的解决方案
        [Route("abnormalJudgmentProblem")]
        [HttpPost]
        public IActionResult AbnormalJudgmentProblem([FromBody] int id)
        {
            var ctx = new DB();

            try
            {
                var abnormalInfo = ctx.ItemAndStates.Include(u => u.Item).Include(u => u.HazardRecordDetail)
                               .Include(u => u.Item)
                               .ToList()
                               .Where(u => u.ItemId == id && u.HazardStateId == 1)
                               .Select(u =>
                               {
                                   var amount = (ctx.WarehouseRecords.ToList().Where(u => u.ItemId == id && u.RecordTypeId == 2).Sum(u => u.ItemCount)) -
                                  (ctx.WarehouseRecords.ToList().Where(u => u.ItemId == id && u.RecordTypeId == 1).Sum(u => u.ItemCount));
                                   return new
                                   {
                                       ItemCount = u.HazardRecordDetailId == 2 ? (u.Item.SafetyInventory - amount) + 10 : amount,
                                       Anomaly = u.HazardRecordDetail.Hint,
                                       Type = u.HazardRecordDetailId == 2 ? false : true
                                   };
                               }).ToList();

                if (abnormalInfo == null)
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
                    Data = abnormalInfo
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

        //显示所有的物品情况
        [Route("showSearchItems")]
        [HttpPost]
        public IActionResult ShowSearchItems([FromBody] SearchItems data)
        {
            var ctx = new DB();
            try
            {
                var allItemInfo = ctx.Items
                    .Include(u => u.ItemType)
                    .ToList()
                    .Where(u => u.IsDelete == false && (data.typeId == -1 || u.ItemTypeId == data.typeId) && (string.IsNullOrEmpty(data.itemName) || u.Name == data.itemName)).ToList()
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

        //添加新的物品
        [Route("addItems")]
        [HttpPost]
        public IActionResult AddItems([FromBody] ItemData data)
        {
            var ctx = new DB();
            try
            {
                var addInfo = new Item
                {
                    Name = data.ItemName,
                    ItemTypeId = data.ItemTypeId,
                    SafetyInventory = data.SafeCount,
                    Image = data.ImageFileName,
                    IsFixedAssets = data.IsFixedAsset,
                    IsDelete = false
                };
                ctx.Items.Add(addInfo);
                ctx.SaveChanges();

                if (data.IsFixedAsset == true)
                {
                    foreach (var item in data.AddFixedAssets)
                    {
                        ctx.FixedAssets.Add(new FixedAsset
                        {
                            Code = item.Code,
                            ItemId = addInfo.Id,
                            IsDelete = false
                        });
                    }
                    ctx.SaveChanges();
                }

                return Ok(new
                {
                    Success = true,
                    Message = "",
                    Timestamp = DateTime.UtcNow,
                    Data = addInfo.Id
                });
            }
            catch
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = "fail to add",
                    Timestamp = DateTime.UtcNow,
                    Data = (string)null
                });
            }
        }

        //更改对应的物品
        [Route("editItems")]
        [HttpPost]
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
                itemInfo.Name = data.ItemName;
                itemInfo.ItemTypeId = data.ItemTypeId;
                itemInfo.SafetyInventory = data.SafeCount;
                itemInfo.Image = data.ImageFileName;
                ctx.SaveChanges();

                if (data.IsFixedAsset == true)
                {
                    foreach (var item in data.AddFixedAssets)
                    {
                        ctx.FixedAssets.Add(new FixedAsset
                        {
                            Code = item.Code,
                            ItemId = item.ItemId,
                            IsDelete = false
                        });
                    }
                    ctx.SaveChanges();

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

        //返回最大的固定资产的Id
        [Route("showLargestFixedAsset")]
        [HttpGet]
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

        //添加物品和记录
        [Route("AddItemsAndRecord")]
        [HttpPost]
        public IActionResult AddItemsAndRecord([FromBody] AddItemData addData)
        {
            var ctx = new DB();
            try
            {
                var itemInfo = ctx.Items.ToList()
                    .FirstOrDefault(u => u.Name == addData.ItemName &&
                u.ItemTypeId == addData.TypeId &&
                u.SafetyInventory == addData.SafeCount);
                if (itemInfo != null)
                {
                    int recordBatch = (int)ctx.WarehouseRecords.OrderByDescending(u => u.Batch).FirstOrDefault(u => u.Item.Name == addData.ItemName).Batch + 1;
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
                        Batch = recordBatch
                    });
                    ctx.SaveChanges();
                    _Batch = recordBatch;
                    _ItemId = itemInfo.Id;
                }
                else
                {
                    var itmeInfo = ctx.Items.Add(new Item
                    {
                        Name = addData.ItemName,
                        ItemTypeId = addData.TypeId,
                        SafetyInventory = addData.SafeCount,
                        IsDelete = false,
                        IsFixedAssets = addData.IsFixedAssets,
                    });
                    ctx.SaveChanges();
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

        //查看这个固定资产的所有的历史记录
        [Route("showAssetHistory")]
        [HttpGet]
        public IActionResult ShowAssetHistory(string id, DateTime first, DateTime end)
        {
            var ctx = new DB();
            try
            {
                var assetHistoryInfo = ctx.AssetHistories.ToList()
                    .Where(u => u.FixedAsset.Code == id && u.OperationTime >= first && u.OperationTime <= end)
                    .Select(u => new
                    {
                        ItemName = u.FixedAsset.Item.Name,
                        Code = u.FixedAsset.Code,
                        UserName = u.User.Name,
                        u.Note,
                        u.OperationTime,
                        PlaceForStorageDetail = u.PlaceForStorageDetail.Name
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

        //查看某个固定资产
        [Route("showFixedAsset")]
        [HttpPost]
        public IActionResult ShowFixedAsset([FromBody] int id)
        {
            var ctx = new DB();
            try
            {
                var fixedAssetInfo = ctx.FixedAssets
                    .Include(u => u.Item)
                    .ToList()
                    .Where(u => u.ItemId == id && u.IsDelete == false)
                    .Select(u => new
                    {
                        Code = u.Code,
                        UserName = u.AssetHistories.OrderByDescending(u => u.OperationTime).FirstOrDefault() == null ? null : u.AssetHistories.OrderByDescending(u => u.OperationTime).FirstOrDefault().User.Name,
                        ItemName = u.Item.Name,
                        PlaceForStorageDetail = u.AssetHistories.OrderByDescending(u => u.OperationTime).FirstOrDefault() == null ? null : u.AssetHistories.OrderByDescending(u => u.OperationTime).FirstOrDefault().PlaceForStorageDetail.Name,
                        UserId = u.AssetHistories.OrderByDescending(u => u.OperationTime).FirstOrDefault() == null ? null : u.AssetHistories.OrderByDescending(u => u.OperationTime).FirstOrDefault().UserId
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

        //删除某个固定资产
        [Route("deleteFixedAsset")]
        [HttpGet]
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

        //删除固定资产历史记录
        [Route("deleteFixAssetHistory")]
        [HttpGet]
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

                deleteHistoryInfo.IsDelete = true;
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

        //删除对应的物品
        [Route("deleteItems")]
        [HttpPost]
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

        //显示物品的图片
        [Route("showItemImage")]
        [HttpPost]
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
                        IsFixedAsset = itemInfo.IsFixedAssets == null ? false : true,
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
    }
    public class SearchItems
    {
        public int typeId { get; set; }
        public string itemName { get; set; }
    }
    public class ItemData
    {
        public int? Id { get; set; }
        public string ItemName { get; set; }
        public int ItemTypeId { get; set; }
        public bool IsFixedAsset { get; set; }
        public int SafeCount { get; set; }
        public string ImageFileName { get; set; }
        public List<AddFixedAsset>? AddFixedAssets { get; set; }
        public List<DeleteFixedAsset>? DeleteFixedAssets { get; set; }
    }
    public class AddItemData
    {
        public string ItemName { get; set; }
        public int TypeId { get; set; }
        public int SafeCount { get; set; }
        public int Count { get; set; }
        public bool IsFixedAssets { get; set; }
        public int PlaceForStorageDetailId { get; set; }
        public List<AddFixedAsset>? AddFixedAssets { get; set; }
    }
    public class AddFixedAsset
    {
        public string Code { get; set; }
        public int ItemId { get; set; }
    }
    public class DeleteFixedAsset
    {
        public string Code { get; set; }
        public int ItemId { get; set; }
    }

}
