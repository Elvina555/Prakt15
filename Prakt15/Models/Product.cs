using System;
using System.Collections.Generic;

namespace Prakt15.Models;

public partial class Product
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double? Price { get; set; }
    public double? Stock { get; set; }
    public double? Rating { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }

    public virtual Brand? Brand { get; set; }
    public virtual Category? Category { get; set; }
    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}
