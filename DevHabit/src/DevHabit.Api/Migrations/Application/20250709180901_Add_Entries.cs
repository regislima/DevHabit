using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevHabit.Api.Migrations.Application
{
    /// <inheritdoc />
    public partial class Add_Entries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AutomationSource",
                schema: "dev_habit",
                table: "Habits",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Entries",
                schema: "dev_habit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HabitId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UserId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entries_Habits_HabitId",
                        column: x => x.HabitId,
                        principalSchema: "dev_habit",
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Entries_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dev_habit",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entries_ExternalId",
                schema: "dev_habit",
                table: "Entries",
                column: "ExternalId",
                unique: true,
                filter: "\"ExternalId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_HabitId",
                schema: "dev_habit",
                table: "Entries",
                column: "HabitId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_UserId",
                schema: "dev_habit",
                table: "Entries",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entries",
                schema: "dev_habit");

            migrationBuilder.DropColumn(
                name: "AutomationSource",
                schema: "dev_habit",
                table: "Habits");
        }
    }
}
