using RedmineApp;
using RedmineApp.Filter;
using RedmineApp.MiddleWares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    // Lỗi validation trả ApiResponse 400 thay vì ProblemDetails, để frontend luôn đọc được response.data.message
    .ConfigureApiBehaviorOptions(options =>
        options.InvalidModelStateResponseFactory = ValidationErrorResponse.Create);
builder.Services.AddSwaggerGen();
builder.Services.AddServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();
var allowOrigins = builder.Configuration.GetSection("AllowOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", policy =>
    {
        if (allowOrigins.Length > 0)
        {
            // Nếu có cấu hình domain cụ thể
            policy.WithOrigins(allowOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // Cho phép gửi Token/Cookie
        }
        else
        {
            // Nếu chưa có cấu hình (thường dùng lúc Dev)
            // LƯU Ý: Nếu dùng AllowAnyOrigin thì KHÔNG ĐƯỢC dùng AllowCredentials
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }

        // Luôn cho phép FE đọc Header này nếu bạn có xử lý file
        policy.WithExposedHeaders("Content-Disposition");
    });
});
var app = builder.Build();
app.UseCors("AllowOrigin");
// Ngay sau CORS (response lỗi vẫn có header CORS) để bắt lỗi của mọi middleware/controller phía sau
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseMiddleware<CustomAuthorizeMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
