using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistantDatabase.Migrations
{
    /// <inheritdoc />
    public partial class add_trigger_to_task : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TriggerTasks",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    TriggerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriggerTasks", x => new { x.TriggerId, x.TaskId });
                    table.ForeignKey(
                        name: "FK_TriggerTasks_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TriggerTasks_Trigers_TriggerId",
                        column: x => x.TriggerId,
                        principalTable: "Trigers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TriggerTasks_TaskId",
                table: "TriggerTasks",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TriggerTasks");
        }
    }
}
