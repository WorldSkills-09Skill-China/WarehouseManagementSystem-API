namespace WarehouseManagement.ItemsClass
{
    public class ImportData
    {
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public int SafeCount { get; set; }
        public int Count { get; set; }
        public bool IsFixedAsset { get; set; }
        public string PlaceForStorageDetail { get; set; }
    }
}
