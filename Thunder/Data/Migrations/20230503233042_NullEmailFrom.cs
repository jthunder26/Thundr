using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Data.Migrations
{
    public partial class NullEmailFrom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "UnfinishedLabel");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "UpsOrderDetails",
                newName: "FromEmail");

            migrationBuilder.AddColumn<string>(
                name: "FromEmail",
                table: "UnfinishedLabel",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromEmail",
                table: "UnfinishedLabel");

            migrationBuilder.RenameColumn(
                name: "FromEmail",
                table: "UpsOrderDetails",
                newName: "Email");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "UnfinishedLabel",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
