namespace DoctorAppointmentSystem.DTO
{
    public class UserDTO
    {
        public required string Id { get; set; }

        public required string FullName { get; set; }

        public required string Email { get; set; }
        public bool IsActive { get; set; }

        public required string Role { get; set; } 
    }
}
