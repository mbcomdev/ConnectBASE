using System;
using connectBase.Entities;
using connectBase.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace connectBase.Controllers
{
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    public class LoginController : ControllerBase
    {
        private IUserService _userService;

        public LoginController(IUserService userService)
        {
            this._userService = userService;
        }

        /// <summary>
        /// Login to Büro+
        /// Mandant is not required because it´s automatically set in the AppSettings!
        /// </summary>   
        /// /// <remarks>
        /// Sample request:
        /// 
        ///     POST {$HOST}/api/v1/Login
        ///     Content-Type: application/json
        ///     {
        ///         "username": "connect",
        ///         "password": "string",
        ///     }
        /// </remarks>
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Authenticate([FromBody] User model)
        {
            try
            {
                var authentificationToken = this._userService.AuthenthicateUser(model);
                return Ok(authentificationToken);
            }
            catch (Exception e)
            {
                return BadRequest("Error: " + e.Message);
            }
        }
    }
}
