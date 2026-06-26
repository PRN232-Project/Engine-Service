using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN232.GradingEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateRelational : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExamRubrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamCode = table.Column<string>(type: "text", nullable: false),
                    MaxScore = table.Column<decimal>(type: "numeric", nullable: false),
                    SolutionPattern = table.Column<string>(type: "text", nullable: false),
                    ForbidHardcodedConnectionString = table.Column<bool>(type: "boolean", nullable: false),
                    DeductionPointsPerNamingError = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRubrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<string>(type: "text", nullable: false),
                    ExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Band0Passed = table.Column<bool>(type: "boolean", nullable: false),
                    NamingViolations = table.Column<List<string>>(type: "text[]", nullable: false),
                    BuildErrors = table.Column<List<string>>(type: "text[]", nullable: false),
                    ScoreDeductions = table.Column<decimal>(type: "numeric", nullable: false),
                    FinalScore = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GradedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequiredFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamRubricId = table.Column<Guid>(type: "uuid", nullable: false),
                    Pattern = table.Column<string>(type: "text", nullable: false),
                    MustExist = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequiredFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequiredFiles_ExamRubrics_ExamRubricId",
                        column: x => x.ExamRubricId,
                        principalTable: "ExamRubrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequiredProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamRubricId = table.Column<Guid>(type: "uuid", nullable: false),
                    Pattern = table.Column<string>(type: "text", nullable: false),
                    MustExist = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequiredProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequiredProjects_ExamRubrics_ExamRubricId",
                        column: x => x.ExamRubricId,
                        principalTable: "ExamRubrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequiredFiles_ExamRubricId",
                table: "RequiredFiles",
                column: "ExamRubricId");

            migrationBuilder.CreateIndex(
                name: "IX_RequiredProjects_ExamRubricId",
                table: "RequiredProjects",
                column: "ExamRubricId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequiredFiles");

            migrationBuilder.DropTable(
                name: "RequiredProjects");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "ExamRubrics");
        }
    }
}
