# WorkforceManagement — EF6 Code First TPH Practice

A step-by-step reference: start with a workforce model, represent it with C# inheritance, create a SQL Server database, and inspect how Entity Framework 6 stores different employee types in one table.

**TPH means Table Per Hierarchy.** One table stores the whole employee inheritance hierarchy, and a `Discriminator` column identifies the concrete type of each row.

This exercise uses **Entity Framework 6 (EF6)**, Code First, SQL Server, conventions, and Data Annotations. It uses no Fluent API mapping.


## 1. Start with the requirements

We want to store three kinds of workers:

| Worker type | Shared information | Information specific to this type |
| --- | --- | --- |
| Full-time employee | ID, name, birth date, address | Monthly salary and annual bonus |
| Part-time employee | ID, name, birth date, address | Hourly rate and hours per week |
| Contractor | ID, name, birth date, address | Hourly rate and contract end date |

Before thinking about tables, ask two questions:

1. What information belongs to every employee?
2. What information only makes sense for a particular employee type?

The answers determine where each property belongs. For example, every employee has a name, but a contractor does not have the full-time employee's annual bonus in this model.

This is a persistence exercise. The sample birth dates and payment values are practice data; the project does not yet implement age validation, payroll calculations, or employment rules.

## 2. Turn the requirements into a class design

Use inheritance for the **is-an-employee** relationship, and composition for the **has-an-address** relationship.

```text
Employee (abstract)
├── ID
├── Name
├── BirthDate
├── Address ──> Address (complex type)
│              ├── City
│              ├── Street
│              └── ZipCode (optional)
│
├── FullTimeEmployee
│   ├── MonthlySalary
│   └── AnnualBonus
├── PartTimeEmployee
│   ├── HourlyRate
│   └── HoursPerWeek
└── Contractor
    ├── HourlyRate
    └── ContractEndDate
```

Why these decisions?

- `Employee` holds the common identity and information, avoiding repeated definitions in three classes.
- `Employee` is abstract because this exercise creates specific employee types, not a generic employee.
- `Address` groups related values. It has no independent identity or lifecycle in this design.
- Each concrete class contains its own additional properties.
- `HourlyRate` stays in both `PartTimeEmployee` and `Contractor`, matching the practiced design. It is not on `Employee`, because the full-time type does not use an hourly rate here.

An abstract class can still be the root of an EF model. `abstract` prevents `new Employee()`; it does not prevent EF from mapping its inherited properties.

## 3. Set up EF6

For a traditional Visual Studio Console App (.NET Framework) setup, open **Tools → NuGet Package Manager → Package Manager Console**. Select the workforce project as the console's Default project and use the console application as the startup project.

Install the EF6 package if it is not already installed:

```powershell
Install-Package EntityFramework
```

The package is named `EntityFramework`. EF6's context namespace is `System.Data.Entity`; the examples here do not use `Microsoft.EntityFrameworkCore` or EF Core migration commands. Keep the project's existing EF6 version when revisiting the completed exercise.

Suggested file organization:

```text
WorkforceManagement/
├── Address.cs
├── Employee.cs
├── FullTimeEmployee.cs
├── PartTimeEmployee.cs
├── Contractor.cs
├── Context.cs
├── Program.cs
├── App.config
└── Migrations/             # Created by migration tooling
```

## 4. Define Address as a complex type

**Reconstructed minimal model listing:**

```csharp
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkforceManagement
{
    [ComplexType]
    public class Address
    {
        public string City { get; set; }
        public string Street { get; set; }
        public int? ZipCode { get; set; }
    }
}
```

`[ComplexType]` tells EF that `Address` is a group of values embedded in its owner. We do not give it an `ID`, create a `DbSet<Address>`, or create an independent `Addresses` table.

The address values become employee-table columns:

| C# property path | SQL column |
| --- | --- |
| `Employee.Address.City` | `Address_City` |
| `Employee.Address.Street` | `Address_Street` |
| `Employee.Address.ZipCode` | `Address_ZipCode` |

`int?` means `Nullable<int>`: the zip code can hold an integer or `null`. Omitting it from Mohammed's address therefore stores `NULL`, not `0`.

Create an `Address` instance for each employee. EF6 does not support a null complex-type instance; individual members may be nullable. Here the missing zip code is a missing value inside an existing address object. See Microsoft's [EF6 Data Annotations reference](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/data-annotations) for complex-type mapping.

