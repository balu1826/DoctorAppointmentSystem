using DoctorAppointmentSystem.Model.Enums;

namespace DoctorAppointmentSystem.Model
{
    public class Patient
    {
        public int Id { get; set; }

        public required string UserId { get; set; }
        public  ApplicationUser? User { get; set; }

        public required string BloodGroup { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }   

        public bool IsVerified { get; set; } = false;
    }
}
