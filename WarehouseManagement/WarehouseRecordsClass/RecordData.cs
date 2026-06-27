namespace WarehouseManagement.WarehouseRecordsClass
{
    public class RecordData
    {
        public int? Id { set; get; }
        public int RecordTypeId { set; get; }
        public int ItemId { set; get; }
        public int ItemCount { set; get; }
        public int RecordStateId { set; get; }
        public string Note { set; get; }
        public DateTime CreateTime { set; get; }
        public DateTime EndTime { set; get; }
        public DateTime? FinishedTime { set; get; }
        public int? UserId { set; get; }
        public int PlaceForStorageDetailId { set; get; }
        public bool IsDelete { set; get; }
    }
}
