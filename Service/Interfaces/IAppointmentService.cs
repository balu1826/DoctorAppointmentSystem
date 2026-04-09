namespace DoctorAppointmentSystem.Service.Interfaces
{
    public interface IAppointmentService
    {
        Task<string> BookAppointmentAsync(string patientId, int slotId);
        Task<string> CancelAppointmentAsync(int appointmentId, string patientId);
        Task<string> RescheduleAppointmentAsync(int appointmentId, int newSlotId, string patientId);


        }
}
