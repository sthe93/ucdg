using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace UCDG.Persistence.Enums
{
    public enum ApplicationStepsEnum
    {
        [Description("Applicant Details")]
        ApplicantDetails = 1,

        [Description("Approve Projects")]
        ApproveProjects = 2,

        [Description("Improvement Of Staff Qualifications")]
        ImprovementOfStaffQualifications = 3,

        [Description("Supporting Documents")] 
        SupportingDocuments = 4,

        [Description("Improving Staff Research Productivity")] 
        ImproveStaffResearch = 5,

        [Description("Mobility Programmes")]
        MobilityProgrammes = 6,

        [Description("Research career development of emerging and mid-career researchers")]
        CareerDevelopment = 7,

        [Description("Final Step")]
        FinalStep = 8
    }
}
