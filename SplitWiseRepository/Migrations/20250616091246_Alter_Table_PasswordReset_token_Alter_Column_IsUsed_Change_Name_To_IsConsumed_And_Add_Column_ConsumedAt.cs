using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitWiseRepository.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Table_PasswordReset_token_Alter_Column_IsUsed_Change_Name_To_IsConsumed_And_Add_Column_ConsumedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailAddress",
                table: "PasswordResetTokens");

            migrationBuilder.RenameColumn(
                name: "IsUsed",
                table: "PasswordResetTokens",
                newName: "IsConsumed");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsumedAt",
                table: "PasswordResetTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "PasswordResetTokens",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordResetTokens_Users_UserId",
                table: "PasswordResetTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordResetTokens_Users_UserId",
                table: "PasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens");

            migrationBuilder.DropColumn(
                name: "ConsumedAt",
                table: "PasswordResetTokens");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PasswordResetTokens");

            migrationBuilder.RenameColumn(
                name: "IsConsumed",
                table: "PasswordResetTokens",
                newName: "IsUsed");

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                table: "PasswordResetTokens",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }
    }
}
