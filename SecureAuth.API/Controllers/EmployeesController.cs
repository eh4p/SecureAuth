using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureAuth.API.Data;
using SecureAuth.API.Models;

namespace SecureAuth.API.Controllers;

/// <summary>
/// Manages employee CRUD operations with role-based authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(AppDbContext context, ILogger<EmployeesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all employees (accessible by any authenticated user).
    /// </summary>
    /// <returns>List of employees.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Employee>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .ToListAsync();
        
        return Ok(employees);
    }

    /// <summary>
    /// Gets an employee by ID (accessible by any authenticated user).
    /// </summary>
    /// <param name="id">Employee ID.</param>
    /// <returns>Employee details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
        {
            _logger.LogWarning("Employee not found: {EmployeeId}", id);
            return NotFound(new { Message = "Employee not found" });
        }

        return Ok(employee);
    }

    /// <summary>
    /// Creates a new employee (Admin only).
    /// </summary>
    /// <param name="employee">Employee data.</param>
    /// <returns>Created employee.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Employee employee)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var department = await _context.Departments.FindAsync(employee.DepartmentId);
        if (department is null)
        {
            _logger.LogWarning("Attempted to create employee with non-existent department: {DepartmentId}", employee.DepartmentId);
            return BadRequest(new { Message = "Department not found" });
        }

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Employee created: {EmployeeId} - {FullName}", employee.Id, employee.FullName);

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    /// <summary>
    /// Updates an existing employee (Manager or Admin only).
    /// </summary>
    /// <param name="id">Employee ID.</param>
    /// <param name="employee">Updated employee data.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] Employee employee)
    {
        if (id != employee.Id)
        {
            return BadRequest(new { Message = "ID mismatch" });
        }

        var existingEmployee = await _context.Employees.FindAsync(id);
        if (existingEmployee is null)
        {
            _logger.LogWarning("Attempted to update non-existent employee: {EmployeeId}", id);
            return NotFound(new { Message = "Employee not found" });
        }

        var department = await _context.Departments.FindAsync(employee.DepartmentId);
        if (department is null)
        {
            _logger.LogWarning("Attempted to update employee with non-existent department: {DepartmentId}", employee.DepartmentId);
            return BadRequest(new { Message = "Department not found" });
        }

        existingEmployee.FullName = employee.FullName;
        existingEmployee.Position = employee.Position;
        existingEmployee.Email = employee.Email;
        existingEmployee.DepartmentId = employee.DepartmentId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Employee updated: {EmployeeId}", id);

        return NoContent();
    }

    /// <summary>
    /// Deletes an employee (Admin only).
    /// </summary>
    /// <param name="id">Employee ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
        {
            _logger.LogWarning("Attempted to delete non-existent employee: {EmployeeId}", id);
            return NotFound(new { Message = "Employee not found" });
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Employee deleted: {EmployeeId}", id);

        return NoContent();
    }
}
