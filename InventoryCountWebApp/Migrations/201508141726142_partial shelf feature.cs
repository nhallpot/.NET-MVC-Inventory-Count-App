namespace InventoryCountWebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class partialshelffeature : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Items", "OnPartial", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Items", "OnPartial");
        }
    }
}
