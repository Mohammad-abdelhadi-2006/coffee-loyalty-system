using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeLoyalty.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOpeningBalanceUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_PointsTransaction_Customer_OpeningBalance",
                table: "PointsTransactions",
                column: "CustomerId",
                unique: true,
                filter: "[Type] = 'OpeningBalance'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_PointsTransaction_Customer_OpeningBalance",
                table: "PointsTransactions");
        }
    }
}
