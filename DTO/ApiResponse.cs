namespace DoctorAppointmentSystem.DTO
{
    public class ApiResponse<T>
    {
       
        public int Status { get; set; }
        public bool Success { get; set; }
        public required string Message { get; set; }
        public  T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public String? Error { get; set; } 
    }
}
