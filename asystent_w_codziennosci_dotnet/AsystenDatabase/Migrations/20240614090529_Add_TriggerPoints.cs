using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistantDatabase.Migrations
{
    /// <inheritdoc />
    public partial class Add_TriggerPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TriggerPoints",
                columns: table => new
                {
                    PointId = table.Column<int>(type: "int", nullable: false),
                    TriggerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriggerPoints", x => new { x.TriggerId, x.PointId });
                    table.ForeignKey(
                        name: "FK_TriggerPoints_TaskPoints_PointId",
                        column: x => x.PointId,
                        principalTable: "TaskPoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TriggerPoints_Trigers_TriggerId",
                        column: x => x.TriggerId,
                        principalTable: "Trigers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TriggerPoints_PointId",
                table: "TriggerPoints",
                column: "PointId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TriggerPoints");
        }
    }
}
