using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkforceManagement {
  internal class Contractor: Employee {
    public decimal HourlyRate { get; set; }
    public DateTime ContractEndDate { get; set; }
  }
}
