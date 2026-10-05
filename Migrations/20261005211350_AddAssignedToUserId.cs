using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleToDoAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedToUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedToUserId",
                table: "TodoItems",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_AssignedToUserId",
                table: "TodoItems",
                column: "AssignedToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_AspNetUsers_AssignedToUserId",
                table: "TodoItems",
                column: "AssignedToUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_AspNetUsers_AssignedToUserId",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_AssignedToUserId",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "TodoItems");
        }
    }
}
