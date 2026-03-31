using TodoApi.DTOs.Auth;

namespace TodoApi.Services;

public interface IAuthService
{
    Task<AuthUserResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
}