using Microsoft.EntityFrameworkCore;
using RedmineApp.Models.CommonModels;
using RedmineApp.Models.EntityModels;
using RedmineApp.Repositories.Interfaces;

namespace RedmineApp.Repositories.Implement
{
    public class ProjectRepository : BaseRepository<Project>, IProjectRepository
    {
        private readonly AppDbContext _context;
        public ProjectRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public void AddMember(ProjectMember member)
        {
            _context.ProjectMembers.Add(member);
        }

        public async Task<Project?> GetProjectDetailAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Role)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
                .FirstOrDefaultAsync(p => p.Id == projectId && !p.DeleteFlg);
        }

        public async Task<List<Project>> GetProjectsByUserIdAsync(int userId)
        {
            return await _context.Projects
                .Where(p => !p.DeleteFlg)
                .Where(p => p.ProjectMembers.Any(pm => pm.UserId == userId))
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Role)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<PagedResult<Project>> GetPagedProjectsAsync(int? userId, string? search, int page, int size)
        {
            var query = _context.Projects.Where(p => !p.DeleteFlg);

            // userId = null: Admin xem tất cả. Ngược lại: dự án mình là thành viên hoặc là người quản lý
            if (userId != null)
            {
                query = query.Where(p => p.ManagerId == userId || p.ProjectMembers.Any(pm => pm.UserId == userId));
            }

            var keyword = search?.Trim();
            if (!string.IsNullOrEmpty(keyword))
            {
                // Không phân biệt hoa thường và dấu tiếng Việt
                var pattern = SearchHelper.ContainsPattern(keyword);
                query = query.Where(p =>
                    EF.Functions.Like(EF.Functions.Collate(p.Title, SearchHelper.Collation), pattern)
                    || EF.Functions.Like(EF.Functions.Collate(p.ProjectCode, SearchHelper.Collation), pattern));
            }

            return await query
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.Role)
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .AsSplitQuery()
                .ToPagedResultAsync(page, size);
        }

        public void RemoveMembers(IEnumerable<ProjectMember> members)
        {
            _context.ProjectMembers.RemoveRange(members);
        }
    }
}
