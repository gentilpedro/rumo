using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EnadeAds.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendActivityAndInvites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Duels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShareActivity",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "FriendInvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromId = table.Column<string>(type: "text", nullable: false),
                    ToId = table.Column<string>(type: "text", nullable: false),
                    DuelId = table.Column<int>(type: "integer", nullable: true),
                    RoomId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FriendInvites_AspNetUsers_FromId",
                        column: x => x.FromId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FriendInvites_AspNetUsers_ToId",
                        column: x => x.ToId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FriendInvites_Duels_DuelId",
                        column: x => x.DuelId,
                        principalTable: "Duels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FriendInvites_StudyRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "StudyRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Duels_RoomId",
                table: "Duels",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendInvites_DuelId",
                table: "FriendInvites",
                column: "DuelId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendInvites_FromId",
                table: "FriendInvites",
                column: "FromId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendInvites_RoomId",
                table: "FriendInvites",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendInvites_ToId",
                table: "FriendInvites",
                column: "ToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Duels_StudyRooms_RoomId",
                table: "Duels",
                column: "RoomId",
                principalTable: "StudyRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Duels_StudyRooms_RoomId",
                table: "Duels");

            migrationBuilder.DropTable(
                name: "FriendInvites");

            migrationBuilder.DropIndex(
                name: "IX_Duels_RoomId",
                table: "Duels");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Duels");

            migrationBuilder.DropColumn(
                name: "ShareActivity",
                table: "AspNetUsers");
        }
    }
}