## 5. Define the abstract Employee base class

**Reconstructed minimal model listing:**

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace WorkforceManagement
{
    public abstract class Employee
    {
        [Key]
        public int ID { get; set; }

        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public Address Address { get; set; }
    }
}
```

`[Key]` explicitly identifies the primary key. EF also recognizes `ID` by convention, so the annotation is optional for this particular name. An integer primary key is identity-generated by the usual SQL Server Code First convention; the insertion code does not assign it.

The minimal listing does not invent `[Required]` or string-length limits missing from the retrieved source. If the actual class has those annotations, its migration will reflect them. `BirthDate` is a non-nullable value type, shared by every employee.

## 6. Define the three concrete types

**Reconstructed minimal model listings:** each class goes in its corresponding file.

### FullTimeEmployee.cs

```csharp
namespace WorkforceManagement
{
    public class FullTimeEmployee : Employee
    {
        public decimal MonthlySalary { get; set; }
        public decimal AnnualBonus { get; set; }
    }
}
```

### PartTimeEmployee.cs

```csharp
namespace WorkforceManagement
{
    public class PartTimeEmployee : Employee
    {
        public decimal HourlyRate { get; set; }
        public int HoursPerWeek { get; set; }
    }
}
```

### Contractor.cs

```csharp
using System;

namespace WorkforceManagement
{
    public class Contractor : Employee
    {
        public decimal HourlyRate { get; set; }
        public DateTime ContractEndDate { get; set; }
    }
}
```

These teaching listings use `decimal` for money. Each concrete class inherits `ID`, `Name`, `BirthDate`, and `Address`; those properties do not need to be declared again.

Notice that `PartTimeEmployee.HourlyRate` and `Contractor.HourlyRate` are two separately declared properties. Their matching names do not make them one inherited property. This matters when EF puts both types into the same SQL table.

## 7. Create the DbContext

`DbContext` connects the model to the database and tracks objects that will be saved.

**Reconstructed context example:** the confirmed class name is `Context`, and the set is `Employees`. This example uses an explicit named connection string.

```csharp
using System.Data.Entity;

namespace WorkforceManagement
{
    public class Context : DbContext
    {
        public Context() : base("name=WorkforceDB")
        {
        }

        public DbSet<Employee> Employees { get; set; }
    }
}
```

### Why only `DbSet<Employee>`?

All three concrete types are employees, so all can be added to the base set. EF6 discovers derived types in the same assembly as the base class; separate sets are unnecessary for this exercise. The number of `DbSet` properties does not, by itself, choose the inheritance mapping strategy. See [EF6 conventions](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/conventions/built-in).

### Why does this produce TPH?

TPH is EF6 Code First's default inheritance strategy. We keep that convention and do not assign separate tables to the derived types. No `OnModelCreating` override or manual discriminator property is needed. See Microsoft's [EF6 inheritance tutorial](https://learn.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/implementing-inheritance-with-the-entity-framework-in-an-asp-net-mvc-application).

### Connection configuration

The screenshot shows `localhost\SQLEXPRESS` and `WorkforceDB`. The following is an **example connection entry** for that server/database using Windows authentication; the original authentication settings were not available.

Merge this section into `App.config` inside `<configuration>`, keeping EF's existing configuration sections and provider settings. If `<configSections>` exists, it must remain the first child of `<configuration>`.

```xml
<connectionStrings>
  <add name="WorkforceDB"
       connectionString="Data Source=localhost\SQLEXPRESS;Initial Catalog=WorkforceDB;Integrated Security=True"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

`name=WorkforceDB` in the constructor refers to the entry's `name`; `Initial Catalog` identifies the database. Use the working connection already configured in the actual project. See [EF6 connection strings and models](https://learn.microsoft.com/en-us/ef/ef6/fundamentals/configuring/connection-strings).

## 8. Generate InitialCreate

Build the project first. For a new copy of the exercise with no migrations, run these commands in Visual Studio's Package Manager Console:

```powershell
Enable-Migrations
Add-Migration InitialCreate
```

`Enable-Migrations` creates migration configuration. `Add-Migration` generates the schema-change code from the model. It does not apply that code to the database. If `InitialCreate` already exists in the completed project, open it instead of creating another one. The official [EF6 migrations guide](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/migrations/) explains this workflow.

