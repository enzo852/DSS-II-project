using TodoApi.Enums;

namespace TodoApi.Models;

public class TodoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Priority Priority { get; set; } = Priority.medium;
    public DateOnly? DueDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool IsPublic { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property — gives EF Core access to the related User object
    public User User { get; set; } = null!;
}