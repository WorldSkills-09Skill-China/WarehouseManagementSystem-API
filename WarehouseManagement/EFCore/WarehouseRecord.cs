using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class WarehouseRecord
{
    public int Id { get; set; }

    public int RecordTypeId { get; set; }

    public int ItemId { get; set; }

    public int ItemCount { get; set; }

    public string? Note { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime EndTime { get; set; }

    public DateTime? FinishedTime { get; set; }

    public int RecordStateId { get; set; }

    public int? UserId { get; set; }

    public int PlaceForStorageDetailId { get; set; }

    public bool IsDelete { get; set; }

    public int? Batch { get; set; }

    public virtual ICollection<FixedAsset> FixedAssets { get; set; } = new List<FixedAsset>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual Item Item { get; set; } = null!;

    public virtual PlaceForStorageDetail PlaceForStorageDetail { get; set; } = null!;

    public virtual ReocordState RecordState { get; set; } = null!;

    public virtual RecordType RecordType { get; set; } = null!;

    public virtual User? User { get; set; }
}
