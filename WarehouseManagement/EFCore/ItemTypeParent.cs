using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class ItemTypeParent
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ItemTypeSon> ItemTypeSons { get; set; } = new List<ItemTypeSon>();
}
