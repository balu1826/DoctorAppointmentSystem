using DoctorAppointmentSystem.DB;
using DoctorAppointmentSystem.Model;
using DoctorAppointmentSystem.Model.Enums;
using DoctorAppointmentSystem.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace DoctorAppointmentSystem.Service
{
    public class AppointmentService:IAppointmentService
    {
        
        private readonly AppDbContext _context;

        public AppointmentService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<string> BookAppointmentAsync(string patientId, int slotId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //  ATOMIC UPDATE (prevents race condition)
                var rowsAffected = await _context.AppointmentSlots
                    .Where(s => s.Id == slotId && !s.IsBooked && !s.IsBlocked)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsBooked, true));

                if (rowsAffected == 0)
                    return "Slot is already booked or unavailable";

                // Create appointment
                var appointment = new Appointment
                {
                    SlotId = slotId,
                    PatientId = patientId,
                    Status = AppointmentStatus.Booked,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Appointments.Add(appointment);

                // Get doctor userId (for notification)
                var doctorUserId = await _context.AppointmentSlots
                    .Where(s => s.Id == slotId)
                    .Select(s => s.Doctor!.UserId)
                    .FirstOrDefaultAsync();
                if(doctorUserId == null)
                {
                    await transaction.RollbackAsync();
                    return "Doctor not found for the selected slot";
                }
                
                //  Create notification for doctor
                var notification = new Notification
                {
                    CreatedByUserId =patientId,
                    ReferenceId=slotId,
                    TargetUserId = doctorUserId,
                    Title = "New Appointment Request",
                    Message = "You have a new appointment request from a patient"
                };

                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Appointment booked successfully";
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }


}
