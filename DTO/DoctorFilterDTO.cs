namespace DoctorAppointmentSystem.DTO
{
    public class DoctorFilterDTO
    {
        public string? Name { get; set; }
        public string? Specialization { get; set; }
        public decimal? MinFee { get; set; }
        public decimal? MaxFee { get; set; }
        public int? Experience { get; set; }
        public DateTime? AvailableDate { get; set; }
        public TimeSpan? AvailableTime { get; set; }
    }
}
