using System.ComponentModel.DataAnnotations;

namespace RedmineApp.Models.DtoModels.Project
{
    public class UpdateMemberDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Thành viên không hợp lệ!")]
        public int UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn vai trò cho thành viên!")]
        public int RoleId { get; set; }
    }
}
