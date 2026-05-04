namespace DoctorAppointmentSystem.Pagination
{
    public static class SpecificationEvaluator<T> where T : class
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, BaseSpecification<T> spec)
        {
            var query = inputQuery;

            if (spec.Criteria != null)
                query = query.Where(spec.Criteria);

            if (spec.OrderBy != null)
                query = query.OrderBy(spec.OrderBy);

            if (spec.OrderByDescending != null)
                query = query.OrderByDescending(spec.OrderByDescending);

            if (spec.IsPagingEnabled)
                query = query.Skip(spec.Skip).Take(spec.Take);
            else if (spec.Take > 0)
                query = query.Take(spec.Take);

            return query;
        }
    }
}
