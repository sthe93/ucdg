using System;
using System.Collections.Generic;
using System.Text;

namespace UCDG.Domain.Entities
{
    public class TemporaryAppointees
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string EmailAddress { get; set; }
        public string ContactNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string IdNumber { get; set; }
        public string StaffStatus { get; set; }
        public bool CanDisplay { get; set; }
        public int ApplicationsId { get; set; }
        //Foreign Keys
        public Applications Applications { get; set; }
    }
}
