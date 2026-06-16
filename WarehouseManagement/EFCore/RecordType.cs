using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class RecordType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<WarehouseRecord> WarehouseRecords { get; set; } = new List<WarehouseRecord>();
}
