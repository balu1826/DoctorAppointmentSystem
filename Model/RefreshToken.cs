namespace DoctorAppointmentSystem.Model
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string Token { get; set; } = Guid.NewGuid().ToString();

        public required string UserId { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsRevoked { get; set; } = false;

        public ApplicationUser? User { get; set; }
    }
}
