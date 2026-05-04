namespace DoctorAppointmentSystem.Pagination
{
    public class BaseQueryParams
    {
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Cursor (for large datasets like audit logs)
        public DateTime? Cursor { get; set; }

        // Sorting
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = true;

        // Searching
        public string? Search { get; set; }

    }
}
