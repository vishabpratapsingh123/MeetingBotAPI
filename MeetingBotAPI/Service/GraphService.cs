using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace MeetingBotAPI.Service
{
    public class GraphService
    {
        private readonly GraphServiceClient _graphClient;

        public GraphService(IConfiguration config)
        {
            var clientId = config["AzureAd:ClientId"];
            var tenantId = config["AzureAd:TenantId"];
            var clientSecret = config["AzureAd:ClientSecret"];

            var clientSecretCredential = new ClientSecretCredential(
                tenantId,
                clientId,
                clientSecret
            );

            _graphClient = new GraphServiceClient(clientSecretCredential);
        }

        public async Task<string> ScheduleMeetingAsync(string title, DateTime startTime, DateTime endTime)
        {
            var meeting = new OnlineMeeting
            {
                Subject = title,
                StartDateTime = startTime,
                EndDateTime = endTime
            };

            var result = await _graphClient.Me.OnlineMeetings
                                 .PostAsync(meeting);

            return result.JoinWebUrl;
        }
    }
}
