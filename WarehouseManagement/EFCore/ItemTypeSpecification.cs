using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class ItemTypeSpecification
{
    public int Id { get; set; }

    public int ItemTypeId { get; set; }

    public int ItemSpecificationId { get; set; }

    public virtual ItemSpecification ItemSpecification { get; set; } = null!;

    public virtual ItemType ItemType { get; set; } = null!;

    public virtual ICollection<Item> Items { get; set; } = new List<Item>();
}
