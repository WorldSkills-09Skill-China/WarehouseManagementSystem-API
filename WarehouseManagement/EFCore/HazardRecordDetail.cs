using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class HazardRecordDetail
{
    public int Id { get; set; }

    public string Hint { get; set; } = null!;

    public virtual ICollection<ItemAndState> ItemAndStates { get; set; } = new List<ItemAndState>();
}
