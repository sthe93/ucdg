using System;
using System.Collections.Generic;
using System.Text;

namespace UCDG.Domain.Entities
{
    public class EmailsSent
    {
        public int Id { get; set; }
        public string EmailName { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? SentDate { get; set; }

        //Foreign Keys
        public Applications Applications { get; set; }
        
    }
}
