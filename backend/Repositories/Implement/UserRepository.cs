using Microsoft.EntityFrameworkCore;
using RedmineApp.Models.CommonModels;
using RedmineApp.Models.EntityModels;
using RedmineApp.Repositories.Interfaces;

namespace RedmineApp.Repositories.Implement
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _context.Users.AnyAsync(u => u.Code == code);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Positions.Where(p => !p.DeleteFlg))
                .AsNoTracking().
                FirstOrDefaultAsync(u => u.Email == email);
        }


        public async Task<List<Position>> GetPositionsByIdsAsync(List<int> ids)
        {
            return await _context.Positions.Where(p => ids.Contains(p.Id) && !p.DeleteFlg).ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            Console.WriteLine("Id: " + id);
            return await _context.Users
                .Include(u => u.Positions.Where(p => !p.DeleteFlg))
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserDetailByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Positions.Where(p => !p.DeleteFlg))
                .FirstOrDefaultAsync(u => u.Id == id && !u.DeleteFlg);
        }

        public async Task<List<User>> GetActiveUsersByPositionAsync(string positionName)
        {
            return await _context.Users
                .Where(u => !u.DeleteFlg && u.IsActive)
                .Where(u => u.Positions.Any(p => !p.DeleteFlg && p.Name == positionName))
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        /// <summary>
        /// field: "name" | "email" | "code" (khác/null = tìm trên cả 3 trường). isActive: null = mọi trạng thái.
        /// Tìm không phân biệt hoa thường và dấu tiếng Việt.
        /// </summary>
        public async Task<PagedResult<User>> GetPagedUsersAsync(string? field, string? keyword, bool? isActive, int page, int size)
        {
            var query = _context.Users.Where(u => !u.DeleteFlg);

            if (isActive != null)
            {
                query = query.Where(u => u.IsActive == isActive);
            }

            var trimmed = keyword?.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                var pattern = SearchHelper.ContainsPattern(trimmed);
                const string c = SearchHelper.Collation;
                query = field?.ToLowerInvariant() switch
                {
                    "name" => query.Where(u => EF.Functions.Like(EF.Functions.Collate(u.Name, c), pattern)),
                    "email" => query.Where(u => EF.Functions.Like(EF.Functions.Collate(u.Email, c), pattern)),
                    "code" => query.Where(u => EF.Functions.Like(EF.Functions.Collate(u.Code, c), pattern)),
                    _ => query.Where(u =>
                        EF.Functions.Like(EF.Functions.Collate(u.Name, c), pattern)
                        || EF.Functions.Like(EF.Functions.Collate(u.Email, c), pattern)
                        || EF.Functions.Like(EF.Functions.Collate(u.Code, c), pattern)),
                };
            }

            return await query.OrderBy(u => u.Id).ToPagedResultAsync(page, size);
        }

        public async Task<List<User>> GetUsersNotInProjectAsync(int projectId)
        {
            return await _context.Users
                .Where(u => !u.DeleteFlg)
                .Where(u => !_context.ProjectMembers.Any(pm => pm.ProjectId == projectId && pm.UserId == u.Id))
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();  
        }
    }
}

