using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class FixedAssetDetail
{
    public int Id { get; set; }

    public string Specification { get; set; } = null!;

    public virtual ICollection<FixedAsset> FixedAssets { get; set; } = new List<FixedAsset>();
}
