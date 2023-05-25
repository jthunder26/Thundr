using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Migrations
{
    public partial class CostDataType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TotalCost",
                table: "RateCosts",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "TotalCharge",
                table: "RateCosts",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "TotalCost",
                table: "RateCosts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "TotalCharge",
                table: "RateCosts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
