using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prakt15.Validation
{
    public static class ProductValidator
    {
        public static bool ValidatePrice(string priceText, out double price, out string errorMessage)
        {
            price = 0;
            errorMessage = string.Empty;

            if (!double.TryParse(priceText, out price))
            {
                errorMessage = "Введите корректную цену (только цифры и точка)";
                return false;
            }

            if (price < 0)
            {
                errorMessage = "Цена не может быть отрицательной";
                return false;
            }

            return true;
        }

        public static bool ValidateStock(string stockText, out double stock, out string errorMessage)
        {
            stock = 0;
            errorMessage = string.Empty;

            if (!double.TryParse(stockText, out stock))
            {
                errorMessage = "Введите корректное количество (только цифры)";
                return false;
            }

            if (stock < 0)
            {
                errorMessage = "Количество не может быть отрицательным";
                return false;
            }

            return true;
        }

        public static bool ValidateRating(string ratingText, out double? rating, out string errorMessage)
        {
            rating = null;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(ratingText))
            {
                return true; 
            }

            if (!double.TryParse(ratingText, out double ratingValue))
            {
                errorMessage = "Введите корректный рейтинг (от 0 до 5)";
                return false;
            }

            if (ratingValue < 0 || ratingValue > 5)
            {
                errorMessage = "Рейтинг должен быть от 0 до 5";
                return false;
            }

            rating = ratingValue;
            return true;
        }

        public static bool ValidateRequiredField(string field, string fieldName, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(field))
            {
                errorMessage = $"Введите {fieldName}";
                return false;
            }

            return true;
        }
    }
}
