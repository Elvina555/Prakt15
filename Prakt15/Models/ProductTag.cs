using System;
using System.Collections.Generic;

namespace Prakt15.Models;

public partial class ProductTag
{
    public double? ProductId { get; set; }
    public double? TagId { get; set; }

    public virtual Product? Product { get; set; }
    public virtual Tag? Tag { get; set; }
}