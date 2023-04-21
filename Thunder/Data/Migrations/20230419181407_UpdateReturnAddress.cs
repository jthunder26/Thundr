using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Data.Migrations
{
    public partial class UpdateReturnAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromPhone",
                table: "ReturnAddress");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "ReturnAddress");

           
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
