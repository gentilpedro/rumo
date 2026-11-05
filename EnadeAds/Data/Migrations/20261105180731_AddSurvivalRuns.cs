using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EnadeAds.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSurvivalRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SurvivalRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    TrackId = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Lives = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Queue = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurvivalRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurvivalRuns_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SurvivalRuns_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurvivalRuns_TrackId",
                table: "SurvivalRuns",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_SurvivalRuns_UserId_TrackId",
                table: "SurvivalRuns",
                columns: new[] { "UserId", "TrackId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurvivalRuns");
        }
    }
}
