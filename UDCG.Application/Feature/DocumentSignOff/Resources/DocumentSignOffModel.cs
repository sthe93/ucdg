using System;
using System.Collections.Generic;
using System.Text;
using UDCG.Application.Feature.Project.Resources;
using UDCG.Application.Feature.Roles.Resources;
using UDCG.Application.Feature.Users.Resources;

namespace UDCG.Application.Feature.DocumentSignOff.Resources
{
    public class DocumentSignOffModel
    {

        public DocumentSignOffModel()
        {
            Qualifications = new List<UserQualificationViewModel>();
            UserRoles = new List<ReadRoleResource>();
            Roles = new List<ReadRoleResourceVM>();
            DocumentSignOffsVM = new ReadDocumentSignOffViewModel();
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
        public IEnumerable<ReadRoleResource> UserRoles { get; set; }
        public IEnumerable<ReadRoleResourceVM> Roles { get; set; }
        public ReadDocumentSignOffViewModel DocumentSignOffsVM { get; }
        public string CostCentreNumber { get; set; }
        public Guid ReferenceId { get; set; }
    }

  
    public class DocumentSignOffsVM
    {
 
        public int DocumentSignOffID { get; set; }
        public int UserId { get; set; }

        public string UserFullName { get; set; }
        public string DocumentType { get; set; }
        public DateTime SignedDate { get; set; }
        public string UserRoleName { get; set; }
        public string ReferenceNumber { get; set; }
       
        public int ApplicationId { get; set; }

        public ReadDocumentSignOffViewModel ToReadDocumentSignOff()
        {
            ReadDocumentSignOffViewModel data = new ReadDocumentSignOffViewModel()
            {
                ApplicationsId = ApplicationId,
                DocumentType = "PDF",
                ReferenceNumber = ReferenceNumber,
                UserFullName = UserFullName,
                UserId = UserId,
                UserRoleName = UserRoleName,
                SignedDate = DateTime.Now,

            };

            return data;
        }

    }
    public class ReadRoleResourceVM
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
    }
}
 

        
