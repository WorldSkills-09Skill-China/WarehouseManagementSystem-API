using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

    public virtual ICollection<AssetHistory> AssetHistories { get; set; } = new List<AssetHistory>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<WarehouseRecord> WarehouseRecords { get; set; } = new List<WarehouseRecord>();
}
