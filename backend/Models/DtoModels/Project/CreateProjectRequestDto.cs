using System.ComponentModel.DataAnnotations;
using RedmineApp.Models.DtoModels.ProjectMember;

namespace RedmineApp.Models.DtoModels.Project
{
    public class CreateProjectRequestDto : IValidatableObject
    {
        [Required(ErrorMessage = "Vui lòng nhập mã dự án!")]
        [MaxLength(50, ErrorMessage = "Mã dự án tối đa 50 ký tự!")]
        public string ProjectCode { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập tên dự án!")]
        [MaxLength(300, ErrorMessage = "Tên dự án tối đa 300 ký tự!")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu!")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc!")]
        public DateTime? EndDate { get; set; }

        public string? Description { get; set; }
        public int? ProjectManagerId { get; set; }

        [Range(0, 2, ErrorMessage = "Trạng thái dự án không hợp lệ!")]
        public int? Status { get; set; }

        public bool? IsPublic { get; set; }
        public bool? IsActive { get; set; }
        public List<ProjectMemberRequestDto> MemberIds { get; set; } = new List<ProjectMemberRequestDto>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
            ProjectDateRules.Validate(StartDate, EndDate);
    }

    internal static class ProjectDateRules
    {
        public static IEnumerable<ValidationResult> Validate(DateTime? startDate, DateTime? endDate)
        {
            if (startDate != null && endDate != null && startDate > endDate)
            {
                yield return new ValidationResult("Ngày bắt đầu không được lớn hơn ngày kết thúc!", new[] { "EndDate" });
            }
        }
    }
}
