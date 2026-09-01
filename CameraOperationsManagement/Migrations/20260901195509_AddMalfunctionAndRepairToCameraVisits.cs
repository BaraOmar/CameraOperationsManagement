using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CameraOperationsManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddMalfunctionAndRepairToCameraVisits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MalfunctionDescription",
                table: "CameraVisits",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MalfunctionType",
                table: "CameraVisits",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepairResult",
                table: "CameraVisits",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepairWorkPerformed",
                table: "CameraVisits",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MalfunctionDescription",
                table: "CameraVisits");

            migrationBuilder.DropColumn(
                name: "MalfunctionType",
                table: "CameraVisits");

            migrationBuilder.DropColumn(
                name: "RepairResult",
                table: "CameraVisits");

            migrationBuilder.DropColumn(
                name: "RepairWorkPerformed",
                table: "CameraVisits");
        }
    }
}
