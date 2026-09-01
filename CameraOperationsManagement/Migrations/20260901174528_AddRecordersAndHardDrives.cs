using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CameraOperationsManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordersAndHardDrives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recorders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NetworkSwitchId = table.Column<int>(type: "int", nullable: true),
                    HasStorage = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recorders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recorders_NetworkSwitches_NetworkSwitchId",
                        column: x => x.NetworkSwitchId,
                        principalTable: "NetworkSwitches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recorders_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecorderHardDrives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecorderId = table.Column<int>(type: "int", nullable: false),
                    CapacityGb = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecorderHardDrives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecorderHardDrives_Recorders_RecorderId",
                        column: x => x.RecorderId,
                        principalTable: "Recorders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecorderHardDrives_RecorderId",
                table: "RecorderHardDrives",
                column: "RecorderId");

            migrationBuilder.CreateIndex(
                name: "IX_Recorders_NetworkSwitchId",
                table: "Recorders",
                column: "NetworkSwitchId");

            migrationBuilder.CreateIndex(
                name: "IX_Recorders_SiteId",
                table: "Recorders",
                column: "SiteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecorderHardDrives");

            migrationBuilder.DropTable(
                name: "Recorders");
        }
    }
}
