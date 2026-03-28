using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace UCDG.Domain.Entities
{
    public class Comments
    {
        public int Id { get; set; }
        public int ApplicationsId { get; set; }
        public Applications Applications { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        [NotMapped]
        public int? UserStoreUserId { get; set; }
        public string Comment { get; set; }
        [NotMapped]
        public string DisplayName { get; set; }
        [NotMapped]
        public string Username { get; set; }
    }
}
