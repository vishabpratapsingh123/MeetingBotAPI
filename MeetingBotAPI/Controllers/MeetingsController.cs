using MeetingBotAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace MeetingBotAPI.Controllers
{

    [ApiController]
    [Route("api/meetings")]
    public class MeetingsController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        private readonly GraphService _graphService;

        public MeetingsController(GraphService graphService)
        {
            _graphService = graphService;
        }

        [HttpPost("schedule")]
        public async Task<IActionResult> ScheduleMeeting([FromBody] MeetingRequest request)
        {
            var meetingUrl = await _graphService.ScheduleMeetingAsync(request.Title, request.StartTime, request.EndTime);
            return Ok(new { MeetingUrl = meetingUrl });
        }



    }
    public class MeetingRequest
    {
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
