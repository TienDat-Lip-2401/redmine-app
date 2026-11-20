using System.ComponentModel.DataAnnotations;

namespace RedmineApp.Models.DtoModels.ProjectMember
{
    public class ProjectMemberRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Thành viên không hợp lệ!")]
        public int UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn vai trò cho thành viên!")]
        public int RoleId { get; set; }
    }
}
