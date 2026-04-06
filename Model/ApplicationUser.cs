using Microsoft.AspNetCore.Identity;

namespace DoctorAppointmentSystem.Model
{
    public class ApplicationUser : IdentityUser
    {
        public required string FullName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
