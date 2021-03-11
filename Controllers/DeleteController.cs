/*using connectBase.Services;
using connectBase.Services.Scheduler;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace connectBase.Controllers
{
    [ApiController]
    [Route("api/v1/")]
    [ApiKey]
    public class DeleteController : ControllerBase
    {

        private IDeleteService DeleteService { get; }
        private IValidationService ValidationService { get; }
        private ISchedulerService SchedulerService { get; }


        public DeleteController(IDeleteService deleteService, IValidationService validationService, ISchedulerService schedulerService)
        {
            DeleteService = deleteService;
            ValidationService = validationService;
            SchedulerService = schedulerService;
        }

        /// <summary>
        /// delete dataset
        /// </summary>
        /// 
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE {$HOST}/api/v1/Artikel/ID
        ///     x-api-key: {$API-Key}
        ///     Content-Type: application/json
        /// </remarks>
        /// <response code="200">ok</response>
        /// <response code="500">cant delete dataset</response>
        [HttpDelete]
        [Route("{table}/{index}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public IActionResult DeleteAll([FromRoute] string table, [FromRoute] string index)
        {
            try
            {
                SchedulerService.IsLocked();
                DeleteService.DeleteAll(table, index);
                SchedulerService.Release();

                return Ok("Dataset was successfully deleted");
            }
            catch (Exception e)
            {
                if (!(e is UserIsLockedException))
                {
                    SchedulerService.Release();
                }
                return BadRequest(e);
            }
        }

        /// <summary>
        /// delete one value of dataset
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE {$HOST}/api/v1/Artikel/ID/99
        ///     x-api-key: {$API-Key}
        ///     Content-Type: application/json
        /// </remarks>
        /// <response code="200">ok</response>
        /// <response code="500">cant delete value</response>
        [HttpDelete]
        [Route("{table}/{index}/{key}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public IActionResult DeleteDatasetValue([FromRoute] string table, [FromRoute] string index, [FromRoute] string key)
        {
            try
            {
                SchedulerService.IsLocked();
                var delete = DeleteService.DeleteDatasetValue(table, index, key);
                SchedulerService.Release();
                return Ok(delete);
            }
            catch (Exception e)
            {
                if (!(e is UserIsLockedException))
                {
                    SchedulerService.Release();
                }
                return BadRequest(e);
            }
        }
    }
}
*/