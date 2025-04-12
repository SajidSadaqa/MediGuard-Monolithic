using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdherenceService.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdherenceRecords",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    MedicationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScheduledDoseTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualDoseTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDoseTaken = table.Column<bool>(type: "bit", nullable: false),
                    AlertTriggered = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdherenceRecords", x => new { x.UserId, x.MedicationId, x.ScheduledDoseTime });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdherenceRecords");
        }
    }
}
