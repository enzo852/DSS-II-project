using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs.Todos;

public class SetCompletionRequest
{
    [Required]
    public bool IsCompleted { get; set; }
}