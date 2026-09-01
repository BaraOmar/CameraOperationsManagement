using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CameraOperationsManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddCameraVisits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CameraVisits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CameraId = table.Column<int>(type: "int", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CameraVisits_Cameras_CameraId",
                        column: x => x.CameraId,
                        principalTable: "Cameras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CameraVisitWorkers",
                columns: table => new
                {
                    CameraVisitId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CameraVisitWorkers", x => new { x.CameraVisitId, x.WorkerId });
                    table.ForeignKey(
                        name: "FK_CameraVisitWorkers_CameraVisits_CameraVisitId",
                        column: x => x.CameraVisitId,
                        principalTable: "CameraVisits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CameraVisitWorkers_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CameraVisits_CameraId",
                table: "CameraVisits",
                column: "CameraId");

            migrationBuilder.CreateIndex(
                name: "IX_CameraVisitWorkers_WorkerId",
                table: "CameraVisitWorkers",
                column: "WorkerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CameraVisitWorkers");

            migrationBuilder.DropTable(
                name: "CameraVisits");
        }
    }
}