### Read the migration before applying it

The following is a **representative reconstruction** using the confirmed column layout and the minimal model above. It is not the recovered original migration. String constraints and exact generated column ordering/naming must be checked against the real file.

```csharp
using System.Data.Entity.Migrations;

namespace WorkforceManagement.Migrations
{
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Employees",
                c => new
                {
                    ID = c.Int(nullable: false, identity: true),
                    Name = c.String(),
                    BirthDate = c.DateTime(nullable: false),
                    Address_City = c.String(),
                    Address_Street = c.String(),
                    Address_ZipCode = c.Int(),
                    HourlyRate = c.Decimal(precision: 18, scale: 2),
                    ContractEndDate = c.DateTime(),
                    MonthlySalary = c.Decimal(precision: 18, scale: 2),
                    AnnualBonus = c.Decimal(precision: 18, scale: 2),
                    HourlyRate1 = c.Decimal(precision: 18, scale: 2),
                    HoursPerWeek = c.Int(),
                    Discriminator = c.String(nullable: false, maxLength: 128)
                })
                .PrimaryKey(t => t.ID);
        }

        public override void Down()
        {
            DropTable("dbo.Employees");
        }
    }
}
```

The migration API above describes database operations. It is generated migration code, not Fluent API model configuration.

| Migration part | What to learn from it |
| --- | --- |
| `Up()` | Operations to apply this migration |
| One `CreateTable("dbo.Employees", ...)` | The whole employee hierarchy shares one table |
| `identity: true` | SQL Server generates employee IDs |
| `.PrimaryKey(t => t.ID)` | `ID` identifies each row |
| `Address_*` columns | The complex type is flattened into its owner |
| Columns from all three derived types | One table accommodates every employee kind |
| `Discriminator` | EF records the concrete type represented by a row |
| `Down()` | Reverse operation; here it drops the employee table on rollback |

In these column definitions, omitting `nullable: false` permits SQL `NULL`. For `decimal(18, 2)`, 18 is the total digit capacity and 2 is the number of digits after the decimal point. Compare these settings with your actual generated migration rather than manually replacing it with this example.

## 9. Understand the resulting TPH table

The confirmed application table has this shape:

```text
dbo.Employees
├── ID
├── Name
├── BirthDate
├── Address_City
├── Address_Street
├── Address_ZipCode
├── HourlyRate          # Contractor in this project's result
├── ContractEndDate
├── MonthlySalary
├── AnnualBonus
├── HourlyRate1         # PartTimeEmployee in this project's result
├── HoursPerWeek
└── Discriminator
```

### What does Discriminator do?

When saving, EF uses the object's runtime type to choose the discriminator value. When reading, EF uses that stored value to create the appropriate concrete C# type.

| Concrete C# type | Observed discriminator |
| --- | --- |
| `FullTimeEmployee` | `FullTimeEmployee` |
| `PartTimeEmployee` | `PartTimeEmployee` |
| `Contractor` | `Contractor` |

You do not set this column in the object initializer. EF manages it as part of the inheritance mapping. No generic `Employee` object is inserted because the base class is abstract.

### Why are derived-type columns nullable?

Consider Ahmed, a full-time employee. His row needs a salary and bonus, but it has no contractor end date or part-time weekly hours. Those table columns still exist because other rows need them, so Ahmed's unused columns contain `NULL`.

This is different from making `MonthlySalary` a `decimal?` in C#. A `FullTimeEmployee` has a non-nullable salary property, while the shared SQL column must allow nulls for employees of other types. `[Required]` on a derived property cannot make that shared column universally `NOT NULL`. Microsoft's [EF6 mapping reference](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/fluent/types-and-properties) explains this TPH constraint; this project still uses conventions and annotations only.

There are two different reasons for `NULL` in the observed rows:

| Example | Reason |
| --- | --- |
| Mohammed's `Address_ZipCode` | An optional value was omitted (`int?`) |
| Ahmed's `ContractEndDate` | That property does not belong to his employee type |

Do not replace inapplicable values with `0` or dummy dates. A zero amount and an inapplicable property mean different things.

### Why HourlyRate and HourlyRate1?

Both sibling classes independently declare a property named `HourlyRate`. In the observed EF6 mapping, EF gives those separate properties separate columns and adds a suffix to keep the SQL names unique:

