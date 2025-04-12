using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecommendationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RecommendationSummary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "MedicationRecommendations",
                columns: table => new
                {
                    MedicationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecommendationUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    MedicationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Timing = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecommendationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationRecommendations", x => new { x.MedicationId, x.RecommendationUserId });
                    table.ForeignKey(
                        name: "FK_MedicationRecommendations_Recommendations_RecommendationUserId",
                        column: x => x.RecommendationUserId,
                        principalTable: "Recommendations",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRecommendations_RecommendationUserId",
                table: "MedicationRecommendations",
                column: "RecommendationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicationRecommendations");

            migrationBuilder.DropTable(
                name: "Recommendations");
        }
    }
}
