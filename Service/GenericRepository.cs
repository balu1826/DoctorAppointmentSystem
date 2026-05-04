using DoctorAppointmentSystem.DB;
using DoctorAppointmentSystem.Pagination;
using DoctorAppointmentSystem.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointmentSystem.Service
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _context;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<T>> ListAsync(BaseSpecification<T> spec)
        {
            var query = SpecificationEvaluator<T>.GetQuery(_context.Set<T>(), spec);
            return await query.AsNoTracking().ToListAsync();
        }
        public async Task<int> CountAsync(BaseSpecification<T> spec)
        {

            var query = SpecificationEvaluator<T>.GetQuery(_context.Set<T>(), spec);
            return await query.CountAsync();
        }
    }
}
