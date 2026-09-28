using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkforceManagement {
  internal abstract class Employee {
    public int ID { get; set; }
    [Required]
    public string Name { get; set; }
    public DateTime BirthDate { get; set; }
    [Required]
    public Address Address { get; set; }
  }
}
