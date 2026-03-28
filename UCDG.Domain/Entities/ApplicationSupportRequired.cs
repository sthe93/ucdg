

namespace UCDG.Domain.Entities
{
    public class ApplicationSupportRequired
    {
        public int ApplicationsId { get; set; }
        public Applications Applications { get; set; }

        public int SupportRequiredId { get; set; }
        public SupportRequired SupportRequired { get; set; }
    }
}
