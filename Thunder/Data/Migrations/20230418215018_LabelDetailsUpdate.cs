using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Data.Migrations
{
    public partial class LabelDetailsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "LabelDetails",
                newName: "Uid");

            migrationBuilder.AddColumn<string>(
                name: "OrderId",
                table: "LabelDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "LabelDetails");

            migrationBuilder.RenameColumn(
                name: "Uid",
                table: "LabelDetails",
                newName: "Id");
        }
    }
}
