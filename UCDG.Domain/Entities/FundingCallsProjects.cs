
namespace UCDG.Domain.Entities
{
    public class FundingCallsProjects
    {

        public int FundingCallsId { get; set; }
        public FundingCalls FundingCalls { get; set; }

        public int ProjectsId { get; set; }
        public Projects Projects { get; set; }
    }
}
