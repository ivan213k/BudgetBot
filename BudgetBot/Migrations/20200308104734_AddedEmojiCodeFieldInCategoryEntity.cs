using Microsoft.EntityFrameworkCore.Migrations;

namespace BudgetBot.Migrations
{
    public partial class AddedEmojiCodeFieldInCategoryEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "Categories",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "Categories");
        }
    }
}
