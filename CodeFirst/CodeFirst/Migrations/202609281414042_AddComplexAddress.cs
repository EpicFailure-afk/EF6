namespace CodeFirst.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddComplexAddress : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "Address_City", c => c.String());
            AddColumn("dbo.Employees", "Address_Street", c => c.String());
            AddColumn("dbo.Employees", "Address_ZipCode", c => c.Int());
            AddColumn("dbo.Employees", "SupervisedDepartmentID", c => c.Int());
            CreateIndex("dbo.Employees", "SupervisedDepartmentID");
            AddForeignKey("dbo.Employees", "SupervisedDepartmentID", "HR.Department", "ID");
            DropColumn("dbo.Employees", "Address");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Employees", "Address", c => c.String());
            DropForeignKey("dbo.Employees", "SupervisedDepartmentID", "HR.Department");
            DropIndex("dbo.Employees", new[] { "SupervisedDepartmentID" });
            DropColumn("dbo.Employees", "SupervisedDepartmentID");
            DropColumn("dbo.Employees", "Address_ZipCode");
            DropColumn("dbo.Employees", "Address_Street");
            DropColumn("dbo.Employees", "Address_City");
        }
    }
}
