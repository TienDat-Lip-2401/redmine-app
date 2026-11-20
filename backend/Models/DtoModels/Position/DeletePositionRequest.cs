using System.ComponentModel.DataAnnotations;

namespace RedmineApp.Models.DtoModels.Position
{
    public class DeletePositionRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Người dùng không hợp lệ!")]
        public int userId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Chức vụ không hợp lệ!")]
        public int positionId { get; set; }
    }
}
