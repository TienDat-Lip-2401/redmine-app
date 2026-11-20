# RedmineApp – Cổng quản trị nhân sự và dự án

Hệ thống nội bộ giúp quản trị viên quản lý tài khoản nhân viên, chức vụ, dự án và thành viên dự án. Nhân viên đăng nhập để xem hồ sơ và các dự án mình tham gia.

| Phần | Công nghệ | Thư mục |
| --- | --- | --- |
| Backend | ASP.NET Core 8 Web API, Entity Framework Core 8, SQL Server, Azure Communication Services (email) | [`backend/`](backend/) |
| Frontend | Vue 3, Vite, Pinia, Vue Router, Vue I18n, Axios | [`frontend/`](frontend/) |

## Chức năng chính

- **Tài khoản:** đăng nhập, đăng xuất, làm mới phiên đăng nhập, đổi mật khẩu. Không có tự đăng ký: tài khoản do Admin tạo.
- **Nhân viên:** danh sách, thêm, sửa, xóa tài khoản; khi admin tạo tài khoản, hệ thống gửi mật khẩu ngẫu nhiên qua email; xem hồ sơ cá nhân.
- **Chức vụ:** gán và gỡ chức vụ cho nhân viên.
- **Dự án:** danh sách, tạo, sửa, xóa dự án; thêm và xóa thành viên dự án; xem dự án của tôi.
- **Phân quyền** theo vai trò người dùng (Admin, Project Manager, nhân viên).
- **Giao diện** tiếng Việt và tiếng Anh.

## Chạy dự án

### Backend

Cần .NET 8 SDK và SQL Server.

```bash
cd backend
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=RedmineApp;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "JwtSettings:Key" "<chuỗi ngẫu nhiên >= 32 ký tự>"
dotnet user-secrets set "AzureEmailConnectionString" "<connection string Azure Communication Services>"   # tùy chọn, để gửi email
dotnet ef database update
dotnet run --launch-profile https      # https://localhost:7066, Swagger: /swagger
```

Không đưa secret vào `appsettings.json`.

### Frontend

Cần Node.js 20+.

```bash
cd frontend
cp .env.example .env      # VITE_API_URL trỏ tới backend
npm install
npm run dev               # http://localhost:5173
```

### Chạy bằng Docker

Cần Docker Desktop. Không cần cài .NET, Node hay SQL Server.

```bash
cp .env.example .env      # đặt MSSQL_SA_PASSWORD, JWT_KEY
docker compose up -d --build
```

| Dịch vụ | Địa chỉ |
| --- | --- |
| Frontend | http://localhost:3000 |
| Backend / Swagger | http://localhost:5164/swagger |
| SQL Server | `localhost,1434` (user `sa`) |

Service `db-init` tự tạo database lần đầu và áp dụng migration mới mỗi lần khởi động. Dữ liệu nằm trong volume `mssql-data`; `docker compose down -v` để xoá sạch. Đổi port trong `.env` (`BACKEND_PORT`, `FRONTEND_PORT`, `DB_PORT`); nếu đổi port backend thì đặt lại `VITE_API_URL` và build lại frontend.

### Dữ liệu khởi tạo và tài khoản Admin mặc định

Migration `SeedInitialData` tạo sẵn:

- **Tài khoản Admin:** `admin@redmine.local` / `Admin@123`. Hãy đổi mật khẩu ngay lần đăng nhập đầu, nhất là khi triển khai ở nơi người khác truy cập được.
- **Chức vụ:** Admin, Project Manager, Developer, Tester.
- **Vai trò trong dự án:** Backend Developer, FrontEnd Developer, Project Manager, Tester (Id 1–4, khớp với frontend).

Migration chỉ thêm những bản ghi chưa có (so theo tên, email, mã), nên chạy được cả trên database đã có dữ liệu.

## Cấu trúc thư mục

```
backend/
  Controllers/    Auth, User, Position, Project
  Services/       Logic nghiệp vụ (Interfaces/ + Implement/)
  Repositories/   Truy vấn EF Core (Interfaces/ + Implement/)
  Models/         Entity, DTO, ApiResponse
  MiddleWares/    Middleware xác thực
  Filter/         Phân quyền theo vai trò
  Migrations/     EF Core migrations
frontend/src/
  pages/          auth, home, user, projects
  components/     common, layouts, user, project
  services/       Gọi API: auth, user, position, project
  stores/         Pinia store
  router/         Route theo module + guard đăng nhập
  locales/        vi/, en/
```
