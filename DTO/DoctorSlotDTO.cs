namespace DoctorAppointmentSystem.DTO
{
    public class DoctorSlotDTO
    {
        public int SlotId { get; set; }

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public bool IsBooked { get; set; }
        public bool IsBlocked { get; set; }
    }
}
