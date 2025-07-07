using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitWiseRepository.Migrations
{
    /// <inheritdoc />
    public partial class Alter_Table_GroupActivities_Add_Column_PaymentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentId",
                table: "GroupActivities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupActivities_PaymentId",
                table: "GroupActivities",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupActivities_Payments_PaymentId",
                table: "GroupActivities",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupActivities_Payments_PaymentId",
                table: "GroupActivities");

            migrationBuilder.DropIndex(
                name: "IX_GroupActivities_PaymentId",
                table: "GroupActivities");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "GroupActivities");
        }
    }
}
