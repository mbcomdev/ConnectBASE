using connectBase.Entities;
using connectBase.Helper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace connectBase.Services
{

    public interface IUserService
    {
        string AuthenthicateUser(User model);
        void LogoutUser(string username, string mandant);
    }
    public class UserService : IUserService
    {
        private COMConnection _COMConnection { get; }
        private readonly AppSettings _appSettings;
        private ISchemeService _schemeService;
        private IValidationService _validationService;
        private ILogger<UserService> _logger;

        public UserService(
            COMConnection comConnection,
            IOptions<AppSettings> appSettings,
            ISchemeService schemeService,
            IValidationService validationService,
            ILogger<UserService> logger)
        {

            this._schemeService = schemeService;
            this._COMConnection = comConnection;
            this._appSettings = appSettings.Value;
            this._validationService = validationService;
            _logger = logger;
        }

        public void LogoutUser(string username, string mandant)
        {
            this._COMConnection.Logout(username, mandant);
        }

        public string AuthenthicateUser(User user)
        {
            try
            {
                this._COMConnection.Login(user);
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.getSecret());
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(type: "mandant", AppSettings.MANDANT)
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(300),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
                return token;
            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }
}
