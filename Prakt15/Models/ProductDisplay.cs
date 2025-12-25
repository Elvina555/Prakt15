using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Prakt15.Models
{
    public class ProductDisplay
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double Price { get; set; }
        public double Stock { get; set; }
        public double? Rating { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = new List<string>();

        public bool IsLowStock => Stock < 10;

        public ProductDisplay()
        {
        }

        public ProductDisplay(Product product)
        {
            Id = product.Id;
            Name = product.Name ?? string.Empty;
            Description = product.Description;
            Price = product.Price ?? 0;
            Stock = product.Stock ?? 0;
            Rating = product.Rating;
            CategoryName = product.Category?.Name ?? string.Empty;
            BrandName = product.Brand?.Name ?? string.Empty;
            Tags = new List<string>();
            if (product.ProductTags != null)
            {
                Tags = product.ProductTags
                    .Where(pt => pt.Tag != null && !string.IsNullOrEmpty(pt.Tag.Name))
                    .Select(pt => pt.Tag!.Name!)
                    .ToList();
            }
        }
        public string TagsString
        {
            get
            {
                if (Tags == null || !Tags.Any())
                    return string.Empty;

                return string.Join(" ", Tags.Select(t => $"#{t}"));
            }
        }
    }
}


