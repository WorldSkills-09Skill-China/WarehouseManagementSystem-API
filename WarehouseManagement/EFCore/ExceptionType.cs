using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class ExceptionType
{
    public int Id { get; set; }

    public string? Problem { get; set; }

    public virtual ICollection<Exception> Exceptions { get; set; } = new List<Exception>();
}
