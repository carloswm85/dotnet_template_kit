using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitectureTemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsStatic = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestContact",
                columns: table => new
                {
                    TestContactId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Zip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestContact", x => x.TestContactId);
                });

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
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

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
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "TestContact");

            migrationBuilder.DropTable(
                name: "TestCourseAssignment");

            migrationBuilder.DropTable(
                name: "TestEnrollment");

            migrationBuilder.DropTable(
                name: "TestOfficeAssignment");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

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
