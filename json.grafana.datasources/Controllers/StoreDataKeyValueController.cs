namespace Json.Grafana.DataSources.Controllers
{
    using System.Net;
    using Logic;
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [Route("storedatakeyvalue")]
    [ApiController]
    public class StoreDataKeyValueController : ControllerBase
    {
        private readonly IPathServices pathServices;

        public StoreDataKeyValueController(IPathServices pathServices)
        {
            this.pathServices = pathServices;
        }

        [Produces("application/json")]
        [HttpGet]
        [Route("{name}")]
        public ActionResult<GetInfo> Get(string name)
        {
            return null;
        }

        [Route("set_info")]
        [HttpPost]
        public IActionResult SetInfo([FromBody] GetInfo value)
        {
            return StatusCode((int)HttpStatusCode.OK);
        }

        [Route("send_data")]
        [HttpPost]
        public IActionResult SendData([FromBody] dynamic value)
        {
            return StatusCode((int)HttpStatusCode.OK);
        }
    }
}
