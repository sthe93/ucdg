using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCDG.Domain.Entities
{
    public class ApplicationAction
    {
        [Key]
        public int ActionId { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; } = null!;

        public DateTime ActionDateUtc { get; set; } = DateTime.UtcNow;

        public int? ActorUserStoreUserId { get; set; }

        [MaxLength(50)]
        public string? ActorStaffNumber { get; set; }

        public int? ApplicantLegacyUserId { get; set; }

        [MaxLength(50)]
        public string? ApplicantStaffNumber { get; set; }

        [MaxLength(50)]
        public string? ApplicantLineManagerStaffNumberAtAction { get; set; }

        [MaxLength(200)]
        public string? ApplicantDepartmentAtAction { get; set; }

        [MaxLength(200)]
        public string? ApplicantFacultyAtAction { get; set; }

        public int? FromStatusId { get; set; }
        public int? ToStatusId { get; set; }

        public string ActingForStaffNumber { get; set; }
        public bool IsTemporaryActor { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public Applications Application { get; set; } = null!;
    }
}

