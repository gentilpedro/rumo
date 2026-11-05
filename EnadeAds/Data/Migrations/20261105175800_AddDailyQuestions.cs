using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnadeAds.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyQuestions",
                columns: table => new
                {
                    TrackId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    QuestionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyQuestions", x => new { x.TrackId, x.Date });
                    table.ForeignKey(
                        name: "FK_DailyQuestions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyQuestions_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyQuestions_QuestionId",
                table: "DailyQuestions",
                column: "QuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyQuestions");
        }
    }
}
