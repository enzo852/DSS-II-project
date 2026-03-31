using TodoApi.DTOs.Todos;

namespace TodoApi.Services;

public interface ITodoService
{
    Task<PagedResponse<TodoResponse>> GetPublicTodosAsync(TodoQueryParameters parameters);
    Task<PagedResponse<TodoResponse>> GetUserTodosAsync(Guid userId, TodoQueryParameters parameters);
    Task<TodoResponse> GetByIdAsync(Guid id, Guid userId);
    Task<TodoResponse> CreateAsync(Guid userId, CreateTodoRequest request);
    Task<TodoResponse> UpdateAsync(Guid id, Guid userId, UpdateTodoRequest request);
    Task<TodoResponse> SetCompletionAsync(Guid id, Guid userId, SetCompletionRequest request);
    Task DeleteAsync(Guid id, Guid userId);
}