using System;
using connectBase.Services;
using connectBase.Services.COM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Newtonsoft.Json;
using connectBase.Services.Scheduler;

namespace connectBase.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/")]
    public class GetController : ControllerBase
    {
        private IGetService GetService { get; }
        private IAuthenticationService AuthenticationService { get; }
        private ISchedulerService SchedulerService { get; }

        public GetController(IGetService getService, IAuthenticationService authenticationService, ISchedulerService schedulerService)
        {
            GetService = getService;
            AuthenticationService = authenticationService;
            SchedulerService = schedulerService;
        }

        /// <summary>
        /// Get all data from table
        /// </summary>
        /// 
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET {$HOST}/api/v1/Artikel
        ///     Authorization: Bearer {$JWT_TOKEN}
        /// </remarks>
        /// <response code="200">ok</response>
        /// <response code="500">cant provide data from table</response>
        [HttpGet]
        [Route("{table}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public IActionResult GetAll([FromRoute] string table, [FromQuery] List<string> returnFields)
        {
            var user = AuthenticationService.GetAuthenticationCredentials(Request.Headers["Authorization"]);
            try
            {
                SchedulerService.IsLocked(user);
                var data = GetService.GetAll(user, table, returnFields);
                SchedulerService.Release(user);
                return Content(data.ToString(), "application/json");
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
        /// Get data from range
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET {$HOST}/api/v1/Artikel/ID/FromRange
        ///     Authorization: Bearer {$JWT_TOKEN}
        ///     Content-Type: application/json
        /// </remarks>
        /// <response code="200">ok</response>
        /// <response code="500">cant provide data from table</response>
        [HttpGet]
        [Route("{table}/{index}/FromRange")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public IActionResult GetRange([FromRoute] string table, [FromRoute] string index, [FromQuery] List<string> returnFields)
        {
            var user = AuthenticationService.GetAuthenticationCredentials(Request.Headers["Authorization"]);
            var indexFields = JsonConvert.DeserializeObject<Dictionary<string, string>>((Request.Query)["indexFields"]);
            try
            {
                SchedulerService.IsLocked(user);
                var data = GetService.GetRange(user, table, index, indexFields, returnFields);
                SchedulerService.Release(user);
                return Content(data.ToString(), "application/json");
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
        /// Get all data from a table selected by a indexList
        /// </summary>
        /// 
        /// <response code="200">ok</response>
        /// <response code="500">cant provide data from indexlist</response>
        [HttpGet]
        [Route("{table}/{index}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public IActionResult GetFromIndexList([FromRoute] string table, [FromRoute] string index, [FromQuery] List<string> returnFields)
        {
            var user = AuthenticationService.GetAuthenticationCredentials(Request.Headers["Authorization"]);
            var kvp = (Request.Query)["indexList"];
            try
            {
                SchedulerService.IsLocked(user);
                var data = GetService.GetFromIndexList(user, table, index, kvp, returnFields);
                SchedulerService.Release(user);
                return Content(data.ToString(), "application/json");
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
        /// Get all indices from table
        /// </summary>
        /// 
        /// <response code="200">ok</response>
        /// <response code="500">cant provide data from index</response>
        [HttpGet]
        [Route("{table}/IndexList")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public IActionResult GetIndexList([FromRoute] string table)
        {
            var user = AuthenticationService.GetAuthenticationCredentials(Request.Headers["Authorization"]);
            try
            {
                SchedulerService.IsLocked(user);
                var data = GetService.GetIndexList(user, table);
                SchedulerService.Release(user);
                return Content(data.ToString(), "application/json");
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
        /// Get all data from nestedTable
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET {$HOST}/api/v1/Artikel/Nested/Ums
        ///     Authorization: Bearer {$JWT_TOKEN}
        ///     Content-Type: application/json
        /// </remarks>
        /// <response code="200">ok</response>
        /// <response code="500">cant provide data from table</response>
        [HttpGet]
        [Route("{table}/Nested/{nestedTable}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public IActionResult GetFromNestedTable([FromRoute] string table, [FromRoute] string nestedTable, [FromQuery] List<string> returnFields)
        {
            var user = AuthenticationService.GetAuthenticationCredentials(Request.Headers["Authorization"]);
            try
            {
                SchedulerService.IsLocked(user);
                var data = GetService.GetFromNestedTable(user, table, nestedTable, returnFields);
                SchedulerService.Release(user);
                return Content(data.ToString(), "application/json");
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
