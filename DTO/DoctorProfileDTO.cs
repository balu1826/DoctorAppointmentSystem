using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.DTO
{
    public class DoctorProfileDto
    {
        [Required]
        public required string Specialization { get; set; }

        [Range(0, 60)]
        public int ExperienceYears { get; set; }

        [Required]
        [Range(0, 100000, ErrorMessage = "Consultation fee must be between 0 and 100000")]
        public decimal ConsultationFee { get; set; }
    }
}
