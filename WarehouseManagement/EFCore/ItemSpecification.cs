using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class ItemSpecification
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ItemTypeSpecification> ItemTypeSpecifications { get; set; } = new List<ItemTypeSpecification>();
}
