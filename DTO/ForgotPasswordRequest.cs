using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.DTO
{
    public class ForgotPasswordRequest
    {
        
        [EmailAddress]
        public required string Email { get; set; }
    }
}
