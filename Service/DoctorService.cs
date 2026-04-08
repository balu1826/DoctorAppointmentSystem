using DoctorAppointmentSystem.DB;
using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Exceptions;
using DoctorAppointmentSystem.Model;
using DoctorAppointmentSystem.Model.Enums;
using DoctorAppointmentSystem.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

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
            existingDoctor.ConsultationFee = dto.ConsultationFee;
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
            doctor.ConsultationFee = dto.ConsultationFee;

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

        public async Task SetAvailabilityAsync(string userId, DoctorAvailabilityDTO dto)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                throw new Exception("Doctor not found");

            if (!doctor.IsApproved)
                throw new Exception("Doctor not approved");

            if (dto.StartTime >= dto.EndTime)
                throw new Exception("Start time must be less than end time");

          

            //  Prevent duplicate for same day
            var existing = await _context.DoctorAvailability
                .FirstOrDefaultAsync(a => a.DoctorId == doctor.Id && a.DayOfWeek == dto.DayOfWeek);

            if (existing != null)
            {
                // Update existing
                existing.StartTime = dto.StartTime;
                existing.EndTime = dto.EndTime;
                existing.SlotDurationInMinutes = dto.SlotDurationInMinutes;
             
            }
            else
            {
                var availability = new DoctorAvailability
                {
                    DoctorId = doctor.Id,
                    DayOfWeek = dto.DayOfWeek,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    SlotDurationInMinutes = dto.SlotDurationInMinutes,
                   
                };

                _context.DoctorAvailability.Add(availability);
            }

            await _context.SaveChangesAsync();
        }

        public async Task GenerateSlotsAsync(string userId, int numberOfDays)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                throw new Exception("Doctor not found");

            if (!doctor.IsApproved)
                throw new Exception("Doctor not approved");

            var availabilities = await _context.DoctorAvailability
                .Where(a => a.DoctorId == doctor.Id)
                .ToListAsync();

            if (!availabilities.Any())
                throw new Exception("No availability found");

            var today = DateTime.UtcNow.Date;

            var slotsToAdd = new List<AppointmentSlot>();

            for (int i = 0; i < numberOfDays; i++)
            {
                var currentDate = today.AddDays(i);

                var availability = availabilities
                    .FirstOrDefault(a => a.DayOfWeek == currentDate.DayOfWeek);

                if (availability == null)
                    continue;

                var start = currentDate.Add(availability.StartTime);
                var end = currentDate.Add(availability.EndTime);

                while (start < end)
                {
                    var slotEnd = start.AddMinutes(availability.SlotDurationInMinutes);

                  

                    //  Prevent duplicates
                    bool exists = await _context.AppointmentSlots.AnyAsync(s =>
                        s.DoctorId == doctor.Id &&
                        s.StartDateTime == start);

                    if (!exists)
                    {
                        slotsToAdd.Add(new AppointmentSlot
                        {
                            DoctorId = doctor.Id,
                            StartDateTime = start,
                            EndDateTime = slotEnd
                        });
                    }

                    start = slotEnd;
                }
            }

            if (slotsToAdd.Any())
            {
                _context.AppointmentSlots.AddRange(slotsToAdd);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<DoctorSlotDTO>> GetDoctorSlotsAsync(string userId)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                throw new Exception("Doctor not found");

            var slots = await _context.AppointmentSlots
                .Where(s => s.DoctorId == doctor.Id)
                .OrderBy(s => s.StartDateTime)
                .Select(s => new DoctorSlotDTO
                {
                    SlotId = s.Id,
                    StartDateTime = s.StartDateTime,
                    EndDateTime = s.EndDateTime,
                    IsBooked = s.IsBooked,
                    IsBlocked = s.IsBlocked
                })
                .ToListAsync();

            return slots;
        }

        public async Task BlockSlotAsync(string userId, int slotId)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                throw new Exception("Doctor not found");

            var slot = await _context.AppointmentSlots
                .FirstOrDefaultAsync(s => s.Id == slotId);

            if (slot == null)
                throw new Exception("Slot not found");

            //  Ownership check
            if (slot.DoctorId != doctor.Id)
                throw new Exception("Unauthorized");

            //  Cannot block booked slot
            if (slot.IsBooked)
                throw new BadRequestException("Slot already booked");
            if (slot.IsBlocked)
                throw new BadRequestException("Slot already blocked");

            slot.IsBlocked = true;

            await _context.SaveChangesAsync();
        }

        public async Task UnblockSlotAsync(string userId, int slotId)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                throw new Exception("Doctor not found");

            var slot = await _context.AppointmentSlots
                .FirstOrDefaultAsync(s => s.Id == slotId);

            if (slot == null)
                throw new Exception("Slot not found");

            if (slot.DoctorId != doctor.Id)
                throw new Exception("Unauthorized");
            if (slot.IsBooked)
                throw new BadRequestException("Slot already booked");
            if (!slot.IsBlocked)
                throw new BadRequestException("Slot isn't blocked yet!");


            slot.IsBlocked = false;

            await _context.SaveChangesAsync();
        }


    }
}
