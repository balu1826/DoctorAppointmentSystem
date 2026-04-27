namespace DoctorAppointmentSystem.Attributes
{
    public class AuditLogAttribute : Attribute
    {
        public string Action { get; }
        public string EntityType { get; }
        public string ReferenceIdParam { get; } 
        public string Description { get; }

        public AuditLogAttribute(
            string action,
            string entityType,
            string? referenceIdParam = null,
            string? description = null)
        {
            Action = action;
            EntityType = entityType;
            ReferenceIdParam = referenceIdParam!;
            Description = description!;
        }
    }
}
