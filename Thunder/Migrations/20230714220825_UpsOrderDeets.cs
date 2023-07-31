using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Migrations
{
    public partial class UpsOrderDeets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OgPrice",
                table: "UpsOrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PercentSaved",
                table: "UpsOrderDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OgPrice",
                table: "UpsOrderDetails");

            migrationBuilder.DropColumn(
                name: "PercentSaved",
                table: "UpsOrderDetails");
        }
    }
}
