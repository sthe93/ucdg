using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UDCG.Application.Common;
using UDCG.Application.Feature.Notifications.Resources;

namespace UDCG.Application.Interface
{
    public interface INotificationService
    {
        public ApiInfoModel ApiInfo { get; set; }
        void SendAsync(NotificationResourceModel messageCreationModel);

        void SendAsyncReminder(int messageRecipientId);

        string SendAsyncWithResponse(NotificationResourceModel messageCreationModel);
    }
}
