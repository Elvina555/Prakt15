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
        public double Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public double Stock { get; set; }
        public double? Rating { get; set; }
        public string CreatedAt { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string TagsString { get; set; }
        public bool IsLowStock => Stock < 10;

        public ProductDisplay(Product product)
        {
            Id = product.Id;
            Name = product.Name ?? "Без названия";
            Description = product.Description ?? "";
            Price = product.Price ?? 0;
            Stock = product.Stock ?? 0;
            Rating = product.Rating;
            CreatedAt = product.CreatedAt ?? "";
            CategoryName = product.Category?.Name ?? "Без категории";
            BrandName = product.Brand?.Name ?? "Без бренда";

            if (product.ProductTags != null && product.ProductTags.Any())
            {
                var tagNames = product.ProductTags
                    .Where(pt => pt.Tag != null && !string.IsNullOrEmpty(pt.Tag.Name))
                    .Select(pt => $"#{pt.Tag.Name}")
                    .ToList();
                TagsString = string.Join(" ", tagNames);
            }
            else
            {
                TagsString = "";
            }
        }
    }
}

