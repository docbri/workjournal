using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkJournalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkItemPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "WorkItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "WorkItems");
        }
    }
}
