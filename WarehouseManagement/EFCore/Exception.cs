using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class Exception
{
    public int Id { get; set; }

    public int TargetTypeId { get; set; }

    public int? FixedAssetId { get; set; }

    public int? BatchId { get; set; }

    public int ExceptionTypeId { get; set; }

    public DateTime CreateTime { get; set; }

    public int ExceptionStateId { get; set; }

    public virtual WarehouseRecord? Batch { get; set; }

    public virtual ExceptionState ExceptionState { get; set; } = null!;

    public virtual ExceptionType ExceptionType { get; set; } = null!;

    public virtual FixedAsset? FixedAsset { get; set; }
}
