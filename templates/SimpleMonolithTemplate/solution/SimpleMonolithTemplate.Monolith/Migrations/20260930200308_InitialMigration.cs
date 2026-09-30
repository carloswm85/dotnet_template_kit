using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleMonolithTemplate.Monolith.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestInstructor",
                columns: table => new
                {
                    TestInstructorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestInstructor", x => x.TestInstructorId);
                });

            migrationBuilder.CreateTable(
                name: "TestStudent",
                columns: table => new
                {
                    TestStudentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GovernmentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TestEnrollmentDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestStudent", x => x.TestStudentId);
                });

            migrationBuilder.CreateTable(
                name: "TestDepartment",
                columns: table => new
                {
                    TestDepartmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Budget = table.Column<decimal>(type: "money", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TestInstructorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestDepartment", x => x.TestDepartmentId);
                    table.ForeignKey(
                        name: "FK_TestDepartment_TestInstructor_TestInstructorId",
                        column: x => x.TestInstructorId,
                        principalTable: "TestInstructor",
                        principalColumn: "TestInstructorId");
                });

            migrationBuilder.CreateTable(
                name: "TestOfficeAssignment",
                columns: table => new
                {
                    TestInstructorId = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestOfficeAssignment", x => x.TestInstructorId);
                    table.ForeignKey(
                        name: "FK_TestOfficeAssignment_TestInstructor_TestInstructorId",
                        column: x => x.TestInstructorId,
                        principalTable: "TestInstructor",
                        principalColumn: "TestInstructorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestCourse",
                columns: table => new
                {
                    TestCourseId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Credits = table.Column<int>(type: "int", nullable: false),
                    TestDepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCourse", x => x.TestCourseId);
                    table.ForeignKey(
                        name: "FK_TestCourse_TestDepartment_TestDepartmentId",
                        column: x => x.TestDepartmentId,
                        principalTable: "TestDepartment",
                        principalColumn: "TestDepartmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestCourseAssignment",
                columns: table => new
                {
                    TestInstructorId = table.Column<int>(type: "int", nullable: false),
                    TestCourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCourseAssignment", x => new { x.TestCourseId, x.TestInstructorId });
                    table.ForeignKey(
                        name: "FK_TestCourseAssignment_TestCourse_TestCourseId",
                        column: x => x.TestCourseId,
                        principalTable: "TestCourse",
                        principalColumn: "TestCourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestCourseAssignment_TestInstructor_TestInstructorId",
                        column: x => x.TestInstructorId,
                        principalTable: "TestInstructor",
                        principalColumn: "TestInstructorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestEnrollment",
                columns: table => new
                {
                    TestEnrollmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TestCourseId = table.Column<int>(type: "int", nullable: false),
                    TestStudentId = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestEnrollment", x => x.TestEnrollmentId);
                    table.ForeignKey(
                        name: "FK_TestEnrollment_TestCourse_TestCourseId",
                        column: x => x.TestCourseId,
                        principalTable: "TestCourse",
                        principalColumn: "TestCourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestEnrollment_TestStudent_TestStudentId",
                        column: x => x.TestStudentId,
                        principalTable: "TestStudent",
                        principalColumn: "TestStudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestCourse_TestDepartmentId",
                table: "TestCourse",
                column: "TestDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCourseAssignment_TestInstructorId",
                table: "TestCourseAssignment",
                column: "TestInstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_TestDepartment_TestInstructorId",
                table: "TestDepartment",
                column: "TestInstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_TestEnrollment_TestCourseId",
                table: "TestEnrollment",
                column: "TestCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TestEnrollment_TestStudentId",
                table: "TestEnrollment",
                column: "TestStudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestCourseAssignment");

            migrationBuilder.DropTable(
                name: "TestEnrollment");

            migrationBuilder.DropTable(
                name: "TestOfficeAssignment");

            migrationBuilder.DropTable(
                name: "TestCourse");

            migrationBuilder.DropTable(
                name: "TestStudent");

            migrationBuilder.DropTable(
                name: "TestDepartment");

            migrationBuilder.DropTable(
                name: "TestInstructor");
        }
    }
}
