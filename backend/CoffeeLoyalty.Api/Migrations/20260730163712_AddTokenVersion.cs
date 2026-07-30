using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeLoyalty.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TokenVersion",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TokenVersion",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenVersion",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TokenVersion",
                table: "Customers");
        }
    }
}
