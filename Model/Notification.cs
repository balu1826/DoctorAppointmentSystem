using DoctorAppointmentSystem.Model.Enums;
namespace DoctorAppointmentSystem.Model
{
    public class Notification
    {
        public int Id { get; set; }

        // Who triggered (Doctor/User)
        public  string? CreatedByUserId { get; set; }
        public  virtual ApplicationUser? CreatedByUser { get; set; }

        // Who receives (Admin)
        public required string TargetUserId { get; set; }
        public  virtual ApplicationUser? TargetUser { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? ReferenceId { get; set; } // DoctorId
    }
}
