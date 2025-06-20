using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitWiseRepository.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Table_Change_Column_Name_ProfileImagePath_To_ImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfileImagePath",
                table: "Groups",
                newName: "ImagePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Groups",
                newName: "ProfileImagePath");
        }
    }
}
