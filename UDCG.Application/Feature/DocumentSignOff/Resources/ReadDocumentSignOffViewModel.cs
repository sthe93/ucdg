using System;
using System.Collections.Generic;
using System.Text;
using UCDG.Domain.Entities;

namespace UDCG.Application.Feature.DocumentSignOff.Resources
{

    public class ReadDocumentSignOffViewModel
    {
        public int DocumentSignOffID { get; set; }

        // legacy
        public int UserId { get; set; }

        // new (add)
        public int? UserStoreUserId { get; set; }

        public UCDG.Domain.Entities.UserStoreUser User { get; set; }
        public string UserFullName { get; set; }
        public string DocumentType { get; set; }
        public DateTime SignedDate { get; set; }
        public string UserRoleName { get; set; }
        public string ReferenceNumber { get; set; }

        public Applications Applications { get; set; }
        public int ApplicationsId { get; set; }
    }
}
