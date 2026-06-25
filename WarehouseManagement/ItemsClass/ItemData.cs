using WarehouseManagement.Controllers;

namespace WarehouseManagement.ItemsClass
{
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
}
