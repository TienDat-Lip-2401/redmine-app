using RedmineApp.Models.CommonModels;
using RedmineApp.Models.DtoModels.User;
using RedmineApp.Models.EntityModels;

namespace RedmineApp.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse> GetUserInfoById(int id);
        Task<ApiResponse> GetMeAsync();
        Task<ApiResponse> GetAllUserAsync();
        Task<ApiResponse> GetPagedUsersAsync(int page, int size, string? field, string? keyword, string? status);
        Task<ApiResponse> GetProjectManagersAsync();
        //Task<ApiResponse> GetUserByIdAsync(int id);
        Task<ApiResponse> DeleteUserByIdAsync(int id);
        Task<ApiResponse> GetUserByIdAsync(int id);
        Task<ApiResponse> UpdateUserAsync(int id, UpdateUserRequestDto dto);
        Task<ApiResponse> CreateUserAsync(CreateUserRequestDto dto);
        Task<ApiResponse> CheckEmailExistsAsync(string email);
    }
}
