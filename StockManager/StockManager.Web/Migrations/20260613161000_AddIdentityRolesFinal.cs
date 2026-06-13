using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManager.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityRolesFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Reference",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_Reference",
                table: "Products",
                column: "Reference",
                unique: true,
                filter: "[Reference] IS NOT NULL");
        }
    }
}
