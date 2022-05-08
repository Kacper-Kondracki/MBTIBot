using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MBTIBot.Migrations
{
    public partial class AddTrackedMessageGuild : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "Guild",
                table: "TrackedMessages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Guild",
                table: "TrackedMessages");
        }
    }
}
