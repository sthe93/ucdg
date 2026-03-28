
using System.ComponentModel;


namespace UCDG.Persistence.Enums
{
    public enum AssignableUserRoles
    {
        [Description("Applicant")]
        Applicant = 1,

        [Description("Temporary Approver")]
        TemporaryApprover = 2,

        [Description("HOD")]
        HOD = 3,

        [Description("Fund Administrator")]
        FundAdministrator = 4,

        [Description("SIA Director")]
        SIADirector = 5,

        [Description("Financial Business Partner")]
        FinancialBusinessPartner = 6,

        [Description("Executive / Vice Dean")]
        ViceDean = 7,
    }
}
