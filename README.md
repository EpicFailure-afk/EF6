# Entity Framework 6 — My Study Book

**Entity Framework is a .NET library that helps an application read and save database data using C# objects.**

Instead of manually converting every database row into an object and writing every SQL command yourself, you describe a model and let EF handle much of that work. You still need to understand tables, keys, relationships, and the SQL behavior your code produces.

This book connects the lessons in this repository: **Designer-based EF → querying and saving → Code First → relationships and migrations → complex types → TPH inheritance**. Read it from the beginning when learning, or use the topic index when revising.

> **Version matters:** Although the repository is named `EF-Core`, all three projects currently use **Entity Framework 6.5.2**, target **.NET Framework 4.8**, and use SQL Server. These facts come from their `packages.config` and project files. This book teaches **EF6**, not EF Core.

## Contents

1. [The basic idea: EF and ORM](#1-the-basic-idea-ef-and-orm)
2. [The repository and its three projects](#2-the-repository-and-its-three-projects)
3. [Designer-based development and EDMX](#3-designer-based-development-and-edmx)
4. [DbContext, DbSet, and connections](#4-dbcontext-dbset-and-connections)
5. [LINQ queries and deferred execution](#5-linq-queries-and-deferred-execution)
6. [Finding one entity](#6-finding-one-entity)
7. [Navigation properties and loading related data](#7-navigation-properties-and-loading-related-data)
8. [Change tracking, entity states, and CRUD](#8-change-tracking-entity-states-and-crud)
9. [Code First and conventions](#9-code-first-and-conventions)
10. [Data Annotations](#10-data-annotations)
11. [Relationships and foreign keys](#11-relationships-and-foreign-keys)
12. [Migrations and the history of this model](#12-migrations-and-the-history-of-this-model)
13. [Complex types and Address](#13-complex-types-and-address)
14. [Inheritance and TPH](#14-inheritance-and-tph)
15. [Discriminator, NULL, and duplicate property names](#15-discriminator-null-and-duplicate-property-names)
16. [The complete workforce example](#16-the-complete-workforce-example)
17. [Revisiting and running the projects](#17-revisiting-and-running-the-projects)
18. [Clarifications to keep beside the early notes](#18-clarifications-to-keep-beside-the-early-notes)
19. [Revision questions and practice](#19-revision-questions-and-practice)
20. [Source-file reading map](#20-source-file-reading-map)

## Key concepts at a glance

| Keyword | Small definition | Chapter |
| --- | --- | --- |
| **EF / ORM** | Maps database data to objects and back. | [1](#1-the-basic-idea-ef-and-orm) |
| **Entity / Primary Key** | An object with an identity; its key identifies it. | [1](#1-the-basic-idea-ef-and-orm) |
| **EDMX / Designer** | A visual EF model stored with database and mapping metadata. | [3](#3-designer-based-development-and-edmx) |
| **DbContext** | A session for querying, tracking, and saving entities. | [4](#4-dbcontext-dbset-and-connections) |
| **DbSet** | An entry point for working with an entity type. | [4](#4-dbcontext-dbset-and-connections) |
| **LINQ / IQueryable** | Express a query that EF can translate to SQL. | [5](#5-linq-queries-and-deferred-execution) |
| **Deferred Execution** | Building a query and executing it happen at different times. | [5](#5-linq-queries-and-deferred-execution) |
| **Materialization** | Turning returned data into C# objects or values. | [5](#5-linq-queries-and-deferred-execution) |
| **Find / Single / First** | Different ways to request one result. | [6](#6-finding-one-entity) |
| **Navigation Property** | A reference or collection leading to related entities. | [7](#7-navigation-properties-and-loading-related-data) |
| **Lazy / Eager / Explicit Loading** | Different ways to fetch related data. | [7](#7-navigation-properties-and-loading-related-data) |
| **Change Tracker / EntityState** | Records the context's knowledge of objects and pending changes. | [8](#8-change-tracking-entity-states-and-crud) |
| **CRUD / SaveChanges** | Create, read, update, delete; persist pending changes. | [8](#8-change-tracking-entity-states-and-crud) |
| **AsNoTracking** | Read entities without adding them to this context's change tracker. | [8](#8-change-tracking-entity-states-and-crud) |
| **Code First / Conventions** | Derive the model from classes and default mapping rules. | [9](#9-code-first-and-conventions) |
| **Data Annotations** | Attributes that describe validation or mapping rules. | [10](#10-data-annotations) |
| **ForeignKey / InverseProperty** | Connect an FK to a navigation, or pair opposite navigations. | [11](#11-relationships-and-foreign-keys) |
| **Link Entity / WorksFor** | Represents a relationship that has its own data, such as hours. | [11](#11-relationships-and-foreign-keys) |
| **Migration / Up / Down** | Versioned schema changes and their forward/reverse operations. | [12](#12-migrations-and-the-history-of-this-model) |
| **Complex Type** | A group of values with no independent entity identity. | [13](#13-complex-types-and-address) |
| **Abstract / Derived Class** | A shared base definition and its specific employee types. | [14](#14-inheritance-and-tph) |
| **TPH** | One table stores an inheritance hierarchy. | [14](#14-inheritance-and-tph) |
| **Discriminator** | Identifies the concrete type represented by a TPH row. | [15](#15-discriminator-null-and-duplicate-property-names) |
| **NULL / Nullable** | Represents a missing or inapplicable value. | [15](#15-discriminator-null-and-duplicate-property-names) |

## 1. The basic idea: EF and ORM

### Start with a familiar object

An employee in C# might have an `ID`, a `Name`, and a `Salary`. A database stores similar information in columns and rows. **Object-relational mapping**, shortened to **ORM**, connects these two representations.

| Application idea | Database idea | Example here |
| --- | --- | --- |
| Entity type | Mapped storage structure | `Employee` |
| Entity instance | Usually one row | A particular employee |
| Scalar property | Column value | `Name`, `Salary` |
| Primary key | Row identity | `ID` |
| Relationship | Foreign key and related rows | Employee belongs to a department |
| Navigation property | Object access to that relationship | `employee.Dept` |

“One class equals one table” is a useful starting approximation, but this repository already shows two exceptions: **Address** is flattened into another table, and **TPH** stores several employee classes in one table.

```text
C# application
    ↓ LINQ query or object changes
Entity Framework + model + database provider
    ↓ SQL commands
SQL Server
    ↓ results
EF materializes objects and returns them to the application
```

EF helps translate queries, map results, track changes, and generate data-change commands. In Code First, migrations also help manage schema changes. It does not decide your business requirements, invent valid data, or remove database constraints.

**Remember:** EF is the data-access library; SQL Server is the database engine. `SaveChanges()` saves data; it is not a general command to redesign the schema.

## 2. The repository and its three projects

| Folder | What it teaches | Context | Database configured in source |
| --- | --- | --- | --- |
| [DesignFirst](DesignFirst/README.md) | EDMX model, LINQ, loading, tracking, CRUD | `CompanyEntities` | `Company` |
| [CodeFirst](CodeFirst/README.md) | Classes, annotations, relationships, migrations, Address | `Context` | `Intake46` |
| [WorkforceManagement](WorkforceManagement/README.md) | Abstract employee hierarchy and TPH | `Context` | `WorkforceDB` |

These are separate learning projects. Their `Employee` classes belong to different namespaces and have different designs. For example, `CodeFirst.Employee` has department relationships, while `WorkforceManagement.Employee` is an abstract base for three worker types.

The common package is `EntityFramework`; the main EF6 namespace is `System.Data.Entity`. Look at [CodeFirst/packages.config](CodeFirst/CodeFirst/packages.config) and [WorkforceManagement/packages.config](WorkforceManagement/WorkforceManagement/packages.config). EF Core examples often use `Microsoft.EntityFrameworkCore`; do not mix those APIs into these exercises.

The short code examples in this book are excerpts or clearly labeled adaptations. Follow the file links to see their surrounding code. Where an older lesson note and the current implementation differ, this book describes the current code and migration.

## 3. Designer-based development and EDMX

### What does “DesignFirst” mean here?

The folder name is `DesignFirst`. Its checked-in implementation uses the **EF Designer**, an EDMX model, and generated classes mapped to the `Company` database. The files prove that model structure; they do not record every original wizard choice.

Use these more precise terms when studying:

| Approach | Starting point | Typical direction |
| --- | --- | --- |
| Database First | Existing database | Database → EDMX → generated classes |
| Model First | Visual model in the Designer | EDMX → generated database schema and classes |
| Code First | C# classes and mapping rules | Classes → EF model → migrations/database |

Both Database First and Model First can use EDMX. An EDMX file alone does not prove which came first. Microsoft documents the two workflows separately: [Database First](https://learn.microsoft.com/en-us/ef/ef6/modeling/designer/workflows/database-first) and [Model First](https://learn.microsoft.com/en-us/ef/ef6/modeling/designer/workflows/model-first).

### Read the model in three layers

Look at [Model1.edmx](DesignFirst/Day1/Model1.edmx):

- **Storage model (SSDL):** database tables, columns, keys, and associations.
- **Conceptual model (CSDL):** the entities and relationships the application works with.
- **Mapping (MSL):** how conceptual properties map to stored columns.

For example, the context exposes `Departments`, but this EDMX maps it to the singular SQL table `dbo.Department`. This is why a `DbSet` name alone is not enough to infer the actual table name.

### Generated files

Look at [Model1.Context.cs](DesignFirst/Day1/Model1.Context.cs):

```csharp
public partial class CompanyEntities : DbContext
{
    public CompanyEntities() : base("name=CompanyEntities")
    {
    }

    // Other generated members omitted here.
    public virtual DbSet<Department> Departments { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }
}
```

[Model1.tt](DesignFirst/Day1/Model1.tt) generates entity classes; [Model1.Context.tt](DesignFirst/Day1/Model1.Context.tt) generates the context. The generated files warn that regeneration can overwrite manual edits. `partial` allows another file to contribute to the same class, but it does not automatically change the EDMX mapping.

The generated `OnModelCreating` throws `UnintentionalCodeFirstException`. This is a guard against accidentally treating the Designer model as Code First; it is not an instruction to configure this project with Fluent API.

### The relationships already present

The EDMX contains employees belonging to departments, departments having managers, and employees having managers who are also employees. This explains the generated navigation names:

| Generated navigation | Meaning in this EDMX |
| --- | --- |
| `Department.Employees` | Employees belonging to the department through `DeptID` |
| `Department.Employee` | The department manager through `ManagerID` |
| `Employee.Department` | The employee's department |
| `Employee.Departments` | Departments managed by this employee |
| `Employee.Employee2` | This employee's manager |
| `Employee.Employee1` | Employees reporting to this employee |

Look at [Department.cs](DesignFirst/Day1/Department.cs), [Employee.cs](DesignFirst/Day1/Employee.cs), and the association mappings in the EDMX together. Similar names do not mean the relationships are identical.

## 4. DbContext, DbSet, and connections

**DbContext** is the object you use for a unit of database work: query some data, track relevant objects, change them, and save.

**DbSet&lt;T&gt;** is a starting point for queries and operations on an entity type. It is not an in-memory copy of the entire table, and accessing the property does not load every row.

Look at [CodeFirst/Context.cs](CodeFirst/CodeFirst/Context.cs):

```csharp
internal class Context : DbContext
{
    public Context() : base(@"Data source= localhost\SQLEXPRESS; initial catalog = Intake46; Integrated security = true")
    {
    }

    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
}
```

| Connection setting | Meaning |
| --- | --- |
| `Data source` | SQL Server instance to connect to |
| `Initial catalog` | Database name |
| `Integrated security = true` | Use Windows authentication |
| C# `@"..."` | A verbatim string, convenient for the backslash in the instance name |

The Code First contexts contain their SQL connection strings directly. The Designer context instead resolves `name=CompanyEntities` from [DesignFirst/App.config](DesignFirst/Day1/App.config), including EDMX metadata locations. Do not replace that Designer connection with an ordinary SQL-only string without understanding the model distinction. See [EF6 connection strings](https://learn.microsoft.com/en-us/ef/ef6/fundamentals/configuring/connection-strings).

For a small console operation, this adapted lifetime pattern disposes the context when finished:

```csharp
using (var context = new Context())
{
    // Query or modify data here.
}
```

Keeping a context alive does not mean it holds the SQL connection open continuously. Context lifetime, tracking lifetime, and individual database operations are related but different ideas.

## 5. LINQ queries and deferred execution

Look at [DesignFirst/Program.cs](DesignFirst/Day1/Program.cs), especially the first query examples.

### Query definition versus execution

```csharp
var query = context.Departments.Where(c => c.ID == 2);
```

This builds an EF query. Because the source is an EF `DbSet`, the expression is represented for EF's query provider to translate. It is still LINQ—specifically LINQ to Entities.

```csharp
foreach (var dept in query)
{
    Console.WriteLine(dept.Name);
}
```

Enumeration executes the query. EF reads the results and materializes objects. `ToList()` also executes and collects results; terminal operations such as `First()`, `Single()`, and `Count()` request results immediately. Merely assigning the query to a variable does not fetch its rows. See [EF6 querying](https://learn.microsoft.com/en-us/ef/ef6/querying/).

### Filter before materializing

Compare these adapted versions of the repository example:

```csharp
// SQL filters the rows before they are returned.
var filtered = context.Departments
    .Where(d => d.ID == 2)
    .ToList();

// Fetch all department entities, then filter in application memory.
var filteredInMemory = context.Departments
    .ToList()
    .Where(d => d.ID == 2);
```

The position of `ToList()` changes where the filter runs. “Client-side” means the application process, not Visual Studio itself. For a large table, fetching unnecessary rows costs memory, transfer, and processing time.

`IQueryable<T>` lets the provider build a database query. Once the data is a `List<T>`, subsequent LINQ operators work on objects in memory. Not every arbitrary C# method can be translated into SQL.

### Projection: ask for the values you need

The repository also uses query syntax:

```csharp
var id = int.Parse("2");
var query_3 =
    from dept in context.Departments
    where dept.ID == id
    select dept.Name;
```

`select dept.Name` asks for names, not complete `Department` entities. Method syntax and query syntax are two ways to express LINQ; neither alone determines whether execution happens in SQL or memory.

### Watch what EF sends

The source includes this commented logging line:

```csharp
context.Database.Log = Log => Console.WriteLine(Log);
```

Enable it during practice to compare query definition with execution and observe extra related-data queries. Re-enumerating a database query can execute it again; holding an `IQueryable` is not the same as storing a result list.

**Check yourself:** Which line actually retrieves data: `Where(...)`, or the later `ToList()`?

## 6. Finding one entity

The `Find`/`Single` examples are in [DesignFirst/Program.cs](DesignFirst/Day1/Program.cs). These methods make different promises:

| Method | What it asks for | No match | Multiple matches |
| --- | --- | --- | --- |
| `Find(key)` | An entity by primary key; check this context's tracked entities first | `null` | A primary key identifies one entity |
| `Single(predicate)` | Exactly one matching result | Throws | Throws |
| `SingleOrDefault(predicate)` | Zero or one matching result | Default, usually `null` | Throws |
| `First(predicate)` | The first matching result | Throws | Returns the first |
| `FirstOrDefault(predicate)` | The first match, if any | Default, usually `null` | Returns the first |

Unlike `Find`, a normal `DbSet` LINQ query using `Single` is not a tracker-first lookup. It queries the database even if that entity is tracked; tracking can then reuse the existing object. Use `OrderBy` when the meaning of “first” depends on a particular order. See [querying and finding entities](https://learn.microsoft.com/en-us/ef/ef6/querying/).

Adapted safe lookup:

```csharp
var department = context.Departments.Find(3);
if (department != null)
{
    Console.WriteLine(department.Name);
}
```

The checked-in `Program.cs` has two adjacent declarations named `dept` in the same scope for the `Find` and `Single` demonstrations. They are alternatives: choose one or rename a variable before compiling. This book does not treat the file as an already validated runnable script.

## 7. Navigation properties and loading related data

A foreign-key value tells you **which related row**. A navigation property lets you work with **the related object or collection**.

From [DesignFirst/Department.cs](DesignFirst/Day1/Department.cs):

```csharp
public virtual Employee Employee { get; set; }
public virtual ICollection<Employee> Employees { get; set; }
```

The first is a single reference to the manager; the second is a collection of department employees. The collection is initialized with a `HashSet<Employee>` in the generated constructor. Creating that empty collection does not fetch database rows.

### Lazy loading

In the Designer example, obtaining a department and then accessing `dept_1.Employees` can trigger another database query. EF6 uses a generated proxy with overridden `virtual` navigation properties for this behavior. The context must still be usable and lazy loading/proxy requirements must be satisfied.

`virtual` alone is not a guarantee. In particular, the Code First entities here are `internal`; do not assume the proxy behavior demonstrated with the public Designer entities applies automatically. See [EF6 proxies](https://learn.microsoft.com/en-us/ef/ef6/fundamentals/proxies).

### Eager and explicit loading

These short comparison examples extend the loading concept; they are not copied from the current `Program.cs`:

```csharp
// Eager loading: request related employees with the department query.
var departments = context.Departments
    .Include(d => d.Employees)
    .ToList();

// Explicit loading: deliberately load the collection later.
var department = context.Departments.Find(3);
if (department != null)
{
    context.Entry(department).Collection(d => d.Employees).Load();
}
```

`Include` needs `using System.Data.Entity;`. Removing `virtual` does **not** automatically enable eager loading. Related objects can also already be present because the context tracked them earlier and connected the relationships. See [EF6 related-data loading](https://learn.microsoft.com/en-us/ef/ef6/querying/related-data).

### Deferred execution is a different question

| Concept | Question it answers |
| --- | --- |
| Deferred execution | When does this query execute? |
| Lazy loading | When does accessing a navigation fetch related data? |

A loop over many departments that lazily loads each employee collection may issue many extra queries. Use the repository's logging line to observe this rather than assuming one C# loop means one SQL query.

## 8. Change tracking, entity states, and CRUD

### Why changing a property can become an UPDATE

The Designer project contains:

```csharp
var dept_2 = context.Departments.First();
dept_2.Name = "Intake 42";
context.SaveChanges();
```

A normal entity query tracks the returned department. With default change detection, `SaveChanges()` notices the property change and sends an update. The database is not updated when the assignment alone runs.

Tracking has a cost because the context retains information about entities. That is useful bookkeeping, not automatically a memory leak. Keep the context's scope appropriate to the task.

### Entity states

| State | Context's interpretation | Effect of a successful save |
| --- | --- | --- |
| `Added` | New entity scheduled for insertion | INSERT, then tracked as `Unchanged` |
| `Unchanged` | Tracked with no pending change | No data-change command for that entity |
| `Modified` | Tracked with changes to persist | UPDATE, then `Unchanged` |
| `Deleted` | Tracked and scheduled for deletion | DELETE, then detached |
| `Detached` | Not tracked by this context | No automatic persistence |

`context.Entry(entity)` gives access to EF's tracking information. Assigning `EntityState.Added` schedules insertion; it does not mean “this already exists.” Assigning `Modified` to an existing detached object is a more deliberate operation than changing a property on a queried object. See [EF6 entity states](https://learn.microsoft.com/en-us/ef/ef6/saving/change-tracking/entity-state).

### CRUD in this repository

| Operation | Pattern | Example location |
| --- | --- | --- |
| Create | Construct → `Add` → `SaveChanges` | All three `Program.cs` files |
| Read | Query → enumerate or request a result | Designer `Program.cs` |
| Update | Query tracked entity → change property → save | `dept_2.Name` example |
| Delete | Find existing entity → `Remove` → save | Commented delete example |

Adapted delete pattern for the Designer context:

```csharp
var department = context.Departments.Find(60);
if (department != null)
{
    context.Departments.Remove(department);
    context.SaveChanges();
}
```

Database relationships may restrict deletion or cascade it. Always connect the operation to the actual relationship model.

### Inserting an object graph

The Designer exercise creates a department with a new employee collection and also creates an employee whose `Department` is a new department. These are connected object graphs. Adding the root can mark reachable untracked new entities for insertion too.

An object with a key value is not automatically known to the context as an existing row. For existing relationships, use an appropriate existing tracked entity or FK value rather than presenting a new object as if it were already stored. Also inspect the Designer model's key generation settings: its checked-in EDMX does not mark the IDs as store-generated, so the old insert examples should not be assumed to receive identity values automatically.

### AsNoTracking

For a read-only entity query, this adapted example avoids tracking the returned entities:

```csharp
var departments = context.Departments
    .AsNoTracking()
    .Where(d => d.ID > 10)
    .ToList();
```

Changing those objects later will not automatically be saved by this context. They are still ordinary mutable C# objects; `AsNoTracking` is not an immutable-object feature. The original `query_4` selects only `d.ID`, so it already returns scalar integers rather than entities to track. See [EF6 no-tracking queries](https://learn.microsoft.com/en-us/ef/ef6/querying/no-tracking).

## 9. Code First and conventions

**Code First means the EF model is derived from C# classes and their mapping configuration.** It does not mean EF becomes a general XML-file or in-memory storage engine. LINQ itself can work over several kinds of sources; that is a separate idea.

The learning sequence in [CodeFirst](CodeFirst/README.md) is:

```text
Define Department and Employee
    → create Context : DbContext
    → expose entity sets
    → generate and inspect a migration
    → apply it to SQL Server
    → use the model to query and save data
```

### What EF infers

The first [init migration](CodeFirst/CodeFirst/Migrations/202609231540529_init.cs) demonstrates default mapping: integer identity keys, string columns, a non-nullable salary column, and two tables.

Common conventions used here include recognizing `ID` or a type-name-plus-`Id` as a key, pluralized table naming when not overridden, and relationships inferred from navigation properties. `int` and `DateTime` normally produce required scalar columns; `int?` allows null. In these EF6 projects, strings are optional unless configured otherwise. TPH introduces a further nullability rule for subtype columns.

Entity discovery also follows mapped relationships. [Context.cs](CodeFirst/CodeFirst/Context.cs) has only `Departments` and `Employees` sets, yet the migration creates `Projects` and `WorksFors` because their types are reachable through the model. A separate `DbSet` for every entity is not mandatory. See [EF6 conventions](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/conventions/built-in).

Conventions are a starting point. The next step is overriding selected rules with Data Annotations.

## 10. Data Annotations

**Data Annotations are attributes attached to classes or properties.** In this repository they express mapping and validation rules without a Fluent API model configuration.

Look at [CodeFirst/Employee.cs](CodeFirst/CodeFirst/Employee.cs):

```csharp
[Key]
[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
public int ID { get; set; }

[Column("FullName")]
[Required, MaxLength(100)]
public string Name { get; set; }

[Column(TypeName = "Date")]
public DateTime Birthdate { get; set; }
```

Read this as: “`ID` is the key and its value is generated; the C# `Name` property maps to a required SQL column named `FullName` with a maximum length of 100; `Birthdate` uses SQL `date`.”

| Annotation in the repository | What it expresses |
| --- | --- |
| `[Key]` | Primary-key property |
| `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` | Database-generated value on insert |
| `[Table("Department", Schema = "HR")]` | Map the class to `HR.Department` |
| `[Column("FullName")]` | Use a different SQL column name |
| `[Column(TypeName = "Date")]` | Choose a SQL store type |
| `[Required]` | Required value; appropriate validation and mapping consequences |
| `[MaxLength(100)]` | Maximum string length |
| `[ForeignKey("DepartmentID")]` | Associate a navigation with its FK property |
| `[InverseProperty("Dept")]` | Identify the navigation on the other side of a relationship |
| `[ComplexType]` | Map a class as a group of values rather than a separate entity |

The `Table` annotation is on the **class**, not the namespace. Look at [Department.cs](CodeFirst/CodeFirst/Department.cs). Attributes such as `Required` and `Key` use `System.ComponentModel.DataAnnotations`; mapping attributes such as `Table`, `Column`, and `ComplexType` use its `.Schema` namespace. See [EF6 Data Annotations](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/data-annotations).

Annotations can affect EF validation and the generated schema, but those are not identical mechanisms. For example, SQL `NOT NULL` prevents a null value; it does not alone prevent an empty string. After a mapping change, inspect and apply a migration before expecting the database schema to match.

## 11. Relationships and foreign keys

### One department, many employees

From [CodeFirst/Employee.cs](CodeFirst/CodeFirst/Employee.cs):

```csharp
public int DepartmentID { get; set; }

[ForeignKey("DepartmentID")]
public virtual Department Dept { get; set; }
```

`DepartmentID` is the scalar FK. `Dept` is the related object. The non-nullable FK expresses a required department for each employee in this model. The referenced department must actually exist.

From [Department.cs](CodeFirst/CodeFirst/Department.cs):

```csharp
[InverseProperty("Dept")]
public virtual ICollection<Employee> Employees { get; set; }
```

The collection is the opposite direction of the same relationship. A department can have many employees; an employee has one work department.

### Two relationships between the same classes

An employee also may supervise a department:

```csharp
public int? SupervisedDepartmentID { get; set; }

[ForeignKey("SupervisedDepartmentID")]
public virtual Department SupervisedDept { get; set; }
```

The matching department navigation is:

```csharp
[InverseProperty("SupervisedDept")]
public virtual ICollection<Employee> Supervisors { get; set; }
```

`InverseProperty` removes ambiguity by pairing each collection with the correct opposite navigation:

```text
Department.Employees    ↔ Employee.Dept
Department.Supervisors  ↔ Employee.SupervisedDept
```

This model allows a department to have many supervisors, while each employee can supervise zero or one department. It does not model one employee supervising several departments. The `?` permits an employee who supervises none.

`ForeignKey` answers “which scalar property holds the related key?” `InverseProperty` answers “which navigation represents the other side?” They solve different problems.

### Department and Project

[Project.cs](CodeFirst/CodeFirst/Project.cs) has `Department`; `Department.cs` has `Projects`. There is no explicit `DepartmentID` on `Project`. The [updates3 migration](CodeFirst/CodeFirst/Migrations/202609241741448_updates3.cs) shows the resulting nullable `Department_ID` column. In this current mapping, a project may have no department.

### Many-to-many with data on the relationship

An employee can work on many projects, and a project can have many employees. The number of **hours** belongs to a specific employee–project assignment.

Look at [WorksFor.cs](CodeFirst/CodeFirst/WorksFor.cs):

```csharp
internal class WorksFor
{
    public int ID { get; set; }
    public int Hours { get; set; }

    public virtual Employee Employee { get; set; }
    public virtual Project Project { get; set; }
}
```

```text
Employee 1 ── many WorksFor many ── 1 Project
                       │
                     Hours
```

This explicit link entity gives the relationship somewhere to store `Hours`. EF6 can also map simple many-to-many associations without an explicit class, but the extra relationship data makes `WorksFor` useful here.

The [updates4 migration](CodeFirst/CodeFirst/Migrations/202609241816127_updates4.cs) creates `dbo.WorksFors` with `ID`, `Hours`, `Employee_ID`, and `Project_ID`. Both FKs are nullable in this version. It also does not enforce uniqueness of an employee–project pair: two rows could reference the same pair with different IDs. These are properties of the actual model, not assumptions based on the intended business relationship.

## 12. Migrations and the history of this model

**A migration records a change to the database schema.** It lets the project retain the steps between earlier and later versions of its model.

### The three commands

For a new EF6 Code First project, use Visual Studio's Package Manager Console:

```powershell
Enable-Migrations
Add-Migration InitialCreate
Update-Database
```

| Command | Responsibility |
| --- | --- |
| `Enable-Migrations` | Set up migration configuration |
| `Add-Migration <Name>` | Generate code for model changes |
| `Update-Database` | Apply pending migrations to the configured database |

For an existing project in this repository, migrations already exist: do not create another initial migration simply to run them. `Update-Database` can apply pending migrations. After an intentional new model change, generate a descriptively named migration, inspect it, then apply it. See [EF6 migrations](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/migrations/).

### Up and Down

From the [updates migration](CodeFirst/CodeFirst/Migrations/202609231903282_updates.cs):

```csharp
public override void Up()
{
    AddColumn("dbo.Employees", "Address", c => c.String());
}

public override void Down()
{
    DropColumn("dbo.Employees", "Address");
}
```

`Up` describes the forward change; `Down` describes a reversal of the schema operation. Recreating a dropped column does not restore its old values. Generated migration operations such as `CreateTable(...).PrimaryKey(...)` are migration code, not a Fluent API model configuration.

### Read this project's history as a story

| Migration | What changed | What to learn |
| --- | --- | --- |
| [init](CodeFirst/CodeFirst/Migrations/202609231540529_init.cs) | Creates Departments and Employees | Initial tables, key generation, scalar types |
| [updates](CodeFirst/CodeFirst/Migrations/202609231903282_updates.cs) | Adds an Address string | Adding a property adds a column |
| [updates2](CodeFirst/CodeFirst/Migrations/202609241633278_updates2.cs) | Moves Department to schema HR, renames Name to FullName, adds Location/Birthdate, changes constraints | Annotations become concrete schema operations |
| [updates3](CodeFirst/CodeFirst/Migrations/202609241741448_updates3.cs) | Adds Projects and required Employee.DepartmentID | Relationships become FKs and indexes |
| [updates4](CodeFirst/CodeFirst/Migrations/202609241816127_updates4.cs) | Adds WorksFors | A link entity becomes a table |
| [AddComplexAddress](CodeFirst/CodeFirst/Migrations/202609281414042_AddComplexAddress.cs) | Adds Address members and supervision FK; drops the old Address string | Complex types and multiple relationships evolve the schema |

Inspect real details, not only migration names:

- `updates2` sets a default empty string when adding required `Location`. Existing data and application validation still deserve attention.
- `updates3` makes `DepartmentID` non-nullable and enables cascade delete on that FK. Applying this to a database with existing employees requires valid department values; existing rows do not magically acquire the correct department.
- `AddComplexAddress` drops the old `Address` column without copying its text into the new fields. It is a schema conversion, not a completed data-conversion routine.
- The supervision relationship is added in `AddComplexAddress`, despite not appearing in that migration's name.

### Model metadata and migration history

The accompanying `.Designer.cs` and `.resx` files carry migration metadata. `__MigrationHistory` records applied EF6 migrations in the database. Both matter when reproducing or advancing the schema; keep the migration files in source control.

Scaffolding compares the current model with previous migration model metadata. Applying migrations consults the database's applied history. Neither action is a general audit that automatically captures arbitrary changes someone made directly in SQL Server.

Look at [CodeFirst/Configuration.cs](CodeFirst/CodeFirst/Migrations/Configuration.cs): `AutomaticMigrationsEnabled = false`. Its `Seed` method has only template comments. The sample data is inserted from `Program.cs`, not this seed method.

## 13. Complex types and Address

Initially the Code First employee used a single address string. Later the design separated it into values with their own meaning:

```csharp
[ComplexType]
internal class Address
{
    public string City { get; set; }
    public string Street { get; set; }
    public int? ZipCode { get; set; }
}
```

Look at [CodeFirst/Address.cs](CodeFirst/CodeFirst/Address.cs). It has no key and no independent set. Its values are stored with the owning employee:

| Object property | SQL column |
| --- | --- |
| `Address.City` | `Address_City` |
| `Address.Street` | `Address_Street` |
| `Address.ZipCode` | `Address_ZipCode` |

This is **composition**: an employee has an address. It is different from inheritance, where a full-time employee is an employee.

### The two Address classes are slightly different

| Rule | CodeFirst Address | WorkforceManagement Address |
| --- | --- | --- |
| `[ComplexType]` | Yes | Yes |
| City and Street | Optional strings in the migration | `[Required]`, non-null columns |
| ZipCode | `int?` | `int?` |
| Separate address table | No | No |

Look at [WorkforceManagement/Address.cs](WorkforceManagement/WorkforceManagement/Address.cs). These explicit `Required` attributes are present in the actual source; do not infer the workforce model's constraints from the earlier minimal README reconstruction.

With EF6, create the complex object itself; its nullable members may be missing. `Address = new Address { City = "Cairo", Street = "negm" }` creates an address with a null zip code. That differs from setting `Address = null`.

**Remember:** grouping values into a class does not necessarily mean creating a new entity or table.

## 14. Inheritance and TPH

### Begin with the requirements

The workforce model stores full-time employees, part-time employees, and contractors. They share identity, name, birth date, and address; their payment and contract information differs.

```text
Employee (abstract)
├── ID, Name, BirthDate, Address
├── FullTimeEmployee: MonthlySalary, AnnualBonus
├── PartTimeEmployee: HourlyRate, HoursPerWeek
└── Contractor: HourlyRate, ContractEndDate
```

The actual base definition in [WorkforceManagement/Employee.cs](WorkforceManagement/WorkforceManagement/Employee.cs), with unused imports omitted:

```csharp
internal abstract class Employee
{
    public int ID { get; set; }
    [Required]
    public string Name { get; set; }
    public DateTime BirthDate { get; set; }
    [Required]
    public Address Address { get; set; }
}
```

`abstract` prevents `new Employee()`. It still lets EF map shared properties and lets code work with a base `Employee` reference holding a concrete subtype. The actual class has no `[Key]`; EF recognizes `ID` by convention.

Look at [FullTimeEmployee.cs](WorkforceManagement/WorkforceManagement/FullTimeEmployee.cs), [PartTimeEmployee.cs](WorkforceManagement/WorkforceManagement/PartTimeEmployee.cs), and [Contractor.cs](WorkforceManagement/WorkforceManagement/Contractor.cs). Monetary values use `decimal` here; the earlier Code First exercise used `double Salary`. Preserve that distinction when reading the examples.

### Table Per Hierarchy

**TPH maps this entire inheritance hierarchy into one table.** It is the default EF6 Code First inheritance mapping used by this project.

```text
Three concrete C# employee types
                  ↓
          dbo.Employees
 shared columns + subtype columns + Discriminator
```

[WorkforceManagement/Context.cs](WorkforceManagement/WorkforceManagement/Context.cs) exposes only:

```csharp
public DbSet<Employee> Employees { get; set; }
```

The derived classes are in the same assembly and are included in the model. Separate sets are unnecessary. Having one set is not itself the definition of TPH; the inheritance mapping determines storage. See [EF6 inheritance](https://learn.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/implementing-inheritance-with-the-entity-framework-in-an-asp-net-mvc-application).

**Tradeoff visible in this exercise:** all employees are easy to represent together, but the table contains columns that apply only to some rows. That leads directly to the next chapter.

## 15. Discriminator, NULL, and duplicate property names

### Discriminator

The discriminator records the concrete employee type:

| C# object | Stored discriminator |
| --- | --- |
| `FullTimeEmployee` | `FullTimeEmployee` |
| `PartTimeEmployee` | `PartTimeEmployee` |
| `Contractor` | `Contractor` |

EF writes the value based on the object type and uses it when materializing rows. There is no `Discriminator` property in the C# classes and no manual assignment in `Program.cs`.

### Read the actual InitialCreate migration

Look at [202609281731304_InitialCreate.cs](WorkforceManagement/WorkforceManagement/Migrations/202609281731304_InitialCreate.cs). This excerpt is from the checked-in migration:

```csharp
ID = c.Int(nullable: false, identity: true),
Name = c.String(nullable: false),
BirthDate = c.DateTime(nullable: false),
Address_City = c.String(nullable: false),
Address_Street = c.String(nullable: false),
Address_ZipCode = c.Int(),
HourlyRate = c.Decimal(precision: 18, scale: 2),
ContractEndDate = c.DateTime(),
MonthlySalary = c.Decimal(precision: 18, scale: 2),
AnnualBonus = c.Decimal(precision: 18, scale: 2),
HourlyRate1 = c.Decimal(precision: 18, scale: 2),
HoursPerWeek = c.Int(),
Discriminator = c.String(nullable: false, maxLength: 128),
```

The migration creates one employee table and assigns its primary key to `ID`. `decimal(18,2)` means 18 total digits with 2 after the decimal point. `Down()` drops that table.

### Why subtype columns allow NULL

Ahmed is full-time. He has a salary and bonus, but no contractor end date. The table contains `ContractEndDate` for other rows, so Ahmed's value in that column is `NULL`.

Although `MonthlySalary` is a non-nullable C# `decimal` on `FullTimeEmployee`, its shared SQL column must permit nulls for part-time and contractor rows. C# subtype requirements and shared-table nullability are different levels of the model.

| Example NULL | Explanation |
| --- | --- |
| Mohammed's `Address_ZipCode` | An optional value was omitted |
| Ahmed's `ContractEndDate` | The property does not apply to this type |

Do not make every C# subtype property nullable just because its TPH column allows SQL null. Also, adding `[Required]` to a subtype property does not make that column universally `NOT NULL` across the hierarchy. See [EF6 TPH mapping constraints](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/fluent/types-and-properties).

### Why HourlyRate and HourlyRate1 exist

The two sibling classes each declare their own `HourlyRate`; it is not one property inherited from the base. The observed mapping gives them separate column names:

| Property | Column in this project |
| --- | --- |
| `Contractor.HourlyRate` | `HourlyRate` |
| `PartTimeEmployee.HourlyRate` | `HourlyRate1` |

The `1` resolves a name collision. It has no special business meaning and does not mean one employee has two rates. Which sibling gets a suffix is not a rule to memorize for all projects; inspect the generated mapping and stored values.

## 16. The complete workforce example

Look at [WorkforceManagement/Program.cs](WorkforceManagement/WorkforceManagement/Program.cs) for all initializers. The final save sequence is:

```csharp
context.Employees.Add(Emp_1);
context.Employees.Add(Emp_2);
context.Employees.Add(Emp_3);

context.SaveChanges();
```

Each object is a different concrete type, but all can be added through `DbSet<Employee>`. `Add` tracks them as new; `SaveChanges` inserts them; the identity key and discriminator follow the mapping.

### Exact sample values

| Name | Type | Birth date | City / street / zip |
| --- | --- | --- | --- |
| Ahmed | FullTimeEmployee | 2001-09-16 | Almahalla / Tawheed / 1250 |
| Mohammed | PartTimeEmployee | 2010-02-14 | Cairo / negm / NULL |
| Youssef | Contractor | 2016-12-12 | Alexandria / Shaarawy / 12437 |

The existing workforce lesson records this resulting table shape and data:

| Name | MonthlySalary | AnnualBonus | HourlyRate | ContractEndDate | HourlyRate1 | HoursPerWeek |
| --- | --- | --- | --- | --- | --- | --- |
| Ahmed | 20000.00 | 5000.00 | NULL | NULL | NULL | NULL |
| Mohammed | NULL | NULL | NULL | NULL | 150.00 | 20 |
| Youssef | NULL | NULL | 300.00 | 2027-01-01 | NULL | NULL |

Each row also has its shared columns and the discriminator shown in the previous chapter. This book documents the repository's sample and recorded outcome; it does not claim a new database execution was performed while writing it.

### Inspect the result yourself

```sql
USE [WorkforceDB];

SELECT ID, Name, BirthDate,
       Address_City, Address_Street, Address_ZipCode,
       MonthlySalary, AnnualBonus,
       HourlyRate, ContractEndDate,
       HourlyRate1, HoursPerWeek, Discriminator
FROM dbo.Employees
ORDER BY ID;
```

You may also see `dbo.__MigrationHistory`. It is EF's infrastructure table; “one TPH table” means one application table for this hierarchy.

### Two small execution details

- Use `new DateTime(2001, 09, 16)`. Writing `new DateTime(2001 - 09 - 16)` performs subtraction and passes a ticks value, not a year/month/day date.
- Running the sample again inserts another set of employees. The current program has no duplicate check.

The sample values are for persistence practice. No age eligibility or payroll calculation rules are implemented.

## 17. Revisiting and running the projects

### Read before running

1. Open the appropriate solution or project in Visual Studio with .NET Framework 4.8 support.
2. Restore its NuGet packages. For a new exercise using the same package version, the installation command is `Install-Package EntityFramework -Version 6.5.2`.
3. Confirm the SQL Server instance and database in the relevant context or `App.config`.
4. Set the intended startup project and Package Manager Console Default project.
5. Build and resolve any lesson-snippet issues before running database operations.

### Designer project

The project targets the existing `Company` database described by its EDMX. There is no committed Code First migration chain for creating that database. Check the schema and sample data it expects. If the database schema changes, synchronize the Designer model appropriately rather than assuming a C# property edit updates the database.

The current `Program.cs` contains the duplicate `dept` declaration noted earlier, assumes rows such as department 3 exist, and demonstrates inserts whose IDs are not configured as generated in the checked-in EDMX. Treat it as a collection of lessons to run selectively.

### CodeFirst project

Apply its existing migrations to the intended practice database. [Program.cs](CodeFirst/CodeFirst/Program.cs) first inserts an `SD` department with `Location = "El-mahalla"`, then inserts Youssef with `DepartmentID = 1` and an Address object.

That `1` assumes the referenced department exists. It is not guaranteed to be the ID of the department just inserted, particularly after repeated runs. An adapted exercise can keep the new department object and use its generated `ID` after saving, or assign it through `Dept`.

### WorkforceManagement project

Apply its existing `InitialCreate`, run the three-object insertion once, then inspect the columns and discriminator values. Use its current context connection, which contains the `WorkforceDB` connection string directly.

### Short troubleshooting map

| Symptom | First thing to inspect |
| --- | --- |
| Cannot connect | Server instance, database, and authentication settings |
| Table missing | Correct database and pending migrations |
| Model differs from database | Current classes, migration chain, applied history |
| Foreign-key violation | Referenced row exists; FK value is correct |
| Validation error | Required values, maximum lengths, complex object initialization |
| Navigation is empty/null | Loading strategy, related data, proxy eligibility, context lifetime |
| Too many queries | Logging, lazy-loading access inside loops |
| Update was not saved | Tracking state and whether `SaveChanges()` ran |
| Duplicate sample rows | Insertion program was run more than once |

## 18. Clarifications to keep beside the early notes

The folder READMEs preserve the learning journey. Use these distinctions when revising so that an early shorthand does not become a permanent rule.

| Early shorthand or example | More precise understanding |
| --- | --- |
| “No virtual means all related data loads” | Removing `virtual` does not request eager loading. |
| “Single checks memory first” | `Find` has the primary-key tracker-first behavior; a normal `Single` query executes against the database. |
| “AsNoTracking makes data read-only” | It disables query-result tracking; the returned objects can still change in memory. |
| “Tracking is a performance leak” | Tracking has overhead and purpose; lifetime and query choices determine whether it is unnecessary. |
| “Add-Migration creates the tables” | It generates code; applying the migration changes the database. |
| “Migration connects the app to the database” | The connection settings select the database; migrations evolve its schema. |
| “One DbSet means one table, and every table needs a DbSet” | Relationships and inheritance affect model discovery and mapping. |
| “Delete the initial migration without affecting the project” | Preserve migration code and metadata so the schema can be reproduced and evolved. |
| “Changing a class updates SQL automatically” | These projects use explicit migrations; generate, inspect, and apply the change. |
| “Every non-nullable C# property means a NOT NULL SQL column” | TPH subtype properties are the important exception studied here. |
| “CodeFirst Salary is int” | The checked-in property is `double`; workforce monetary properties are `decimal`. |
| Workforce README's reconstructed classes and context | Actual code has required Name/City/Street, convention-based ID, internal classes, and a constructor connection string. |

For schema facts, follow this chain: **current entity definitions → mapping rules → actual migration → actual database**. A copied example alone is not proof of the current schema.

## 19. Revision questions and practice

### Explain without looking

1. What does an ORM do, and what is still the database's responsibility?
2. Why is this repository currently an EF6 repository despite its name?
3. What does an EDMX store? How do Database First and Model First differ?
4. How does `DbContext` differ from `DbSet`?
5. What changes when `ToList()` moves before `Where()`?
6. How do deferred execution and lazy loading differ?
7. When does `Find` avoid a database query? What does `Single` guarantee?
8. What happens between `Add` and `SaveChanges`?
9. How do `ForeignKey` and `InverseProperty` solve different problems?
10. Why does `WorksFor` deserve a class of its own?
11. Why are Projects and WorksFors mapped without dedicated sets in Context?
12. What can `Down()` reverse, and what data might it not recover?
13. Why does Address produce columns rather than a table?
14. Why can an abstract Employee be used with `DbSet<Employee>`?
15. How does EF know which concrete type a TPH row represents?
16. Why are `MonthlySalary` and `Address_ZipCode` nullable in SQL for different reasons?
17. What explains `HourlyRate1`?

### Practice with visible evidence

| Exercise | Evidence to look for |
| --- | --- |
| Enable logging and compare the two filter orders | SQL filtering before materialization versus filtering in memory |
| Call `Find` twice for an existing tracked key | The second lookup can use the context's tracked entity |
| Change a tracked department name and save | UPDATE behavior |
| Repeat a read using `AsNoTracking` | No automatic update from changing the returned entity alone |
| Trace the six CodeFirst migrations in order | How today's schema developed from the original model |
| Inspect `WorksFors` foreign keys | Nullable columns and absence of pair uniqueness |
| Inspect the workforce rows | Discriminator and subtype-specific NULL values |

Use a practice database for changes. Keep each experiment small enough that you can predict its result before running it.

### Next small exercise: query the hierarchy

This is a suggested continuation, not code already present in the workforce `Program.cs`:

```csharp
var allEmployees = context.Employees.ToList();
var fullTimeEmployees = context.Employees
    .OfType<FullTimeEmployee>()
    .ToList();
var contractors = context.Employees
    .OfType<Contractor>()
    .ToList();
```

Use `System.Linq`, inspect the runtime types, and compare the generated SQL. This completes the opposite direction of the insertion exercise: **row → discriminator → concrete object**. TPT, TPC, and Fluent API mapping are outside the current implemented lessons.

## 20. Source-file reading map

### Designer-based EF

| File | Read it to understand |
| --- | --- |
| [DesignFirst/README.md](DesignFirst/README.md) | Original querying, loading, and tracking notes |
| [Program.cs](DesignFirst/Day1/Program.cs) | Query order, lookup, update, graph insertion, deletion examples |
| [Model1.edmx](DesignFirst/Day1/Model1.edmx) | Storage model, conceptual model, and mappings |
| [Model1.Context.cs](DesignFirst/Day1/Model1.Context.cs) | Generated CompanyEntities context |
| [Department.cs](DesignFirst/Day1/Department.cs) / [Employee.cs](DesignFirst/Day1/Employee.cs) | Generated scalar and navigation properties |
| [Model1.tt](DesignFirst/Day1/Model1.tt) / [Model1.Context.tt](DesignFirst/Day1/Model1.Context.tt) | Templates behind generated code; reference material, not a required memorization task |
| [App.config](DesignFirst/Day1/App.config) | Named connection and EDMX metadata |

### Code First

| File | Read it to understand |
| --- | --- |
| [CodeFirst/README.md](CodeFirst/README.md) | Original progression from classes to relationships |
| [Context.cs](CodeFirst/CodeFirst/Context.cs) | Intake46 connection and entry-point sets |
| [Employee.cs](CodeFirst/CodeFirst/Employee.cs) | Key, column annotations, two department FKs, Address |
| [Department.cs](CodeFirst/CodeFirst/Department.cs) | Schema/table annotation and inverse relationships |
| [Project.cs](CodeFirst/CodeFirst/Project.cs) / [WorksFor.cs](CodeFirst/CodeFirst/WorksFor.cs) | Project relationship and assignment entity |
| [Address.cs](CodeFirst/CodeFirst/Address.cs) | Complex type without entity identity |
| [Program.cs](CodeFirst/CodeFirst/Program.cs) | Department and employee inserts |
| [Migrations](CodeFirst/CodeFirst/Migrations) | Six schema changes and their metadata |
| [Configuration.cs](CodeFirst/CodeFirst/Migrations/Configuration.cs) | Explicit migration setup and empty Seed method |

### Workforce TPH

| File | Read it to understand |
| --- | --- |
| [WorkforceManagement/README.md](WorkforceManagement/README.md) | Detailed TPH walkthrough and original observed results |
| [Employee.cs](WorkforceManagement/WorkforceManagement/Employee.cs) | Abstract base and common properties |
| [FullTimeEmployee.cs](WorkforceManagement/WorkforceManagement/FullTimeEmployee.cs) | Salary and bonus |
| [PartTimeEmployee.cs](WorkforceManagement/WorkforceManagement/PartTimeEmployee.cs) | Hourly rate and weekly hours |
| [Contractor.cs](WorkforceManagement/WorkforceManagement/Contractor.cs) | Hourly rate and contract end |
| [Address.cs](WorkforceManagement/WorkforceManagement/Address.cs) | Required city/street and optional zip code |
| [Context.cs](WorkforceManagement/WorkforceManagement/Context.cs) | WorkforceDB connection and base employee set |
| [InitialCreate](WorkforceManagement/WorkforceManagement/Migrations/202609281731304_InitialCreate.cs) | Exact table shape, discriminator, and column nullability |
| [Program.cs](WorkforceManagement/WorkforceManagement/Program.cs) | All three concrete insertions |
| [Configuration.cs](WorkforceManagement/WorkforceManagement/Migrations/Configuration.cs) | Explicit migration setup |

The `.csproj` files record framework and references; `packages.config` records package versions; `AssemblyInfo.cs` records assembly metadata. Migration `.Designer.cs`/`.resx` files support migration history. They are part of the project, but the application concepts are clearest in the files above.

---

*Repository basis: the three project READMEs, source classes, programs, context/configuration files, EDMX and generated code, and migrations at commit `508f3e34e3b09b14dc9d0601aa19aecd582e7e6c`. This guide preserves the current learning scope and distinguishes current implementation from illustrative snippets and suggested next exercises.*
