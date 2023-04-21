using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Data.Migrations
{
    public partial class ReturnAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReturnAddress",
                columns: table => new
                {
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StateProvinceCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsReturnAddress = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnAddress", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "UpsOrder",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FromName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromCompany = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromZip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromCity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToCompany = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToZip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToCity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    Class = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpsOrder", x => x.UserName);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReturnAddress");

            migrationBuilder.DropTable(
                name: "UpsOrder");
        }
    }
}
