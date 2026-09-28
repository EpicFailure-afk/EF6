using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkforceManagement {
  internal class FullTimeEmployee: Employee {
    public decimal MonthlySalary { get; set; }
    public decimal AnnualBonus { get; set; }
  }
}
