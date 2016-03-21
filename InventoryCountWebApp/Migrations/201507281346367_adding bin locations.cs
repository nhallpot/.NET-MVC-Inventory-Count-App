namespace InventoryCountWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addingbinlocations : DbMigration
    {
        public override void Up()
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
        
        public override void Down()
        {
            DropTable("dbo.BinLocations");
        }
    }
}
