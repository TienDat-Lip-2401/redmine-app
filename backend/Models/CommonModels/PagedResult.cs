using Microsoft.EntityFrameworkCore;

namespace RedmineApp.Models.CommonModels
{
    /// <summary>
    /// Kết quả phân trang, nằm trong ApiResponse.Data:
    /// { data: [...], totalItems, totalPages, currentPage, pageSize }
    /// </summary>
    public class PagedResult<T>
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;

        public List<T> Data { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }

        public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) => new()
        {
            Data = Data.Select(selector).ToList(),
            TotalItems = TotalItems,
            TotalPages = TotalPages,
            CurrentPage = CurrentPage,
            PageSize = PageSize,
        };
    }

    public static class SearchHelper
    {
        // Collation SQL Server không phân biệt hoa thường và dấu: "nguyen" khớp "Nguyễn", "cuong" khớp "Cường", "dat" khớp "Đạt".
        // Không dùng Vietnamese_CI_AI: nó coi ă/â/ê/ô/ơ/ư/đ là chữ cái riêng nên "cuong" không khớp "Cường".
        public const string Collation = "Latin1_General_100_CI_AI";

        // Mẫu LIKE "chứa từ khoá"; escape [ % _ để người dùng gõ các ký tự này được hiểu theo nghĩa đen
        public static string ContainsPattern(string keyword) =>
            "%" + keyword.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";
    }

    public static class PagedResultExtensions
    {
        /// <summary>
        /// Phân trang một query đã lọc và sắp xếp. page &lt; 1 coi là 1, page vượt quá trang cuối thì lấy trang cuối;
        /// size nằm trong khoảng 1..MaxPageSize (không hợp lệ thì dùng DefaultPageSize).
        /// </summary>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int page, int size)
        {
            if (size < 1) size = PagedResult<T>.DefaultPageSize;
            if (size > PagedResult<T>.MaxPageSize) size = PagedResult<T>.MaxPageSize;

            var totalItems = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)size));
            page = Math.Clamp(page, 1, totalPages);

            var data = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            return new PagedResult<T>
            {
                Data = data,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = size,
            };
        }
    }
}
