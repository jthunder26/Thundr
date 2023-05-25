using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Migrations
{
    public partial class RateCosts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RateCosts",
                columns: table => new
                {
                    LabelId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    serviceClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalCost = table.Column<long>(type: "bigint", nullable: false),
                    TotalCharge = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RateCosts", x => x.LabelId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RateCosts");
        }
    }
}
