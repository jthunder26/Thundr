using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Migrations
{
    public partial class CheckoutLabel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "checkedOut",
                table: "UnfinishedLabel");

            migrationBuilder.AddColumn<bool>(
                name: "checkedOut",
                table: "UpsOrderDetails",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "checkedOut",
                table: "UpsOrderDetails");

            migrationBuilder.AddColumn<bool>(
                name: "checkedOut",
                table: "UnfinishedLabel",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
