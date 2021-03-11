using connectBase.Services;
using connectBase.Services.Scheduler;
using connectBase.Services.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;


namespace connectBase.Controllers
{
    [ApiController]
    [Route("api/v1/")]
    [ApiKey]
    public class PostController : ControllerBase
    {
        private IPostService PostService { get; }
        private IValidationService ValidationService { get; }
        private ISchedulerService SchedulerService { get; }


        public PostController(IPostService postService,
            IValidationService validationService, ISchedulerService schedulerService)
        {
            PostService = postService;
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
        ///     x-api-key: {$API-Key}
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
            Dictionary<string, object> convertedData = Util.ConvertMVCDirectory(data);
            try
            {
                SchedulerService.IsLocked();
                ValidationService.Validate(convertedData, table).Wait();
                PostService.Post(table, convertedData);
                SchedulerService.Release();
                return Ok();
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
        /// Write into nested table
        /// </summary>
        ///  /// <remarks>
        /// Sample requests:
        /// 
        ///     POST {$HOST}/api/v1/Artikel/Nested/Ums
        ///     x-api-key: {$API-Key}
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
            Dictionary<string, object> convertedData = Util.ConvertMVCDirectory(data);
            try
            {
                SchedulerService.IsLocked();
                ValidationService.Validate(convertedData, table).Wait();
                PostService.Post(table, nestedTable, data);
                SchedulerService.Release();
                return Ok();
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
