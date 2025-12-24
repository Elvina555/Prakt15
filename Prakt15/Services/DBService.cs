using Prakt15.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prakt15.Services
{
    public sealed class DBService
    {
        private static readonly Lazy<DBService> _instance = new Lazy<DBService>(() => new DBService());
        public static DBService Instance => _instance.Value;

        public YourDbContext Context { get; private set; }

        private DBService()
        {
            Context = new YourDbContext();
        }

        public void RefreshContext()
        {
            Context.Dispose();
            Context = new YourDbContext();
        }
    }
}
