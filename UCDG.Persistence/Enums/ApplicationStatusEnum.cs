using System.ComponentModel;


namespace UCDG.Persistence.Enums
{
    public enum ApplicationStatusEnum
    {
        [Description("Incomplete")]
        Incomplete = 1,

        [Description("Pending Approval")]
        PendingApproval = 2,

        [Description("Approved")]
        Approved = 3,

        [Description("Declined")]
        Declined = 4,

        [Description("Pending DVC Approval")]
        PendingDVCApproval = 5,

        [Description("DVC Declined")]
        DVCDeclined = 6,

        [Description("DVC Approved")]
        DVCApproved = 7,

        [Description("Approved by UCDG_HOD")]
        ApprovedbyHOD = 8,

        [Description("Approved by UCDG_Fin_Bus_Partner")]
        ApprovedbyFinancialBusinessPartner = 9,

        [Description("Approved by UCDG_Fund_Admin")]
        ApprovedbyFundAdmin = 10,

        [Description("Approved by UCDG_VICE_DEAN")]
        ApprovedbyViceDean = 11,

        [Description("Approved by UCDG_SIA_Director")]
        ApprovedbySIADirector = 12,

        [Description("Award Letter Declined")]
        AwawrdLetterDeclined = 13,

        [Description("Award Letter Accepted")]
        AwawrdLetterAccepted = 14,
            
        [Description("Pending approval by HOD")]
        PendingApprovalByHod = 29,

        [Description("Pending Approval by UCDG_VICE_DEAN")]
        PendingApprovalByEdOrVd = 30,

        [Description("Pending Approval by UCDG_Fund_Admin")]
        PendingApprovalByFa = 31,

        [Description("Pending Approval by UCDG_SIA_Director")]
        PendingApprovalBySia = 32,
        [Description("Pending approval by FBP")]
        PendingApprovalByFbp =19,

        [Description("Returned for Info")]
        ReturnedforInfo = 15,











    }
}
