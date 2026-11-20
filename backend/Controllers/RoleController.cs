using Microsoft.AspNetCore.Mvc;
using RedmineApp.Constants;
using RedmineApp.Filter;
using RedmineApp.Models.DtoModels.Role;
using RedmineApp.Services.Interfaces;

namespace RedmineApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : Controller
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // Mọi user đã đăng nhập đều xem được (PM cần danh sách này khi thêm thành viên vào dự án)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _roleService.GetAllAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [AuthorizeRole(PositionNames.Admin)]
        public async Task<IActionResult> Create([FromBody] SaveRoleRequestDto dto)
        {
            var result = await _roleService.CreateAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        [AuthorizeRole(PositionNames.Admin)]
        public async Task<IActionResult> Update(int id, [FromBody] SaveRoleRequestDto dto)
        {
            var result = await _roleService.UpdateAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        [AuthorizeRole(PositionNames.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _roleService.DeleteAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
