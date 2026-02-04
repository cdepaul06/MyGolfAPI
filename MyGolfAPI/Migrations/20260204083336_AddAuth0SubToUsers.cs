using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGolfAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAuth0SubToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Auth0Sub",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Auth0Sub",
                table: "Users",
                column: "Auth0Sub",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Auth0Sub",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Auth0Sub",
                table: "Users");
        }
    }
}
