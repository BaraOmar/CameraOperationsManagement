using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CameraOperationsManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueSiteAndSwitchNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NetworkSwitches_SiteId",
                table: "NetworkSwitches");

            migrationBuilder.CreateIndex(
                name: "IX_Sites_Name",
                table: "Sites",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NetworkSwitches_SiteId_Name",
                table: "NetworkSwitches",
                columns: new[] { "SiteId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sites_Name",
                table: "Sites");

            migrationBuilder.DropIndex(
                name: "IX_NetworkSwitches_SiteId_Name",
                table: "NetworkSwitches");

            migrationBuilder.CreateIndex(
                name: "IX_NetworkSwitches_SiteId",
                table: "NetworkSwitches",
                column: "SiteId");
        }
    }
}
