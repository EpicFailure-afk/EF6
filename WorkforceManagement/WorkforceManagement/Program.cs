using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkforceManagement {
  internal class Program {
    static void Main(string[] args) {
      Context context = new Context();

      var Emp_1 = new FullTimeEmployee {
        Name = "Ahmed",
        BirthDate = new DateTime(2001, 09, 16),
        Address = new Address {
          City = "Almahalla",
          Street = "Tawheed",
          ZipCode = 1250,
        },
        MonthlySalary = 20000,
        AnnualBonus = 5000
      };

      var Emp_2 = new PartTimeEmployee {
        Name = "Mohammed",
        BirthDate = new DateTime(2010, 02, 14),
        Address = new Address {
          City = "Cairo",
          Street = "negm"
        },
        HourlyRate = 150,
        HoursPerWeek = 20
      };

      var Emp_3 = new Contractor {
        Name = "Youssef",
        BirthDate = new DateTime(2016, 12, 12),
        Address = new Address {
          City = "Alexandria",
          Street = "Shaarawy",
          ZipCode = 12437
        },
        HourlyRate = 300,
        ContractEndDate = new DateTime(2027, 01, 01)
      };

      context.Employees.Add(Emp_1);
      context.Employees.Add(Emp_2);
      context.Employees.Add(Emp_3);

      context.SaveChanges();
    }
  }
}
