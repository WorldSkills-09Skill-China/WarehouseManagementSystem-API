using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class PlaceForStorage
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Image { get; set; }

    public virtual ICollection<PlaceForStorageDetail> PlaceForStorageDetails { get; set; } = new List<PlaceForStorageDetail>();
}
