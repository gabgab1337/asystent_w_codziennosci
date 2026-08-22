using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistantDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddNumberToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Number",
                table: "Tasks");
        }
    }
}
