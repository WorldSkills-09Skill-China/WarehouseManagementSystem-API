using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class Item
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int ItemTypeId { get; set; }

    public int SafetyInventory { get; set; }

    public string? Image { get; set; }

    public bool IsDelete { get; set; }

    public bool IsFixedAssets { get; set; }

    public string Unit { get; set; } = null!;

    public virtual ICollection<FixedAsset> FixedAssets { get; set; } = new List<FixedAsset>();

    public virtual ItemType ItemType { get; set; } = null!;

    public virtual ICollection<WarehouseRecord> WarehouseRecords { get; set; } = new List<WarehouseRecord>();
}
