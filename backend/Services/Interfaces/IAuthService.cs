using RedmineApp.Models.CommonModels;
using RedmineApp.Models.DtoModels.Auth;

namespace RedmineApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse> LoginAsync(LoginRequestDto requestDto, string? ipAddress, string? userAgent);
        Task<ApiResponse> RefreshTokenAsync(string? ipAddress, string? userAgent);
        Task<ApiResponse> LogoutAsync();
        Task<ApiResponse> RevokeAllSessionsAsync();
        Task<ApiResponse> ResetPasswordAsync(int userId, string newPassword);
    }
}