| Property | Observed SQL column |
| --- | --- |
| `Contractor.HourlyRate` | `HourlyRate` |
| `PartTimeEmployee.HourlyRate` | `HourlyRate1` |

The suffix has no business meaning. It does not mean an employee has a second rate, and it is not a second table. Treat which type receives the suffix as an observed result of this project's generated mapping, not a universal naming guarantee. The insertion results confirm the correspondence: Youssef's `300` is in `HourlyRate`; Mohammed's `150` is in `HourlyRate1`.

Keep these names while reproducing the exercise. Renaming a SQL column directly would leave the database inconsistent with EF's mapping.

## 10. Apply the migration

After inspecting `InitialCreate`, run:

```powershell
Update-Database
```

EF applies pending migration operations to the configured database. Use `Update-Database -Verbose` when you want to see the SQL being executed.

In SQL Server Management Studio, connect to the configured server and refresh:

```text
Databases
└── WorkforceDB
    └── Tables
        ├── dbo.__MigrationHistory
        └── dbo.Employees
```

`__MigrationHistory` is EF6's migration bookkeeping table. Saying TPH creates “one table” means one table for the employee hierarchy; it does not exclude EF infrastructure tables.

Open `dbo.Employees` and verify the columns before inserting anything. An empty editable grid may show a `NULL` placeholder row with an asterisk; that is the editor's new-row placeholder, not an inserted employee and not proof that every column allows nulls.

## 11. Insert the three employees

The following preserves the user's final employee values, variable names, and insertion sequence. Unused imports are removed, and a `using` block disposes the context after saving.

```csharp
using System;

namespace WorkforceManagement
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (Context context = new Context())
            {
                var Emp_1 = new FullTimeEmployee
                {
                    Name = "Ahmed",
                    BirthDate = new DateTime(2001, 09, 16),
                    Address = new Address
                    {
                        City = "Almahalla",
                        Street = "Tawheed",
                        ZipCode = 1250
                    },
                    MonthlySalary = 20000,
                    AnnualBonus = 5000
                };

                var Emp_2 = new PartTimeEmployee
                {
                    Name = "Mohammed",
                    BirthDate = new DateTime(2010, 02, 14),
                    Address = new Address
                    {
                        City = "Cairo",
                        Street = "negm"
                    },
                    HourlyRate = 150,
                    HoursPerWeek = 20
                };

                var Emp_3 = new Contractor
                {
                    Name = "Youssef",
                    BirthDate = new DateTime(2016, 12, 12),
                    Address = new Address
                    {
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
}
```

### What happens during insertion?

1. Each initializer creates a concrete C# employee object and its address.
2. `Add` tells the context to track that employee as a new entity. All three calls work because the objects inherit from `Employee`.
3. `SaveChanges()` writes the pending inserts to the database.
4. EF supplies discriminator values and maps each object's values to the appropriate columns.
5. SQL Server generates the identity values. All types share the same table and primary-key space.

Run this insertion program once for the three-row exercise. Running it again adds three more employees; the code does not check for existing rows.

### The DateTime mistake corrected during practice

```csharp
// Wrong: subtraction produces one numeric argument.
new DateTime(2001 - 09 - 16)

// Correct: separate year, month, and day arguments.
new DateTime(2001, 09, 16)
```

The first expression calculates `1976` and calls the ticks constructor. It does not represent September 16, 2001. Use the three-argument constructor for each birth date and the contract end date.

## 12. Inspect dbo.Employees

Refresh the table in SQL Server Management Studio, or run:

```sql
USE [WorkforceDB];

SELECT
    ID, Name, BirthDate,
    Address_City, Address_Street, Address_ZipCode,
    HourlyRate, ContractEndDate,
    MonthlySalary, AnnualBonus,
    HourlyRate1, HoursPerWeek,
    Discriminator
FROM dbo.Employees
ORDER BY ID;
```

The final screenshot confirms these values. The table is split below for readability; all fields are stored in the same physical row per employee. Actual identity values are omitted because they were not visible in the final screenshot.

### Shared and address values

| Name | BirthDate | Address_City | Address_Street | Address_ZipCode |
| --- | --- | --- | --- | --- |
| Ahmed | 2001-09-16 | Almahalla | Tawheed | 1250 |
| Mohammed | 2010-02-14 | Cairo | negm | NULL |
| Youssef | 2016-12-12 | Alexandria | Shaarawy | 12437 |

