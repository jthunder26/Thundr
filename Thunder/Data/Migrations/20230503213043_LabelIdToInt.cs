using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Data.Migrations
{
    public partial class LabelIdToInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ReturnAddress",
                newName: "Uid");

          
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Uid",
                table: "ReturnAddress",
                newName: "Id");

           
           
        }
    }
}
