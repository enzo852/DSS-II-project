using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.DTOs.Todos;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/todos")]
public class TodosController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodosController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    // Helper — reads the logged-in user's ID from the JWT token
    private Guid GetUserId()
    {
        // Try all possible claim name variations
        var sub = User.FindFirstValue("sub")
            ?? User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        Console.WriteLine($"[DEBUG] All claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");

        if (sub == null || !Guid.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        return userId;
    }

    // TEMPORARY DIAGNOSTIC — remove after fixing
[HttpGet("debug-auth")]
public IActionResult DebugAuth()
{
    var authHeader = Request.Headers["Authorization"].ToString();
    var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
    var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
    
    return Ok(new {
        isAuthenticated,
        authHeader = authHeader.Length > 20 
            ? authHeader.Substring(0, 20) + "..." 
            : authHeader,
        claims,
        identityName = User.Identity?.Name
    });
}

    // ─────────────────────────────────────────
    // PUBLIC — no auth required
    // ─────────────────────────────────────────

    // GET /api/todos/public
    [HttpGet("public")]
    public async Task<IActionResult> GetPublic([FromQuery] TodoQueryParameters parameters)
    {
        var result = await _todoService.GetPublicTodosAsync(parameters);
        return Ok(result);
    }

    // ─────────────────────────────────────────
    // PROTECTED — JWT required
    // ─────────────────────────────────────────

    // GET /api/todos
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TodoQueryParameters parameters)
    {
        var result = await _todoService.GetUserTodosAsync(GetUserId(), parameters);
        return Ok(result);
    }

    // POST /api/todos
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTodoRequest request)
    {
        var result = await _todoService.CreateAsync(GetUserId(), request);
        return StatusCode(201, result);
    }

    // GET /api/todos/{id}
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _todoService.GetByIdAsync(id, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Todo not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new { message = "Access denied." });
        }
    }

    // PUT /api/todos/{id}
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTodoRequest request)
    {
        try
        {
            var result = await _todoService.UpdateAsync(id, GetUserId(), request);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Todo not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new { message = "Access denied." });
        }
    }

    // PATCH /api/todos/{id}/completion
    [Authorize]
    [HttpPatch("{id:guid}/completion")]
    public async Task<IActionResult> SetCompletion(Guid id, [FromBody] SetCompletionRequest request)
    {
        try
        {
            var result = await _todoService.SetCompletionAsync(id, GetUserId(), request);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Todo not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new { message = "Access denied." });
        }
    }

    // DELETE /api/todos/{id}
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _todoService.DeleteAsync(id, GetUserId());
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Todo not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new { message = "Access denied." });
        }
    }
}