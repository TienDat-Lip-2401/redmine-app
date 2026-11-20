using Microsoft.EntityFrameworkCore;

namespace RedmineApp.Models.EntityModels;

/// <summary>
/// Dữ liệu khởi tạo cố định: vai trò trong dự án, chức vụ và tài khoản Admin mặc định.
/// Mọi giá trị phải là hằng số (kể cả ngày và hash mật khẩu), nếu không EF sẽ sinh migration mới mỗi lần.
/// </summary>
public partial class AppDbContext
{
    public const string SeedAdminEmail = "admin@redmine.local";

    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Id 1-4 khớp với các vai trò đang viết cứng ở frontend (CreateProjectPage, UpdateProjectPage, AddMemberModal).
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Backend Developer", CreatedAt = SeedDate, UpdatedAt = SeedDate },
            new Role { Id = 2, Name = "FrontEnd Developer", CreatedAt = SeedDate, UpdatedAt = SeedDate },
            new Role { Id = 3, Name = "Project Manager", CreatedAt = SeedDate, UpdatedAt = SeedDate },
            new Role { Id = 4, Name = "Tester", CreatedAt = SeedDate, UpdatedAt = SeedDate });

        // "Admin" và "Project Manager" phải khớp với chuỗi mà CustomAuthorizeMiddleware dùng để xác định quyền.
        modelBuilder.Entity<Position>().HasData(
            new Position { Id = 1, Name = "Admin", CreatedAt = SeedDate, UpdatedAt = SeedDate },
            new Position { Id = 2, Name = "Project Manager", CreatedAt = SeedDate, UpdatedAt = SeedDate },
            new Position { Id = 3, Name = "Developer", CreatedAt = SeedDate, UpdatedAt = SeedDate },
            new Position { Id = 4, Name = "Tester", CreatedAt = SeedDate, UpdatedAt = SeedDate });

        // Mật khẩu mặc định "Admin@123" (BCrypt), bắt buộc đổi ở lần đăng nhập đầu (IsFirstLogin).
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Code = "ADM001",
            Name = "Administrator",
            Email = SeedAdminEmail,
            Password = "$2b$11$GHTvstPIA6dxnUa5hklbSensqCwfG34Nqlbosgx3sv094bzRH5BZe",
            IsActive = true,
            IsFirstLogin = true,
            CreatedAt = SeedDate,
            UpdatedAt = SeedDate,
        });

        modelBuilder.Entity("UserPosition").HasData(new { UserId = 1, PositionId = 1 });
    }
}
