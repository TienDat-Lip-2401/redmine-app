using System.Runtime.ExceptionServices;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RedmineApp.Models.CommonModels;

namespace RedmineApp.MiddleWares
{
    /// <summary>
    /// Bắt mọi exception chưa được xử lý, ghi log kèm mã tra cứu (TraceIdentifier) và trả ApiResponse.
    /// Không bao giờ trả ex.Message/stack trace cho client (trừ môi trường Development, nằm trong Data).
    /// Đăng ký ngay sau UseCors (để response lỗi vẫn có header CORS) và trước các middleware còn lại.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        // Mã lỗi SQL Server khi vi phạm unique index / unique constraint
        private static readonly int[] UniqueViolationNumbers = { 2601, 2627 };

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // Client tự huỷ request (đóng tab, chuyển trang): không phải lỗi hệ thống
                _logger.LogInformation("Request {Method} {Path} bị client huỷ", context.Request.Method, context.Request.Path);
            }
            catch (Exception ex)
            {
                await HandleAsync(context, ex);
            }
        }

        private async Task HandleAsync(HttpContext context, Exception ex)
        {
            var traceId = context.TraceIdentifier;

            if (context.Response.HasStarted)
            {
                // Đã gửi một phần response: không thể đổi status/body nữa
                _logger.LogError(ex, "Lỗi sau khi response đã bắt đầu gửi. TraceId={TraceId}", traceId);
                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            ApiResponse response;
            if (IsUniqueViolation(ex))
            {
                // Hai request cùng tạo bản ghi trùng (vd. trùng mã dự án) vượt qua được kiểm tra ở service
                _logger.LogWarning(ex, "Vi phạm ràng buộc duy nhất. TraceId={TraceId}", traceId);
                response = ApiResponse.Response(StatusCodes.Status409Conflict, "Dữ liệu đã tồn tại (bị trùng), vui lòng kiểm tra lại!");
            }
            else
            {
                _logger.LogError(ex, "Lỗi chưa xử lý: {Method} {Path}. TraceId={TraceId}", context.Request.Method, context.Request.Path, traceId);
                response = ApiResponse.Response(
                    StatusCodes.Status500InternalServerError,
                    $"Lỗi hệ thống, vui lòng thử lại sau. Mã tra cứu: {traceId}",
                    _environment.IsDevelopment() ? ex.ToString() : null);
            }

            // Không gọi Response.Clear(): nó xoá cả header CORS, trình duyệt sẽ chặn frontend đọc thông báo lỗi
            context.Response.StatusCode = response.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(response);
        }

        private static bool IsUniqueViolation(Exception ex) =>
            ex is DbUpdateException { InnerException: SqlException sql } && UniqueViolationNumbers.Contains(sql.Number);
    }
}
