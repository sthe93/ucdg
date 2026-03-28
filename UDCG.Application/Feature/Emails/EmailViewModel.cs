using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.Emails
{
    public class EmailViewModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int ApplicationsId { get; set; }
        public string EmailName { get; set; }
        public DateTime? SentDate { get; set; }
    }
}
