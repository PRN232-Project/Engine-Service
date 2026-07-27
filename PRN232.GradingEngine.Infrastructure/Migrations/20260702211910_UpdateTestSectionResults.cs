using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232.GradingEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTestSectionResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Band1Passed",
                table: "Submissions",
                schema: "grading",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TestSectionResults",
                table: "Submissions",
                schema: "grading",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Band1Passed",
                table: "Submissions",
                schema: "grading");

            migrationBuilder.DropColumn(
                name: "TestSectionResults",
                table: "Submissions",
                schema: "grading");
        }
    }
}
