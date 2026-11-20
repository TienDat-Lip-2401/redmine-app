using Microsoft.EntityFrameworkCore;
using RedmineApp.Models.EntityModels;
using RedmineApp.Repositories.Interfaces;
namespace RedmineApp.Repositories.Implement
{
    public class PositionRepository : BaseRepository<Position>, IPositionRepository
    {
        private readonly AppDbContext _context;
        public PositionRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Position>> GetAvailablePositionsForUserAsync(int userId)
        {
            var assignedPositionIds = await _context.Users
                .Where(u => u.Id == userId && !u.DeleteFlg)
                .SelectMany(u => u.Positions)
                .Select(p =>p.Id)
                .ToListAsync();
            return await _context.Positions
                .Where(p => !p.DeleteFlg && !assignedPositionIds.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<List<Position>> GetPositionsByUserIdAsync(int userId)
        {
            return await _context.Users
                .Where(u=> u.Id == userId && !u.DeleteFlg)
                .SelectMany(u => u.Positions)
                .Where(p => !p.DeleteFlg)
                .ToListAsync();
        }

        public async Task<User?> GetUserWithPositionsAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Positions)
                .FirstOrDefaultAsync(u => u.Id == userId && !u.DeleteFlg);
        }
        public async Task<List<Position>> GetPositionsByIdsAsync(List<int> ids)
        {
            return await _context.Positions.Where(p => ids.Contains(p.Id) && !p.DeleteFlg).ToListAsync();
        }

        public async Task<List<(Position Position, int UserCount)>> GetAllWithUserCountAsync()
        {
            var rows = await _context.Positions
                .Where(p => !p.DeleteFlg)
                .OrderBy(p => p.Id)
                .Select(p => new { Position = p, UserCount = p.Users.Count(u => !u.DeleteFlg) })
                .ToListAsync();
            return rows.Select(r => (r.Position, r.UserCount)).ToList();
        }

        // SQL Server mặc định so sánh không phân biệt hoa thường; ToLower để chắc chắn với mọi collation
        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            var lowered = name.ToLower();
            return await _context.Positions.AnyAsync(p =>
                !p.DeleteFlg && p.Name.ToLower() == lowered && (excludeId == null || p.Id != excludeId));
        }

        public async Task<int> CountUsersAsync(int positionId)
        {
            return await _context.Users
                .Where(u => !u.DeleteFlg)
                .CountAsync(u => u.Positions.Any(p => p.Id == positionId));
        }
    }
}
