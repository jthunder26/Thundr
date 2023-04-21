using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Data.Migrations
{
    public partial class ShipFrom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShipFrom",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Zip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipFrom", x => x.UserName);
                });

            migrationBuilder.CreateTable(
                name: "ShipTo",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Zip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipTo", x => x.UserName);
                });

            migrationBuilder.CreateTable(
                name: "UpsOrder",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FromName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromCompany = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromZip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromCity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToCompany = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToZip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToAddress1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToAddress2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                name: "ShipFrom");

            migrationBuilder.DropTable(
                name: "ShipTo");

            migrationBuilder.DropTable(
                name: "UpsOrder");
        }
    }
}
