using MeetingBotAPI.Interface;
using MeetingBotAPI.Models;
using MeetingBotAPI.Service;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Amazon.Runtime;
using System.Web;

namespace MeetingBotAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class LoginController : Controller
    {

        public IConfiguration _configuration;
        private readonly Employee _employee;
        //private readonly SessionContext _sessionContext;
        //public readonly AirportBandContext _airportBandContext;
        //public readonly CountryContext _countryContext;
        private readonly IEmployee _Employee;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;


        public LoginController(IConfiguration _configuration, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            this._configuration = _configuration;
            _employee = new Employee(_configuration);
            _httpClientFactory = httpClientFactory;
            _config = config;


        }

        [HttpGet("login")]
        public IActionResult Login1()
        {
            var redirectUrl = Url.Action(nameof(Callback), "MicrosoftAuth");
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("callback")]
        public IActionResult Callback()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                return Ok(new { message = "Login successful", email });
            }
            return Unauthorized();
        }

        #region Login
        [AllowAnonymous]
        [HttpPost]
        [Route("CheckUserCredentials")]
        public IActionResult CheckUserCredentials(Login login)
        {
            try
            {
                string Message = "";
                int remainingAttempt = 10;
                bool IsUserBlocked = false;
                var jDataAuthenticate = new
                {
                    isError = false,
                    message = "Check Credentials!",
                    token = "",
                    firstname = "",
                    lastname = "",
                    employeeid = "",
                    emaiId = "",
                    DesignationId = "",
                    CurrentSession = "",
                    ISACTIVEFORTHISSESSION = false,
                    RemainingAttempt = 0
                };
                var jdata = new
                {
                    isError = true,
                    message = "",
                    RemainingAttempt = 10,
                };

                DataTable UserCredentialList = _employee.checkUserLogin(login.userid);
                List<DataRow> list = UserCredentialList.AsEnumerable().ToList();
                var EmployeeList = list.Count;
                DataTable dtUserDetails = new DataTable();



                if (EmployeeList != 0)
                {
                    var tokenstring = "";
                    DateTime CurrentDatetime = DateTime.UtcNow;
                    string DecryptPassword = Decrypt(UserCredentialList.Rows[0]["UserPassword"].ToString());
                    bool isauthenticated = false;
                    bool isBase64String = IsBase64String(login.password);
                    if (isBase64String)
                    {
                        DecryptPassword = "InvalidCharacter";
                    }

                    if (DecryptPassword == "InvalidCharacter")
                    {
                        if (UserCredentialList.Rows[0]["EMAILID"].ToString() == login.userid && UserCredentialList.Rows[0]["UserPassword"].ToString() == login.password)
                        {
                            isauthenticated = true;
                        }
                    }
                    else
                    {
                        if (UserCredentialList.Rows[0]["EMAILID"].ToString() == login.userid && DecryptPassword == login.password)
                        {
                            isauthenticated = true;
                        }
                    }

                    int NoOfAttempt = UserCredentialList.Rows[0].Field<int?>("NoOfAttempt") ?? 0;

                    if (NoOfAttempt == 3 || NoOfAttempt == 10)
                    {
                        DateTime blockedDateTime = Convert.ToDateTime(UserCredentialList.Rows[0]["BlockedDatetime"]);
                        IsUserBlocked = GetIsUserBlocked(blockedDateTime, NoOfAttempt, ref Message, ref remainingAttempt);
                    }


                    if (IsUserBlocked)
                    {
                        jdata = new
                        {
                            isError = true,
                            message = Message,
                            RemainingAttempt = 0,
                        };
                        return Ok(jdata);
                    }
                    else
                    {
                        dtUserDetails = _employee.CheckUpdateUserBlocked(UserCredentialList.Rows[0]["EMPLOYEEID"].ToIntFromNull(), isauthenticated);
                        NoOfAttempt = dtUserDetails.Rows[0].Field<int?>("NoOfAttempt") ?? 0;
                        if (isauthenticated == true)
                        {
                            string emaiId = list[0]["EMAILID"].ToString();
                            string UserName = list[0]["FIRSTNAME"].ToString() + " " + list[0]["LASTNAME"].ToString();
                            tokenstring = GenerateJSONWebToken();
                            jDataAuthenticate = new
                            {
                                isError = false,
                                message = "Check Credentials!",
                                token = tokenstring,
                                firstname = list[0]["FIRSTNAME"].ToString(),
                                lastname = list[0]["LASTNAME"].ToString(),
                                employeeid = list[0]["EMPLOYEEID"].ToString(),
                                emaiId = list[0]["EMAILID"].ToString(),
                                DesignationId = list[0]["DESIGNATIONID"].ToString(),
                                CurrentSession = list[0]["CURRENTSESSION"].ToString(),
                                ISACTIVEFORTHISSESSION = list[0]["ISACTIVEFORTHISSESSION"].ToboolFromNull(),
                                RemainingAttempt = 0
                            };
                            return Ok(jDataAuthenticate);

                        }
                        else
                        {
                            var MessageInvalidUser = "Incorrect Username or Password";
                            remainingAttempt = 0;
                            if (NoOfAttempt == 3)
                            {
                                MessageInvalidUser = "Your session is blocked for 2 minutes";
                            }
                            else if (NoOfAttempt == 10)
                            {
                                MessageInvalidUser = "Your session is blocked for 1 hour. Please contact the Verifavia IT Team";
                            }
                            else
                            {
                                if (NoOfAttempt < 3)
                                {
                                    remainingAttempt = 3 - NoOfAttempt;
                                }
                                else if (NoOfAttempt < 10)
                                {
                                    remainingAttempt = 10 - NoOfAttempt;
                                }

                            }

                            jdata = new
                            {
                                isError = true,
                                message = MessageInvalidUser,
                                RemainingAttempt = remainingAttempt,
                            };
                            return Ok(jdata);
                        }
                    }
                }
                else
                {
                    jdata = new
                    {
                        isError = true,
                        message = "User not found",
                        RemainingAttempt = remainingAttempt,
                    };
                    return Ok(jdata);

                }
            }


            catch (Exception ex)
            {
                var jdata = new
                {
                    isError = true,
                    message = ex.Message
                };
                return Ok(jdata);
            }
        }

        #endregion

        private bool IsBase64String(string password)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(password);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "verifavia-shipping";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(string cipherText)
        {
            try
            {
                string EncryptionKey = "verifavia-shipping";
                cipherText = cipherText.Replace(" ", "+");
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }

            }
            catch (FormatException ex)
            {
                cipherText = "InvalidCharacter";
                return cipherText;
            }

            return cipherText;
        }


        private string GenerateJSONWebToken()
        {
            //var securityKey = new SymmetricSecurityKey(Encoding.UTF7.GetBytes(_configuration["Jwt:Key"]));
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
              _configuration["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddMinutes(480),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private bool GetIsUserBlocked(DateTime BlockedDatetime, int NoOfAttempt, ref string message, ref int RemainingAttempt)
        {
            Boolean IsBlocked = false;
            TimeSpan difference = DateTime.UtcNow - BlockedDatetime;
            var dif = difference.TotalMinutes;
            if (NoOfAttempt == 3 && difference.TotalSeconds < 120)
            {
                message = $"Your session was blocked for 2 minutes, {120 - Math.Floor(difference.TotalSeconds)} seconds left";
                IsBlocked = true;
            }
            else if (NoOfAttempt == 10 && difference.TotalMinutes < 60)
            {
                if (difference.TotalMinutes < 58)
                {

                    message = $"Your session was blocked for 1 hour, {Math.Ceiling(60 - difference.TotalMinutes)} minutes left. Please contact the Verifavia IT Team";
                    IsBlocked = true;
                }
                else
                {
                    message = $"Your session was blocked for 1 hour, {Math.Ceiling(3600 - difference.TotalSeconds)} seconds left. Please contact the Verifavia IT Team";
                    IsBlocked = true;
                }
            }

            return IsBlocked;
        }

        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<IActionResult> GetToken([FromBody] TokenRequestDto dto)
        {
            var client = _httpClientFactory.CreateClient();
            var values = new Dictionary<string, string>
        {
            { "client_id", _config["Authentication:Microsoft:ClientId"] },
            { "grant_type", "authorization_code" },
            { "code", dto.AuthorizationCode },
            { "redirect_uri", dto.RedirectUri },
            { "code_verifier", dto.CodeVerifier }
        };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(
                $"https://login.microsoftonline.com/{_config["Authentication:Microsoft:TenantId"]}/oauth2/v2.0/token", content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return BadRequest(responseContent);

            return Ok(JsonSerializer.Deserialize<JsonElement>(responseContent));
        }
        [HttpGet("loginAUT")]
        [AllowAnonymous]

        public IActionResult Login()
        {
            var clientId = _config["Authentication:Microsoft:ClientId"];
            var tenantId = _config["Authentication:Microsoft:TenantId"]; // Or use "common"
            var redirectUri = "https://localhost:7198/swagger/oauth2-redirect.html"; // Must match Azure

            var scope = "openid profile email offline_access";
            var responseType = "code";
            var responseMode = "query";
            var state = Guid.NewGuid().ToString();

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["client_id"] = clientId;
            query["response_type"] = responseType;
            query["redirect_uri"] = redirectUri;
            query["response_mode"] = responseMode;
            query["scope"] = scope;
            query["state"] = state;

            //var url = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?{query}";
            var url = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?" +
              $"client_id={clientId}&response_type={responseType}&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
              $"&response_mode={responseMode}&scope={Uri.EscapeDataString(scope)}&state={state}";

            return Redirect(url);
        }
        [HttpGet("/auth/callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            var client = new HttpClient();
            var tokenEndpoint = "https://login.microsoftonline.com/YOUR_TENANT_ID/oauth2/v2.0/token";

            var parameters = new Dictionary<string, string>
    {
        {"client_id", _config["Authentication:Microsoft:ClientId"]},
        {"scope", "openid profile email offline_access"},
        {"code", code},
        {"redirect_uri", "https://localhost:7198/swagger/oauth2-redirect.html"},
        {"grant_type", "authorization_code"},
        {"client_secret", _config["Authentication:Microsoft:ClientSecret"]}
    };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(tokenEndpoint, content);
            var token = await response.Content.ReadAsStringAsync();

            return Ok(token); // You can parse and return access_token if needed
        }

    }
}
