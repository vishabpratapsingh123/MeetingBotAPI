using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;

namespace MeetingBotAPI.Service
{
    public class GraphService
    {
        private readonly GraphServiceClient _graphClient;

        public GraphService(IConfiguration config)
        {
            var clientId = config["Authentication:Microsoft:ClientId"];
            var tenantId = config["Authentication:Microsoft:TenantId"];
            var clientSecret = config["Authentication:Microsoft:ClientSecret"];
            var scopes = new[] { "https://graph.microsoft.com/.default" };


            var clientSecretCredential = new ClientSecretCredential(
                tenantId,
                clientId,
                clientSecret
            );

            //_graphClient = new GraphServiceClient(clientSecretCredential);
             _graphClient = new GraphServiceClient(clientSecretCredential, scopes);


        }
        public async Task<bool> UserExistsAsync(string email)
        {
            try
            {
                // Use the email parameter correctly to check if the user exists
                //var user = await _graphClient.Users[email]
                //                             .GetAsync();
                var users = await _graphClient.Users
    .GetAsync(config =>
    {
        config.QueryParameters.Top = 10;
        config.QueryParameters.Select = new[] { "displayName", "mail", "userPrincipalName" };
    });

                foreach (var user in users.Value)
                {
                    Console.WriteLine($"Name: {user.DisplayName}, Email: {user.Mail ?? user.UserPrincipalName}");
                }

                // If the request is successful, the user exists.
                //Console.WriteLine($"User found: {users.DisplayName}");
                return true;
            }
            catch (ServiceException ex)
            {
                // If the error status code is "Not Found", the user doesn't exist.
                if (ex.ResponseStatusCode == 404)
                {
                    Console.WriteLine($"User with email {email} not found.");
                    return false;
                }
                // Handle other potential errors
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<string> ScheduleMeetingAsync(string title, DateTime startTime, DateTime endTime)
        {
            //var meeting = new OnlineMeeting
            //{
            //    Subject = title,
            //    StartDateTime = startTime,
            //    EndDateTime = endTime
            //};
            var meeting = new OnlineMeeting
            {
                StartDateTime = DateTime.UtcNow.AddMinutes(15),
                EndDateTime = DateTime.UtcNow.AddMinutes(45),
                Subject = "Test Meeting"
            };

            //bool exists = await UserExistsAsync("garima.sharma@normecgroup.com");
            //if (exists)
            //{
            //    Console.WriteLine("User exists in Azure AD.");
            //}
            //else
            //{
            //    Console.WriteLine("User does not exist in Azure AD.");
            //}

            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var credential = new ClientSecretCredential(
                tenantId: "3e6f87ce-fda6-48d7-a98e-5fbcd9318977",
                clientId: "d0f89e85-54da-4e92-8f6a-80e18a9c6b1e",
                clientSecret: "ZHn8Q~Anu9DDTSwFcG-yk5WyqHt3QJ8A-6RTNbbm"
            );

            var graphClient = new GraphServiceClient(credential, scopes);

            var meeting1 = new OnlineMeeting
            {
                StartDateTime = DateTimeOffset.UtcNow.AddMinutes(5),
                EndDateTime = DateTimeOffset.UtcNow.AddMinutes(35),
                Subject = "App-only Meeting"
            };

            var result1= await graphClient.Users["garima.sharma@normecgroup.com"].OnlineMeetings.PostAsync(meeting1);

            Console.WriteLine("Meeting URL: " + result1.JoinWebUrl);

            try
            {
                //var result = await _graphClient.Me.OnlineMeetings.PostAsync(meeting);

                var result = await _graphClient.Users["garima.sharma@normecgroup.com"].OnlineMeetings.PostAsync(meeting);
                return result.JoinWebUrl;
            }
            catch (Microsoft.Graph.Models.ODataErrors.ODataError odataError)
            {
                string error = $"Graph API OData Error: {odataError.Error?.Code} - {odataError.Error?.Message}";
      
                Console.WriteLine($"Graph API OData Error: {odataError.Error?.Code} - {odataError.Error?.Message}");
                //throw;

                return error;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                throw;
            }


        }
    }
}
