namespace TodoApi.DTOs.Todos;

public class TodoQueryParameters
{
    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Filters
    public string Status { get; set; } = "all";      // all | active | completed
    public string? Priority { get; set; }             // low | medium | high
    public DateOnly? DueFrom { get; set; }
    public DateOnly? DueTo { get; set; }

    // Search
    public string? Search { get; set; }

    // Sorting
    public string SortBy { get; set; } = "createdAt"; // createdAt | dueDate | priority | title
    public string SortDir { get; set; } = "desc";     // asc | desc
}