using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class HazardState
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ItemAndState> ItemAndStates { get; set; } = new List<ItemAndState>();
}
