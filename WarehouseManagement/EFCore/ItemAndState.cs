using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class ItemAndState
{
    public int Id { get; set; }

    public int ItemId { get; set; }

    public int HazardRecordDetailId { get; set; }

    public int? HazardStateId { get; set; }

    public virtual HazardRecordDetail HazardRecordDetail { get; set; } = null!;

    public virtual HazardState? HazardState { get; set; }

    public virtual Item Item { get; set; } = null!;
}
