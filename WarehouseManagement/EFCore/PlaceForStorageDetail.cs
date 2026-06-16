using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class PlaceForStorageDetail
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int StateId { get; set; }

    public int PlaceForStorageId { get; set; }

    public virtual ICollection<AssetHistory> AssetHistories { get; set; } = new List<AssetHistory>();

    public virtual PlaceForStorage PlaceForStorage { get; set; } = null!;

    public virtual PlaceForStorageDetailState State { get; set; } = null!;

    public virtual ICollection<WarehouseRecord> WarehouseRecords { get; set; } = new List<WarehouseRecord>();
}
