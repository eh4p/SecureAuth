using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecureAuth.API.Models;

namespace SecureAuth.API.Data;

/// <summary>
/// Application database context with Identity and ERP entities.
/// </summary>
public class AppDbContext : IdentityDbContext<Microsoft.AspNetCore.Identity.IdentityUser>
{
    /// <summary>
    /// Employees table.
    /// </summary>
    public DbSet<Employee> Employees { get; set; } = null!;

    /// <summary>
    /// Departments table.
    /// </summary>
    public DbSet<Department> Departments { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId);
    }
}
