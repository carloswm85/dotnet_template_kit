using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace NLayerTemplate.Service.Dtos.ContosoUniversity
{
    /// <summary>
    /// This is a bad use of DTOs, but it's for demonstration purposes only.
    ///
    /// CORRECT APPROACH:
    /// - DTOs should be naked of annotations
    /// - All annotations should be in ViewModels
    /// - DTOs should be mapped to ViewModels in the MVC layer
    /// - This way, validation and display concerns are separated from
    ///   data transfer concerns.
    ///
    /// WHY IS THIS BAD EXAMPLE HERE:
    /// - For simplicity of the sample project
    /// </summary>
    public class TestStudentDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Government ID is required")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Government ID must contain only numbers")]
        [StringLength(
            20,
            MinimumLength = 7,
            ErrorMessage = "Government ID must be between 8 and 20 digits"
        )]
        [Display(Name = "Government ID")]
        public string GovernmentId { get; set; } = string.Empty;

        public string GovernmentIdFormatted =>
            long.TryParse(GovernmentId, out long num)
                ? num.ToString(
                    "#,###,###",
                    new System.Globalization.NumberFormatInfo
                    {
                        NumberGroupSeparator = ".",
                        NumberDecimalDigits = 0,
                    }
                )
                : GovernmentId; // Fallback to original value

        [Required(ErrorMessage = "First name is required")]
        [StringLength(
            80,
            MinimumLength = 2,
            ErrorMessage = "First Name must be between 2 and 80 characters"
        )]
        [RegularExpression(
            @"^[a-zA-ZÁÉÍÓÚÑáéíóúñüÜ][a-zA-ZÁÉÍÓÚÑáéíóúñüÜ\s'-]*$",
            ErrorMessage = "First name must start with a letter and can only contain letters, spaces, hyphens, and apostrophes"
        )]
        [Display(Name = "First Name")]
        public string FirstMidName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(
            80,
            MinimumLength = 2,
            ErrorMessage = "Last Name must be between 2 and 80 characters"
        )]
        [RegularExpression(
            @"^[a-zA-ZÁÉÍÓÚÑáéíóúñüÜ][a-zA-ZÁÉÍÓÚÑáéíóúñüÜ\s'-]*$",
            ErrorMessage = "Last name must start with a letter and can only contain letters, spaces, hyphens, and apostrophes"
        )]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string FullName => $"{LastName}, {FirstMidName}";

        public string? ImagePath { get; set; }
        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "TestEnrollment date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "TestEnrollment Date")]
        [DateRange(
            "1950-01-01",
            50,
            ErrorMessage = "TestEnrollment Date must be between 1950 and 50 years from now"
        )]
        public DateOnly? TestEnrollmentDate { get; set; }

        public List<TestEnrollmentDto> TestEnrollments { get; set; } = [];
    }

    /// <summary>
    /// Custom validation attribute for DateOnly range validation.
    /// Allows specifying a start date and years into the future from current date.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DateRangeAttribute : ValidationAttribute
    {
        private readonly DateOnly _minDate;
        private readonly int _yearsIntoFuture;

        public DateRangeAttribute(string minDate, int yearsIntoFuture)
        {
            _minDate = DateOnly.Parse(minDate, CultureInfo.InvariantCulture);
            _yearsIntoFuture = yearsIntoFuture;
        }

        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext
        )
        {
            if (value is DateOnly date)
            {
                var maxDate = DateOnly.FromDateTime(DateTime.Now.AddYears(_yearsIntoFuture));

                if (date < _minDate || date > maxDate)
                {
                    return new ValidationResult(
                        ErrorMessage
                            ?? $"Date must be between {_minDate:yyyy-MM-dd} and {maxDate:yyyy-MM-dd}"
                    );
                }
            }

            return ValidationResult.Success;
        }
    }
}
