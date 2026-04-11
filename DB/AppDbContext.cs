using DoctorAppointmentSystem.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentSystem.DB
{
    public class AppDbContext: IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<DoctorAvailability> DoctorAvailability { get; set; }
        public DbSet<AppointmentSlot> AppointmentSlots { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Doctor>()
                .Property(d => d.ConsultationFee)
                .HasPrecision(10, 2);
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.CreatedByUser)
                .WithMany()
                .HasForeignKey(n => n.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.TargetUser)
                .WithMany()
                .HasForeignKey(n => n.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DoctorAvailability>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Availabilities)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AppointmentSlot>()
                .HasOne(s => s.Doctor)
                .WithMany(d => d.AppointmentSlot)
                .HasForeignKey(s => s.DoctorId);
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Slot)
                .WithMany()
                .HasForeignKey(a => a.SlotId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
