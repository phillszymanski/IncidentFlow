using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IncidentFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Incidents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Incidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Incidents");
        }
    }
}
