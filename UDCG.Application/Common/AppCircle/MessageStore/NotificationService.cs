using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Common.Request.Models;
using UDCG.Application.Feature.Notifications.Resources;
using UDCG.Application.Interface;

namespace UDCG.Application.Common.AppCircle.MessageStore
{
   public class NotificationService : INotificationService
    {
        private readonly IExternalRequestHelper _request;

        public ApiInfoModel ApiInfo { get; set; }
        public NotificationService(IExternalRequestHelper request)
        {
            _request = request;
        }

        public void SendAsync(NotificationResourceModel messageCreationModel)
        {
            var json = JsonConvert.SerializeObject(messageCreationModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var requestInfo = new RequestInfoJsonModel
            {
                Controller = "messages/createMessage",
                HttpVerb = HttpVerb.Post,
                PayLoad = messageCreationModel
            };

            var response = _request.MakeJsonDataRequest(ApiInfo, requestInfo);

        }

        public String SendAsyncWithResponse(NotificationResourceModel messageCreationModel)
        {
            var json = JsonConvert.SerializeObject(messageCreationModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
     
            var requestInfo = new RequestInfoJsonModel
            {
                Controller = "messages/createMessage",
                HttpVerb = HttpVerb.Post,
                PayLoad = messageCreationModel
            };

            var response = _request.MakeJsonDataRequest(ApiInfo, requestInfo);

            return response;

         }


        public void SendAsyncReminder(int messageRecipientId)
        {
            var requestInfo = new RequestInfoJsonModel
            {
                Controller = "messages/Reminder/" +  messageRecipientId+"/remove",
                HttpVerb = HttpVerb.Put,
                PayLoad = null
            };
            var response = _request.MakeJsonDataRequest(ApiInfo, requestInfo);
        }
    }
}
