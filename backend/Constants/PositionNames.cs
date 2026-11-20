namespace RedmineApp.Constants
{
    /// <summary>
    /// Tên chức vụ dùng để phân quyền. Phải khớp với dữ liệu seed trong bảng Positions.
    /// </summary>
    public static class PositionNames
    {
        public const string Admin = "Admin";
        public const string ProjectManager = "Project Manager";
        public const string Staff = "Staff";
    }
}
