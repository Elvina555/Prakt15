using System;
using System.Collections.Generic;

namespace Prakt15.Models;

public partial class Tag
{
    public double Id { get; set; }
    public string? Name { get; set; }
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}
