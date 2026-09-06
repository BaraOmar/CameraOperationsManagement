using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CameraOperationsManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddNetworkSwitchPorts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberOfPorts",
                table: "NetworkSwitches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "NetworkSwitchPorts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NetworkSwitchId = table.Column<int>(type: "int", nullable: false),
                    PortNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CameraId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NetworkSwitchPorts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NetworkSwitchPorts_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NetworkSwitchPorts_NetworkSwitches_NetworkSwitchId",
                        column: x => x.NetworkSwitchId,
                        principalTable: "NetworkSwitches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NetworkSwitchPorts_CameraId",
                table: "NetworkSwitchPorts",
                column: "CameraId",
                unique: true,
                filter: "[CameraId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkSwitchPorts_NetworkSwitchId_PortNumber",
                table: "NetworkSwitchPorts",
                columns: new[] { "NetworkSwitchId", "PortNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NetworkSwitchPorts");

            migrationBuilder.DropColumn(
                name: "NumberOfPorts",
                table: "NetworkSwitches");
        }
    }
}
