using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class Inventory
{
    public string Code { get; set; } = null!;

    public int ItemId { get; set; }

    public int Batch { get; set; }

    public int? UserId { get; set; }

    public int WarehouseRecordId { get; set; }

    public virtual Item Item { get; set; } = null!;

    public virtual User? User { get; set; }

    public virtual WarehouseRecord WarehouseRecord { get; set; } = null!;
}
