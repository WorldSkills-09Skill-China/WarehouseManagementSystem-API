using WarehouseManagement.Controllers;

namespace WarehouseManagement.ItemsClass
{
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
}
