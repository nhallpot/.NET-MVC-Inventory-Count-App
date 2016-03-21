namespace InventoryCountWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class noneedforbinindb : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.BinLocations");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.BinLocations",
                c => new
                    {
                        Bin = c.String(nullable: false, maxLength: 128),
                        ContainsOnlyNonZeros = c.Boolean(nullable: false),
                        ContainsOnlyUncountedItems = c.Boolean(nullable: false),
                        ContainsUncountedOnHand = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Bin);
            
        }
    }
}
