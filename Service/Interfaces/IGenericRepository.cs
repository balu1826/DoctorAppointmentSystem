using DoctorAppointmentSystem.Pagination;

namespace DoctorAppointmentSystem.Service.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> ListAsync(BaseSpecification<T> spec);
        Task<int> CountAsync(BaseSpecification<T> spec);
    }
}
