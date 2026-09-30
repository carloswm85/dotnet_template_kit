using Microsoft.EntityFrameworkCore;
using SimpleMonolithTemplate.Monolith.Data.Entities.ContosoUniversity;

namespace SimpleMonolithTemplate.Monolith.Data.Persistence;

/// <summary>
/// Partial class for SimpleMonolithTemplateContext.
/// </summary>
public partial class SimpleMonolithTemplateDbContext : DbContext
{
    #region Contoso University Example DbSet Properties

    public DbSet<TestCourse> TestCourses { get; set; }
    public DbSet<TestEnrollment> TestEnrollments { get; set; }
    public DbSet<TestStudent> TestStudents { get; set; }
    public DbSet<TestDepartment> TestDepartments { get; set; }
    public DbSet<TestInstructor> TestInstructors { get; set; }
    public DbSet<TestOfficeAssignment> TestOfficeAssignments { get; set; }
    public DbSet<TestCourseAssignment> TestCourseAssignments { get; set; }

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

        #region Contoso University Example Built Model Configurations

        modelBuilder.Entity<TestCourse>().ToTable("TestCourse");
        modelBuilder.Entity<TestEnrollment>().ToTable("TestEnrollment");
        modelBuilder.Entity<TestStudent>().ToTable("TestStudent");
        modelBuilder.Entity<TestDepartment>().ToTable("TestDepartment");
        modelBuilder.Entity<TestInstructor>().ToTable("TestInstructor");
        modelBuilder.Entity<TestOfficeAssignment>().ToTable("TestOfficeAssignment");
        modelBuilder.Entity<TestCourseAssignment>().ToTable("TestCourseAssignment");

        // Configures the TestCourseAssignment entity's composite primary key.
        // This mapping can't be done with property attributes.
        modelBuilder
            .Entity<TestCourseAssignment>()
            .HasKey(c => new { c.TestCourseId, c.TestInstructorId });

        // Optional: How to configure many-to-many relationship between
        // the TestInstructor and TestCourse entities.
        /*
         modelBuilder.Entity<TestCourse>().ToTable(nameof(TestCourse))
                .HasMany(c => c.TestInstructors)
                .WithMany(i => i.TestCourses);
         */

        #endregion
    }
}
