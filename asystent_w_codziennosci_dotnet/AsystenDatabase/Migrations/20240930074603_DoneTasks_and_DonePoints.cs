using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistantDatabase.Migrations
{
    /// <inheritdoc />
    public partial class DoneTasks_and_DonePoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DonePoints",
                columns: table => new
                {
                    PointId = table.Column<int>(type: "int", nullable: false),
                    FinishTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonePoints", x => new { x.PointId, x.FinishTime });
                    table.ForeignKey(
                        name: "FK_DonePoints_TaskPoints_PointId",
                        column: x => x.PointId,
                        principalTable: "TaskPoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoneTasks",
                columns: table => new
                {
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    FinishTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoneTasks", x => new { x.TaskId, x.FinishTime });
                    table.ForeignKey(
                        name: "FK_DoneTasks_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DonePoints");

            migrationBuilder.DropTable(
                name: "DoneTasks");
        }
    }
}
