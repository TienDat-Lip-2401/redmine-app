using System.ComponentModel.DataAnnotations;
using RedmineApp.Models.DtoModels.Position;

namespace RedmineApp.Models.DtoModels.User
{
    public class UpdateUserRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mã nhân viên!")]
        [MaxLength(50, ErrorMessage = "Mã nhân viên tối đa 50 ký tự!")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên!")]
        [MaxLength(200, ErrorMessage = "Họ tên tối đa 200 ký tự!")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email!")]
        [MaxLength(200, ErrorMessage = "Email tối đa 200 ký tự!")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng!")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự!")]
        public string? PhoneNumber { get; set; }

        [Range(0, 2, ErrorMessage = "Giới tính không hợp lệ!")]
        public int? Gender { get; set; }

        public DateTime? Birthday { get; set; }
        public DateTime? JoinedDate { get; set; }
        public DateTime? LeavedDate { get; set; }
        public bool IsActive { get; set; }
        public List<PositionDto> Positions { get; set; } = new();
    }
}
