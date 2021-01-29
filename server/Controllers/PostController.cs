using System;
using System.Collections.Generic;
using connectBase.Entities;
using connectBase.Services;
using connectBase.Services.COM;
using connectBase.Services.Scheduler;
using connectBase.Services.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace connectBase.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/")]
    public class PostController : ControllerBase
    {
        private IPostService PostService { get; }
        private IAuthenticationService AuthenticationService { get; }
        private IValidationService ValidationService { get; }
        private ISchedulerService SchedulerService { get; }


        public PostController(IPostService postService, IAuthenticationService authenticationService, 
            IValidationService validationService, ISchedulerService schedulerService)
        {
            PostService = postService;
            AuthenticationService = authenticationService;
            ValidationService = validationService;
            SchedulerService = schedulerService;
        }

        /// <summary>
        /// Write into table
        /// </summary>
        /// <remarks>
        /// Sample requests:
        /// 
        ///     POST {$HOST}/api/v1/Artikel
        ///     Authorization: Bearer {$JWT_TOKEN}
        ///     Content-Type: application/json
        ///
        ///     {
        ///        "ID":"1503",
        ///        "KuBez1":"I was just created",
        ///        "ArtNr": "test55"
        ///     }
        /// </remarks>
        /// <response code="200">ok</response>
        /// <response code="400">cant write data into table</response>
        [HttpPost]
        [Route("{table}")]
        public IActionResult Post(string table, [FromBody] Dictionary<string, object> data)
        {
            User user = AuthenticationService.GetAuthenticationCredentials(Request.Headers["Authorization"]);
            Dictionary<string, object> convertedData = Util.ConvertMVCDirectory(data);
            try
            {
                SchedulerService.IsLocked(user);
                ValidationService.Validate(convertedData, table).Wait();
                PostService.Post(user, table, convertedData);
                SchedulerService.Release(user);
                return Ok();
            }
            catch (Exception e)
            {
                if (!(e is UserIsLockedException))
                {
                    SchedulerService.Release(user);
                }
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Write into nested table
        /// </summary>
        ///  /// <remarks>
        /// Sample requests:
        /// 
        ///     POST {$HOST}/api/v1/Artikel/Nested/Ums
        ///     Authorization: Bearer {$JWT_TOKEN}
        ///     Content-Type: application/json
        ///
        ///     {
        ///        "Jahr": 2020,
        ///        "FilNr": 99,
        ///        "UmsJan": 10000
        ///     }
        /// </remarks>
        /// <response code="200">ok</response>
        /// <response code="400">cant write data into table</response>
        [HttpPost]
        [Route("{table}/Nested/{nestedTable}")]
        public IActionResult Post(string table, string nestedTable, [FromBody] Dictionary<string, object> data)
        {
            User user = AuthenticationService.GetAuthenticationCredentials(Request.Headers["Authorization"]);
            Dictionary<string, object> convertedData = Util.ConvertMVCDirectory(data);
            try
            {
                SchedulerService.IsLocked(user);
                ValidationService.Validate(convertedData, table).Wait();
                PostService.Post(user, table, nestedTable, data);
                SchedulerService.Release(user);
                return Ok();
            }
            catch (Exception e)
            {
                if (!(e is UserIsLockedException))
                {
                    SchedulerService.Release(user);
                }
                return BadRequest(e.Message);
            }
        }
    }
}
