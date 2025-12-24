using System;
using System.Collections.Generic;

namespace Prakt15.Models;

public partial class Brand
{
    public double Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
