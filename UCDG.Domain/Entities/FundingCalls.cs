using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UCDG.Domain.Entities
{
    public class FundingCalls
    {
        public int Id { get; set; }
        public string FundingCallName { get; set; }
        public string FundingBudget { get; set; }
        public string ShortDescription { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public DateTime? AmendedClosingDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public FundingCallStatus FundingCallStatus { get; set; }

        [NotMapped]
        public string[] ProjectId { get; set; }

        [NotMapped]
        public List<Projects> FundingCallProjects { get; set; }

    }

    public class Projects
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public ProjectCycles ProjectCycles { get; set; }
        public bool IsActive { get; set; }

        [NotMapped]
        public string ProjectCycleId { get; set; }

    }

    public class UpdateFundingCallClosingDate
    {
        public int FundingCallId { get; set; }
        public int UserId { get; set; }
        public DateTime ClosingDate { get; set; }
      
    }

}
