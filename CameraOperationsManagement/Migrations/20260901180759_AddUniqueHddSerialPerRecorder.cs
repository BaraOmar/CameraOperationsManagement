using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CameraOperationsManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueHddSerialPerRecorder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecorderHardDrives_RecorderId",
                table: "RecorderHardDrives");

            migrationBuilder.CreateIndex(
                name: "IX_RecorderHardDrives_RecorderId_SerialNumber",
                table: "RecorderHardDrives",
                columns: new[] { "RecorderId", "SerialNumber" },
                unique: true,
                filter: "[SerialNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecorderHardDrives_RecorderId_SerialNumber",
                table: "RecorderHardDrives");

            migrationBuilder.CreateIndex(
                name: "IX_RecorderHardDrives_RecorderId",
                table: "RecorderHardDrives",
                column: "RecorderId");
        }
    }
}
