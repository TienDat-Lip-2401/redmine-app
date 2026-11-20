namespace RedmineApp.Models.DtoModels.Position
{
    public class PositionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        // Số nhân viên đang giữ chức vụ (chỉ có giá trị ở API danh sách)
        public int UserCount { get; set; }
        // Chức vụ hệ thống (Admin, Project Manager) dùng để phân quyền: không được đổi tên hay xoá
        public bool IsSystem { get; set; }
    }

    public class SavePositionRequestDto
    {
        public string? Name { get; set; }
    }
}
