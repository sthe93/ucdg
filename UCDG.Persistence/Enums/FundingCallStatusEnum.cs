using System.ComponentModel;

namespace UCDG.Persistence.Enums
{
    public enum FundingCallStatusEnum
    {
        [Description("New")]
        New = 1,

        [Description("In Progress")]
        InProgress = 2,

        [Description("Expired")]
        Expired = 3
    }
}
