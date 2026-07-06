using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PRN232.LMS.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Semester",
                columns: table => new
                {
                    SemesterId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SemesterName = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semester", x => x.SemesterId);
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.StudentId);
                });

            migrationBuilder.CreateTable(
                name: "Subject",
                columns: table => new
                {
                    SubjectId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubjectCode = table.Column<string>(type: "text", nullable: false),
                    SubjectName = table.Column<string>(type: "text", nullable: false),
                    Credit = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subject", x => x.SubjectId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseName = table.Column<string>(type: "text", nullable: false),
                    SemesterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.CourseId);
                    table.ForeignKey(
                        name: "FK_Course_Semester_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semester",
                        principalColumn: "SemesterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrollment",
                columns: table => new
                {
                    EnrollmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    EnrollDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollment", x => x.EnrollmentId);
                    table.ForeignKey(
                        name: "FK_Enrollment_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrollment_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Semester",
                columns: new[] { "SemesterId", "EndDate", "SemesterName", "StartDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fall 2023", new DateTime(2023, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Spring 2024", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2024, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Summer 2024", new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Fall 2024", new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Spring 2025", new DateTime(2025, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Student",
                columns: new[] { "StudentId", "DateOfBirth", "Email", "FullName" },
                values: new object[,]
                {
                    { 1, new DateTime(2003, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "an.nv@fpt.edu.vn", "Nguyen Van An" },
                    { 2, new DateTime(2003, 3, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "binh.lt@fpt.edu.vn", "Le Thi Binh" },
                    { 3, new DateTime(2002, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "cuong.tv@fpt.edu.vn", "Tran Van Cuong" },
                    { 4, new DateTime(2003, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "dung.pt@fpt.edu.vn", "Pham Thi Dung" },
                    { 5, new DateTime(2002, 4, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "em.hv@fpt.edu.vn", "Hoang Van Em" },
                    { 6, new DateTime(2003, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "phuong.vt@fpt.edu.vn", "Vo Thi Phuong" },
                    { 7, new DateTime(2002, 9, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "quan.nm@fpt.edu.vn", "Nguyen Minh Quan" },
                    { 8, new DateTime(2003, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "hoa.bt@fpt.edu.vn", "Bui Thi Hoa" },
                    { 9, new DateTime(2002, 12, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "khoa.dv@fpt.edu.vn", "Do Van Khoa" },
                    { 10, new DateTime(2003, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "lan.tt@fpt.edu.vn", "Tran Thi Lan" },
                    { 11, new DateTime(2002, 8, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "manh.pv@fpt.edu.vn", "Phan Van Manh" },
                    { 12, new DateTime(2003, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "nam.dt@fpt.edu.vn", "Dinh Thi Nam" },
                    { 13, new DateTime(2002, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "oanh.lv@fpt.edu.vn", "Le Van Oanh" },
                    { 14, new DateTime(2003, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "phuong.nt@fpt.edu.vn", "Nguyen Thi Phuong" },
                    { 15, new DateTime(2002, 1, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "quy.tv@fpt.edu.vn", "Tran Van Quy" },
                    { 16, new DateTime(2003, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "rang.ht@fpt.edu.vn", "Hoang Thi Rang" },
                    { 17, new DateTime(2002, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "son.vv@fpt.edu.vn", "Vo Van Son" },
                    { 18, new DateTime(2003, 9, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "thanh.bv@fpt.edu.vn", "Bui Van Thanh" },
                    { 19, new DateTime(2002, 11, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "uyen.dt@fpt.edu.vn", "Do Thi Uyen" },
                    { 20, new DateTime(2003, 2, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "viet.pv@fpt.edu.vn", "Pham Van Viet" },
                    { 21, new DateTime(2002, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "xuan.nt@fpt.edu.vn", "Nguyen Thi Xuan" },
                    { 22, new DateTime(2003, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "yen.lv@fpt.edu.vn", "Le Van Yen" },
                    { 23, new DateTime(2002, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "zung.tm@fpt.edu.vn", "Tran Minh Zung" },
                    { 24, new DateTime(2003, 12, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "anh.pt@fpt.edu.vn", "Phan Thi Anh" },
                    { 25, new DateTime(2002, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "bach.dv@fpt.edu.vn", "Dinh Van Bach" },
                    { 26, new DateTime(2003, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "chi.ht@fpt.edu.vn", "Hoang Thi Chi" },
                    { 27, new DateTime(2002, 7, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "dat.vv@fpt.edu.vn", "Vo Van Dat" },
                    { 28, new DateTime(2003, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "diem.bt@fpt.edu.vn", "Bui Thi Diem" },
                    { 29, new DateTime(2002, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "duc.dv@fpt.edu.vn", "Do Van Duc" },
                    { 30, new DateTime(2003, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "gia.nv@fpt.edu.vn", "Nguyen Van Gia" },
                    { 31, new DateTime(2002, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "ha.lt@fpt.edu.vn", "Le Thi Ha" },
                    { 32, new DateTime(2003, 6, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "hung.tv@fpt.edu.vn", "Tran Van Hung" },
                    { 33, new DateTime(2002, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "huong.pt@fpt.edu.vn", "Pham Thi Huong" },
                    { 34, new DateTime(2003, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "khanh.hv@fpt.edu.vn", "Hoang Van Khanh" },
                    { 35, new DateTime(2002, 12, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "kim.vt@fpt.edu.vn", "Vo Thi Kim" },
                    { 36, new DateTime(2003, 2, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "long.bv@fpt.edu.vn", "Bui Van Long" },
                    { 37, new DateTime(2002, 5, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "mai.dt@fpt.edu.vn", "Do Thi Mai" },
                    { 38, new DateTime(2003, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "minh.pv@fpt.edu.vn", "Phan Van Minh" },
                    { 39, new DateTime(2002, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "ngoc.dt@fpt.edu.vn", "Dinh Thi Ngoc" },
                    { 40, new DateTime(2003, 11, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "phong.nv@fpt.edu.vn", "Nguyen Van Phong" },
                    { 41, new DateTime(2002, 1, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "quyen.lt@fpt.edu.vn", "Le Thi Quyen" },
                    { 42, new DateTime(2003, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "sang.tv@fpt.edu.vn", "Tran Van Sang" },
                    { 43, new DateTime(2002, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "tam.pt@fpt.edu.vn", "Pham Thi Tam" },
                    { 44, new DateTime(2003, 8, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "tuan.hv@fpt.edu.vn", "Hoang Van Tuan" },
                    { 45, new DateTime(2002, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "tuyen.vt@fpt.edu.vn", "Vo Thi Tuyen" },
                    { 46, new DateTime(2003, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "vuong.bv@fpt.edu.vn", "Bui Van Vuong" },
                    { 47, new DateTime(2002, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "xuan.dv@fpt.edu.vn", "Do Van Xuan" },
                    { 48, new DateTime(2003, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "yen.pt@fpt.edu.vn", "Phan Thi Yen" },
                    { 49, new DateTime(2002, 7, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "phuc.dv@fpt.edu.vn", "Dinh Van Phuc" },
                    { 50, new DateTime(2003, 11, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "bao.nt@fpt.edu.vn", "Nguyen Thi Bao" }
                });

            migrationBuilder.InsertData(
                table: "Subject",
                columns: new[] { "SubjectId", "Credit", "SubjectCode", "SubjectName" },
                values: new object[,]
                {
                    { 1, 3, "NET101", ".NET Programming" },
                    { 2, 3, "DB201", "Database Design" },
                    { 3, 3, "SE301", "Software Engineering" },
                    { 4, 3, "WEB102", "Web Development" },
                    { 5, 4, "ALG202", "Algorithms & DS" },
                    { 6, 3, "NET302", "ASP.NET Core REST API" },
                    { 7, 4, "AI401", "Artificial Intelligence" },
                    { 8, 3, "SEC201", "Cybersecurity Basics" },
                    { 9, 3, "MOB301", "Mobile Development" },
                    { 10, 3, "CLD401", "Cloud Computing" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "UserId", "PasswordHash", "RefreshToken", "RefreshTokenExpiryTime", "Role", "Username" },
                values: new object[] { 1, "AQAAAAIAAYagAAAAEJm1H0kW6r4bq6iPjz8y0m3iK4sQYc5f4QW4Jt4qgq6Xb1q+f3mD0e0t1pK5k7g==", null, null, "Admin", "admin" });

            migrationBuilder.InsertData(
                table: "Course",
                columns: new[] { "CourseId", "CourseName", "SemesterId" },
                values: new object[,]
                {
                    { 1, ".NET Programming - K17A", 1 },
                    { 2, "Database Design - K17B", 1 },
                    { 3, "Software Engineering - K17A", 1 },
                    { 4, "Web Development - K17C", 1 },
                    { 5, "Algorithms & DS - K17A", 2 },
                    { 6, "ASP.NET Core REST API - K17B", 2 },
                    { 7, "Artificial Intelligence - K17A", 2 },
                    { 8, "Cybersecurity Basics - K17C", 2 },
                    { 9, ".NET Programming - K18A", 3 },
                    { 10, "Mobile Development - K18B", 3 },
                    { 11, "Cloud Computing - K18A", 3 },
                    { 12, "Database Design - K18C", 3 },
                    { 13, "Software Engineering - K18A", 4 },
                    { 14, "Web Development - K18B", 4 },
                    { 15, "Algorithms & DS - K18C", 4 },
                    { 16, "ASP.NET Core REST API - K18A", 4 },
                    { 17, "Artificial Intelligence - K19A", 5 },
                    { 18, "Cybersecurity Basics - K19B", 5 },
                    { 19, "Mobile Development - K19A", 5 },
                    { 20, "Cloud Computing - K19B", 5 }
                });

            migrationBuilder.InsertData(
                table: "Enrollment",
                columns: new[] { "EnrollmentId", "CourseId", "EnrollDate", "Status", "StudentId" },
                values: new object[,]
                {
                    { 1, 11, new DateTime(2023, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 11 },
                    { 2, 12, new DateTime(2023, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 11 },
                    { 3, 13, new DateTime(2023, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 11 },
                    { 4, 14, new DateTime(2023, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 11 },
                    { 5, 15, new DateTime(2023, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 11 },
                    { 6, 16, new DateTime(2023, 8, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 11 },
                    { 7, 17, new DateTime(2023, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 11 },
                    { 8, 18, new DateTime(2023, 8, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 11 },
                    { 9, 19, new DateTime(2023, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 11 },
                    { 10, 20, new DateTime(2023, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 11 },
                    { 11, 12, new DateTime(2023, 8, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 12 },
                    { 12, 13, new DateTime(2023, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 12 },
                    { 13, 14, new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 12 },
                    { 14, 15, new DateTime(2023, 8, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 12 },
                    { 15, 16, new DateTime(2023, 8, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 12 },
                    { 16, 17, new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 12 },
                    { 17, 18, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 12 },
                    { 18, 19, new DateTime(2023, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 12 },
                    { 19, 20, new DateTime(2023, 9, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 12 },
                    { 20, 1, new DateTime(2023, 9, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 12 },
                    { 21, 13, new DateTime(2023, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 13 },
                    { 22, 14, new DateTime(2023, 9, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 13 },
                    { 23, 15, new DateTime(2023, 9, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 13 },
                    { 24, 16, new DateTime(2023, 9, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 13 },
                    { 25, 17, new DateTime(2023, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 13 },
                    { 26, 18, new DateTime(2023, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 13 },
                    { 27, 19, new DateTime(2023, 9, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 13 },
                    { 28, 20, new DateTime(2023, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 13 },
                    { 29, 1, new DateTime(2023, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 13 },
                    { 30, 2, new DateTime(2023, 9, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 13 },
                    { 31, 14, new DateTime(2023, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 14 },
                    { 32, 15, new DateTime(2023, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 14 },
                    { 33, 16, new DateTime(2023, 9, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 14 },
                    { 34, 17, new DateTime(2023, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 14 },
                    { 35, 18, new DateTime(2023, 9, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 14 },
                    { 36, 19, new DateTime(2023, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 14 },
                    { 37, 20, new DateTime(2023, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 14 },
                    { 38, 1, new DateTime(2023, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 14 },
                    { 39, 2, new DateTime(2023, 9, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 14 },
                    { 40, 3, new DateTime(2023, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 14 },
                    { 41, 15, new DateTime(2023, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 15 },
                    { 42, 16, new DateTime(2023, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 15 },
                    { 43, 17, new DateTime(2023, 9, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 15 },
                    { 44, 18, new DateTime(2023, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 15 },
                    { 45, 19, new DateTime(2023, 9, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 15 },
                    { 46, 20, new DateTime(2023, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 15 },
                    { 47, 1, new DateTime(2023, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 15 },
                    { 48, 2, new DateTime(2023, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 15 },
                    { 49, 3, new DateTime(2023, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 15 },
                    { 50, 4, new DateTime(2023, 10, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 15 },
                    { 51, 16, new DateTime(2023, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 16 },
                    { 52, 17, new DateTime(2023, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 16 },
                    { 53, 18, new DateTime(2023, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 16 },
                    { 54, 19, new DateTime(2023, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 16 },
                    { 55, 20, new DateTime(2023, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 16 },
                    { 56, 1, new DateTime(2023, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 16 },
                    { 57, 2, new DateTime(2023, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 16 },
                    { 58, 3, new DateTime(2023, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 16 },
                    { 59, 4, new DateTime(2023, 10, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 16 },
                    { 60, 5, new DateTime(2023, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 16 },
                    { 61, 17, new DateTime(2023, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 17 },
                    { 62, 18, new DateTime(2023, 10, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 17 },
                    { 63, 19, new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 17 },
                    { 64, 20, new DateTime(2023, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 17 },
                    { 65, 1, new DateTime(2023, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 17 },
                    { 66, 2, new DateTime(2023, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 17 },
                    { 67, 3, new DateTime(2023, 10, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 17 },
                    { 68, 4, new DateTime(2023, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 17 },
                    { 69, 5, new DateTime(2023, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 17 },
                    { 70, 6, new DateTime(2023, 10, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 17 },
                    { 71, 18, new DateTime(2023, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 18 },
                    { 72, 19, new DateTime(2023, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 18 },
                    { 73, 20, new DateTime(2023, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 18 },
                    { 74, 1, new DateTime(2023, 10, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 18 },
                    { 75, 2, new DateTime(2023, 10, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 18 },
                    { 76, 3, new DateTime(2023, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 18 },
                    { 77, 4, new DateTime(2023, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 18 },
                    { 78, 5, new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 18 },
                    { 79, 6, new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 18 },
                    { 80, 7, new DateTime(2023, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 18 },
                    { 81, 19, new DateTime(2023, 11, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 19 },
                    { 82, 20, new DateTime(2023, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 19 },
                    { 83, 1, new DateTime(2023, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 19 },
                    { 84, 2, new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 19 },
                    { 85, 3, new DateTime(2023, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 19 },
                    { 86, 4, new DateTime(2023, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 19 },
                    { 87, 5, new DateTime(2023, 11, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 19 },
                    { 88, 6, new DateTime(2023, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 19 },
                    { 89, 7, new DateTime(2023, 11, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 19 },
                    { 90, 8, new DateTime(2023, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 19 },
                    { 91, 20, new DateTime(2023, 11, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 20 },
                    { 92, 1, new DateTime(2023, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 20 },
                    { 93, 2, new DateTime(2023, 11, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 20 },
                    { 94, 3, new DateTime(2023, 11, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 20 },
                    { 95, 4, new DateTime(2023, 11, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 20 },
                    { 96, 5, new DateTime(2023, 11, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 20 },
                    { 97, 6, new DateTime(2023, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 20 },
                    { 98, 7, new DateTime(2023, 11, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 20 },
                    { 99, 8, new DateTime(2023, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 20 },
                    { 100, 9, new DateTime(2023, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 20 },
                    { 101, 1, new DateTime(2023, 11, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 21 },
                    { 102, 2, new DateTime(2023, 11, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 21 },
                    { 103, 3, new DateTime(2023, 11, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 21 },
                    { 104, 4, new DateTime(2023, 11, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 21 },
                    { 105, 5, new DateTime(2023, 11, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 21 },
                    { 106, 6, new DateTime(2023, 11, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 21 },
                    { 107, 7, new DateTime(2023, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 21 },
                    { 108, 8, new DateTime(2023, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 21 },
                    { 109, 9, new DateTime(2023, 12, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 21 },
                    { 110, 10, new DateTime(2023, 12, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 21 },
                    { 111, 2, new DateTime(2023, 12, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 22 },
                    { 112, 3, new DateTime(2023, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 22 },
                    { 113, 4, new DateTime(2023, 12, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 22 },
                    { 114, 5, new DateTime(2023, 12, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 22 },
                    { 115, 6, new DateTime(2023, 12, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 22 },
                    { 116, 7, new DateTime(2023, 12, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 22 },
                    { 117, 8, new DateTime(2023, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 22 },
                    { 118, 9, new DateTime(2023, 12, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 22 },
                    { 119, 10, new DateTime(2023, 12, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 22 },
                    { 120, 11, new DateTime(2023, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 22 },
                    { 121, 3, new DateTime(2023, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 23 },
                    { 122, 4, new DateTime(2023, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 23 },
                    { 123, 5, new DateTime(2023, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 23 },
                    { 124, 6, new DateTime(2023, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 23 },
                    { 125, 7, new DateTime(2023, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 23 },
                    { 126, 8, new DateTime(2023, 8, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 23 },
                    { 127, 9, new DateTime(2023, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 23 },
                    { 128, 10, new DateTime(2023, 8, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 23 },
                    { 129, 11, new DateTime(2023, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 23 },
                    { 130, 12, new DateTime(2023, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 23 },
                    { 131, 4, new DateTime(2023, 8, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 24 },
                    { 132, 5, new DateTime(2023, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 24 },
                    { 133, 6, new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 24 },
                    { 134, 7, new DateTime(2023, 8, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 24 },
                    { 135, 8, new DateTime(2023, 8, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 24 },
                    { 136, 9, new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 24 },
                    { 137, 10, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 24 },
                    { 138, 11, new DateTime(2023, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 24 },
                    { 139, 12, new DateTime(2023, 9, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 24 },
                    { 140, 13, new DateTime(2023, 9, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 24 },
                    { 141, 5, new DateTime(2023, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 25 },
                    { 142, 6, new DateTime(2023, 9, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 25 },
                    { 143, 7, new DateTime(2023, 9, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 25 },
                    { 144, 8, new DateTime(2023, 9, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 25 },
                    { 145, 9, new DateTime(2023, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 25 },
                    { 146, 10, new DateTime(2023, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 25 },
                    { 147, 11, new DateTime(2023, 9, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 25 },
                    { 148, 12, new DateTime(2023, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 25 },
                    { 149, 13, new DateTime(2023, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 25 },
                    { 150, 14, new DateTime(2023, 9, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 25 },
                    { 151, 6, new DateTime(2023, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 26 },
                    { 152, 7, new DateTime(2023, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 26 },
                    { 153, 8, new DateTime(2023, 9, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 26 },
                    { 154, 9, new DateTime(2023, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 26 },
                    { 155, 10, new DateTime(2023, 9, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 26 },
                    { 156, 11, new DateTime(2023, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 26 },
                    { 157, 12, new DateTime(2023, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 26 },
                    { 158, 13, new DateTime(2023, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 26 },
                    { 159, 14, new DateTime(2023, 9, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 26 },
                    { 160, 15, new DateTime(2023, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 26 },
                    { 161, 7, new DateTime(2023, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 27 },
                    { 162, 8, new DateTime(2023, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 27 },
                    { 163, 9, new DateTime(2023, 9, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 27 },
                    { 164, 10, new DateTime(2023, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 27 },
                    { 165, 11, new DateTime(2023, 9, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 27 },
                    { 166, 12, new DateTime(2023, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 27 },
                    { 167, 13, new DateTime(2023, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 27 },
                    { 168, 14, new DateTime(2023, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 27 },
                    { 169, 15, new DateTime(2023, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 27 },
                    { 170, 16, new DateTime(2023, 10, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 27 },
                    { 171, 8, new DateTime(2023, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 28 },
                    { 172, 9, new DateTime(2023, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 28 },
                    { 173, 10, new DateTime(2023, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 28 },
                    { 174, 11, new DateTime(2023, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 28 },
                    { 175, 12, new DateTime(2023, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 28 },
                    { 176, 13, new DateTime(2023, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 28 },
                    { 177, 14, new DateTime(2023, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 28 },
                    { 178, 15, new DateTime(2023, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 28 },
                    { 179, 16, new DateTime(2023, 10, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 28 },
                    { 180, 17, new DateTime(2023, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 28 },
                    { 181, 9, new DateTime(2023, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 29 },
                    { 182, 10, new DateTime(2023, 10, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 29 },
                    { 183, 11, new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 29 },
                    { 184, 12, new DateTime(2023, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 29 },
                    { 185, 13, new DateTime(2023, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 29 },
                    { 186, 14, new DateTime(2023, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 29 },
                    { 187, 15, new DateTime(2023, 10, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 29 },
                    { 188, 16, new DateTime(2023, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 29 },
                    { 189, 17, new DateTime(2023, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 29 },
                    { 190, 18, new DateTime(2023, 10, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 29 },
                    { 191, 10, new DateTime(2023, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 30 },
                    { 192, 11, new DateTime(2023, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 30 },
                    { 193, 12, new DateTime(2023, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 30 },
                    { 194, 13, new DateTime(2023, 10, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 30 },
                    { 195, 14, new DateTime(2023, 10, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 30 },
                    { 196, 15, new DateTime(2023, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 30 },
                    { 197, 16, new DateTime(2023, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 30 },
                    { 198, 17, new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 30 },
                    { 199, 18, new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 30 },
                    { 200, 19, new DateTime(2023, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 30 },
                    { 201, 11, new DateTime(2023, 11, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 31 },
                    { 202, 12, new DateTime(2023, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 31 },
                    { 203, 13, new DateTime(2023, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 31 },
                    { 204, 14, new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 31 },
                    { 205, 15, new DateTime(2023, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 31 },
                    { 206, 16, new DateTime(2023, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 31 },
                    { 207, 17, new DateTime(2023, 11, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 31 },
                    { 208, 18, new DateTime(2023, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 31 },
                    { 209, 19, new DateTime(2023, 11, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 31 },
                    { 210, 20, new DateTime(2023, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 31 },
                    { 211, 12, new DateTime(2023, 11, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 32 },
                    { 212, 13, new DateTime(2023, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 32 },
                    { 213, 14, new DateTime(2023, 11, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 32 },
                    { 214, 15, new DateTime(2023, 11, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 32 },
                    { 215, 16, new DateTime(2023, 11, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 32 },
                    { 216, 17, new DateTime(2023, 11, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 32 },
                    { 217, 18, new DateTime(2023, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 32 },
                    { 218, 19, new DateTime(2023, 11, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 32 },
                    { 219, 20, new DateTime(2023, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 32 },
                    { 220, 1, new DateTime(2023, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 32 },
                    { 221, 13, new DateTime(2023, 11, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 33 },
                    { 222, 14, new DateTime(2023, 11, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 33 },
                    { 223, 15, new DateTime(2023, 11, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 33 },
                    { 224, 16, new DateTime(2023, 11, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 33 },
                    { 225, 17, new DateTime(2023, 11, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 33 },
                    { 226, 18, new DateTime(2023, 11, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 33 },
                    { 227, 19, new DateTime(2023, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 33 },
                    { 228, 20, new DateTime(2023, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 33 },
                    { 229, 1, new DateTime(2023, 12, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 33 },
                    { 230, 2, new DateTime(2023, 12, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 33 },
                    { 231, 14, new DateTime(2023, 12, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 34 },
                    { 232, 15, new DateTime(2023, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 34 },
                    { 233, 16, new DateTime(2023, 12, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 34 },
                    { 234, 17, new DateTime(2023, 12, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 34 },
                    { 235, 18, new DateTime(2023, 12, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 34 },
                    { 236, 19, new DateTime(2023, 12, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 34 },
                    { 237, 20, new DateTime(2023, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 34 },
                    { 238, 1, new DateTime(2023, 12, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 34 },
                    { 239, 2, new DateTime(2023, 12, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 34 },
                    { 240, 3, new DateTime(2023, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 34 },
                    { 241, 15, new DateTime(2023, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 35 },
                    { 242, 16, new DateTime(2023, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 35 },
                    { 243, 17, new DateTime(2023, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 35 },
                    { 244, 18, new DateTime(2023, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 35 },
                    { 245, 19, new DateTime(2023, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 35 },
                    { 246, 20, new DateTime(2023, 8, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 35 },
                    { 247, 1, new DateTime(2023, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 35 },
                    { 248, 2, new DateTime(2023, 8, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 35 },
                    { 249, 3, new DateTime(2023, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 35 },
                    { 250, 4, new DateTime(2023, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 35 },
                    { 251, 16, new DateTime(2023, 8, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 36 },
                    { 252, 17, new DateTime(2023, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 36 },
                    { 253, 18, new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 36 },
                    { 254, 19, new DateTime(2023, 8, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 36 },
                    { 255, 20, new DateTime(2023, 8, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 36 },
                    { 256, 1, new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 36 },
                    { 257, 2, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 36 },
                    { 258, 3, new DateTime(2023, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 36 },
                    { 259, 4, new DateTime(2023, 9, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 36 },
                    { 260, 5, new DateTime(2023, 9, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 36 },
                    { 261, 17, new DateTime(2023, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 37 },
                    { 262, 18, new DateTime(2023, 9, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 37 },
                    { 263, 19, new DateTime(2023, 9, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 37 },
                    { 264, 20, new DateTime(2023, 9, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 37 },
                    { 265, 1, new DateTime(2023, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 37 },
                    { 266, 2, new DateTime(2023, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 37 },
                    { 267, 3, new DateTime(2023, 9, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 37 },
                    { 268, 4, new DateTime(2023, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 37 },
                    { 269, 5, new DateTime(2023, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 37 },
                    { 270, 6, new DateTime(2023, 9, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 37 },
                    { 271, 18, new DateTime(2023, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 38 },
                    { 272, 19, new DateTime(2023, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 38 },
                    { 273, 20, new DateTime(2023, 9, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 38 },
                    { 274, 1, new DateTime(2023, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 38 },
                    { 275, 2, new DateTime(2023, 9, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 38 },
                    { 276, 3, new DateTime(2023, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 38 },
                    { 277, 4, new DateTime(2023, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 38 },
                    { 278, 5, new DateTime(2023, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 38 },
                    { 279, 6, new DateTime(2023, 9, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 38 },
                    { 280, 7, new DateTime(2023, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 38 },
                    { 281, 19, new DateTime(2023, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 39 },
                    { 282, 20, new DateTime(2023, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 39 },
                    { 283, 1, new DateTime(2023, 9, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 39 },
                    { 284, 2, new DateTime(2023, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 39 },
                    { 285, 3, new DateTime(2023, 9, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 39 },
                    { 286, 4, new DateTime(2023, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 39 },
                    { 287, 5, new DateTime(2023, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 39 },
                    { 288, 6, new DateTime(2023, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 39 },
                    { 289, 7, new DateTime(2023, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 39 },
                    { 290, 8, new DateTime(2023, 10, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 39 },
                    { 291, 20, new DateTime(2023, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 40 },
                    { 292, 1, new DateTime(2023, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 40 },
                    { 293, 2, new DateTime(2023, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 40 },
                    { 294, 3, new DateTime(2023, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 40 },
                    { 295, 4, new DateTime(2023, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 40 },
                    { 296, 5, new DateTime(2023, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 40 },
                    { 297, 6, new DateTime(2023, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 40 },
                    { 298, 7, new DateTime(2023, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 40 },
                    { 299, 8, new DateTime(2023, 10, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 40 },
                    { 300, 9, new DateTime(2023, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 40 },
                    { 301, 1, new DateTime(2023, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 41 },
                    { 302, 2, new DateTime(2023, 10, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 41 },
                    { 303, 3, new DateTime(2023, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 41 },
                    { 304, 4, new DateTime(2023, 10, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 41 },
                    { 305, 5, new DateTime(2023, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 41 },
                    { 306, 6, new DateTime(2023, 10, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 41 },
                    { 307, 7, new DateTime(2023, 10, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 41 },
                    { 308, 8, new DateTime(2023, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 41 },
                    { 309, 9, new DateTime(2023, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 41 },
                    { 310, 10, new DateTime(2023, 10, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 41 },
                    { 311, 2, new DateTime(2023, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 42 },
                    { 312, 3, new DateTime(2023, 10, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 42 },
                    { 313, 4, new DateTime(2023, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 42 },
                    { 314, 5, new DateTime(2023, 10, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 42 },
                    { 315, 6, new DateTime(2023, 10, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 42 },
                    { 316, 7, new DateTime(2023, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 42 },
                    { 317, 8, new DateTime(2023, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 42 },
                    { 318, 9, new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 42 },
                    { 319, 10, new DateTime(2023, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 42 },
                    { 320, 11, new DateTime(2023, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 42 },
                    { 321, 3, new DateTime(2023, 11, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 43 },
                    { 322, 4, new DateTime(2023, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 43 },
                    { 323, 5, new DateTime(2023, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 43 },
                    { 324, 6, new DateTime(2023, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 43 },
                    { 325, 7, new DateTime(2023, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 43 },
                    { 326, 8, new DateTime(2023, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 43 },
                    { 327, 9, new DateTime(2023, 11, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 43 },
                    { 328, 10, new DateTime(2023, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 43 },
                    { 329, 11, new DateTime(2023, 11, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 43 },
                    { 330, 12, new DateTime(2023, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 43 },
                    { 331, 4, new DateTime(2023, 11, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 44 },
                    { 332, 5, new DateTime(2023, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 44 },
                    { 333, 6, new DateTime(2023, 11, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 44 },
                    { 334, 7, new DateTime(2023, 11, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 44 },
                    { 335, 8, new DateTime(2023, 11, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 44 },
                    { 336, 9, new DateTime(2023, 11, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 44 },
                    { 337, 10, new DateTime(2023, 11, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 44 },
                    { 338, 11, new DateTime(2023, 11, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 44 },
                    { 339, 12, new DateTime(2023, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 44 },
                    { 340, 13, new DateTime(2023, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 44 },
                    { 341, 5, new DateTime(2023, 11, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 45 },
                    { 342, 6, new DateTime(2023, 11, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 45 },
                    { 343, 7, new DateTime(2023, 11, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 45 },
                    { 344, 8, new DateTime(2023, 11, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 45 },
                    { 345, 9, new DateTime(2023, 11, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 45 },
                    { 346, 10, new DateTime(2023, 11, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 45 },
                    { 347, 11, new DateTime(2023, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 45 },
                    { 348, 12, new DateTime(2023, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 45 },
                    { 349, 13, new DateTime(2023, 12, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 45 },
                    { 350, 14, new DateTime(2023, 12, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 45 },
                    { 351, 6, new DateTime(2023, 12, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 46 },
                    { 352, 7, new DateTime(2023, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 46 },
                    { 353, 8, new DateTime(2023, 12, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 46 },
                    { 354, 9, new DateTime(2023, 12, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 46 },
                    { 355, 10, new DateTime(2023, 12, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 46 },
                    { 356, 11, new DateTime(2023, 12, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 46 },
                    { 357, 12, new DateTime(2023, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 46 },
                    { 358, 13, new DateTime(2023, 12, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 46 },
                    { 359, 14, new DateTime(2023, 12, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 46 },
                    { 360, 15, new DateTime(2023, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 46 },
                    { 361, 7, new DateTime(2023, 8, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 47 },
                    { 362, 8, new DateTime(2023, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 47 },
                    { 363, 9, new DateTime(2023, 8, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 47 },
                    { 364, 10, new DateTime(2023, 8, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 47 },
                    { 365, 11, new DateTime(2023, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 47 },
                    { 366, 12, new DateTime(2023, 8, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 47 },
                    { 367, 13, new DateTime(2023, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 47 },
                    { 368, 14, new DateTime(2023, 8, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 47 },
                    { 369, 15, new DateTime(2023, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 47 },
                    { 370, 16, new DateTime(2023, 8, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 47 },
                    { 371, 8, new DateTime(2023, 8, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 48 },
                    { 372, 9, new DateTime(2023, 8, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 48 },
                    { 373, 10, new DateTime(2023, 8, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 48 },
                    { 374, 11, new DateTime(2023, 8, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 48 },
                    { 375, 12, new DateTime(2023, 8, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 48 },
                    { 376, 13, new DateTime(2023, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 48 },
                    { 377, 14, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 48 },
                    { 378, 15, new DateTime(2023, 9, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 48 },
                    { 379, 16, new DateTime(2023, 9, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 48 },
                    { 380, 17, new DateTime(2023, 9, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 48 },
                    { 381, 9, new DateTime(2023, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 49 },
                    { 382, 10, new DateTime(2023, 9, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 49 },
                    { 383, 11, new DateTime(2023, 9, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 49 },
                    { 384, 12, new DateTime(2023, 9, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 49 },
                    { 385, 13, new DateTime(2023, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 49 },
                    { 386, 14, new DateTime(2023, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 49 },
                    { 387, 15, new DateTime(2023, 9, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 49 },
                    { 388, 16, new DateTime(2023, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 49 },
                    { 389, 17, new DateTime(2023, 9, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 49 },
                    { 390, 18, new DateTime(2023, 9, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 49 },
                    { 391, 10, new DateTime(2023, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 50 },
                    { 392, 11, new DateTime(2023, 9, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 50 },
                    { 393, 12, new DateTime(2023, 9, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 50 },
                    { 394, 13, new DateTime(2023, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 50 },
                    { 395, 14, new DateTime(2023, 9, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 50 },
                    { 396, 15, new DateTime(2023, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 50 },
                    { 397, 16, new DateTime(2023, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 50 },
                    { 398, 17, new DateTime(2023, 9, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 50 },
                    { 399, 18, new DateTime(2023, 9, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", 50 },
                    { 400, 19, new DateTime(2023, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 50 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Course_SemesterId",
                table: "Course",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CourseId",
                table: "Enrollment",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_StudentId",
                table: "Enrollment",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollment");

            migrationBuilder.DropTable(
                name: "Subject");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "Semester");
        }
    }
}
