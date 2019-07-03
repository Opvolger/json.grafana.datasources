namespace Json.Grafana.DataSources.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json;


    [ApiController]
    public class HomeController : ControllerBase
    {
        public static AppSettings Settings;

        // GET api/values
        [HttpGet]
        [Route("")]
        public ActionResult<string> Get()
        {
            return "Hack the planet!";
        }

        [Produces("application/json")]
        [HttpGet]
        [Route("/{id}")]
        public ActionResult<GetInfo> Get(string id)
        {
            var response = new GetInfo(){Name = id};
            string docPath = GetNamePath(id);
            if (Directory.Exists(docPath))
            {
                using (StreamReader r = new StreamReader($"{docPath}\\table.json"))
                {
                    string json = r.ReadToEnd();
                    response.Table = JsonConvert.DeserializeObject<List<GetInfoTableColumn>>(json);
                }
                using (StreamReader r = new StreamReader($"{docPath}\\info.json"))
                {
                    string json = r.ReadToEnd();
                    response.Info = JsonConvert.DeserializeObject<GetInfoInfo>(json);
                }

                return response;

            }
            return null;
        }
        
        [Route("set_info")]
        [HttpPost]
        public IActionResult SetInfo([FromBody] GetInfo value)
        {
            string docPath = GetNamePath(value.Name);
            if (!Directory.Exists(docPath))
            {
                Directory.CreateDirectory(docPath);
            }
            System.IO.File.WriteAllText(Path.Combine(docPath, "table.json"), JsonConvert.SerializeObject(value.Table, Formatting.Indented));
            System.IO.File.WriteAllText(Path.Combine(docPath, "info.json"), JsonConvert.SerializeObject(value.Info, Formatting.Indented));
            return StatusCode((int)HttpStatusCode.OK);
        }

        [Route("send_data")]
        [HttpPost]
        public IActionResult SendData([FromBody] dynamic value)
        {
            try
            {
                if (value.name.ToString().Contains('.') || value.name.ToString().Contains('/'))
                {
                    throw new Exception("invalid name");
                }
                string docPath = GetNamePath(value.name.ToString());
                // create dir met data
                var datetime_dir = DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss");
                if (!Directory.Exists(docPath))
                {
                    Directory.CreateDirectory(docPath);
                }
                var fullPath = Path.Combine(docPath, datetime_dir);
                Directory.CreateDirectory(fullPath);
                dynamic data_is_json_test = JsonConvert.DeserializeObject<List<dynamic>>(value.json_data.ToString());
                System.IO.File.WriteAllText(Path.Combine(fullPath, "data.json"), value.json_data.ToString());
                return StatusCode((int)HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        private static string GetNamePath(string name)
        {
            string docPath = Path.Combine(Settings.DirectoryGrafanaJSON, name);
            return docPath;
        }
    }
}
