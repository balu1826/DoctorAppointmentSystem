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
        private readonly IAdminService _adminService;

        public DoctorService(AppDbContext context, UserManager<ApplicationUser> userManager, IAdminService adminService)
        {
            _context = context;
            _userManager = userManager;
            _adminService = adminService;
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
            await _adminService.LogAsync(
            "Doctor Verification Request",
             userId,
             "Doctor"
            );

            await _context.SaveChangesAsync();
        }

        public async Task UpdateDoctorProfileAsync(string userId, DoctorProfileDto dto)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                throw new Exception("Doctor profile not found");
            if(!doctor.IsApproved)
                throw new BadRequestException("Doctor profile not approved yet");

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

            await _adminService.LogAsync(
                "Doctor Re-Verification Request",
                userId,
                "Doctor"
                );
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
            //Audit log
            await _adminService.LogAsync(
         "Availability Updates",
          userId,
          "Doctor",
          null,
          "Doctor set availability for " + dto.DayOfWeek.ToString()
         );
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
                await _adminService.LogAsync(
                 "Slots Generation",
                  userId,
                  "AppointmentSlots",
                  null,
                  "Doctor generated " + slotsToAdd.Count + " slots"
                 );
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
            //Audit log
            await _adminService.LogAsync(
             "Block Slot",
              userId,
              "AppointmentSlots",
              slotId,
              "Doctor blocked slot  "
             );
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
            //Audit log
            await _adminService.LogAsync(
             "Un-Block Slot",
              userId,
              "AppointmentSlots",
              slotId,
              "Doctor unblocked slot  "
             );
            await _context.SaveChangesAsync();
        }
        //  Doctor accepts appointment (from booked to accepted)
        public async Task<string> AcceptAppointmentAsync(int appointmentId, string doctorUserId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Slot)
                .ThenInclude(s => s!.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);


            if (appointment == null)
                return "Appointment not found";
            else if (appointment.Slot!.Doctor!.UserId != doctorUserId)
                return "Unauthorized";

            else if (appointment.Slot != null&&appointment.Slot.StartDateTime <= DateTime.UtcNow)
                return "Cannot accept appointment after slot time";

            if (appointment.Status != AppointmentStatus.Booked)
                return "Only booked appointments can be accepted";

            appointment.Status = AppointmentStatus.Accepted;
            appointment.UpdatedAt = DateTime.UtcNow;

            // Notify patient
            var notification = new Notification
            {
                TargetUserId = appointment.PatientId,
                CreatedByUserId = doctorUserId,
                Title = "Appointment Accepted",
                Message = "Your appointment has been accepted by the doctor"
            };

            _context.Notifications.Add(notification);
            //Audit log
            await _adminService.LogAsync(
             "Accept Appointment",
              doctorUserId,
              "Appointment",
              appointmentId,
              "Doctor Accepted Appointment "
             );

            await _context.SaveChangesAsync();

            return "Appointment accepted";
        }
        //  Doctor rejects appointment (from booked to rejected)
        public async Task<string> RejectAppointmentAsync(int appointmentId, string doctorUserId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Slot)
                .ThenInclude(s => s!.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return "Appointment not found";
            else if (appointment.Slot!.Doctor!.UserId != doctorUserId)
                return "Unauthorized";

            if (appointment.Status != AppointmentStatus.Booked)
                return "Only booked appointments can be rejected";

            appointment.Status = AppointmentStatus.Rejected;
            appointment.UpdatedAt = DateTime.UtcNow;

            //  FREE THE SLOT
            appointment.Slot.IsBooked = false;

            //  Notify patient
            var notification = new Notification
            {
                CreatedByUserId=doctorUserId,
                TargetUserId = appointment.PatientId,
                Title = "Appointment Rejected",
                Message = "Your appointment has been rejected by the doctor"
            };

            _context.Notifications.Add(notification);
            
            //Audit log
            await _adminService.LogAsync(
             "Reject Appointment",
              doctorUserId,
              "Appointment",
              appointmentId,
              "Doctor Rejected Appointment "
             );

            await _context.SaveChangesAsync();

            return "Appointment rejected";
        }
        //  Doctor completes appointment (from accepted to completed)
        public async Task<string> CompleteAppointmentAsync(int appointmentId, string doctorUserId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Slot)
                .ThenInclude(s => s!.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return "Appointment not found";

            else if (appointment.Slot!.Doctor!.UserId != doctorUserId)
                return "Unauthorized";

            if (appointment.Status != AppointmentStatus.Accepted)
                return "Only accepted appointments can be completed";
            if (appointment.Slot.EndDateTime > DateTime.UtcNow)
                return "Cannot complete appointment before slot end time";

            appointment.Status = AppointmentStatus.Completed;
            appointment.UpdatedAt = DateTime.UtcNow;
            var notification = new Notification
            {
                TargetUserId = appointment.PatientId,
                CreatedByUserId = doctorUserId,
                Title = "Appointment Completed",
                Message = "Your appointment has been successfully completed"
            };

            _context.Notifications.Add(notification);
            
            //Audit log
            await _adminService.LogAsync(
             "Complete Appointment",
              doctorUserId,
              "Appointment",
              appointmentId,
              "Doctor Completed Appointment "
             );

            await _context.SaveChangesAsync();

            return "Appointment completed";
        }
        //  Get doctor's appointments (both upcoming and completed)
        public async Task<DoctorAppointmentsDTO> GetDoctorAppointmentsAsync(string doctorUserId)
        {
            var now = DateTime.UtcNow;

            var appointments = await _context.Appointments
                .Include(a => a.Slot)
                .ThenInclude(s => s!.Doctor)
                .Where(a => a.Slot!.Doctor!.UserId == doctorUserId)
                .Select(a => new DoctorAppointmentItemDTO
                {
                    AppointmentId = a.Id,
                    PatientId = a.PatientId,
                    StartTime = a.Slot!.StartDateTime,
                    EndTime = a.Slot.EndDateTime,
                    Status = a.Status
                })
                .ToListAsync();

            var result = new DoctorAppointmentsDTO
            {
                //  Upcoming 
                UpcomingAppointments = appointments
                    .Where(a => a.StartTime > now &&
                                a.Status != AppointmentStatus.Cancelled &&
                                a.Status != AppointmentStatus.Rejected)
                    .OrderBy(a => a.StartTime)
                    .ToList(),

                //  Completed 
                CompletedAppointments = appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .OrderByDescending(a => a.StartTime)
                    .ToList()
            };

            return result;
        }

    }
}
