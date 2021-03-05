using connectBase.Services;
using connectBase.Services.Scheduler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace connectBase.Controllers
{
    [ApiController]
    [Route("api/v1/")]
    [ApiKey]
    public class GetController : ControllerBase
    {
        private IGetService GetService { get; }
        private ISchedulerService SchedulerService { get; }

        public GetController(IGetService getService, ISchedulerService schedulerService)
        {
            GetService = getService;
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
            try
            {
                SchedulerService.IsLocked();
                var data = GetService.GetAll(table, returnFields);
                SchedulerService.Release();
                return Content(data.ToString(), "application/json");
            }
            catch (Exception e)
            {
                if (!(e is UserIsLockedException))
                {
                    SchedulerService.Release();
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
            var indexFields = JsonConvert.DeserializeObject<Dictionary<string, string>>((Request.Query)["indexFields"]);
            try
            {
                SchedulerService.IsLocked();
                var data = GetService.GetRange(table, index, indexFields, returnFields);
                SchedulerService.Release();
                return Content(data.ToString(), "application/json");
            }
            catch (Exception e)
            {
                if (!(e is UserIsLockedException))
                {
                    SchedulerService.Release();
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
            var kvp = (Request.Query)["indexList"];
            try
            {
                SchedulerService.IsLocked();
                var data = GetService.GetFromIndexList(table, index, kvp, returnFields);
                SchedulerService.Release();
                return Content(data.ToString(), "application/json");
            }
            catch (Exception e)
            {
                if (!(e is UserIsLockedException))
                {
                    SchedulerService.Release();
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
            try
            {
                SchedulerService.IsLocked();
                var data = GetService.GetIndexList(table);
                SchedulerService.Release();
                return Content(data.ToString(), "application/json");
            }
            catch (Exception e)
            {
                if (!(e is UserIsLockedException))
                {
                    SchedulerService.Release();
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
            try
            {
                SchedulerService.IsLocked();
                var data = GetService.GetFromNestedTable(table, nestedTable, returnFields);
                SchedulerService.Release();
                return Content(data.ToString(), "application/json");
            }
            catch (Exception e)
            {
                if (!(e is UserIsLockedException))
                {
                    SchedulerService.Release();
                }
                return BadRequest(e.Message);
            }
        }
    }
}
