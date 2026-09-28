using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkforceManagement {
  internal class Context: DbContext {
    public Context(): base(@"Data source= localhost\SQLEXPRESS; initial catalog = WorkforceDB; Integrated security = true") {

    }
    public DbSet<Employee> Employees { get; set; }
  }
}
