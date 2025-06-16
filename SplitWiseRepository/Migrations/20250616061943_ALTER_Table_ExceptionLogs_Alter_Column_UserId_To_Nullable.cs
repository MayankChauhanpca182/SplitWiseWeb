using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitWiseRepository.Migrations
{
    /// <inheritdoc />
    public partial class ALTER_Table_ExceptionLogs_Alter_Column_UserId_To_Nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExceptionLogs_Users_UserId",
                table: "ExceptionLogs");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "ExceptionLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ExceptionLogs_Users_UserId",
                table: "ExceptionLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExceptionLogs_Users_UserId",
                table: "ExceptionLogs");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "ExceptionLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExceptionLogs_Users_UserId",
                table: "ExceptionLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
