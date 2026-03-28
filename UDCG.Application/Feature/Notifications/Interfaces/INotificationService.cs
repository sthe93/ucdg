using System;
using System.Collections.Generic;
using System.Text;
using UDCG.Application.Common;
using UDCG.Application.Feature.Notifications.Resources;

namespace UDCG.Application.Feature.Notifications.Interfaces
{
    public interface INotificationService
    {
        public ApiInfoModel ApiInfo { get; set; }
        void SendAsync(NotificationResourceModel messageCreationModel);
    }
}
