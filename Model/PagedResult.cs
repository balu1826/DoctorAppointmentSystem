namespace DoctorAppointmentSystem.Model
{
    public class PagedResult<T>
    {
        public required List<T> Data { get; set; }

        // Offset
        public int? PageNumber { get; set; }
        public int? TotalPages { get; set; }

        // Cursor
        public DateTime? NextCursor { get; set; }
    }
}
