using RedmineApp.Constants;
using RedmineApp.Models.CommonModels;
using RedmineApp.Models.DtoModels.Position;
using RedmineApp.Models.EntityModels;
using RedmineApp.Repositories.Interfaces;
using RedmineApp.Services.Interfaces;

namespace RedmineApp.Services.Implement
{
    public class PositionService : IPositionService
    {
        private const int NameMaxLength = 100;
        private readonly IPositionRepository _positionRepository;
        private readonly IUserRepository _userRepository;
        public PositionService(IPositionRepository positionRepository, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _positionRepository = positionRepository;
        }
        
        public async Task<ApiResponse> AddPositionListToUserAsync(AssignPositionListRequestDto dto)
        {
            var userId = dto.UserId;
            var user = await _positionRepository.GetUserWithPositionsAsync(userId);
            if (user == null) return ApiResponse.Response(404, "Không tìm thấy User");
            var positions = await _positionRepository.GetPositionsByIdsAsync(dto.PositionIds);
            int count = 0;
            foreach(var pos in positions)
            {
                if(!user.Positions.Any(p => p.Id == pos.Id))
                {
                    count++;
                    user.Positions.Add(pos);
                }
            }
            if(count > 0)
            {
                await _positionRepository.SaveChangesAsync();
                return ApiResponse.Response(200, $"Đã thêm thành công {count} chức vụ cho User");
            }
            return ApiResponse.Response(400, "User đã đảm nhiệm tất cả các chức vụ này rồi");
        
        }

        public async Task<ApiResponse> GetAllPositonsAsync()
        {
            var positions = await _positionRepository.GetAllWithUserCountAsync();
            var result = positions.Select(p => new PositionDto
            {
                Id = p.Position.Id,
                Name = p.Position.Name,
                UserCount = p.UserCount,
                IsSystem = IsSystemPosition(p.Position.Name),
            });
            return ApiResponse.Response(200, "Thành công", result);
        }

        public async Task<ApiResponse> CreatePositionAsync(SavePositionRequestDto dto)
        {
            var name = dto.Name?.Trim();
            var error = await ValidateNameAsync(name, null);
            if (error != null) return error;

            var position = new Position { Name = name! };
            await _positionRepository.CreateAsync(position);
            await _positionRepository.SaveChangesAsync();
            return ApiResponse.Response(201, "Tạo chức vụ thành công!", new PositionDto { Id = position.Id, Name = position.Name });
        }

        public async Task<ApiResponse> UpdatePositionAsync(int id, SavePositionRequestDto dto)
        {
            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null) return ApiResponse.Response(404, "Không tìm thấy chức vụ!");
            if (IsSystemPosition(position.Name))
                return ApiResponse.Response(400, "Không được đổi tên chức vụ hệ thống!");

            var name = dto.Name?.Trim();
            var error = await ValidateNameAsync(name, id);
            if (error != null) return error;

            position.Name = name!;
            await _positionRepository.UpdateAsync(position);
            await _positionRepository.SaveChangesAsync();
            return ApiResponse.Response(200, "Cập nhật chức vụ thành công!", new PositionDto { Id = position.Id, Name = position.Name });
        }

        public async Task<ApiResponse> DeletePositionAsync(int id)
        {
            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null) return ApiResponse.Response(404, "Không tìm thấy chức vụ!");
            if (IsSystemPosition(position.Name))
                return ApiResponse.Response(400, "Không được xoá chức vụ hệ thống!");

            var userCount = await _positionRepository.CountUsersAsync(id);
            if (userCount > 0)
                return ApiResponse.Response(409, $"Chức vụ đang được {userCount} nhân viên sử dụng, không thể xoá!");

            await _positionRepository.DeleteByIdAsync(id);
            return ApiResponse.Response(200, "Đã xoá chức vụ!");
        }

        // CustomAuthorizeMiddleware xác định quyền theo tên các chức vụ này
        private static bool IsSystemPosition(string name) =>
            string.Equals(name, PositionNames.Admin, StringComparison.OrdinalIgnoreCase)
            || string.Equals(name, PositionNames.ProjectManager, StringComparison.OrdinalIgnoreCase);

        private async Task<ApiResponse?> ValidateNameAsync(string? name, int? excludeId)
        {
            if (string.IsNullOrEmpty(name)) return ApiResponse.Response(400, "Tên chức vụ không được để trống!");
            if (name.Length > NameMaxLength) return ApiResponse.Response(400, $"Tên chức vụ tối đa {NameMaxLength} ký tự!");
            // Không cho tạo/đổi tên thành tên trùng chức vụ hệ thống (kể cả "PM" mà middleware coi là Project Manager)
            if (IsSystemPosition(name) || string.Equals(name, "PM", StringComparison.OrdinalIgnoreCase))
                return ApiResponse.Response(400, "Tên này được dành cho chức vụ hệ thống!");
            if (await _positionRepository.ExistsByNameAsync(name, excludeId))
                return ApiResponse.Response(409, "Tên chức vụ đã tồn tại!");
            return null;
        }

        public async Task<ApiResponse> GetAvailablePositionsAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if(user == null)
            {
                return ApiResponse.Response(404, "Không tìm thấy User");
            }
            var positions = await _positionRepository.GetAvailablePositionsForUserAsync(userId);
            var result = positions.Select(p => new PositionDto {
                Id = p.Id,
                Name= p.Name,
            });

            return ApiResponse.Response(200, "Thành công", result);
        }

        public async Task<ApiResponse> GetPositionsByUserIdAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return ApiResponse.Response(404, "Không tìm thấy User");
            var positions = await _positionRepository.GetPositionsByUserIdAsync(userId);
            var result = positions.Select(x => new PositionDto
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
            return ApiResponse.Response(200, "Thành công", result);
        }

        public async Task<ApiResponse> RemovePositionFromUserAsync(DeletePositionRequest dto)
        {
            var user = await _userRepository.GetUserByIdAsync(dto.userId);
            if (user == null) return ApiResponse.Response(404, "Không tìm thấy User ở PositonService");
            var positionRemove = user.Positions.FirstOrDefault(p => p.Id == dto.positionId);
            if (positionRemove == null)
            {
                return ApiResponse.Response(404, "Người dùng không đảm nhiệm chức vụ này");
            }
            user.Positions.Remove(positionRemove);
            await _userRepository.SaveChangesAsync();
            return ApiResponse.Response(200, "Đã xóa chức vụ khỏi người dùng thành công");
        }

    }
}
