using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistantDatabase.Migrations
{
    /// <inheritdoc />
    public partial class Add_pointTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskPoints_Tasks_TaskDMId",
                table: "TaskPoints");

            migrationBuilder.DropIndex(
                name: "IX_TaskPoints_TaskDMId",
                table: "TaskPoints");

            migrationBuilder.DropColumn(
                name: "TaskDMId",
                table: "TaskPoints");

            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "TaskPoints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TaskPoints_TaskId",
                table: "TaskPoints",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskPoints_Tasks_TaskId",
                table: "TaskPoints",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskPoints_Tasks_TaskId",
                table: "TaskPoints");

            migrationBuilder.DropIndex(
                name: "IX_TaskPoints_TaskId",
                table: "TaskPoints");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "TaskPoints");

            migrationBuilder.AddColumn<int>(
                name: "TaskDMId",
                table: "TaskPoints",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskPoints_TaskDMId",
                table: "TaskPoints",
                column: "TaskDMId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskPoints_Tasks_TaskDMId",
                table: "TaskPoints",
                column: "TaskDMId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
