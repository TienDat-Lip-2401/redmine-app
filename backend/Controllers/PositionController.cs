
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedmineApp.Constants;
using RedmineApp.Filter;
using RedmineApp.Models.CommonModels;
using RedmineApp.Models.DtoModels.Position;
using RedmineApp.Services.Interfaces;

namespace RedmineApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PositionController : Controller
    {
        private readonly IPositionService _positionService;
        private readonly ICommonService _commonService;
        public PositionController(IPositionService positionService, ICommonService commonService)
        {
            _positionService = positionService;
            _commonService = commonService;
        }
        [HttpGet]
        [Route("all")]
        public async Task<IActionResult> GetAllPositionsAsync()
        {
            var res = await _positionService.GetAllPositonsAsync();
            return Ok(res);
        }
        [HttpGet]
        [Route("user/{userId}")]
        public async Task<IActionResult> GetUserPositions(int userId)
        {
            // Chỉ Admin hoặc chính user đó được xem chức vụ
            if (_commonService.GetPosition() != PositionNames.Admin && _commonService.GetUserId() != userId)
            {
                var forbidden = ApiResponse.Response(403, "Bạn không có quyền xem chức vụ của người dùng này!");
                return StatusCode(forbidden.StatusCode, forbidden);
            }
            var res = await _positionService.GetPositionsByUserIdAsync(userId);
            return StatusCode(res.StatusCode, res);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPositions()
        {
            var positions = await _positionService.GetAllPositonsAsync();
            return Ok(positions);
        }
        [HttpGet]
        [Route("available/{userId}")]
        [AuthorizeRole(PositionNames.Admin)]
        public async Task<IActionResult> GetAvailablePositions(int userId)
        {
            var result = await _positionService.GetAvailablePositionsAsync(userId);
            return Ok(result);
        }
        [HttpPost]
        [Route("assign-list")]
        [AuthorizeRole(PositionNames.Admin)]
        public async Task<IActionResult> AddPositions(AssignPositionListRequestDto dto)
        {
            var result =await _positionService.AddPositionListToUserAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost]
        [AuthorizeRole(PositionNames.Admin)]
        public async Task<IActionResult> CreatePosition([FromBody] SavePositionRequestDto dto)
        {
            var result = await _positionService.CreatePositionAsync(dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:int}")]
        [AuthorizeRole(PositionNames.Admin)]
        public async Task<IActionResult> UpdatePosition(int id, [FromBody] SavePositionRequestDto dto)
        {
            var result = await _positionService.UpdatePositionAsync(id, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}")]
        [AuthorizeRole(PositionNames.Admin)]
        public async Task<IActionResult> DeletePosition(int id)
        {
            var result = await _positionService.DeletePositionAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [AuthorizeRole(PositionNames.Admin)]
        [HttpDelete]
        [Route("remove-position")]
        public async Task<IActionResult> RemovePosition([FromBody] DeletePositionRequest dto)
        {
            var result = await _positionService.RemovePositionFromUserAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
