using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Migrations
{
    public partial class OrderDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OgPrice",
                table: "LabelDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OurPrice",
                table: "LabelDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PercentSaved",
                table: "LabelDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TotalCharge",
                table: "LabelDetail",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OgPrice",
                table: "LabelDetail");

            migrationBuilder.DropColumn(
                name: "OurPrice",
                table: "LabelDetail");

            migrationBuilder.DropColumn(
                name: "PercentSaved",
                table: "LabelDetail");

            migrationBuilder.DropColumn(
                name: "TotalCharge",
                table: "LabelDetail");
        }
    }
}
