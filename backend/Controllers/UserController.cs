using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RedmineApp.Constants;
using RedmineApp.Filter;
using RedmineApp.Models.CommonModels;
using RedmineApp.Models.DtoModels.User;
using RedmineApp.Services.Interfaces;

namespace RedmineApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICommonService _commonService;
        public UserController(IUserService userService, ICommonService commonService)
        {
            _userService = userService;
            _commonService = commonService;
        }
        [HttpGet]
        [Route("get-all")]
        [AuthorizeRole(PositionNames.Admin)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUserAsync();
            return Ok(users);
        }
        [HttpDelete]
        [Route("{id}")]
        [AuthorizeRole(PositionNames.Admin)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var response = await _userService.DeleteUserByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserRequestDto dto)
        {
            // Quyền (Admin sửa ai cũng được, user chỉ sửa được thông tin cơ bản của chính mình) kiểm tra trong service
            var response = await _userService.UpdateUserAsync(id, dto);
            return StatusCode(response.StatusCode, response);
        }
        [AuthorizeRole(PositionNames.Admin)]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto dto)
        {
            var response = await _userService.CreateUserAsync(dto);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        [Route("check-email")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckEmailExists([FromQuery] string email)
        {
            var result = await _userService.CheckEmailExistsAsync(email);

            if (result.StatusCode == 400)
            {
                return BadRequest(result);
            }
            if (result.StatusCode == 404)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        [HttpGet]
        [Route("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {

            var result = await _userService.GetMeAsync();
            if (result.StatusCode == 400)
            {
                return BadRequest(result);
            }
            if (result.StatusCode == 404)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        [HttpGet]
        [Route("{id}")]
        [AuthorizeRole(PositionNames.Admin)]
        public async Task<IActionResult> GetUserDetailById(int id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            if (result.StatusCode == 400)
            {
                return BadRequest(result);
            }
            if (result.StatusCode == 404)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }
}
