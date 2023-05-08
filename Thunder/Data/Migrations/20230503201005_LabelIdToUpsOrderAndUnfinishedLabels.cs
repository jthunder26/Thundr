using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Data.Migrations
{
    public partial class LabelIdToUpsOrderAndUnfinishedLabels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UpsOrder",
                table: "UpsOrder");

            migrationBuilder.DropColumn(
                name: "OrderID",
                table: "UpsOrder");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "UnfinishedLabelDetails",
                newName: "LabelId");

            migrationBuilder.AlterColumn<string>(
                name: "uid",
                table: "UpsOrder",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "LabelId",
                table: "UpsOrder",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UpsOrder",
                table: "UpsOrder",
                column: "LabelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UpsOrder",
                table: "UpsOrder");

            migrationBuilder.DropColumn(
                name: "LabelId",
                table: "UpsOrder");

            migrationBuilder.RenameColumn(
                name: "LabelId",
                table: "UnfinishedLabelDetails",
                newName: "OrderId");

            migrationBuilder.AlterColumn<string>(
                name: "uid",
                table: "UpsOrder",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "OrderID",
                table: "UpsOrder",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UpsOrder",
                table: "UpsOrder",
                column: "uid");
        }
    }
}
