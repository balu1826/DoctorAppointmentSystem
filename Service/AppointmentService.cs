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

                var slot = await _context.AppointmentSlots
           .Where(s => s.Id == slotId)
           .Select(s => new
           {
               s.Id,
               s.StartDateTime,
               s.IsBooked,
               s.IsBlocked
           })
           .FirstOrDefaultAsync();

                if (slot == null)
                    return "Slot not found";

               

                //  ATOMIC UPDATE (prevents race condition)
                var rowsAffected = await _context.AppointmentSlots
                    .Where(s => s.Id == slotId && !s.IsBooked && !s.IsBlocked)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsBooked, true));

                if (rowsAffected == 0)
                    return "Slot is already booked or unavailable";
                // Time validation 
                if (slot.StartDateTime <= DateTime.UtcNow)
                    return "Cannot book appointment after slot start time";


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
        //  Patient  cancel their appointment
        public async Task<string> CancelAppointmentAsync(int appointmentId, string patientId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Slot)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return "Appointment not found";

            if (appointment.PatientId != patientId)
                return "Unauthorized";

            if (appointment.Slot == null)
                return "Slot not found";

           

            if (appointment.Status == AppointmentStatus.Cancelled ||
                appointment.Status == AppointmentStatus.Completed ||
                appointment.Status == AppointmentStatus.Rejected)
                return "Appointment cannot be cancelled";
            //  cannot cancel after start time
            if (appointment.Slot.StartDateTime <= DateTime.UtcNow)
                return "Cannot cancel after appointment start time";

            // Cancel
            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.UtcNow;

            //  Free slot
            appointment.Slot.IsBooked = false;

            //  Notify doctor
            var doctorUserId = await _context.AppointmentSlots
                .Where(s => s.Id == appointment.SlotId)
                .Select(s => s.Doctor!.UserId)
                .FirstOrDefaultAsync();

            if (doctorUserId != null)
            {
                _context.Notifications.Add(new Notification
                {
                    CreatedByUserId = patientId,
                    TargetUserId = doctorUserId,
                    ReferenceId = appointmentId,
                    Title = "Appointment Cancelled",
                    Message = "Patient has cancelled the appointment"
                });
            }

            await _context.SaveChangesAsync();

            return "Appointment cancelled successfully";
        }
        public async Task<string> RescheduleAppointmentAsync(int appointmentId, int newSlotId, string patientId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Slot)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId);

                if (appointment == null)
                    return "Appointment not found";

                if (appointment.PatientId != patientId)
                    return "Unauthorized";

                if (appointment.Slot == null)
                    return "Slot not found";

                //  cannot reschedule after start time
                if (appointment.Slot.StartDateTime <= DateTime.UtcNow)
                    return "Cannot reschedule after appointment start time";

                if (appointment.Status != AppointmentStatus.Booked &&
                    appointment.Status != AppointmentStatus.Accepted)
                    return "Appointment cannot be rescheduled";

                //  Get new slot
                var newSlot = await _context.AppointmentSlots
                    .Where(s => s.Id == newSlotId)
                    .FirstOrDefaultAsync();

                if (newSlot == null)
                    return "New slot not found";

                if (newSlot.StartDateTime <= DateTime.UtcNow)
                    return "Cannot select past slot";

                // ATOMIC booking of new slot
                var rowsAffected = await _context.AppointmentSlots
                    .Where(s => s.Id == newSlotId &&
                                !s.IsBooked &&
                                !s.IsBlocked &&
                                s.StartDateTime > DateTime.UtcNow)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsBooked, true));

                if (rowsAffected == 0)
                    return "New slot is not available";

                //  Free old slot
                appointment.Slot.IsBooked = false;

                //  Update appointment
                appointment.SlotId = newSlotId;
                appointment.Status = AppointmentStatus.Booked; // reset flow
                appointment.UpdatedAt = DateTime.UtcNow;

                //  Notify doctor
                var doctorUserId = await _context.AppointmentSlots
                    .Where(s => s.Id == newSlotId)
                    .Select(s => s.Doctor!.UserId)
                    .FirstOrDefaultAsync();

                if (doctorUserId != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        CreatedByUserId = patientId,
                        TargetUserId = doctorUserId,
                        ReferenceId = appointmentId,
                        Title = "Appointment Rescheduled",
                        Message = "Patient has rescheduled the appointment"
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return "Appointment rescheduled successfully";
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }


}
