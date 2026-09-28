namespace WorkforceManagement.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Employees",
                c => new
                    {
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
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Employees");
        }
    }
}
