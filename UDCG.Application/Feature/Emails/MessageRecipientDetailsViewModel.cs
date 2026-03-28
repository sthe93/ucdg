using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.Emails
{
    public class MessageRecipientDetailsViewModel
    {
        public int Id { get; set; }
        public int MessageRecipientId { get; set; }
        public int ApplicationId { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
