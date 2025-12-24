using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prakt15.Validation
{
    public static class EntityValidator
    {
        public static bool ValidateName(string name, string entityType, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                errorMessage = $"Введите название {entityType}";
                return false;
            }

            return true;
        }
    }
}
