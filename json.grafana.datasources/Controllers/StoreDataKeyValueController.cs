namespace Json.Grafana.DataSources.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using Logic;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
            var response = new GetInfo { Name = name };
            string docPath = pathServices.GetNamePath(name);
            if (Directory.Exists(docPath))
            {
                response.Table = FileHelper.GetJson<List<GetInfoTableJsonColumn>>(Path.Combine(docPath, "table.json"));
                response.Info = FileHelper.GetJson<GetInfoJson>(Path.Combine(docPath, "info.json"));
                return response;
            }
            return null;
        }

        [Route("set_info")]
        [HttpPost]
        public IActionResult SetInfo([FromBody] GetInfo value)
        {
            var docPath = pathServices.GetNamePath(value.Name, null, true);
            FileHelper.SaveJson(Path.Combine(docPath, "table.json"), value.Table, Formatting.Indented);
            FileHelper.SaveJson(Path.Combine(docPath, "info.json"), value.Info, Formatting.Indented);
            return StatusCode((int)HttpStatusCode.OK);
        }

        [Route("send_data")]
        [HttpPost]
        public IActionResult SendData([FromBody] dynamic value)
        {
            try
            {
                string fullPath = pathServices.CreateToDayPath(value.name.Value, value.subject.Value, true);
                // controle op geldige json
                JsonConvert.DeserializeObject<JObject>(value.json_data.ToString());
                FileHelper.SaveJson(Path.Combine(fullPath, "data.json"), value.json_data);
                return StatusCode((int)HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
