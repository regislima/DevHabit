﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevHabit.Api.Migrations.Application
{
    /// <inheritdoc />
    public partial class Add_Tags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tags",
                schema: "dev_habit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                schema: "dev_habit",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tags",
                schema: "dev_habit");
        }
    }
}
