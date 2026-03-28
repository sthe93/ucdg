using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.FundingCalls
{
    public class SearchFundingViewModel
    {
        public int FundingCallStatusId { get; set; }
        public DateTime? OpeningDateFilter { get; set; }
        public DateTime? ClosingDateFilter { get; set; }
        public string SearchCallName { get; set; }
        public int ProjectId { get; set; }
    }
}
