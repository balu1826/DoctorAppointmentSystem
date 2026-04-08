namespace DoctorAppointmentSystem.DTO
{
    public class DoctorListDTO
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string Specialization { get; set; }

        public int Experience { get; set; }

        public decimal ConsultationFee { get; set; }

        public List<AppointmentSlotDTO>? AvailableSlots { get; set; }
    }
}
