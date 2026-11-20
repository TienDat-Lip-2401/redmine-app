using System.ComponentModel.DataAnnotations;

namespace RedmineApp.Models.DtoModels.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập email!")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        public string Password { get; set; } = null!;
    }
}
