using RedmineApp.Models.CommonModels;
using RedmineApp.Models.DtoModels.Role;
using RedmineApp.Models.EntityModels;
using RedmineApp.Repositories.Interfaces;
using RedmineApp.Services.Interfaces;

namespace RedmineApp.Services.Implement
{
    /// <summary>
    /// Vai trò của thành viên trong dự án (Backend Developer, Tester...). Không ảnh hưởng phân quyền hệ thống.
    /// </summary>
    public class RoleService : IRoleService
    {
        private const int NameMaxLength = 100;
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<ApiResponse> GetAllAsync()
        {
            var roles = await _roleRepository.GetAllWithMemberCountAsync();
            var result = roles.Select(r => new RoleDto
            {
                Id = r.Role.Id,
                Name = r.Role.Name,
                MemberCount = r.MemberCount,
            }).ToList();
            return ApiResponse.Response(200, "Thành công", result);
        }

        public async Task<ApiResponse> CreateAsync(SaveRoleRequestDto dto)
        {
            var name = dto.Name?.Trim();
            var error = await ValidateNameAsync(name, null);
            if (error != null) return error;

            var role = new Role { Name = name! };
            await _roleRepository.CreateAsync(role);
            await _roleRepository.SaveChangesAsync();
            return ApiResponse.Response(201, "Tạo vai trò thành công!", new RoleDto { Id = role.Id, Name = role.Name });
        }

        public async Task<ApiResponse> UpdateAsync(int id, SaveRoleRequestDto dto)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return ApiResponse.Response(404, "Không tìm thấy vai trò!");

            var name = dto.Name?.Trim();
            var error = await ValidateNameAsync(name, id);
            if (error != null) return error;

            role.Name = name!;
            await _roleRepository.UpdateAsync(role);
            await _roleRepository.SaveChangesAsync();
            return ApiResponse.Response(200, "Cập nhật vai trò thành công!", new RoleDto { Id = role.Id, Name = role.Name });
        }

        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return ApiResponse.Response(404, "Không tìm thấy vai trò!");

            var memberCount = await _roleRepository.CountMembersAsync(id);
            if (memberCount > 0)
            {
                return ApiResponse.Response(409, $"Vai trò đang được {memberCount} thành viên dự án sử dụng, không thể xoá!");
            }

            await _roleRepository.DeleteByIdAsync(id);
            return ApiResponse.Response(200, "Đã xoá vai trò!");
        }

        private async Task<ApiResponse?> ValidateNameAsync(string? name, int? excludeId)
        {
            if (string.IsNullOrEmpty(name)) return ApiResponse.Response(400, "Tên vai trò không được để trống!");
            if (name.Length > NameMaxLength) return ApiResponse.Response(400, $"Tên vai trò tối đa {NameMaxLength} ký tự!");
            if (await _roleRepository.ExistsByNameAsync(name, excludeId))
                return ApiResponse.Response(409, "Tên vai trò đã tồn tại!");
            return null;
        }
    }
}
