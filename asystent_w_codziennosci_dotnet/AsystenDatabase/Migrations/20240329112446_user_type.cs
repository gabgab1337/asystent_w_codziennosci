using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistantDatabase.Migrations
{
    /// <inheritdoc />
    public partial class user_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsCaregiver",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCaregiver",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
