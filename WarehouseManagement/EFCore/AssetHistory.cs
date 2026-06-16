using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class AssetHistory
{
    public int Id { get; set; }

    public int FixedAssetId { get; set; }

    public int? PlaceForStorageDetailId { get; set; }

    public int? UserId { get; set; }

    public string? Note { get; set; }

    public DateTime OperationTime { get; set; }

    public bool? IsDelete { get; set; }

    public virtual FixedAsset FixedAsset { get; set; } = null!;

    public virtual PlaceForStorageDetail? PlaceForStorageDetail { get; set; }

    public virtual User? User { get; set; }
}
