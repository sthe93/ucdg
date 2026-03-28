using System;
using System.ComponentModel.DataAnnotations;

namespace UCDG.Domain.Entities
{
    public class DocumentSignOffs
    {
        [Key]
        public int DocumentSignOffID { get; set; }

        //  (legacy user id)
        public int UserId { get; set; }

        // ✅  (new userstore id)
        public int? UserStoreUserId { get; set; }

        public string UserFullName { get; set; }
        public string DocumentType { get; set; }
        public DateTime SignedDate { get; set; }
        public string UserRoleName { get; set; }
        public string ReferenceNumber { get; set; }

        public int ApplicationsId { get; set; }
        public Applications Applications { get; set; }
    }
}