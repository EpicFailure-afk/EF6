using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkforceManagement {

  [ComplexType]  
  internal class Address {
    [Required]
    public string City { get; set; }
    [Required]
    public string Street { get; set; }
    public int? ZipCode { get; set; }
  }
}
