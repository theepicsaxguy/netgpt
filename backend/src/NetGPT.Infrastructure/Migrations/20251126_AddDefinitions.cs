// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetGPT.Infrastructure.Migrations
{
    public partial class AddDefinitions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "Definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Kind = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    ContentYaml = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ContentHash = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Definitions", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(
                name: "Definitions");
        }
    }
}
