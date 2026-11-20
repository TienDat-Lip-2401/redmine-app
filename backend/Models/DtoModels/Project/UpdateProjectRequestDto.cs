using System.ComponentModel.DataAnnotations;

namespace RedmineApp.Models.DtoModels.Project
{
    public class UpdateProjectRequestDto : IValidatableObject
    {
        [Required(ErrorMessage = "Vui lòng nhập tên dự án!")]
        [MaxLength(300, ErrorMessage = "Tên dự án tối đa 300 ký tự!")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu!")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc!")]
        public DateTime? EndDate { get; set; }

        // Không bắt buộc (trước đây kiểu string không nullable nên bị ASP.NET coi là bắt buộc ngầm)
        public string? Description { get; set; }

        [Range(0, 2, ErrorMessage = "Trạng thái dự án không hợp lệ!")]
        public int Status { get; set; }

        public bool IsPublic { get; set; }
        public bool IsActive { get; set; }
        public int? ProjectManagerId { get; set; }
        public List<UpdateMemberDto> MemberIds { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
            ProjectDateRules.Validate(StartDate, EndDate);
    }
}
