using System.Linq.Expressions;

namespace DoctorAppointmentSystem.Pagination
{
    public class BaseSpecification<T>
    {
        public Expression<Func<T, bool>>? Criteria { get; set; }

        public Expression<Func<T, object>>? OrderBy { get; set; }
        public Expression<Func<T, object>>? OrderByDescending { get; set; }

        public int Skip { get; set; }
        public int Take { get; set; }

        public bool IsPagingEnabled { get; set; }
    }
}
