using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class ExceptionState
{
    public int Id { get; set; }

    public string State { get; set; } = null!;

    public virtual ICollection<Exception> Exceptions { get; set; } = new List<Exception>();
}
