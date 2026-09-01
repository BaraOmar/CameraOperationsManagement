using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CameraOperationsManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddCameras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cameras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    InstallationLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InstallationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RecorderId = table.Column<int>(type: "int", nullable: false),
                    NetworkSwitchId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cameras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cameras_NetworkSwitches_NetworkSwitchId",
                        column: x => x.NetworkSwitchId,
                        principalTable: "NetworkSwitches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cameras_Recorders_RecorderId",
                        column: x => x.RecorderId,
                        principalTable: "Recorders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_NetworkSwitchId",
                table: "Cameras",
                column: "NetworkSwitchId");

            migrationBuilder.CreateIndex(
                name: "IX_Cameras_RecorderId",
                table: "Cameras",
                column: "RecorderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cameras");
        }
    }
}
