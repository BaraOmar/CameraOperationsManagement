using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CameraOperationsManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueRecorderNamePerSite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recorders_SiteId",
                table: "Recorders");

            migrationBuilder.CreateIndex(
                name: "IX_Recorders_SiteId_Name",
                table: "Recorders",
                columns: new[] { "SiteId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recorders_SiteId_Name",
                table: "Recorders");

            migrationBuilder.CreateIndex(
                name: "IX_Recorders_SiteId",
                table: "Recorders",
                column: "SiteId");
        }
    }
}
