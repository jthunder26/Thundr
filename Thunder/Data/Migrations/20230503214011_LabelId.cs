using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Data.Migrations
{
    public partial class LabelId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LabelIde",
                table: "UpsOrderDetails",
                newName: "LabelId");

            migrationBuilder.RenameColumn(
                name: "LabelIde",
                table: "UnfinishedLabelDetails",
                newName: "LabelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LabelId",
                table: "UpsOrderDetails",
                newName: "LabelIde");

            migrationBuilder.RenameColumn(
                name: "LabelId",
                table: "UnfinishedLabelDetails",
                newName: "LabelIde");
        }
    }
}
