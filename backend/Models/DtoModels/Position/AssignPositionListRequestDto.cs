using System.ComponentModel.DataAnnotations;

namespace RedmineApp.Models.DtoModels.Position
{
    public class AssignPositionListRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Người dùng không hợp lệ!")]
        public int UserId { get; set; }

        [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất một chức vụ!")]
        public List<int> PositionIds { get; set; } = new();
    }
}
