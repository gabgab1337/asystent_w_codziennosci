using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistantDatabase.Migrations
{
    /// <inheritdoc />
    public partial class DoneTasks_and_DonePoints_FinishDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FinishTime",
                table: "DoneTasks",
                newName: "FinishDate");

            migrationBuilder.RenameColumn(
                name: "FinishTime",
                table: "DonePoints",
                newName: "FinishDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FinishDate",
                table: "DoneTasks",
                newName: "FinishTime");

            migrationBuilder.RenameColumn(
                name: "FinishDate",
                table: "DonePoints",
                newName: "FinishTime");
        }
    }
}
