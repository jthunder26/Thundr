using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Thunder.Migrations
{
    public partial class LabelDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnfinishedLabel");

            migrationBuilder.CreateTable(
                name: "LabelDetail",
                columns: table => new
                {
                    LabelId = table.Column<int>(type: "int", nullable: false),
                    Uid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LabelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Error = table.Column<int>(type: "int", nullable: false),
                    Retries = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LabelService = table.Column<int>(type: "int", nullable: false),
                    AIO_Attempt = table.Column<int>(type: "int", nullable: false),
                    Shipster_Attempt = table.Column<int>(type: "int", nullable: false),
                    ErrorMsg = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabelDetail", x => x.LabelId);
                    table.ForeignKey(
                        name: "FK_LabelDetail_UpsOrderDetails_LabelId",
                        column: x => x.LabelId,
                        principalTable: "UpsOrderDetails",
                        principalColumn: "LabelId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabelDetail");

            migrationBuilder.CreateTable(
                name: "UnfinishedLabel",
                columns: table => new
                {
                    LabelId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Error = table.Column<int>(type: "int", nullable: false),
                    FromEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LabelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Retries = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Uid = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
    }
}
