namespace InventoryCountWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Items",
                c => new
                    {
                        ID = c.Int(nullable: false),
                        Name = c.String(),
                        BinLocation = c.String(),
                        Description = c.String(),
                        OnHand = c.Int(nullable: false),
                        Notes = c.String(),
                        CycleCountCode = c.Int(nullable: false),
                        VendorCode = c.String(),
                        Brand = c.String(),
                        QuantityBackOrdered = c.Int(nullable: false),
                        NewOnHand = c.Int(nullable: false),
                        NewBinLocation = c.String(),
                        WebProductID = c.Int(nullable: false),
                        Confirmed = c.Boolean(nullable: false),
                        CounterInitials = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Items");
        }
    }
}
