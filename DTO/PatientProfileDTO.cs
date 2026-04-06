using DoctorAppointmentSystem.Model.Enums;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.DTO
{
    public class PatientProfileDTO
    {

        [Required(ErrorMessage = "Blood group is required")]
        [RegularExpression(
            "^(A|B|AB|O)[+-]$",
            ErrorMessage = "Invalid blood group (e.g., A+, O-)"
        )]
        public required string BloodGroup { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender")]
        public Gender Gender { get; set; }
    }
}
