using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Common.Constants
{
    public class ApplicationStatuesConstants
    {
        public const string Incomplete = "Incomplete";
        public const string PendingApproval = "PendingApproval";
        public const string Approved = "Approved";
        public const string Declined = "Declined";
        public const string ApprovedByHOD = "Approved by HOD";
        public const string ApprovedbyViceDean = "Approved by Executive / Vice Dean";
        public const string ApprovedByFundAdmin = "Approved by Fund Admin";
        public const string ApprovedBySIADirector = "Approved by SIA Director";
        public const string ApprovedByFinancialBusinessPartner = "Approved by Financial Business Partner";
        public const string ReturnedRorInfo = "Returned for Info";


        public const string PendingApprovalByHOD  = "Pending approval by HOD";
        public const string PendingApprovalByVD = "Pending approval by ED/VD";
        public const string PendingApprovalByFA = "Pending approval by FA";
        public const string PendingApprovalByViceDean = "Pending Approval by Second Line Manager";
        public const string PendingApprovalbySIA = "Pending approval by SIA";
        public const string PendingApprovalByFBP = "Pending approval by FBP";

    }

    public class UserRolesConstants
    {
        public const string Applicant = "Applicant";
        public const string HOD = "HOD";
        public const string ViceDean = "Executive / Vice Dean";
        public const string FundAdministrator = "Fund Administrator";
        public const string SIADirector = "SIA Director";
        public const string FinancialBusinessPartner = "Financial Business Partner";
        public const string AcademicDirector = "Academic Director";
        public const string TemporaryApprover = "Temporary Approver";
    }
}
