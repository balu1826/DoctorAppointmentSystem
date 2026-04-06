using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.DTO
{
    public class ResetPasswordRequest
    {
        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Token { get; set; }

        
        [MinLength(8)]
        public required string NewPassword { get; set; }
    }
}
