namespace DoctorAppointmentSystem.Model
{
    public class Patient
    {
        public int Id { get; set; }

        public required string UserId { get; set; }
        public  ApplicationUser? User { get; set; }

        public required string BloodGroup { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
