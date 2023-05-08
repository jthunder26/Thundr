using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Data.Migrations
{
    public partial class UpsOrderandUnfinishedLabel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.CreateTable(
                name: "UpsOrderDetails",
                columns: table => new
                {
                    LabelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    ToPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    Class = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpsOrderDetails", x => x.LabelId);
                });

            migrationBuilder.CreateTable(
                name: "UnfinishedLabel",
                columns: table => new
                {
                    LabelId = table.Column<int>(type: "int", nullable: false),
                    Uid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LabelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnfinishedLabel", x => x.LabelId);
                    table.ForeignKey(
                        name: "FK_UnfinishedLabel_UpsOrderDetails_LabelId",
                        column: x => x.LabelId,
                        principalTable: "UpsOrderDetails",
                        principalColumn: "LabelId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnfinishedLabel");

            migrationBuilder.DropTable(
                name: "UpsOrderDetails");

           
        }
    }
}
