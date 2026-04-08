namespace DoctorAppointmentSystem.Service.Interfaces
{
    public interface IAppointmentService
    {
        Task<string> BookAppointmentAsync(string patientId, int slotId);
    }
}
