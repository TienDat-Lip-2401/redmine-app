using RedmineApp.Models.CommonModels;
using RedmineApp.Models.EntityModels;

namespace RedmineApp.Repositories.Interfaces
{
    public interface IProjectRepository : IBaseRepository<Project>
    {
        Task<Project?> GetProjectDetailAsync(int projectId);
        Task<List<Project>> GetProjectsByUserIdAsync(int userId);
        Task<PagedResult<Project>> GetPagedProjectsAsync(int? userId, string? search, int page, int size);
        void RemoveMembers(IEnumerable<ProjectMember> members);
        void AddMember(ProjectMember member);
    }
}
