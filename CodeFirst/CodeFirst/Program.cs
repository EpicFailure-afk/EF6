using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirst {
  internal class Program {
    static void Main(string[] args) {
      Context context = new Context();

      
      context.Departments.Add(new Department {
        Name = "SD",
        Location = "El-mahalla"
      });

      context.SaveChanges();
      

      //foreach (var item in context.Departments) {
      //  Console.WriteLine(item.Name);
      //}

      // practice on ComplexTypes
      var Emp_1 = new Employee {
        Name = "Youssef",
        Salary = 10000,
        Birthdate = new DateTime(2016, 12, 12),

        DepartmentID = 1,

        Address = new Address {
          City = "Mahalla",
          Street = "Tawheed",
          ZipCode = 2500
        }
      };

      context.Employees.Add(Emp_1);
      context.SaveChanges();
    }
  }
}
