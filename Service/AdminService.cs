using DoctorAppointmentSystem.DB;
using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Model;
using DoctorAppointmentSystem.Model.Enums;
using DoctorAppointmentSystem.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DoctorAppointmentSystem.Service
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(AppDbContext context,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
        //Get all appointments for admin dashboard
        public async Task<AdminAppointmentsResponseDTO> GetAllAppointmentsAsync()
        {
            var now = DateTime.UtcNow;

            var data = await _context.Appointments
                .Include(a => a.Slot)
                    .ThenInclude(s => s!.Doctor)
                        .ThenInclude(d => d!.User)
                .Join(_context.Users,
                    a => a.PatientId,
                    u => u.Id,
                    (a, u) => new { a, u })
                .Select(x => new
                {
                    x.a.Status,
                    x.a.Slot!.StartDateTime,
                    x.a.Slot.EndDateTime,

                    DTO = new AdminAppointmentDTO
                    {
                        AppointmentId = x.a.Id,
                        DoctorName = x.a.Slot!.Doctor!.User!.FullName,
                        PatientName = x.u.FullName,

                        StartTime = x.a.Slot.StartDateTime,
                        EndTime = x.a.Slot.EndDateTime,

                        Status = x.a.Status == AppointmentStatus.Booked ? "Pending Approval" :
                                 x.a.Status == AppointmentStatus.Accepted ? "Confirmed" :
                                 x.a.Status == AppointmentStatus.Rejected ? "Declined" :
                                 x.a.Status == AppointmentStatus.Cancelled ? "Cancelled" :
                                 x.a.Status == AppointmentStatus.Completed ? "Completed" :
                                 "Unknown"
                    }
                })
                .ToListAsync();

            // Split into Past & Upcoming
            var result = new AdminAppointmentsResponseDTO
            {
                PastAppointments = data
                    .Where(x => x.Status == AppointmentStatus.Completed || x.EndDateTime < now)
                    .OrderByDescending(x => x.EndDateTime)
                    .Select(x => x.DTO)
                    .ToList(),

                UpcomingAppointments = data
                    .Where(x => x.StartDateTime >= now)
                    .OrderBy(x => x.StartDateTime)
                    .Select(x => x.DTO)
                    .ToList()
            };

            return result;
        }

        // Get all users for admin dashboard
        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = _context.Users.ToList();

            var result = new List<UserDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserDTO
                {
                    Id = user.Id,
                    Name = user.FullName,
                    Email = user!.Email!,
                    IsActive = user.IsActive,
                    Role = roles.FirstOrDefault() ?? ""
                });
            }

            return result;
        }
        // Admin can activate/deactivate user accounts
        public async Task<string> ToggleUserStatusAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return "User not found";

            user.IsActive = !user.IsActive;

            await _context.SaveChangesAsync();

            return user.IsActive ? "User activated" : "User deactivated";
        }
    }
}
