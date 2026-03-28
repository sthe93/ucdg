using System;
using System.Collections.Generic;
using System.Text;

namespace UCDG.Domain.Entities
{
    public class TemporaryApproverApplications
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public UserStoreUser User { get; set; }
        public Applications Applications { get; set; }
        public string ApprovedAs { get; set; }
    }
}
