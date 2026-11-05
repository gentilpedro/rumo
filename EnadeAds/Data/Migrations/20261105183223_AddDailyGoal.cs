using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnadeAds.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyGoal",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyGoal",
                table: "AspNetUsers");
        }
    }
}
