using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitWiseRepository.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Table_Addd_Column_FriendRequestId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FriendRequestId",
                table: "Friends",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friends_FriendRequestId",
                table: "Friends",
                column: "FriendRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friends_FriendRequests_FriendRequestId",
                table: "Friends",
                column: "FriendRequestId",
                principalTable: "FriendRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friends_FriendRequests_FriendRequestId",
                table: "Friends");

            migrationBuilder.DropIndex(
                name: "IX_Friends_FriendRequestId",
                table: "Friends");

            migrationBuilder.DropColumn(
                name: "FriendRequestId",
                table: "Friends");
        }
    }
}
