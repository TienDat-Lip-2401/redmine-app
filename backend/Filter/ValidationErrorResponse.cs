using Microsoft.AspNetCore.Mvc;
using RedmineApp.Models.CommonModels;

namespace RedmineApp.Filter
{
    /// <summary>
    /// Thay ProblemDetails mặc định của [ApiController] bằng ApiResponse 400:
    /// message = lỗi đầu tiên, data = { "tenTruong": ["lỗi 1", ...] } (tên trường dạng camelCase như JSON).
    /// </summary>
    public static class ValidationErrorResponse
    {
        private const string InvalidFormatMessage = "Dữ liệu gửi lên không đúng định dạng!";
        private const string InvalidBodyMessage = "Dữ liệu gửi lên không hợp lệ!";

        public static IActionResult Create(ActionContext context)
        {
            var errors = new Dictionary<string, List<string>>();
            bool hasFormatError = false;

            foreach (var (key, entry) in context.ModelState)
            {
                if (entry.Errors.Count == 0) continue;

                // Lỗi đọc JSON (sai kiểu, sai định dạng ngày...): key dạng "$.birthday", thông báo gốc là chi tiết nội bộ
                if (key.StartsWith("$"))
                {
                    hasFormatError = true;
                    var field = key.TrimStart('$', '.');
                    if (field.Length > 0) errors[ToCamelCase(field)] = new List<string> { InvalidFormatMessage };
                    continue;
                }

                // Lỗi "The dto field is required." khi body rỗng/không đọc được: key là tên tham số action
                bool isWholeBody = context.ActionDescriptor.Parameters.Any(p => p.Name == key);
                if (isWholeBody || key.Length == 0)
                {
                    errors.TryAdd("", new List<string> { InvalidBodyMessage });
                    continue;
                }

                errors[ToCamelCase(key)] = entry.Errors
                    .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? InvalidFormatMessage : e.ErrorMessage)
                    .ToList();
            }

            // Ưu tiên lỗi của từng trường; chỉ dùng thông báo chung khi không có lỗi trường nào
            string message = hasFormatError
                ? InvalidFormatMessage
                : errors.Where(e => e.Key != "").SelectMany(e => e.Value).FirstOrDefault() ?? InvalidBodyMessage;
            errors.Remove("");

            return new BadRequestObjectResult(ApiResponse.Response(StatusCodes.Status400BadRequest, message, errors));
        }

        // "MemberIds[0].RoleId" -> "memberIds[0].roleId"
        private static string ToCamelCase(string key) =>
            string.Join('.', key.Split('.').Select(part =>
                part.Length > 0 && char.IsUpper(part[0]) ? char.ToLowerInvariant(part[0]) + part[1..] : part));
    }
}
