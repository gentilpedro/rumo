using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnadeAds.Data.Migrations
{
    /// <inheritdoc />
    public partial class WidenQuestionStatement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Statement",
                table: "Questions",
                type: "character varying(12000)",
                maxLength: 12000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(8000)",
                oldMaxLength: 8000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Statement",
                table: "Questions",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(12000)",
                oldMaxLength: 12000);
        }
    }
}
