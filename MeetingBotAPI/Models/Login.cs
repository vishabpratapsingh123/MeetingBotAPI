namespace MeetingBotAPI.Models
{
    public class Login
    {
        public string userid { get; set; }
        public string password { get; set; }
    }
    public class TokenRequestDto
    {
        public string AuthorizationCode { get; set; }
        public string RedirectUri { get; set; }
        public string CodeVerifier { get; set; }
    }

}