### Derived values and type identity

| Name | MonthlySalary | AnnualBonus | HourlyRate | ContractEndDate | HourlyRate1 | HoursPerWeek | Discriminator |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Ahmed | 20000.00 | 5000.00 | NULL | NULL | NULL | NULL | FullTimeEmployee |
| Mohammed | NULL | NULL | NULL | NULL | 150.00 | 20 | PartTimeEmployee |
| Youssef | NULL | NULL | 300.00 | 2027-01-01 | NULL | NULL | Contractor |

Dates are shown without the midnight time component to keep the tables compact.

Read each row as evidence of the mapping:

- **Ahmed:** salary and bonus are populated; the discriminator identifies a full-time employee; all other derived properties are inapplicable.
- **Mohammed:** `HourlyRate1` and `HoursPerWeek` are populated; `Address_ZipCode` is `NULL` because the initializer omitted it.
- **Youssef:** `HourlyRate` and `ContractEndDate` are populated; salary, bonus, and part-time hours are inapplicable.

Together, the rows demonstrate both complex-type flattening and inheritance mapping.

## 13. Review the complete flow

```text
Requirements
    ↓ separate shared information from type-specific information
C# classes + Address complex type
    ↓ expose the hierarchy through DbSet<Employee>
Context + EF6 conventions and Data Annotations
    ↓ Add-Migration InitialCreate
Migration describing one Employees table
    ↓ Update-Database
SQL Server schema
    ↓ create objects, Add, SaveChanges
Three rows with type-specific values and discriminators
    ↓ inspect dbo.Employees
Confirmed TPH behavior
```

Keep these distinctions clear:

| Concept | Responsibility |
| --- | --- |
| C# inheritance | Describes which types are employees |
| `abstract` | Prevents constructing a generic base employee |
| `[ComplexType]` | Groups address values without a separate entity identity |
| `DbSet<Employee>` | Provides access to employee entities in the context |
| TPH mapping | Stores the employee hierarchy in one table |
| `Discriminator` | Identifies the concrete type of a stored row |
| `Add-Migration` | Generates schema-change code |
| `Update-Database` | Applies pending schema changes |
| `Add` + `SaveChanges` | Tracks and persists new employee objects |

## 14. Common questions when revisiting this exercise

**Why is there no Employees table for each subclass?**

TPH maps the hierarchy to one table. Separate subclasses in C# do not require separate SQL tables.

**Why is there no Addresses table?**

`Address` is a complex type in this design. Its members are columns in `Employees`.

**Does a nullable salary column mean a FullTimeEmployee should have no salary?**

No. The shared table must also store other employee types. SQL nullability across the hierarchy and the C# property's nullability answer different questions.

**Should I add Discriminator to Employee?**

No. This exercise relies on EF6's automatically managed discriminator mapping.

**Why did Update-Database not insert Ahmed, Mohammed, and Youssef?**

The migration creates the schema. These objects are inserted by running `Program.cs` and reaching `SaveChanges()`; the exercise does not insert them through migration seeding.

**Why are there six rows after running again?**

The program performs fresh inserts each time. It has no duplicate check.

**What if I cannot see the table or data?**

Check that the startup application's connection and the server/database open in SQL Server Management Studio match. Confirm the migration completed, the program reached `SaveChanges()`, and the table view was refreshed.

**What comes next?**

The completed exercise ends at inserting and inspecting the rows. A later exercise can query all employees and filter with `OfType<FullTimeEmployee>()` or `OfType<Contractor>()` to observe how EF turns rows back into the correct C# objects.

## Quick revision checklist

- [ ] I can explain why `Employee` is abstract.
- [ ] I can distinguish an entity from the `Address` complex type.
- [ ] I can explain why one `DbSet<Employee>` is sufficient here.
- [ ] I can read `InitialCreate` and identify the single hierarchy table.
- [ ] I can distinguish generating a migration from applying one.
- [ ] I can explain the discriminator without adding it to the C# classes.
- [ ] I can explain both reasons for `NULL` in the observed rows.
- [ ] I know which property maps to `HourlyRate` and `HourlyRate1` in this result.
- [ ] I can insert each concrete type through the base set.
- [ ] I can match the three stored rows back to their C# employee types.

**TPH to remember:** one inheritance hierarchy, one shared table, one discriminator per row, and nullable columns where a property does not apply to every employee type.
