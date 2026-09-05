using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CameraOperationsManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddUnifiedVisits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ComponentType = table.Column<int>(type: "int", nullable: false),
                    RecorderId = table.Column<int>(type: "int", nullable: true),
                    NetworkSwitchId = table.Column<int>(type: "int", nullable: true),
                    CameraId = table.Column<int>(type: "int", nullable: true),
                    VisitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MalfunctionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MalfunctionDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RepairWorkPerformed = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RepairResult = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                    table.CheckConstraint("CK_Visits_Component", "(\r\n    [ComponentType] = 1\r\n    AND [RecorderId] IS NOT NULL\r\n    AND [NetworkSwitchId] IS NULL\r\n    AND [CameraId] IS NULL\r\n)\r\nOR\r\n(\r\n    [ComponentType] = 2\r\n    AND [RecorderId] IS NULL\r\n    AND [NetworkSwitchId] IS NOT NULL\r\n    AND [CameraId] IS NULL\r\n)\r\nOR\r\n(\r\n    [ComponentType] = 3\r\n    AND [RecorderId] IS NULL\r\n    AND [NetworkSwitchId] IS NULL\r\n    AND [CameraId] IS NOT NULL\r\n)");
                    table.ForeignKey(
                        name: "FK_Visits_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_NetworkSwitches_NetworkSwitchId",
                        column: x => x.NetworkSwitchId,
                        principalTable: "NetworkSwitches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Recorders_RecorderId",
                        column: x => x.RecorderId,
                        principalTable: "Recorders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Visits_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VisitWorkers",
                columns: table => new
                {
                    VisitId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitWorkers", x => new { x.VisitId, x.WorkerId });
                    table.ForeignKey(
                        name: "FK_VisitWorkers_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisitWorkers_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_CameraId",
                table: "Visits",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_NetworkSwitchId",
                table: "Visits",
                column: "NetworkSwitchId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_RecorderId",
                table: "Visits",
                column: "RecorderId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_SiteId_VisitDate",
                table: "Visits",
                columns: new[] { "SiteId", "VisitDate" });

            migrationBuilder.CreateIndex(
                name: "IX_VisitWorkers_WorkerId",
                table: "VisitWorkers",
                column: "WorkerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VisitWorkers");

            migrationBuilder.DropTable(
                name: "Visits");
        }
    }
}
