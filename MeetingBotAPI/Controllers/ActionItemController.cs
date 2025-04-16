using Microsoft.AspNetCore.Mvc;
using MeetingBotAPI.Services;   // <-- Make sure this namespace is correct!
using System.Threading.Tasks;
using MeetingBotAPI.Models;

namespace MeetingBotAPI.Controllers
{
    [ApiController]
    [Route("api/actions")]
    public class ActionItemController : Controller
    {
        private readonly ActionService _actionService;

        public ActionItemController(ActionService actionService) 
        {
            _actionService = actionService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAction([FromBody] ActionItem action)
        {
            await _actionService.AddActionAsync(action);
            return Ok();
        }

        [HttpGet("{meetingId}")]
        public async Task<IActionResult> GetActions(string meetingId)
        {
            var actions = await _actionService.GetActionsByMeeting(meetingId);
            return Ok(actions);
        }
    }
}
