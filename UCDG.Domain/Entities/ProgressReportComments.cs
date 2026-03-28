

using System.ComponentModel.DataAnnotations.Schema;

namespace UCDG.Domain.Entities
{
    public class ProgressReportComments
    {
        public int Id { get; set; }
        public int ProgressReportsId { get; set; }

        public ProgressReports ProgressReports { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public string Comment { get; set; }

        public int? UserStoreUserId { get; set; }

        [NotMapped] public string? AddedBy { get; set; }
        [NotMapped] public string? DisplayName { get; set; }
    }
}
