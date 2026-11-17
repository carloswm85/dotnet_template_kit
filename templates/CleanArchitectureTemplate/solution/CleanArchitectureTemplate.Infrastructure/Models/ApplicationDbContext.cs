using CleanArchitectureTemplate.Infrastructure.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureTemplate.Infrastructure.Models;

/// <summary>
/// Partial class for CleanArchitectureTemplateContext.
///
/// - This file content is intentionally left minimal.
/// - All content here is deleted and replaced when scafolding from the database.
/// - DO NOT use this file for customizations. Instead use additional partial class files.
/// - This is the convention.
/// </summary>
public partial class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext() { }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    /// <summary>
    /// Use this method for extending the DbContext’s model configuration without modifying the generated file.
    /// This is the convention.
    /// </summary>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
