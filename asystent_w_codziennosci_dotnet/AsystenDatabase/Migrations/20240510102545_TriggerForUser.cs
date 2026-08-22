using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistantDatabase.Migrations
{
    /// <inheritdoc />
    public partial class TriggerForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CaregiverId",
                table: "Trigers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Trigers_CaregiverId",
                table: "Trigers",
                column: "CaregiverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trigers_Users_CaregiverId",
                table: "Trigers",
                column: "CaregiverId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trigers_Users_CaregiverId",
                table: "Trigers");

            migrationBuilder.DropIndex(
                name: "IX_Trigers_CaregiverId",
                table: "Trigers");

            migrationBuilder.DropColumn(
                name: "CaregiverId",
                table: "Trigers");
        }
    }
}
