﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevHabit.Api.Migrations.Application
{
    /// <inheritdoc />
    public partial class Add_HabitTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HabitTags",
                schema: "dev_habit",
                columns: table => new
                {
                    HabitId = table.Column<string>(type: "character varying(500)", nullable: false),
                    TagId = table.Column<string>(type: "character varying(500)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitTags", x => new { x.HabitId, x.TagId });
                    table.ForeignKey(
                        name: "FK_HabitTags_Habits_HabitId",
                        column: x => x.HabitId,
                        principalSchema: "dev_habit",
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HabitTags_Tags_TagId",
                        column: x => x.TagId,
                        principalSchema: "dev_habit",
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HabitTags_TagId",
                schema: "dev_habit",
                table: "HabitTags",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HabitTags",
                schema: "dev_habit");
        }
    }
}
