using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistantDatabase.Migrations
{
    /// <inheritdoc />
    public partial class addPersonToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CaregiverId = table.Column<int>(type: "int", nullable: false),
                    AsdPersonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_AsdPersonId",
                        column: x => x.AsdPersonId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_CaregiverId",
                        column: x => x.CaregiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MinTime = table.Column<int>(type: "int", nullable: false),
                    AvgTime = table.Column<int>(type: "int", nullable: false),
                    TaskDMId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskPoints_Tasks_TaskDMId",
                        column: x => x.TaskDMId,
                        principalTable: "Tasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskPoints_TaskDMId",
                table: "TaskPoints",
                column: "TaskDMId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AsdPersonId",
                table: "Tasks",
                column: "AsdPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CaregiverId",
                table: "Tasks",
                column: "CaregiverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskPoints");

            migrationBuilder.DropTable(
                name: "Tasks");
        }
    }
}
