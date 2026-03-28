using System;


namespace UCDG.Domain.Entities
{
    public class Payments
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
        public string Step { get; set; }

        //Foreign Keys
        public Applications Applications { get; set; }
    }
}
