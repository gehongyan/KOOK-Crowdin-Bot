using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kook.Bot.Crowdin.Migrations
{
    public partial class AddTermItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Terms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    GlossaryId = table.Column<int>(type: "INTEGER", nullable: false),
                    LanguageId = table.Column<string>(type: "TEXT", nullable: true),
                    Text = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    PartOfSpeech = table.Column<int>(type: "INTEGER", nullable: false),
                    Lemma = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ParentEntityId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Terms_Terms_ParentEntityId",
                        column: x => x.ParentEntityId,
                        principalTable: "Terms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Terms_ParentEntityId",
                table: "Terms",
                column: "ParentEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Terms");
        }
    }
}
