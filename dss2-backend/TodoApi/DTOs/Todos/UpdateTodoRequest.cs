using System.ComponentModel.DataAnnotations;
using TodoApi.Enums;

namespace TodoApi.DTOs.Todos;

public class UpdateTodoRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Details { get; set; }

    [Required]
    public Priority Priority { get; set; } = Priority.medium;

    public DateOnly? DueDate { get; set; }

    public bool IsPublic { get; set; } = false;

    public bool IsCompleted { get; set; } = false;
}