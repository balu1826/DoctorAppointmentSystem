namespace DoctorAppointmentSystem.DTO
{
    public class DoctorProfileViewDTO
    {
      
        public required string Name { get; set; }
        public required string Specialization { get; set; }
        public int Experience { get; set; }
        public decimal ConsultationFee { get; set; }
    
    }
}
