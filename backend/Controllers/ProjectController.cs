using Microsoft.AspNetCore.Mvc;
using RedmineApp.Constants;
using RedmineApp.Filter;
using RedmineApp.Models.CommonModels;
using RedmineApp.Models.DtoModels.Project;
using RedmineApp.Models.DtoModels.ProjectMember;
using RedmineApp.Services.Interfaces;

namespace RedmineApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly ICommonService _commonService;
        public ProjectController(IProjectService projectService, ICommonService commonService)
        {
            _projectService = projectService;
            _commonService = commonService;
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetProjectDetail(int id)
        {
            var result = await _projectService.GetProjectDetailAsync(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet]
        [Route("my-projects")]
        public async Task<IActionResult> GetMyProjects(
            [FromQuery] int page = 1,
            [FromQuery] int size = PagedResult<object>.DefaultPageSize,
            [FromQuery] string? search = null)
        {
            var result = await _projectService.GetProjectsForCurrentUserAsync(page, size, search);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost]
        [AuthorizeRole(PositionNames.Admin)]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequestDto dto)
        {
            // CustomAuthorizeMiddleware đã chặn request chưa đăng nhập; dữ liệu đã được validate theo DTO
            var result = await _projectService.CreateProjectAsync(_commonService.GetUserId(), dto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost]
        [Route("{projectId}/member")]
        public async Task<IActionResult> AddMembersToProject(int projectId, [FromBody] List<ProjectMemberRequestDto> dtos)
        {
            if (dtos == null || !dtos.Any())
            {
                var empty = ApiResponse.Response(400, "Danh sách thành viên không được để trống");
                return StatusCode(empty.StatusCode, empty);
            }
            var result = await _projectService.AddMembersToProjectAsync(projectId, dtos);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete]
        [Route("{projectId}/member/{userId}")]
        public async Task<IActionResult> RemoveMemberFromProject(int projectId, int userId)
        {
            var result = await _projectService.RemoveMemberFromProjectAsync(projectId, userId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet]
        [Route("{projectId}/available-users")]
        public async Task<IActionResult> GetAvailableUsers(int projectId)
        {
            var result = await _projectService.GetAvailableUsersForProjectAsync(projectId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut]
        [Route("{projectId}")]
        public async Task<IActionResult> UpdateProject(int projectId, [FromBody] UpdateProjectRequestDto dto)
        {
            var result = await _projectService.UpdateProjectAsync(projectId, dto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete]
        [Route("{projectId}")]
        [AuthorizeRole(PositionNames.Admin)]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            var response = await _projectService.DeleteProjectByIdAsync(projectId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
