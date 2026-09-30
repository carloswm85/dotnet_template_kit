using System.ComponentModel.DataAnnotations;

namespace NLayerTemplate.Data.Model
{
    public partial class TestOfficeAssignment
    {
        /* You could put a [Required] attribute on the TestInstructor navigation
         * property to specify that there must be a related instructor, but
         * you don't have to do that because the TestInstructorID foreign key
         * (which is also the key to this table) is non-nullable.
         */
        [Key]
        public int TestInstructorId { get; set; }

        [StringLength(50)]
        [Display(Name = "Office Location")]
        public string Location { get; set; } = string.Empty;

        public TestInstructor TestInstructor { get; set; } = default!;
    }
}
