using System;
using System.Collections.Generic;
using connectBase.Services;
using connectBase.Services.COM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using connectBase.Services.Util;
using connectBase.Services.Scheduler;

namespace connectBase.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/")]
    public class PutController : ControllerBase
    {
        private IAuthenticationService AuthenticationService { get; }
        private IPutService PutService { get; }
        private IValidationService ValidationService { get; }
        private ISchedulerService SchedulerService { get; }

        public PutController(IPutService putService, IAuthenticationService authenticationService, IValidationService validationService, ISchedulerService schedulerService)
        {
            PutService = putService;
            AuthenticationService = authenticationService;
            ValidationService = validationService;
            SchedulerService = schedulerService;
        }

        /// <summary>
        /// modify top level Table will use key values from body
        /// </summary>
        /// /// <remarks>
        /// Sample requests:
        /// 
        ///     ### put index ID
        ///     PUT {$HOST}/api/v1/Artikel/ID
        ///     Authorization: Bearer {$JWT_TOKEN}
        ///     Content-Type: application/json
        ///
        ///     {
        ///        "ID":"1503",
        ///        "KuBez1":"I was updated with index ID (ID was used as key)",
        ///        "ArtNr": "test55"
        ///     }
        ///     ### put index NR
        ///     PUT {$HOST}/api/v1/Artikel/Nr
        ///     Authorization: Bearer {$JWT_TOKEN}
        ///     Content-Type: application/json
        ///
        ///     {
        ///        "ID":"1503",
        ///        "KuBez1":"I was updated with index Nr (ArtNr was used as key)",
        ///        "ArtNr": "test55"
        ///     }
        /// </remarks>
        /// <response code="200">Ok</response>
        [HttpPut]
        [Route("{table}/{index}")]
        public IActionResult PutTopLevel([FromBody] Dictionary<string, object> mvcData, string table, string index)
        {
            var user = AuthenticationService.GetAuthenticationCredentials(Request.Headers["Authorization"]);
            try
            {
                SchedulerService.IsLocked(user);
                Dictionary<string, object> data = Util.ConvertMVCDirectory(mvcData);
                ValidationService.Validate(data, table).Wait();
                SchedulerService.Release(user);
                return Ok(PutService.PutTopLevelTable(user, table, index, data));
            }
            catch (Exception e)

            {
                if (!(e is UserIsLockedException))
                {
                    SchedulerService.Release(user);
                }
                return BadRequest("Error: " + e.Message);
            }
        }
    }
}
