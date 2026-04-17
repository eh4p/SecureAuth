using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureAuth.API.Data;
using SecureAuth.API.Models;

namespace SecureAuth.API.Controllers;

/// <summary>
/// Manages department CRUD operations with role-based authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(AppDbContext context, ILogger<DepartmentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all departments (accessible by any authenticated user).
    /// </summary>
    /// <returns>List of departments.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Department>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _context.Departments
            .Include(d => d.Employees)
            .ToListAsync();
        
        return Ok(departments);
    }

    /// <summary>
    /// Creates a new department (Admin only).
    /// </summary>
    /// <param name="department">Department data.</param>
    /// <returns>Created department.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Department), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Department department)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Department created: {DepartmentId} - {Name}", department.Id, department.Name);

        return CreatedAtAction(nameof(GetAll), new { id = department.Id }, department);
    }
}
