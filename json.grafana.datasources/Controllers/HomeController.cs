using Microsoft.AspNetCore.Mvc;

namespace Json.Grafana.DataSources.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Metadata.Ecma335;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [ApiController]
    public class HomeController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        [Route("")]
        public ActionResult<string> Get()
        {
            return "Hack the planet!";
        }

        // GET api/values
        [Produces("application/json")]
        [Route("search")]
        [HttpPost]
        public ActionResult<IEnumerable<string>> Search()
        {
            // Set a variable to the Documents path.
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            docPath = docPath + "\\GrafanaJson";
            Console.WriteLine(docPath);

            var dirPrograms = new DirectoryInfo(docPath);

            var dirs = dirPrograms.EnumerateDirectories()
                .Where(dir => dir.Name.EndsWith("_daily"))
                .Select(dir => dir.Name);


            return new ActionResult<IEnumerable<string>>(dirs);
        }

        [Produces("application/json")]
        [Route("annotations")]
        [HttpPost]
        public ActionResult<IEnumerable<TimeSerie>> Annotations([FromBody] dynamic value)
        {
            return null;
        }

        [Produces("application/json")]
        [Route("query")]
        [HttpPost]
        public ActionResult<IEnumerable<TimeSerie>> Query([FromBody]dynamic value)
        {
            dynamic data = value;
            var response = new List<TimeSerie>();
            if (data.targets[0].target == null)
            {
                // alle dirs doen
                string docPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                docPath = docPath + "\\GrafanaJson";
                var dirPrograms = new DirectoryInfo(docPath);

                foreach (var enumerateDirectory in dirPrograms.EnumerateDirectories())
                {
                    if (Directory.Exists(enumerateDirectory.FullName))
                    {
                        response.Add(GetTimeSerie(enumerateDirectory.Name.Split("_daily")[0], enumerateDirectory.FullName));
                    }
                }
            }
            else
            {
                foreach (var target in data.targets)
                {
                    // Set a variable to the Documents path.
                    string docPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string name = target.target;
                    name = name.Split("_daily")[0];
                    docPath = docPath + "\\GrafanaJson\\" + target.target;
                    if (Directory.Exists(docPath))
                    {
                        response.Add(GetTimeSerie(name, docPath));
                    }
                }
            }
            return new ActionResult<IEnumerable<TimeSerie>>(response);
        }

        private TimeSerie GetTimeSerie(string name, string dir)
        {
            Console.WriteLine(dir);
            var dirPrograms = new DirectoryInfo(dir);
            var floatList = new List<float[]>();
            foreach (var enumerateDirectory in dirPrograms.EnumerateDirectories())
            {
                var dateStrings = enumerateDirectory.Name.Split("-");
                var dateData = new DateTime(Int32.Parse(dateStrings[0]), Int32.Parse(dateStrings[1]),
                    Int32.Parse(dateStrings[2]));
                float unixTimestamp = (Int32)(dateData.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                using (StreamReader r = new StreamReader($"{enumerateDirectory.FullName}\\data.json"))
                {
                    string json = r.ReadToEnd();
                    List<dynamic> items = JsonConvert.DeserializeObject<List<dynamic>>(json);
                    floatList.Add(new float[] { items.Count, unixTimestamp * 1000 });
                }
            }

            return new TimeSerie
                {Target = name, Datapoints = floatList};
        }
    }
}
