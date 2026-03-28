using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.Application
{
    public class PaymentsViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int NumberOfWeeks { get; set; }
        public int HoursPerWeek { get; set; }
        public int TotalNumberOfHours { get; set; }
        public double RatePerHour { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double MonthTotal { get; set; }
        public int ApplicationsId { get; set; }
        public string Step { get; set; }
    }
}
