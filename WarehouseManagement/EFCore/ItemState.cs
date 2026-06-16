using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class ItemState
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}
