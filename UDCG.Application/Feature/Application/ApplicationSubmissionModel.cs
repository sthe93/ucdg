namespace UDCG.Application.Feature.Application
{
    public class ApplicationSubmissionModel
    {
        public int ApplicationId { get; set; }
        public bool? FlightsChooseCheapest { get; set; }
        public string FlightsCheapestExplanation { get; set; }
        public bool? AccomChooseCheapest { get; set; }
        public string AccomCheapestExplanation { get; set; }

        public string FinancialMotivation { get; set; }
        public string ApplicantProgress { get; set; }
        public string OutputMeasure { get; set; }

        public int OtherFunding { get; set; }
        public int FacultyContibution { get; set; }
        public int DepartmentContribution { get; set; }
        public int ResearchFundsContribution { get; set; }
        public int DHETFundsRequested { get; set; }
        public int TotalCost { get; set; }
        public int FundAdminApprovedAmount { get; set; }
        public int ApprovedAmount { get; set; }
        public string OtherFundingSource { get; set; }
    }
}
