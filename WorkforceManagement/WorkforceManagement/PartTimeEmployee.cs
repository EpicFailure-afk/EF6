using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkforceManagement {
  internal class PartTimeEmployee: Employee {
    public decimal HourlyRate { get; set; }
    public int HoursPerWeek { get; set; }
  }
}
