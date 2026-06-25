using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class FixedAsset
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public int ItemId { get; set; }

    public bool IsDelete { get; set; }

    public int? WarehouseRcordId { get; set; }

    public int? FixedAssetDetailId { get; set; }

    public virtual ICollection<AssetHistory> AssetHistories { get; set; } = new List<AssetHistory>();

    public virtual ICollection<Exception> Exceptions { get; set; } = new List<Exception>();

    public virtual FixedAssetDetail? FixedAssetDetail { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual WarehouseRecord? WarehouseRcord { get; set; }
}
