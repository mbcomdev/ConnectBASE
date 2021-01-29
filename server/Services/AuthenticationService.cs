using connectBase.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace connectBase.Services.COM
{
    public interface IAuthenticationService
    {
        User GetAuthenticationCredentials(string authHeader);
    }
    public class AuthenticationService : IAuthenticationService
    {
        public User GetAuthenticationCredentials(string authHeader)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var authorizationHeader = authHeader.ToString().Split();
            var token = tokenHandler.ReadToken(authorizationHeader[1]) as JwtSecurityToken;
            var user = new User();
            user.Username = token.Claims.FirstOrDefault(claim => claim.Type == "unique_name").Value;
            user.Mandant = token.Claims.FirstOrDefault(claim => claim.Type == "mandant").Value;
            return user;
        }
    }
}
