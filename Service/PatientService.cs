using DoctorAppointmentSystem.DB;
using DoctorAppointmentSystem.DTO;
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
    }
}
