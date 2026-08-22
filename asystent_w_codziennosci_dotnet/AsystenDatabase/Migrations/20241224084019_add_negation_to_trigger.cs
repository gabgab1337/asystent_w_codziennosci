using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistantDatabase.Migrations
{
    /// <inheritdoc />
    public partial class add_negation_to_trigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Negation",
                table: "TriggerTasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Negation",
                table: "TriggerPoints",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Negation",
                table: "TriggerTasks");

            migrationBuilder.DropColumn(
                name: "Negation",
                table: "TriggerPoints");
        }
    }
}
