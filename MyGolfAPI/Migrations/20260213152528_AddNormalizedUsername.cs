using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGolfAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedUsername",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // 1) Backfill from Username (trim + lower)
            migrationBuilder.Sql(@"
UPDATE dbo.Users
SET NormalizedUsername = LOWER(LTRIM(RTRIM(Username)))
WHERE NormalizedUsername IS NULL OR NormalizedUsername = '';
");

            // 2) Fix duplicates by appending row_number (chris, chris2, chris3...)
            migrationBuilder.Sql(@"
WITH d AS (
  SELECT
    Id,
    NormalizedUsername,
    ROW_NUMBER() OVER (PARTITION BY NormalizedUsername ORDER BY Id) AS rn
  FROM dbo.Users
)
UPDATE u
SET NormalizedUsername = CONCAT(u.NormalizedUsername, d.rn)
FROM dbo.Users u
JOIN d ON d.Id = u.Id
WHERE d.rn > 1;
");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedUsername",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedUsername",
                table: "Users",
                column: "NormalizedUsername",
                unique: true);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_NormalizedUsername",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedUsername",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }
    }
}
