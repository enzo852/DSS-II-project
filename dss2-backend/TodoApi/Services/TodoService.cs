using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs.Todos;
using TodoApi.Enums;
using TodoApi.Models;

namespace TodoApi.Services;

public class TodoService : ITodoService
{
    private readonly AppDbContext _context;

    public TodoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<TodoResponse>> GetPublicTodosAsync(TodoQueryParameters p)
    {
        var query = _context.TodoItems
            .Where(t => t.IsPublic)
            .AsQueryable();

        query = ApplyFilters(query, p);
        return await BuildPagedResponseAsync(query, p.Page, p.PageSize);
    }

    public async Task<PagedResponse<TodoResponse>> GetUserTodosAsync(Guid userId, TodoQueryParameters p)
    {
        var query = _context.TodoItems
            .Where(t => t.UserId == userId)
            .AsQueryable();

        query = ApplyFilters(query, p);
        return await BuildPagedResponseAsync(query, p.Page, p.PageSize);
    }

    public async Task<TodoResponse> GetByIdAsync(Guid id, Guid userId)
    {
        var todo = await _context.TodoItems.FindAsync(id)
            ?? throw new KeyNotFoundException("Todo not found.");

        if (todo.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        return MapToResponse(todo);
    }

    public async Task<TodoResponse> CreateAsync(Guid userId, CreateTodoRequest request)
    {
        var todo = new TodoItem
        {
            UserId   = userId,
            Title    = request.Title,
            Details  = request.Details,
            Priority = request.Priority,
            DueDate  = request.DueDate,
            IsPublic = request.IsPublic
        };

        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();
        return MapToResponse(todo);
    }

    public async Task<TodoResponse> UpdateAsync(Guid id, Guid userId, UpdateTodoRequest request)
    {
        var todo = await _context.TodoItems.FindAsync(id)
            ?? throw new KeyNotFoundException("Todo not found.");

        if (todo.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        todo.Title       = request.Title;
        todo.Details     = request.Details;
        todo.Priority    = request.Priority;
        todo.DueDate     = request.DueDate;
        todo.IsPublic    = request.IsPublic;
        todo.IsCompleted = request.IsCompleted;
        todo.UpdatedAt   = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToResponse(todo);
    }

    public async Task<TodoResponse> SetCompletionAsync(Guid id, Guid userId, SetCompletionRequest request)
    {
        var todo = await _context.TodoItems.FindAsync(id)
            ?? throw new KeyNotFoundException("Todo not found.");

        if (todo.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        todo.IsCompleted = request.IsCompleted;
        todo.UpdatedAt   = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToResponse(todo);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var todo = await _context.TodoItems.FindAsync(id)
            ?? throw new KeyNotFoundException("Todo not found.");

        if (todo.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync();
    }

    // ─────────────────────────────────────────
    // PRIVATE HELPERS
    // ─────────────────────────────────────────

    private static IQueryable<TodoItem> ApplyFilters(
        IQueryable<TodoItem> query, TodoQueryParameters p)
    {
        // Status filter
        if (p.Status == "active")
            query = query.Where(t => !t.IsCompleted);
        else if (p.Status == "completed")
            query = query.Where(t => t.IsCompleted);

        // Priority filter
        if (Enum.TryParse<Priority>(p.Priority, out var priority))
            query = query.Where(t => t.Priority == priority);

        // Date range filter
        if (p.DueFrom.HasValue)
            query = query.Where(t => t.DueDate >= p.DueFrom);
        if (p.DueTo.HasValue)
            query = query.Where(t => t.DueDate <= p.DueTo);

        // Keyword search
        if (!string.IsNullOrWhiteSpace(p.Search))
            query = query.Where(t =>
                t.Title.Contains(p.Search) ||
                (t.Details != null && t.Details.Contains(p.Search)));

        // Sorting
        query = p.SortBy switch
        {
            "dueDate"   => p.SortDir == "asc"
                ? query.OrderBy(t => t.DueDate)
                : query.OrderByDescending(t => t.DueDate),
            "priority"  => p.SortDir == "asc"
                ? query.OrderBy(t => t.Priority)
                : query.OrderByDescending(t => t.Priority),
            "title"     => p.SortDir == "asc"
                ? query.OrderBy(t => t.Title)
                : query.OrderByDescending(t => t.Title),
            _           => p.SortDir == "asc"
                ? query.OrderBy(t => t.CreatedAt)
                : query.OrderByDescending(t => t.CreatedAt)
        };

        return query;
    }

    private static async Task<PagedResponse<TodoResponse>> BuildPagedResponseAsync(
        IQueryable<TodoItem> query, int page, int pageSize)
    {
        // Clamp values to spec limits
        page     = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => MapToResponse(t))
            .ToListAsync();

        return new PagedResponse<TodoResponse>
        {
            Items      = items,
            Page       = page,
            PageSize   = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    private static TodoResponse MapToResponse(TodoItem t) => new()
    {
        Id          = t.Id,
        Title       = t.Title,
        Details     = t.Details,
        Priority    = t.Priority,
        DueDate     = t.DueDate,
        IsCompleted = t.IsCompleted,
        IsPublic    = t.IsPublic,
        CreatedAt   = t.CreatedAt,
        UpdatedAt   = t.UpdatedAt
    };
}