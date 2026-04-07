using DoctorAppointmentSystem.Model;
using DoctorAppointmentSystem.Model.Enums;
using DoctorAppointmentSystem.Service.Interfaces;
using DoctorAppointmentSystem.DB;
using Microsoft.EntityFrameworkCore;
using DoctorAppointmentSystem.DTO;

namespace DoctorAppointmentSystem.Service
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task ApproveDoctorAsync(int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification == null || notification.ReferenceId == null)
                throw new Exception("Invalid notification");

            var doctor = await _context.Doctors
                .FindAsync(notification.ReferenceId);

            if (doctor == null)
                throw new Exception("Doctor not found");

            doctor.IsApproved = true;
            doctor.VerificationStatus = VerificationStatus.Approved;

            // Optional: mark notification handled
            notification.Status = NotificationStatus.Resolved;

            var newNotification = new Notification
            {
                CreatedByUserId = null,
                TargetUserId = doctor.UserId,
                Title = "Profile Approved",
                Message = "Your profile has been approved",
                Type = NotificationType.DoctorApproved
            };

            _context.Notifications.Add(newNotification);

            await _context.SaveChangesAsync();
        }

        public async Task RejectDoctorAsync(int notificationId, RejectDoctorDto dto)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification == null || notification.ReferenceId == null)
                throw new Exception("Invalid notification");

            var doctor = await _context.Doctors
                .FindAsync(notification.ReferenceId);

            if (doctor == null)
                throw new Exception("Doctor not found");

            doctor.IsApproved = false;
            doctor.VerificationStatus = VerificationStatus.Rejected;

            notification.Status = NotificationStatus.Resolved;

            var newNotification = new Notification
            {
                CreatedByUserId = null,
                TargetUserId = doctor.UserId,
                Title = "Profile Rejected",
                Message = $"Rejected: {dto.Reason}",
                Type = NotificationType.DoctorRejected
            };

            _context.Notifications.Add(newNotification);

            await _context.SaveChangesAsync();
        }
    }
}
