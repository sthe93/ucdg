using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.Notifications.Resources
{
    public class NotificationResourceModel
    {

        public NotificationResourceModel()
        {
            MessageRecipient = new List<MessageRecipient>();
            MessageAttachment = new List<MessageAttachment>();
        }

        public string ClientApplication { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
        public int MessageTypeId { get; set; }
        public int Priority { get; set; }
        public List<MessageRecipient> MessageRecipient { get; set; }
        public List<MessageAttachment> MessageAttachment { get; set; }
        public bool isReminderOnly { get; set; }
        public string[] reminder { get; set; }
        //public int ApplicationId { get; set; }
   
    }

    public class MessageRecipient
    {
        public string EmailAddress { get; set; }
    }

    public class MessageAttachment
    {
        public Guid DocumentGuid { get; set; }

    }
}
