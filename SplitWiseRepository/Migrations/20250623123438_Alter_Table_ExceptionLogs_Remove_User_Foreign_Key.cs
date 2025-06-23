using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitWiseRepository.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Table_ExceptionLogs_Remove_User_Foreign_Key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExceptionLogs_Users_UserId",
                table: "ExceptionLogs");

            migrationBuilder.DropIndex(
                name: "IX_ExceptionLogs_UserId",
                table: "ExceptionLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ExceptionLogs_UserId",
                table: "ExceptionLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExceptionLogs_Users_UserId",
                table: "ExceptionLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
