using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class PlaceForStorageDetailState
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<PlaceForStorageDetail> PlaceForStorageDetails { get; set; } = new List<PlaceForStorageDetail>();
}
