using System;
using connectBase.Services;
using connectBase.Services.COM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using connectBase.Services.Scheduler;

namespace connectBase.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/")]
    public class DeleteController : ControllerBase
    {

        private IDeleteService DeleteService { get; }
        private IAuthenticationService AuthenticationService { get; }
        private IValidationService ValidationService { get; }
        private ISchedulerService SchedulerService { get; }


        public DeleteController(IDeleteService deleteService, IValidationService validationService, IAuthenticationService authenticationService, ISchedulerService schedulerService)
        {
            DeleteService = deleteService;
            AuthenticationService = authenticationService;
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
        ///     Authorization: Bearer {$JWT_TOKEN}
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
            var user = AuthenticationService.GetAuthenticationCredentials(Request.Headers["Authorization"]);
            try
            {
                SchedulerService.IsLocked(user);
                DeleteService.DeleteAll(user, table, index);
                SchedulerService.Release(user);

                return Ok("Dataset was successfully deleted");
            }
            catch (Exception e)
            {
                if (!(e is UserIsLockedException))
                {
                    SchedulerService.Release(user);
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
        ///     Authorization: Bearer {$JWT_TOKEN}
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
            var user = AuthenticationService.GetAuthenticationCredentials(Request.Headers["Authorization"]);
            try
            {
                SchedulerService.IsLocked(user);
                var delete = DeleteService.DeleteDatasetValue(user, table, index, key);
                SchedulerService.Release(user);
                return Ok(delete);
            }
            catch (Exception e)
            {
                if (!(e is UserIsLockedException))
                {
                    SchedulerService.Release(user);
                }
                return BadRequest(e);
            }
        }
    }
}
