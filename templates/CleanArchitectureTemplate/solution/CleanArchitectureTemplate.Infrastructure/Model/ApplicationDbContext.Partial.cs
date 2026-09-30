using CleanArchitectureTemplate.ApplicationCore.Entities;
using CleanArchitectureTemplate.ApplicationCore.Entities.ContosoUniversity;
using CleanArchitectureTemplate.Infrastructure.Model.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureTemplate.Infrastructure.Model;

/// <summary>
/// Partial class for CleanArchitectureTemplateContext.
/// </summary>
public partial class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    #region Contoso University Example

    public DbSet<TestCourse> TestCourses { get; set; }
    public DbSet<TestEnrollment> TestEnrollments { get; set; }
    public DbSet<TestStudent> TestStudents { get; set; }
    public DbSet<TestDepartment> TestDepartments { get; set; }
    public DbSet<TestInstructor> TestInstructors { get; set; }
    public DbSet<TestOfficeAssignment> TestOfficeAssignments { get; set; }
    public DbSet<TestCourseAssignment> TestCourseAssignments { get; set; }

    #endregion

    #region Identity

    public DbSet<TestContact> TestContact { get; set; }

    #endregion

    /// <summary>
    /// Example content for the partial class to configure the model.
    ///
    /// This `OnModelCreatingPartial` mathod is used for:
    /// - Keeps your custom configurations separate from auto-generated code.
    /// - Prevents losing changes if you re-scaffold the database.
    /// - Follows the partial class pattern (a clean extension mechanism).
    /// </summary>
    /// <param name="modelBuilder"></param>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Always call the base method first to ensure EF Core's default configurations are applied.
        base.OnModelCreating(modelBuilder);

        #region Identity

        modelBuilder.Entity<TestContact>().ToTable("TestContact");

        #endregion


        #region Contoso University

        modelBuilder.Entity<TestCourse>().ToTable("TestCourse");
        modelBuilder.Entity<TestEnrollment>().ToTable("TestEnrollment");
        modelBuilder.Entity<TestStudent>().ToTable("TestStudent");
        modelBuilder.Entity<TestDepartment>().ToTable("TestDepartment");
        modelBuilder.Entity<TestInstructor>().ToTable("TestInstructor");
        modelBuilder.Entity<TestOfficeAssignment>().ToTable("TestOfficeAssignment");
        modelBuilder.Entity<TestCourseAssignment>().ToTable("TestCourseAssignment");

        modelBuilder
            .Entity<TestCourseAssignment>()
            .HasKey(c => new { c.TestCourseId, c.TestInstructorId });

        #endregion
    }
}
