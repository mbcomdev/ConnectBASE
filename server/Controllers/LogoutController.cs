using System;
using connectBase.Services;
using connectBase.Services.COM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace connectBase.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LogoutController : ControllerBase
    {

        private IUserService _userService;
        private IAuthenticationService _authenticationService;

        public LogoutController(IUserService userService, IAuthenticationService authenticationService)
        {
            this._userService = userService;
            this._authenticationService = authenticationService;
        }

        /// <summary>
        /// Logout Büro+
        /// </summary>
        /// 
        ///  <remarks>
        /// Sample request:
        /// 
        ///     GET {$HOST}/api/v1/Artikel/ID/99
        ///     Authorization: Bearer {$TOKEN}
        /// </remarks>
        /// <response code="200">Ok</response>
        [HttpGet]
        public IActionResult Logout()
        {
            try
            {
                var user = this._authenticationService.GetAuthenticationCredentials(Request.Headers["Authorization"]);

                this._userService.LogoutUser(user.Username, user.Mandant);
                return Ok("User successfully logged out");
            }
            catch (Exception e)
            {
                return BadRequest("Error: " + e.Message);
            }
        }
    }
}
