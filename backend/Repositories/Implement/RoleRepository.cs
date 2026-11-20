using Microsoft.EntityFrameworkCore;
using RedmineApp.Models.EntityModels;
using RedmineApp.Repositories.Interfaces;

namespace RedmineApp.Repositories.Implement
{
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<(Role Role, int MemberCount)>> GetAllWithMemberCountAsync()
        {
            var rows = await _context.Roles
                .Where(r => !r.DeleteFlg)
                .OrderBy(r => r.Id)
                .Select(r => new
                {
                    Role = r,
                    MemberCount = r.ProjectMembers.Count(pm => !pm.Project.DeleteFlg && !pm.User.DeleteFlg)
                })
                .ToListAsync();
            return rows.Select(x => (x.Role, x.MemberCount)).ToList();
        }

        // SQL Server mặc định không phân biệt hoa thường; ToLower để chắc chắn với mọi collation
        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            var lowered = name.ToLower();
            return await _context.Roles.AnyAsync(r =>
                !r.DeleteFlg && r.Name.ToLower() == lowered && (excludeId == null || r.Id != excludeId));
        }

        // Chỉ đếm thành viên của dự án và user chưa bị xoá
        public async Task<int> CountMembersAsync(int roleId)
        {
            return await _context.ProjectMembers
                .CountAsync(pm => pm.RoleId == roleId && !pm.Project.DeleteFlg && !pm.User.DeleteFlg);
        }
    }
}
