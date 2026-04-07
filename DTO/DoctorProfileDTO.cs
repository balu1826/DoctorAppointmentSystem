using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.DTO
{
    public class DoctorProfileDto
    {
        [Required]
        public required string Specialization { get; set; }

        [Range(0, 60)]
        public int ExperienceYears { get; set; }
    }
}
