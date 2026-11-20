using RedmineApp.Models.CommonModels;
using RedmineApp.Models.DtoModels.Role;

namespace RedmineApp.Services.Interfaces
{
    public interface IRoleService
    {
        Task<ApiResponse> GetAllAsync();
        Task<ApiResponse> CreateAsync(SaveRoleRequestDto dto);
        Task<ApiResponse> UpdateAsync(int id, SaveRoleRequestDto dto);
        Task<ApiResponse> DeleteAsync(int id);
    }
}
