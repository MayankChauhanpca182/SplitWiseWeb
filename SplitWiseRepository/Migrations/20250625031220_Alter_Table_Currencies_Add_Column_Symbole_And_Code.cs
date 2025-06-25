using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitWiseRepository.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Table_Currencies_Add_Column_Symbole_And_Code : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Currencies",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Symbole",
                table: "Currencies",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "Symbole",
                table: "Currencies");
        }
    }
}
