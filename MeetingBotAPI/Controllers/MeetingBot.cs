//namespace MeetingBotAPI
//{
//    public class MeetingBot
//    {
//    }
//}

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

public class MeetingBot : ActivityHandler
{
    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        var message = turnContext.Activity.Text.ToLower();

        if (message.Contains("schedule meeting"))
        {
            var meetingUrl = await ScheduleMeeting();
            await turnContext.SendActivityAsync($"Meeting Scheduled: {meetingUrl}", cancellationToken: cancellationToken);
        }
    }

    private async Task<string> ScheduleMeeting()
    {
        // Call Microsoft Graph API (implemented in Step 5)
        return "https://teams.microsoft.com/meeting-id";
    }
}
