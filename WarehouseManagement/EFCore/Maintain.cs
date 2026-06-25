using System;
using System.Collections.Generic;

namespace WarehouseManagement.EFCore;

public partial class Maintain
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Note { get; set; }

    public int ExceptionId { get; set; }
}
