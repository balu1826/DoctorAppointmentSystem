using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Model;
using DoctorAppointmentSystem.Model.Enums;
using DoctorAppointmentSystem.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using DoctorAppointmentSystem.DB;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentSystem.Service
{
    public class DoctorService : IDoctorService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorService(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SubmitDoctorProfileAsync(string userId, DoctorProfileDto dto)
        {
            var existingDoctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (existingDoctor == null)
                throw new Exception("Doctor profile not found");

            if (existingDoctor.VerificationStatus == VerificationStatus.Approved)
                throw new Exception("Profile already approved");

            //  Update instead of creating new
            existingDoctor.Specialization = dto.Specialization;
            existingDoctor.ExperienceYears = dto.ExperienceYears;
            existingDoctor.VerificationStatus = VerificationStatus.Pending;
            existingDoctor.IsApproved = false;

            // Get Admin
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var admin = admins.First();

            var notification = new Notification
            {
                CreatedByUserId = userId,
                TargetUserId = admin.Id,
                Title = "Doctor Verification Request",
                Message = "A doctor has submitted profile for verification",
                Type = NotificationType.DoctorVerificationRequest,
                ReferenceId = existingDoctor.Id 
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateDoctorProfileAsync(string userId, DoctorProfileDto dto)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                throw new Exception("Doctor profile not found");

            //  Update fields
            doctor.Specialization = dto.Specialization;
            doctor.ExperienceYears = dto.ExperienceYears;

            //  Reset verification
            doctor.VerificationStatus = VerificationStatus.Pending;
            doctor.IsApproved = false;

            // Get Admin
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var admin = admins.First();

            var notification = new Notification
            {
                CreatedByUserId = userId,
                TargetUserId = admin.Id,
                Title = "Doctor Re-Verification Request",
                Message = "A doctor has updated profile and needs re-verification",
                Type = NotificationType.DoctorVerificationRequest,
                ReferenceId = doctor.Id
            };

            _context.Notifications.Add(notification);

            //  Single save (best practice)
            await _context.SaveChangesAsync();
        }
    }
}
