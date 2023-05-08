using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Data.Migrations
{
    public partial class UnfinishedLabelDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderID",
                table: "UpsOrder",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "UnfinishedLabelDetails",
                columns: table => new
                {
                    OrderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Uid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LabelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnfinishedLabelDetails", x => x.OrderId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnfinishedLabelDetails");

            migrationBuilder.DropColumn(
                name: "OrderID",
                table: "UpsOrder");
        }
    }
}
