using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCDG.Domain.Entities
{
    public class UserStoreUser
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }

        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }

        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LastLoggedIn { get; set; }
        public Guid? UserGuid { get; set; }

        public string? LineManagerStaffNumber { get; set; }
        public string? LineManagerUsername { get; set; }
        public string? LineManagerName { get; set; }
        public string? LineManagerSurname { get; set; }

        public string? FacultyDivision { get; set; }
        public string? Email { get; set; }
        public string? AlternativeEmailAddress { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? HRPostNumber { get; set; }

        public string? OfficeLocation { get; set; }
        public string? OfficeContact { get; set; }
        public string? Campus { get; set; }

        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? IdNumber { get; set; }
        public string? CellPhone { get; set; }

        public int IsSystemDeactivate { get; set; }
        public DateTime? SystemDeactivateDate { get; set; }
        public DateTime? DateDeleted { get; set; }

        public string? CostCentreNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Disability { get; set; }
        public string? Gender { get; set; }
        public string? HodPersonType { get; set; }
        public bool? IsAcademic { get; set; }
        public bool? IsProfileCompleted { get; set; }
        public string? Nationality { get; set; }
        public string? Race { get; set; }
        public string? Title { get; set; }

        public string? ViceDean { get; set; }
        public string? ViceDeanPersonType { get; set; }
        public string? ViceDeanStaffNumber { get; set; }
        

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<TemporaryUserRole> TemporaryUserRoles { get; set; } = new List<TemporaryUserRole>();
        public ICollection<Qualification> Qualifications { get; set; } = new List<Qualification>();
        //public bool IsFundAdministrator { get; set; }
    }

}

