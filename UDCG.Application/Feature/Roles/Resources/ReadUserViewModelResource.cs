using System;
using System.Collections.Generic;
using System.Text;
using UDCG.Application.Feature.Users.Resources;

namespace UDCG.Application.Feature.Roles.Resources
{


    public class ReadUserViewModelResource
    {
        public ReadUserViewModelResource()
        {
            Qualifications = new List<UserQualificationViewModel>();
            UserRoles = new List<UserRoleResource>();
        }

        public int UserId { get; set; }
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
        public string HOD { get; set; }
        public string HODStaffNUmber { get; set; }
        public string ViceDean { get; set; }
        public string ViceDeanStaffNUmber { get; set; }
        public string Disability { get; set; }
        public IEnumerable<UserQualificationViewModel> Qualifications { get; set; }
        public IEnumerable<UserRoleResource> UserRoles { get; set; }
        public string CostCentreNumber { get; set; }
        public Guid ReferenceId { get; set; }
        public string SiaUser { get; set; }
        public bool IsAcknowledge { get; set; }
        public string? HodPersonType { get; set; }
        public string? ViceDeanPersonType { get; set; }
        
    }

    public class UserQualificationViewModel
    {
        public int QualificationId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string InstitutionName { get; set; }
        public string QualificationType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

}
