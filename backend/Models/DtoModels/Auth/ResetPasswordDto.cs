using System.ComponentModel.DataAnnotations;

namespace RedmineApp.Models.DtoModels.Auth
{
    public class ResetPasswordDto
    {
        // Cùng quy tắc với frontend (ResetPassword.vue): >= 8 ký tự, có chữ hoa, chữ số và ký tự đặc biệt
        public const string PasswordPattern = @"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới!")]
        [MaxLength(100, ErrorMessage = "Mật khẩu tối đa 100 ký tự!")]
        [RegularExpression(PasswordPattern, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ số và ký tự đặc biệt!")]
        public string NewPassword { get; set; } = null!;
    }
}
