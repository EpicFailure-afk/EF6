using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;



namespace CodeFirst {
  [ComplexType]
  internal class Address {
    public string City { get; set; }
    public string Street { get; set; }
    public int? ZipCode { get; set; }
  }
}
