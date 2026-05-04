using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;
using DoctorAppointmentSystem.Extensions;
namespace DoctorAppointmentSystem.Pagination.AuditLog
{
    public class AuditLogSpecification : BaseSpecification<DoctorAppointmentSystem.Model.AuditLog>
    {
        public AuditLogSpecification(AuditLogQueryParams queryParams)
        {
            // Filtering
            Criteria = x => true; 
            Criteria = x =>
                (!queryParams.FromDate.HasValue || x.CreatedAt >= queryParams.FromDate) &&
                (!queryParams.ToDate.HasValue || x.CreatedAt <= queryParams.ToDate) &&
                (string.IsNullOrEmpty(queryParams.Action) || x.Action == queryParams.Action) &&
                (string.IsNullOrEmpty(queryParams.EntityType) || x.EntityType == queryParams.EntityType) &&
                (string.IsNullOrEmpty(queryParams.UserId) || x.PerformedByUserId == queryParams.UserId);

            // Cursor pagination (priority)
            if (queryParams.Cursor.HasValue)
        {
                Criteria = Criteria.And(x => x.CreatedAt < queryParams.Cursor.Value);
            }

            // Sorting
            if (!string.IsNullOrEmpty(queryParams.SortBy))
            {
                if (queryParams.SortBy == "CreatedAt")
                OrderByDescending = x => x.CreatedAt;
            }
            else
            {
                OrderByDescending = x => x.CreatedAt;
            }

            // Offset pagination fallback
            if (!queryParams.Cursor.HasValue)
        {
                Skip = (queryParams.PageNumber - 1) * queryParams.PageSize;
                Take = queryParams.PageSize;
                IsPagingEnabled = true;
            }
        else
            {
                Take = queryParams.PageSize;
            }
        }
    }
}
