using System.Data;

namespace MeetingBotAPI.Interface
{
    public interface IEmployee
    {
        public DataTable checkUserLogin(string userid);
        public DataTable CheckUpdateUserBlocked(int EmployeeId, bool IsAuthenticated);

    }
}
