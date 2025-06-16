using MeetingBotAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NewtonsoftJson = Newtonsoft.Json;
using SystemTextJson = System.Text.Json;

namespace MeetingBotAPI.Controllers
{
    [EnableCors("AllowAll")]
    [ApiController]
    [Route("api/meetings")]
    //[Authorize]
    public class MeetingsController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        private readonly GraphService _graphService;
        private readonly GraphServiceClient _graphClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly HttpClient client = new HttpClient();


        public MeetingsController(GraphService graphService, IHttpClientFactory httpClientFactory)
        {
            _graphService = graphService;
            _httpClientFactory = httpClientFactory;
            //_graphClient = graphClient;
        }

        //[HttpPost("schedule")]
        //public async Task<IActionResult> ScheduleMeeting([FromBody] MeetingRequest request)
        //{
        //    var meetingUrl = await _graphService.ScheduleMeetingAsync(request.Title, request.StartTime, request.EndTime);
        //    return Ok(new { MeetingUrl = meetingUrl });
        //}

        public static async Task<bool> CheckApplicationExistsAsync(string tenantId, string clientId, string clientSecret)
        {
            // Define the token request parameters for Azure AD OAuth2
            string tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var requestData = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "client_credentials" },
            { "scope", "https://graph.microsoft.com/.default" } // This scope is needed to get a token for Microsoft Graph API
        };

            var content = new FormUrlEncodedContent(requestData);

            try
            {
                // Send the request to get the token
                HttpResponseMessage response = await client.PostAsync(tokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    // Read the response (it will include the access token)
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject jsonResponse = JObject.Parse(responseBody);

                    // Check if the access_token is returned
                    if (jsonResponse.ContainsKey("access_token"))
                    {
                        return true; // Application exists and credentials are valid
                    }
                    else
                    {
                        return false; // Token wasn't issued
                    }
                }
                else
                {
                    // If the response status code is not successful, check for errors
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {errorResponse}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
        [HttpPost("testapplication")]
        public  async Task<IActionResult> testapplication()
        {
            string error = "";
            // Replace with your actual Azure AD details
            string clientId = "d0f89e85-54da-4e92-8f6a-80e18a9c6b1e"; // Your Azure AD Tenant ID
            string tenantId = "3e6f87ce-fda6-48d7-a98e-5fbcd9318977"; // Your Azure AD Application (Client) ID
            string clientSecret = "ZHn8Q~Anu9DDTSwFcG-yk5WyqHt3QJ8A-6RTNbbm"; // Your Azure AD Application Secret

            bool exists = await CheckApplicationExistsAsync(tenantId, clientId, clientSecret);
            await CheckApplicationPermissionsAsync(tenantId, clientId, clientSecret);

            if (exists)
            {
                error = "Application exists and credentials are valid.";
                Console.WriteLine("Application exists and credentials are valid.");
            }
            else
            {
                error = "Application does not exist or credentials are invalid.";
                Console.WriteLine("Application does not exist or credentials are invalid.");
            }
            return Ok(error);
        }
        public static async Task<string> GetGraphApiAccessTokenAsync(string tenantId, string clientId, string clientSecret)
        {
            string tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var requestData = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "client_credentials" },
            { "scope", "https://graph.microsoft.com/.default" }
        };

            var content = new FormUrlEncodedContent(requestData);

            try
            {
                HttpResponseMessage response = await client.PostAsync(tokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    JObject jsonResponse = JObject.Parse(responseBody);

                    // Extract access token
                    if (jsonResponse.ContainsKey("access_token"))
                    {
                        return jsonResponse["access_token"].ToString();
                    }
                    else
                    {
                        Console.WriteLine("Error: No access token found.");
                        return null;
                    }
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {errorResponse}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        // Function to get the permissions of the Azure AD Application via Microsoft Graph API
        public static async Task CheckApplicationPermissionsAsync(string tenantId, string clientId, string clientSecret)
        {
            // Get the access token
            string accessToken = await GetGraphApiAccessTokenAsync(tenantId, clientId, clientSecret);
            var meeting = await  ScheduleTeamsMeetingAsync(accessToken, "garima.sharma@normecgroup.com");

            if (accessToken != null)
            {
                string apiUrl = $"https://graph.microsoft.com/v1.0/applications/{clientId}/appRoles";

                // Set the Authorization header with the obtained access token
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                try
                {
                    // Send the GET request to fetch application details (including permissions)
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        JObject jsonResponse = JObject.Parse(responseBody);

                        // Check if application roles or permissions exist
                        if (jsonResponse["value"] != null)
                        {
                            Console.WriteLine("Permissions for the Azure AD Application:");
                            foreach (var permission in jsonResponse["value"])
                            {
                                Console.WriteLine($"- {permission["displayName"]}: {permission["id"]}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("No permissions found for the application.");
                        }
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error fetching permissions: {errorResponse}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
        public static async Task<HttpResponseMessage> ScheduleTeamsMeetingAsync(string accessToken, string userId)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var eventData = new
            {
                subject = "Project Sync Meeting",
                start = new
                {
                    dateTime = "2025-05-05T10:00:00",
                    timeZone = "Pacific Standard Time"
                },
                end = new
                {
                    dateTime = "2025-05-05T11:00:00",
                    timeZone = "Pacific Standard Time"
                },
                location = new
                {
                    displayName = "Microsoft Teams Meeting"
                },
                attendees = new[]
                {
            new
            {
                emailAddress = new
                {
                    address = "vishabpratapsdm07@gmail.com",
                    name = "Vishab Sigh"
                },
                type = "required"
            }
        },
                isOnlineMeeting = true,
                onlineMeetingProvider = "teamsForBusiness",
                allowNewTimeProposals = true,
                responseRequested = true
            };

            var content = new StringContent(JsonConvert.SerializeObject(eventData), Encoding.UTF8, "application/json");

            //var response = await client.PostAsync($"https://graph.microsoft.com/v1.0/users/me/events", content);
            var response = await client.PostAsync($"https://graph.microsoft.com/v1.0/users/{"VishabSingh@normecverifavia964.onmicrosoft.com"}/events", content);

            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Meeting scheduled successfully:");
                Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine("Failed to schedule meeting:");
                Console.WriteLine(result);
            }
            return response;
        }
        //[HttpGet("events")]
        //public async Task<IActionResult> GetEvents()
        //{
        //    var events = await _graphClient.Me.Calendar.Events
        //        .()
        //        .get();

        //    return Ok(events);
        //}
        //    [HttpGet("GetEvents1")]
        //    public async Task<IActionResult> GetEvents1()
        //    {
        //        try
        //        {
        //            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(
        //requestMessage =>
        //{
        //    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        //    return Task.CompletedTask;
        //}));
        //            // Create the GraphServiceClient
        //            var graphClient = new GraphServiceClient(authProvider);

        //            // Retrieve events from the calendar


        //            return Ok();
        //        }
        //        catch (ServiceException ex)
        //        {
        //            return BadRequest(new { error = ex.Message });
        //        }
        //    }
        //[HttpPost("ScheduleMeeting1")]
        //public async Task<IActionResult> ScheduleMeeting1([FromBody] MeetingRequest meetingRequest)
        //{
        //    // Extract the Bearer token from the Authorization header
        //    var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        //    if (string.IsNullOrEmpty(accessToken))
        //        return Unauthorized("Missing access token");

        //    var graphApiUrl = "https://graph.microsoft.com/v1.0/me/onlineMeetings";

        //    var meetingPayload = new
        //    {
        //        startDateTime = meetingRequest.StartTime.ToString("o"),
        //        endDateTime = meetingRequest.EndTime.ToString("o"),
        //        subject = meetingRequest.Subject,
        //        // timeZone is optional, MS Graph assumes UTC if not provided
        //        // You can extend with more properties if needed
        //    };

        //    var httpClient = _httpClientFactory.CreateClient();

        //    var request = new HttpRequestMessage(HttpMethod.Post, graphApiUrl);
        //    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        //    request.Content = new StringContent(SystemTextJson.JsonSerializer.Serialize(meetingPayload), Encoding.UTF8, "application/json");

        //    var response = await httpClient.SendAsync(request);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var errorContent = await response.Content.ReadAsStringAsync();
        //        return StatusCode((int)response.StatusCode, errorContent);
        //    }

        //    var responseContent = await response.Content.ReadAsStringAsync();

        //    return Ok(JsonDocument.Parse(responseContent).RootElement);
        //}
        //[HttpPost("ScheduleMeeting1")]
        //public async Task<IActionResult> ScheduleMeeting1([FromBody] MeetingRequest2 meetingRequest)
        //{
        //    var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        //    if (string.IsNullOrEmpty(accessToken))
        //        return Unauthorized("Missing access token");

        //    var graphApiUrl = "https://graph.microsoft.com/v1.0/me/events";

        //    var attendees = new List<object>();
        //    if (meetingRequest.AttendeesEmails != null)
        //    {
        //        foreach (var email in meetingRequest.AttendeesEmails)
        //        {
        //            attendees.Add(new
        //            {
        //                emailAddress = new
        //                {
        //                    address = email,
        //                    name = email // Optionally, add real names if you have them
        //                },
        //                type = "required" // or "optional"
        //            });
        //        }
        //    }

        //    var meetingPayload = new
        //    {
        //        subject = meetingRequest.Subject,
        //        body = new
        //        {
        //            contentType = "HTML",
        //            content = meetingRequest.Body ?? "Scheduled via API"
        //        },
        //        start = new
        //        {
        //            dateTime = meetingRequest.StartTime.ToString("o"),
        //            timeZone = meetingRequest.TimeZone ?? "UTC"
        //        },
        //        end = new
        //        {
        //            dateTime = meetingRequest.EndTime.ToString("o"),
        //            timeZone = meetingRequest.TimeZone ?? "UTC"
        //        },
        //        location = new
        //        {
        //            displayName = meetingRequest.Location ?? "Online"
        //        },
        //        attendees = attendees,
        //        isOnlineMeeting = true,
        //        onlineMeetingProvider = "teamsForBusiness"
        //    };

        //    var httpClient = _httpClientFactory.CreateClient();
        //    var request = new HttpRequestMessage(HttpMethod.Post, graphApiUrl);
        //    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //    request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(meetingPayload), Encoding.UTF8, "application/json");

        //    var response = await httpClient.SendAsync(request);

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var errorContent = await response.Content.ReadAsStringAsync();
        //        return StatusCode((int)response.StatusCode, errorContent);
        //    }

        //    var responseContent = await response.Content.ReadAsStringAsync();
        //    return Ok(JsonDocument.Parse(responseContent).RootElement);
        //}
        [HttpPost("ScheduleMeeting1")]
        public async Task<IActionResult> ScheduleMeeting1([FromBody] MeetingRequest2 meetingRequest)
        {
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized("Missing access token");

            var graphApiUrl = "https://graph.microsoft.com/v1.0/me/events";

            var attendees = new List<object>();
            if (meetingRequest.AttendeesEmails != null)
            {
                foreach (var email in meetingRequest.AttendeesEmails)
                {
                    attendees.Add(new
                    {
                        emailAddress = new { address = email, name = email },
                        type = "required"
                    });
                }
            }

            var meetingPayload = new
            {
                subject = meetingRequest.Subject,
                body = new
                {
                    contentType = "HTML",
                    content = meetingRequest.Body ?? "Scheduled via API"
                },
                start = new
                {
                    dateTime = meetingRequest.StartTime.ToString("o"),
                    //dateTime = "2025-05-22T07:00:21.3820000Z",

                    timeZone = meetingRequest.TimeZone ?? "UTC"
                },
                end = new
                {
                    dateTime = meetingRequest.EndTime.ToString("o"),
                    //dateTime = "2025-05-22T07:00:21.3820000Z",
                    timeZone = meetingRequest.TimeZone ?? "UTC"
                },
                location = new
                {
                    displayName = meetingRequest.Location ?? "Online"
                },
                attendees = attendees,
                isOnlineMeeting = true,
                onlineMeetingProvider = "teamsForBusiness"
            };

            var httpClient = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, graphApiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(meetingPayload), Encoding.UTF8, "application/json");


            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var createdEvent = JsonDocument.Parse(responseContent);
            var joinUrl = createdEvent.RootElement
                .GetProperty("onlineMeeting")
                .GetProperty("joinUrl")
                .GetString();

            // ✅ Now send a custom email with the join link
            var mailPayload = new
            {
                message = new
                {
                    subject = "You're invited: " + meetingRequest.Subject,
                    body = new
                    {
                        contentType = "HTML",
                        content = $@"
                    <p>You are invited to a Microsoft Teams meeting.</p>
                    <p><strong>Subject:</strong> {meetingRequest.Subject}</p>
                    <p><strong>Start:</strong> {meetingRequest.StartTime} ({meetingRequest.TimeZone ?? "UTC"})</p>
                    <p><strong>End:</strong> {meetingRequest.EndTime} ({meetingRequest.TimeZone ?? "UTC"})</p>
                    <p><strong>Join Link:</strong> <a href='{joinUrl}'>{joinUrl}</a></p>"
                    },
                    toRecipients = meetingRequest.AttendeesEmails.Select(email => new
                    {
                        emailAddress = new { address = email }
                    }).ToList()
                },
                saveToSentItems = true
            };

            var mailRequest = new HttpRequestMessage(HttpMethod.Post, "https://graph.microsoft.com/v1.0/me/sendMail");
            mailRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            mailRequest.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(mailPayload), Encoding.UTF8, "application/json");

            var mailResponse = await httpClient.SendAsync(mailRequest);
            if (!mailResponse.IsSuccessStatusCode)
            {
                var mailError = await mailResponse.Content.ReadAsStringAsync();
                return StatusCode((int)mailResponse.StatusCode, $"Meeting created but email failed: {mailError}");
            }

            return Ok(createdEvent.RootElement);
        }
        [HttpGet("GetCalendarEventsSummary")]
        public async Task<IActionResult> GetCalendarEventsSummary([FromQuery] string range = "week")
        {
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(accessToken))
                return Unauthorized("Missing access token");

            DateTime startDate = DateTime.UtcNow.Date;
            DateTime endDate;

            switch (range?.ToLower())
            {
                case "week":
                    endDate = startDate.AddDays(7);
                    break;
                case "month":
                    endDate = startDate.AddMonths(1);
                    break;
                default:
                    return BadRequest("Invalid range. Use 'week' or 'month'.");
            }

            string graphApiUrl = $"https://graph.microsoft.com/v1.0/me/calendarView?startDateTime={startDate:O}&endDateTime={endDate:O}&$orderby=start/dateTime";

            var httpClient = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, graphApiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Prefer", "outlook.timezone=\"UTC\""); // Optional: customize for user timezone

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }

            var content = await response.Content.ReadAsStringAsync();
            //var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            var events = jsonDoc.RootElement.GetProperty("value");

            var simplifiedEvents = new List<object>();

            foreach (var evt in events.EnumerateArray())
            {
                // Safe extraction
                var subject = evt.GetProperty("subject").GetString();

                var startTime = evt.GetProperty("start").GetProperty("dateTime").GetString();
                var endTime = evt.GetProperty("end").GetProperty("dateTime").GetString();

                //string location = evt.TryGetProperty("location", out var locationProp) &&
                //                  locationProp.TryGetProperty("displayName", out var displayNameProp)
                //                  ? displayNameProp.GetString()
                //                  : "N/A";

                bool isOnline = evt.TryGetProperty("isOnlineMeeting", out var isOnlineProp) && isOnlineProp.GetBoolean();

                string joinUrl = null;
                if (evt.TryGetProperty("onlineMeeting", out var onlineMeetingProp) &&
                    onlineMeetingProp.ValueKind == JsonValueKind.Object &&
                    onlineMeetingProp.TryGetProperty("joinUrl", out var joinUrlProp))
                {
                    joinUrl = joinUrlProp.GetString();
                }
                simplifiedEvents.Add(new
                {
                    Subject = subject,
                    StartTime = startTime,
                    EndTime = endTime,
                    //Location = location,
                    IsOnlineMeeting = isOnline,
                    JoinUrl = joinUrl
                });
            }

            return Ok(simplifiedEvents);
        }



        //[HttpPost("create-meeting")]
        //public async Task<IActionResult> CreateMeeting([FromBody] MeetingRequest1 meetingRequest)
        //{
        //    var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        //    if (string.IsNullOrEmpty(accessToken))
        //        return Unauthorized("Access token missing");

        //    var graphClient = new GraphServiceClient(
        //        new DelegateAuthenticationProvider(requestMessage =>
        //        {
        //            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //            return Task.CompletedTask;
        //        })
        //    );

        //    // Prepare the event
        //    var @event = new Event
        //    {
        //        Subject = meetingRequest.Subject,
        //        Body = new ItemBody
        //        {
        //            ContentType = BodyType.Html,
        //            Content = meetingRequest.Body ?? "Scheduled via API"
        //        },
        //        Start = new DateTimeTimeZone
        //        {
        //            DateTime = meetingRequest.StartDateTime,
        //            TimeZone = meetingRequest.TimeZone ?? "UTC"
        //        },
        //        End = new DateTimeTimeZone
        //        {
        //            DateTime = meetingRequest.EndDateTime,
        //            TimeZone = meetingRequest.TimeZone ?? "UTC"
        //        },
        //        Location = new Location
        //        {
        //            DisplayName = meetingRequest.Location ?? "Online"
        //        },
        //        Attendees = meetingRequest.AttendeesEmails?.ConvertAll(email => new Attendee
        //        {
        //            EmailAddress = new EmailAddress
        //            {
        //                Address = email,
        //                Name = email
        //            },
        //            Type = AttendeeType.Required
        //        }),
        //        // Optional: Add an online meeting (Teams link)
        //        IsOnlineMeeting = true,
        //        OnlineMeetingProvider = OnlineMeetingProviderType.TeamsForBusiness
        //    };

        //    try
        //    {
        //        // Create event in signed-in user's calendar
        //        var createdEvent = await graphClient.Me.Events
        //            .Request()
        //            .AddAsync(@event);

        //        return Ok(new { EventId = createdEvent.Id, Message = "Meeting scheduled successfully" });
        //    }
        //    catch (ServiceException ex)
        //    {
        //        return StatusCode((int)ex.StatusCode, ex.Message);
        //    }
        //}
        public class MeetingRequest
        {
            public string Subject { get; set; }
            public DateTimeOffset StartTime { get; set; }
            public DateTimeOffset EndTime { get; set; }
            public string TimeZone { get; set; } = "UTC";
        }
    }
    public class MeetingRequest
    {
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
    // Request body class for creating meeting
    public class MeetingRequest1
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string StartDateTime { get; set; }  // e.g. "2025-05-22T15:00:00"
        public string EndDateTime { get; set; }    // e.g. "2025-05-22T16:00:00"
        public string TimeZone { get; set; }       // e.g. "Pacific Standard Time"
        public string Location { get; set; }
        public List<string> AttendeesEmails { get; set; }
    }
    public class MeetingRequest2
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string TimeZone { get; set; }  // e.g. "Pacific Standard Time"
        public string Location { get; set; }
        public List<string> AttendeesEmails { get; set; }
    }

}
