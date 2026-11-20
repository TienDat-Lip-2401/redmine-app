using RedmineApp.Models.EntityModels;

namespace RedmineApp.Repositories.Interfaces
{
    public interface IRoleRepository : IBaseRepository<Role>
    {
        Task<List<(Role Role, int MemberCount)>> GetAllWithMemberCountAsync();
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
        Task<int> CountMembersAsync(int roleId);
    }
}
