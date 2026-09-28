# Day-4
# Code First 
<br>
means to start from code not from designing database
helps on:
  - if i want to work on different data (Database, XML files or data from mem)

<hr>

### **First** we need to start creating the database from .Net itself
   1. create 2 normal **Department**, **Employee** classes with their props  
   2. create **Context** class, and inherit from <u>**DbContext**</u>
      - by default in EF the package "DbContext" is not exist so we will download it through command line of package manager **NuGet package manager**
      - ![](Pasted%20image%2020260923181527.png) (Ef6)
      - ```cs
        // must use this lib
        using System.Data.Entity; 
        internal class Context : DbContext {
        
        }
        ```

at the class **Context**
- create 2 DbSet for Department and Employee

```cs

    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
```

<hr>

## **Migration**
- to link the app to db
- when update on classes that will also update the db 
<br>
> through ( tools --> NuGet package manager --> Package Manager Console )
- 3 Important commands :
  1. this command written only one time on Project level
     `enable-migrations`
  2. `add-migration init` 
  3. `update-databse` --> to reflect the changes to database

<br>

> every time we wanna reflect changes to database we must write the last two commands 

<hr>

## Steps:
1. create the classes we need with its props 
2. create class **Context** 
   . this is the layer between client and server
   . adding the connection string through the ctor of Context class  
   . inherit from DbContext + downloading the pkg of *EF6*
3. specify the DbSets we need for our example : Employees from Employee and Departments from Department 
4. the three commands 

<hr>

## Rules (what happened behind the scene)
 After the command *add-migration init* the context creates two tables
- At `EntityFW\CodeFirst\CodeFirst\Migrations\202609231540529_init.cs` :
  **Departments** and **Employees**, <u>Why?</u>
  - based on DbSets at the context it decides what exactly the classes will be added as a tables 
- By default when i create table, its name will be the plural of the class name 
  - so the name is based on the class name not the DbSet name 
- By default according to the type of the property 
  - value type will not allow null 
  - ref type will allow null 
- By default if there is **ID/id int/long** the EF will consider it as a **Primary key**
- the file `202609231540529_init.cs` can be deleted without affecting the project

<hr>

## updating the classes and how to reflect the changes to databse

#### adding a new prop in Employee class


```cs
public string Address { get; set; }
```

<br>

- the command `add-migration updaes` will create a new cs file  contains
  ```cs
          public override void Up()
        {
            AddColumn("dbo.Employees", "Address", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "Address");
        }
  ```

> [!NOTE]
> `public override void Down()` when i wanna revoke the changes 

<br>

- every time EF compare the structure of the classes and the last snapshot in the table `__MigrationHistory`
  - that table contains snapshots carries the last database structure  
- `update-database` will update the database and add the new column 
  and add a new snapshot at migration history table 

<br>

> [!NOTE]
> snapshot at migrationHistory table does not track the updates happen on the side of server (database)

<hr>

#### query to insert data 

```cs
      Context context = new Context();

      context.Departments.Add(new Department {
        Name = "SD"
      });

      context.SaveChanges();
```


--- 
---

# Day-5
# How to change the default rules 
<br>

## Data Annotation 
Group of Attributes  
Change columns names, Tables and their datatype

- Change Table Name **on namespace level**

```cs
namespace CodeFirst {
  // Data Annotation 
  [Table("Department", Schema ="HR")]
  internal class Department {
    public int ID { get; set; }
    public string Name { get; set; }
  }
}
```

<br>

- change attributes at Table:

```cs
namespace CodeFirst {
  internal class Employee {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; }
    
    [Column("FullName")]
    [Required, MaxLength(100)]
    public string Name { get; set; }
    public double Salary { get; set; }
    public string Address { get; set; }

    [Column(TypeName = "Date")]
    public DateTime Birthdate { get; set; }
  }
}
```

Salary is int, by default it's Not Null
  - to make it nullable --> public double**?** Salary { get; set; }

> [!NOTE]
> You can Edit migration code but without changing the structure 


---

## Linking 
--> Through **Navigation Properties**

<br>

the best is to add navigation prop at each class

Example: add a relation between Employee and Department 

- Employee.cs:

```cs
   public virtual Department Dept { set; get; }
```


- Department.cs 

```cs
  public virtual ICollection<Employee> Employees { get; set;}
```

**Naming** of the foreign key by default is `class_ID`

<br>

to change the name of the foreign key:

- Adding a new prop at Department `DepartmentID` then link this prop to the navigation prop
   `public int DepartmentId { get; set; }`
   link this prop to the navigation prop through 2 ways
     - either on the prop or on the navigation prop
       ```cs
	       // add new prop to be a FK 
			[ForeignKey("Dept")] 
			public int DepartmentID { get; set; }
			// Navigation props
			[ForeignKey("DepartmentID")]
			public virtual Department Dept { get; set; }
       ```

---

If there a relation many-many between Project and Employee

> best Solution is to create another class to link the other two classes

**Class WorksFor**

```cs
namespace CodeFirst {
  internal class WorksFor {
    public int ID { get; set; }
    public int Hours { get; set; }

    //Nav-Prop
    public virtual Employee Employee { get; set; }
    public virtual Project Project { get; set; }

  }
}
```

also add a nav-prop `ICollection<WorksFor> WorksFors` in each class **Employee** and **Project**


> [!NOTE]
> Updates on Database Done Successfully through **migaration (updates(2, 3 and 4)) For Day-5**


---

# Day-6

if there a relation that:
Employee supervise department  
and this department has more than one supervisor  


- Department will have a collection of Employees **that already done**
  and also has a collection of Employees but for **supervisor**
- Employee will has two objects of Department one called **Dept** already done
  and another object for supervisor **SupervisedDept**
  and add a FK --> **SupervisedDepartmentID** 

```cs
public int DepartmentID { get; set; }
public int? SupervisedDepartmentID { get; set; }

// Navigation props
[ForeignKey("DepartmentID")]
public virtual Department Dept { get; set; }

[ForeignKey("SupervisedDepartmentID")]
public virtual Department SupervisedDept { get; set; }
```

there is a problem here: how could the context know the collection of Employees will get through the object of Dept and the collection of Supervisors will get through the object of SupervisedDept

<br>

to sole this problem we have attribute called --> **InverseProperty**

```cs
    [InverseProperty("Dept")]
    public virtual ICollection<Employee> Employees { get; set; }

    [InverseProperty("SupervisedDept")]
    public virtual ICollection<Employee> Supervisors { get; set; }
```

<hr>

### Complex Types 

like Composite attribute at SQL 

if we have Address
```cs
public string Address { get; set; }
```

that address consists of {city, street, zipcode}

we can create a complex type through a property called --> **ComplexType**



