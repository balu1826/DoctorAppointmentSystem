using DoctorAppointmentSystem.DB;
using DoctorAppointmentSystem.DTO;
using DoctorAppointmentSystem.Model.Enums;
using DoctorAppointmentSystem.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentSystem.Service
{
    public class PatientService : IPatientService
    {
        private readonly AppDbContext _context;

        public PatientService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> CompleteProfileAsync(string userId, PatientProfileDTO model)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                throw new Exception("Patient not found");

            if (patient.IsVerified)
                throw new Exception("Profile already completed");

            if (model.DateOfBirth >= DateTime.UtcNow)
                throw new Exception("Invalid date of birth");

            patient.BloodGroup = model.BloodGroup;
            patient.DateOfBirth = model.DateOfBirth;
            patient.Gender = model.Gender;
            patient.IsVerified = true;

            await _context.SaveChangesAsync();

            return "Your account verified successfully!";
        }

        public async Task<string> UpdateProfileAsync(string userId, PatientProfileDTO model)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                throw new Exception("Patient not found");

            if (model.DateOfBirth >= DateTime.UtcNow)
                throw new Exception("Invalid date of birth");

            patient.BloodGroup = model.BloodGroup;
            patient.DateOfBirth = model.DateOfBirth;
            patient.Gender = model.Gender;

            await _context.SaveChangesAsync();

            return "Profile updated successfully";
        }
        //To get all the doctors
        public async Task<List<DoctorListDTO>> GetDoctorsAsync()
        {
            return await _context.Doctors
       .Include(d => d.User) 
       .Where(d => d.IsApproved)
       .Select(d => new DoctorListDTO
       {
           Id = d.Id,
           Name = d.User!.FullName, 
           Specialization = d.Specialization,
           Experience = d.ExperienceYears,
           ConsultationFee = d.ConsultationFee
       })
       .ToListAsync();
        }
        //To search doctors based on filters
        public async Task<List<DoctorListDTO>> SearchDoctorsAsync(DoctorFilterDTO filter)
        {
            var query = _context.Doctors.Where(d => d.IsApproved).AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(d => d.User != null && d.User.FullName.Contains(filter.Name));
            if (!string.IsNullOrEmpty(filter.Specialization))
                query = query.Where(d => d.Specialization.Contains(filter.Specialization));

            if (filter.MinFee.HasValue)
                query = query.Where(d => d.ConsultationFee >= filter.MinFee.Value);

            if (filter.MaxFee.HasValue)
                query = query.Where(d => d.ConsultationFee <= filter.MaxFee.Value);

            if (filter.Experience.HasValue)
                query = query.Where(d => d.ExperienceYears >= filter.Experience.Value);
            if (filter.AvailableDate.HasValue)
            {
                var Date = filter.AvailableDate.Value.Date;

                query = query.Where(d =>
                    d.AppointmentSlot!.Any(s =>
                        s.StartDateTime.Date == Date &&
                        !s.IsBooked &&
                        !s.IsBlocked
                    ));
            }

            if (filter.AvailableTime.HasValue)
            {
                query = query.Where(d =>
                    d.AppointmentSlot!.Any(s =>
                        s.StartDateTime.TimeOfDay <= filter.AvailableTime &&
                        s.EndDateTime.TimeOfDay >= filter.AvailableTime &&
                        !s.IsBooked &&
                        !s.IsBlocked
                    ));
            }
            var date = filter.AvailableDate?.Date;
            var time = filter.AvailableTime;

            return await query.Select(d => new DoctorListDTO
            {
                Id = d.Id,
                Name = d.User != null ? d.User.FullName : "",
                Specialization = d.Specialization,
                Experience = d.ExperienceYears,
                ConsultationFee = d.ConsultationFee,

                AvailableSlots = d.AppointmentSlot!
                    .Where(s =>
                        !s.IsBooked &&
                        !s.IsBlocked &&
                        (!date.HasValue || s.StartDateTime.Date == date.Value) &&
                        (!time.HasValue ||
                            (s.StartDateTime.TimeOfDay <= time &&
                             s.EndDateTime.TimeOfDay >= time))
                    )
                    .OrderBy(s => s.StartDateTime) 
                    .Select(s => new AppointmentSlotDTO
                    {
                        SlotId = s.Id,
                        StartDateTime = s.StartDateTime,
                        EndDateTime = s.EndDateTime
                    })
                    .ToList()
            }).ToListAsync();

        }
        //To get doctor details by id
        public async Task<DoctorProfileViewDTO?> GetDoctorByIdAsync(int id)
        {
            return await _context.Doctors
                .Where(d => d.Id == id && d.IsApproved)
                .Select(d => new DoctorProfileViewDTO
                {
                  
                    Name = d.User!.FullName,
                    Specialization = d.Specialization,
                    Experience = d.ExperienceYears,
                    ConsultationFee = d.ConsultationFee,
                })
                .FirstOrDefaultAsync();
        }
        //To get patient appointments (upcoming and completed)
        public async Task<PatientAppointmentsDTO> GetPatientAppointmentsAsync(string patientId)
        {
            var now = DateTime.UtcNow;

            var appointments = await _context.Appointments
                .Include(a => a.Slot)
                .ThenInclude(s => s!.Doctor)
                .ThenInclude(d => d!.User)
                .Where(a => a.PatientId == patientId)
                .Select(a => new PatientAppointmentItemDTO
                {
                    AppointmentId = a.Id,
                    DoctorName = a.Slot!.Doctor!.User!.FullName,
                    Specialization = a.Slot.Doctor.Specialization,
                    StartTime = a.Slot.StartDateTime,
                    EndTime = a.Slot.EndDateTime,
                    ConsultationFee = a.Slot.Doctor.ConsultationFee,
                    Status = a.Status
                })
                .ToListAsync();

            var result = new PatientAppointmentsDTO
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
