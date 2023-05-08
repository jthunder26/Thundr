using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Data.Migrations
{
    public partial class UpsOrderDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.CreateTable(
                name: "UpsOrderDetails",
                columns: table => new
                {
                    LabelId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    uid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    OurPrice = table.Column<long>(type: "bigint", nullable: true),
                    TotalAmount = table.Column<long>(type: "bigint", nullable: true),
                    Weight = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Length = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    Class = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpsOrderDetails", x => x.LabelId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UpsOrderDetails");

           
        }
    }
}
