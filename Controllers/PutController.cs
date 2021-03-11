using connectBase.Services;
using connectBase.Services.COM;
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
    public class PutController : ControllerBase
    {
        private IPutService PutService { get; }
        private IValidationService ValidationService { get; }
        private ISchedulerService SchedulerService { get; }

        public PutController(IPutService putService, IValidationService validationService, ISchedulerService schedulerService)
        {
            PutService = putService;
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
        ///     x-api-key: {$API-Key}
        ///     Content-Type: application/json
        ///
        ///     {
        ///        "ID":"1503",
        ///        "KuBez1":"I was updated with index ID (ID was used as key)",
        ///        "ArtNr": "test55"
        ///     }
        ///     ### put index NR
        ///     PUT {$HOST}/api/v1/Artikel/Nr
        ///     x-api-key: {$API-Key}
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
            try
            {
                SchedulerService.IsLocked();
                Dictionary<string, object> data = Util.ConvertMVCDirectory(mvcData);
                ValidationService.Validate(data, table).Wait();
                SchedulerService.Release();
                return Ok(PutService.PutTopLevelTable(table, index, data));
            }
            catch (Exception e)

            {
                if (!(e is UserIsLockedException))
                {
                    SchedulerService.Release();
                }
                return BadRequest("Error: " + e.Message);
            }
        }
    }
}
