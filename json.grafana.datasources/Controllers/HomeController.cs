using Microsoft.AspNetCore.Mvc;

namespace Json.Grafana.DataSources.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection.Metadata.Ecma335;
    using System.Runtime.CompilerServices;
    using Microsoft.CSharp.RuntimeBinder;
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

        public static dynamic BooleanNullToFalse(dynamic value)
        {
            if (value is bool)
                return false;
            return value;
        }

        [Produces("application/json")]
        [Route("query")]
        [HttpPost]
        public ActionResult<dynamic> Query([FromBody]dynamic value)
        {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            docPath = docPath + "\\GrafanaJson";
            dynamic data = value;
            if (data.targets[0].type == "table")
            {
                var dir = $"{docPath}\\{data.targets[0].target}";
                var table = new Table { Type = "table", Rows = new List<dynamic>(), Columns = new List<dynamic>()};
                dynamic timeColum = new System.Dynamic.ExpandoObject();
                timeColum.text = "Time";
                timeColum.type = "time";
                timeColum.description = "Time";
                table.Columns.Add(timeColum);
                using (StreamReader r = new StreamReader($"{dir}\\table.json"))
                {
                    string json = r.ReadToEnd();
                    List<dynamic> columns = JsonConvert.DeserializeObject<List<dynamic>>(json);
                    foreach (var column in columns)
                    {
                        if (column.type == "bool")
                        {
                            dynamic boolColumn = new System.Dynamic.ExpandoObject();
                            boolColumn.text = column.text;
                            boolColumn.type = "number";
                            boolColumn.description = column.description;
                            table.Columns.Add(boolColumn);
                        }
                        else
                        {
                            table.Columns.Add(column);
                        }
                        
                    }
                    
                }

                var dirPrograms = new DirectoryInfo(dir);
                // laatste gegevens alleen weergeven in table
                var enumerateDirectory = dirPrograms.EnumerateDirectories().OrderByDescending(b => b.Name).First();
                var dateStrings = enumerateDirectory.Name.Split("-");
                var dateData = new DateTime(Int32.Parse(dateStrings[0]), Int32.Parse(dateStrings[1]),
                    Int32.Parse(dateStrings[2]));
                float unixTimestamp = (Int32)(dateData.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                using (StreamReader r = new StreamReader($"{enumerateDirectory.FullName}\\data.json"))
                {
                    string json = r.ReadToEnd();
                    List<JObject> items = JsonConvert.DeserializeObject<List<JObject>>(json);
                    foreach (var item in items)
                    {
                        var values = new List<dynamic>();
                        foreach (var tableColumn in table.Columns)
                        {
                            if (tableColumn.text == "Time")
                            {
                                values.Add(unixTimestamp * 1000);
                            }
                            else
                            {
                                var test = item.GetValue((tableColumn.text).Value);
                                if (test.Value is bool b)
                                {
                                    values.Add(b ? 1 : 0);
                                }
                                else
                                {
                                    values.Add(test.Value);
                                }
                            }
                        }
                        table.Rows.Add(values);
                    }
                }
                return new ActionResult<dynamic>(table);
            }
            else
            {
                var response = new List<TimeSerie>();
                if (data.targets[0].target == null)
                {
                    // alle dirs doen
                    
                    
                    var dirPrograms = new DirectoryInfo(docPath);

                    foreach (var enumerateDirectory in dirPrograms.EnumerateDirectories())
                    {
                        if (Directory.Exists(enumerateDirectory.FullName))
                        {
                            response.Add(GetTimeSerie(GetName(enumerateDirectory.FullName), enumerateDirectory.FullName));
                        }
                        
                    }
                }
                else
                {
                    foreach (var target in data.targets)
                    {
                        string name = target.target;
                        name = name.Split("_daily")[0];
                        docPath = docPath + "\\" + target.target;
                        if (Directory.Exists(docPath))
                        {
                            response.Add(GetTimeSerie(GetName(docPath), docPath));
                        }
                    }
                }
                return new ActionResult<dynamic>(response);
            }

            
        }

        private string GetName(string dir)
        {
            using (StreamReader r = new StreamReader($"{dir}\\info.json"))
            {
                string json = r.ReadToEnd();
                dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
                return data.Name;
            }
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
