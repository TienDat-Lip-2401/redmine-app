using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RedmineApp.Migrations
{
    /// <summary>
    /// Seed vai trò dự án, chức vụ và tài khoản Admin mặc định (khai báo trong AppDbContext.Seed.cs).
    ///
    /// Up được viết tay thay cho InsertData do EF sinh ra, để chạy được cả trên DB đã có dữ liệu:
    /// chỉ chèn bản ghi chưa tồn tại (so theo tên / email / mã). Giữ đúng Id của seed nếu Id đó còn trống,
    /// nếu đã bị chiếm thì để SQL Server tự cấp Id. Với DB mới tạo bằng schema.sql (db-init), dữ liệu
    /// seed đã nằm sẵn trong schema.sql nên migration này được đánh dấu là đã chạy và không chạy lại.
    /// </summary>
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @now datetime2 = '2026-01-01T00:00:00';

-- Roles (Id 1-4 khớp với frontend)
DECLARE @roles TABLE (Id int, Name nvarchar(100));
INSERT INTO @roles VALUES (1, N'Backend Developer'), (2, N'FrontEnd Developer'), (3, N'Project Manager'), (4, N'Tester');

SET IDENTITY_INSERT [Roles] ON;
INSERT INTO [Roles] ([Id], [Name], [CreatedAt], [UpdatedAt], [DeleteFlg])
SELECT s.Id, s.Name, @now, @now, 0 FROM @roles s
WHERE NOT EXISTS (SELECT 1 FROM [Roles] r WHERE r.[Id] = s.Id OR r.[Name] = s.Name);
SET IDENTITY_INSERT [Roles] OFF;

INSERT INTO [Roles] ([Name], [CreatedAt], [UpdatedAt], [DeleteFlg])
SELECT s.Name, @now, @now, 0 FROM @roles s
WHERE NOT EXISTS (SELECT 1 FROM [Roles] r WHERE r.[Name] = s.Name);

-- Positions
DECLARE @positions TABLE (Id int, Name nvarchar(100));
INSERT INTO @positions VALUES (1, N'Admin'), (2, N'Project Manager'), (3, N'Developer'), (4, N'Tester');

SET IDENTITY_INSERT [Positions] ON;
INSERT INTO [Positions] ([Id], [Name], [CreatedAt], [UpdatedAt], [DeleteFlg])
SELECT s.Id, s.Name, @now, @now, 0 FROM @positions s
WHERE NOT EXISTS (SELECT 1 FROM [Positions] p WHERE p.[Id] = s.Id OR p.[Name] = s.Name);
SET IDENTITY_INSERT [Positions] OFF;

INSERT INTO [Positions] ([Name], [CreatedAt], [UpdatedAt], [DeleteFlg])
SELECT s.Name, @now, @now, 0 FROM @positions s
WHERE NOT EXISTS (SELECT 1 FROM [Positions] p WHERE p.[Name] = s.Name);

-- Tài khoản Admin mặc định (mật khẩu Admin@123, bắt đổi ở lần đăng nhập đầu)
DECLARE @email nvarchar(200) = N'admin@redmine.local';
DECLARE @code nvarchar(50) = N'ADM001';
DECLARE @hash nvarchar(max) = N'$2b$11$GHTvstPIA6dxnUa5hklbSensqCwfG34Nqlbosgx3sv094bzRH5BZe';

IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = @email OR [Code] = @code)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Id] = 1)
    BEGIN
        SET IDENTITY_INSERT [Users] ON;
        INSERT INTO [Users] ([Id], [Code], [Name], [Email], [Password], [IsActive], [IsFirstLogin], [CreatedAt], [UpdatedAt], [DeleteFlg])
        VALUES (1, @code, N'Administrator', @email, @hash, 1, 1, @now, @now, 0);
        SET IDENTITY_INSERT [Users] OFF;
    END
    ELSE
    BEGIN
        INSERT INTO [Users] ([Code], [Name], [Email], [Password], [IsActive], [IsFirstLogin], [CreatedAt], [UpdatedAt], [DeleteFlg])
        VALUES (@code, N'Administrator', @email, @hash, 1, 1, @now, @now, 0);
    END
END

INSERT INTO [UserPositions] ([UserId], [PositionId])
SELECT u.[Id], p.[Id]
FROM [Users] u
CROSS APPLY (SELECT TOP 1 [Id] FROM [Positions] WHERE [Name] = N'Admin' AND [DeleteFlg] = 0 ORDER BY [Id]) p
WHERE u.[Email] = @email
  AND NOT EXISTS (SELECT 1 FROM [UserPositions] up WHERE up.[UserId] = u.[Id] AND up.[PositionId] = p.[Id]);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Chỉ xoá đúng dữ liệu seed (theo email / tên), bỏ qua những gì đang được sử dụng.
            migrationBuilder.Sql(@"
DECLARE @adminId int = (SELECT [Id] FROM [Users] WHERE [Email] = N'admin@redmine.local');

IF @adminId IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM [ProjectMembers] WHERE [UserId] = @adminId)
    AND NOT EXISTS (SELECT 1 FROM [Projects] WHERE [ManagerId] = @adminId)
BEGIN
    DELETE FROM [UserPositions] WHERE [UserId] = @adminId;
    DELETE FROM [RefreshTokens] WHERE [UserId] = @adminId;
    DELETE FROM [Users] WHERE [Id] = @adminId;
END

DELETE p FROM [Positions] p
WHERE p.[Name] IN (N'Admin', N'Project Manager', N'Developer', N'Tester')
  AND NOT EXISTS (SELECT 1 FROM [UserPositions] up WHERE up.[PositionId] = p.[Id]);

DELETE r FROM [Roles] r
WHERE r.[Name] IN (N'Backend Developer', N'FrontEnd Developer', N'Project Manager', N'Tester')
  AND NOT EXISTS (SELECT 1 FROM [ProjectMembers] pm WHERE pm.[RoleId] = r.[Id]);
");
        }
    }
}
