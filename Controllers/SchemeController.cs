using connectBase.Services;
using connectBase.Services.swagger.examples;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace connectBase.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [ApiKey]
    public class SchemeController : ControllerBase
    {

        private ISchemeService _schemeService;

        public SchemeController(ISchemeService schemeService)
        {
            this._schemeService = schemeService;
        }

        /// <summary>
        /// Get scheme progress status
        /// </summary>
        /// 
        /// <response code="200">ok</response>
        /// <response code="500">cant provide status right now</response>
        [HttpGet]
        [Produces("application/json")]
        public IActionResult GetSchemeStatus()
        {
            try
            {
                var status = this._schemeService.GetCurrentBuildingStatus();
                return Content(status, "application/json");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        /// <summary>
        /// Get all büro+ table names
        /// </summary>
        /// 
        /// <response code="200">ok</response>
        /// <response code="500">cant provide table names right now</response>
        [HttpGet]
        [Produces("application/json")]
        [Route("Tables")]
        public IActionResult GetAllTableNames()
        {
            try
            {
                var names = this._schemeService.GetAllTableNames();
                return Content(names, "application/json");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        /// <summary>
        /// Get büro+ table scheme
        /// </summary>
        /// 
        /// <response code="200">ok</response>
        /// <response code="500">cant provide sheme right now</response>
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SchemeExample), 200)]
        [ProducesResponseType(500)]
        [Route("{table}")]
        public IActionResult GetDatabaseScheme([FromRoute] string table)
        {
            try
            {
                var scheme = this._schemeService.GetDatabaseScheme(table);
                return Content(scheme.ToString(), "application/json");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}
