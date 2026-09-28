using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFirst {
  internal class Employee {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }
    
    [Column("FullName")]
    [Required, MaxLength(100)]
    public string Name { get; set; }
    public double Salary { get; set; }
    //public string Address { get; set; }
    public Address Address { get; set; }

    [Column(TypeName = "Date")]
    public DateTime Birthdate { get; set; }

    // add new prop to be a FK 
    // [ForeignKey("Dept")] 
    public int DepartmentID { get; set; }
    public int? SupervisedDepartmentID { get; set; }

    // Navigation props
    [ForeignKey("DepartmentID")]
    public virtual Department Dept { get; set; }
    
    [ForeignKey("SupervisedDepartmentID")]
    public virtual Department SupervisedDept { get; set; }

    public virtual ICollection<WorksFor> WorksFors { get; set; }
  }
}
