using System;
using System.Collections.Generic;
using System.Text;

namespace UCDG.Domain.Entities
{
    public class UserX
    {
        public int UserXId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Nationality { get; set; }
        public string IdPassportNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string EmailAddress { get; set; }
        public string AlternativeEmailAddress { get; set; }
        public string CellPhoneNumber { get; set; }
        public string TelephoneNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsProfileCompleted { get; set; }
        public string Campus { get; set; }
        public string Title { get; set; }
        public string StaffNumber { get; set; }
        public string Position { get; set; }
        public bool IsAcademic { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Race { get; set; }
        public string Gender { get; set; }
        public bool OtherProgramme { get; set; }
        public string OtherProgrammeName { get; set; }
        public bool OtherFundSource { get; set; }
        public string OtherFundSourceName { get; set; }
    }
}
