using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetingBotAPI.Models; // <-- Assuming your ActionItem model is in this namespace

namespace MeetingBotAPI.Services  // <-- Changed to plural for consistency
{
    public class ActionService
    {
        private readonly ApplicationDbContext _context;

        public ActionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddActionAsync(ActionItem action)
        {
            _context.ActionItems.Add(action);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ActionItem>> GetActionsByMeeting(string meetingId)
        {
            if (!int.TryParse(meetingId, out int meetingIdInt))
            {
                return new List<ActionItem>(); // or throw an exception / handle invalid input
            }

            return await _context.ActionItems
                                 .Where(a => a.MeetingId == meetingIdInt)
                                 .ToListAsync();
        }

    }
}
