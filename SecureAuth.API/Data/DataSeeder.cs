using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecureAuth.API.Models;

namespace SecureAuth.API.Data;

/// <summary>
/// Static class for seeding initial data including users, roles, and ERP entities.
/// All data is fake/mock data for demonstration purposes only.
/// </summary>
public static class DataSeeder
{
    private const string AdminEmail = "admin@erp.local";
    private const string ManagerEmail = "manager@erp.local";
    private const string EmployeeEmail = "employee@erp.local";
    
    private const string AdminPassword = "Admin@123";
    private const string ManagerPassword = "Manager@123";
    private const string EmployeePassword = "Employee@123";

    private const string AdminRole = "Admin";
    private const string ManagerRole = "Manager";
    private const string EmployeeRole = "Employee";

    /// <summary>
    /// Seeds the database with initial fake data for users, roles, departments, and employees.
    /// NOTE: All data is mock/fake data for demonstration purposes only.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedDepartmentsAsync(context);
        await SeedEmployeesAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { AdminRole, ManagerRole, EmployeeRole };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
    {
        await CreateUserAsync(userManager, AdminEmail, AdminPassword, AdminRole);
        await CreateUserAsync(userManager, ManagerEmail, ManagerPassword, ManagerRole);
        await CreateUserAsync(userManager, EmployeeEmail, EmployeePassword, EmployeeRole);
    }

    private static async Task CreateUserAsync(
        UserManager<IdentityUser> userManager, 
        string email, 
        string password, 
        string role)
    {
        if (await userManager.FindByEmailAsync(email) is null)
        {
            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }

    private static async Task SeedDepartmentsAsync(AppDbContext context)
    {
        if (!await context.Departments.AnyAsync())
        {
            var departments = new[]
            {
                new Department { Id = 1, Name = "Engineering" },
                new Department { Id = 2, Name = "Human Resources" },
                new Department { Id = 3, Name = "Finance" }
            };

            context.Departments.AddRange(departments);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedEmployeesAsync(AppDbContext context)
    {
        if (!await context.Employees.AnyAsync())
        {
            var employees = new[]
            {
                new Employee { Id = 1, FullName = "John Smith", Position = "Senior Developer", Email = "john.smith@erp.local", DepartmentId = 1 },
                new Employee { Id = 2, FullName = "Jane Doe", Position = "HR Manager", Email = "jane.doe@erp.local", DepartmentId = 2 },
                new Employee { Id = 3, FullName = "Bob Johnson", Position = "Financial Analyst", Email = "bob.johnson@erp.local", DepartmentId = 3 },
                new Employee { Id = 4, FullName = "Alice Williams", Position = "Junior Developer", Email = "alice.williams@erp.local", DepartmentId = 1 },
                new Employee { Id = 5, FullName = "Charlie Brown", Position = "HR Specialist", Email = "charlie.brown@erp.local", DepartmentId = 2 }
            };

            context.Employees.AddRange(employees);
            await context.SaveChangesAsync();
        }
    }
}
