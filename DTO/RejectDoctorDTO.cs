using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.DTO
{
    public class RejectDoctorDto
    {
        [Required(ErrorMessage ="Reason is required")]
        public required string Reason { get; set; }
    }
}
