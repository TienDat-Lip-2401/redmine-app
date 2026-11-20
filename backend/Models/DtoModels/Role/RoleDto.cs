namespace RedmineApp.Models.DtoModels.Role
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        // Số thành viên dự án đang mang vai trò này
        public int MemberCount { get; set; }
    }

    public class SaveRoleRequestDto
    {
        public string? Name { get; set; }
    }
}
