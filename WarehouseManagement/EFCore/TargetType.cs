using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class TargetType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}
