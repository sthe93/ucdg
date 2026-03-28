

namespace UDCG.Application.Feature.ProgressReport
{
    public class ProgressReportCommentsViewModel
    {
        public int ProgressReportId { get; set; }
        public int UserId { get; set; }
        public int? UserStoreUserId { get; set; }
        public string Comment { get; set; } 
        public string AddedBy { get; set; }
    }
}
