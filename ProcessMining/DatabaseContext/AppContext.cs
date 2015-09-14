using ProcessMining.DataClasses;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMining.DatabaseContext
{
    public class AppContext:DbContext
    {
        public AppContext()
            :base("ProcessMiningDB")
        {
            Database.SetInitializer<AppContext>(new CreateDatabaseIfNotExists<AppContext>());
            this.Configuration.LazyLoadingEnabled = false;
        }
        public DbSet<Log> Logs { get; set; }
        public DbSet<Process> Processes { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Employee> Employees { get; set; }
    }
}
