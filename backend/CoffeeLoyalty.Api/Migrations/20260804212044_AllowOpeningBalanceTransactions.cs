using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeLoyalty.Api.Migrations
{
    /// <inheritdoc />
    public partial class AllowOpeningBalanceTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_PointsTransaction_Order_Type",
                table: "PointsTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "PointsTransactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "UX_PointsTransaction_Order_Type",
                table: "PointsTransactions",
                columns: new[] { "OrderId", "Type" },
                unique: true,
                filter: "[Type] <> 'Refund' AND [OrderId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PointsTransaction_Order",
                table: "PointsTransactions",
                sql: "[Type] = 'OpeningBalance' OR [OrderId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_PointsTransaction_Order_Type",
                table: "PointsTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PointsTransaction_Order",
                table: "PointsTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "PointsTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_PointsTransaction_Order_Type",
                table: "PointsTransactions",
                columns: new[] { "OrderId", "Type" },
                unique: true,
                filter: "[Type] <> 'Refund'");
        }
    }
}
