namespace Json.Grafana.DataSources.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using Logic;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json.Linq;

    [Route("simpeljson")]
    [ApiController]
    public class SimpelJsonController : ControllerBase
    {
        private readonly IPathServices pathServices;

        public SimpelJsonController(IPathServices pathServices)
        {
            this.pathServices = pathServices;
        }

        [HttpGet]
        [Route("")]
        public ActionResult<string> Get()
        {
            return "Hack the planet!";
        }

        [Produces("application/json")]
        [Route("search")]
        [HttpPost]
        public ActionResult<IEnumerable<string>> Search()
        {
            // Set a variable to the Documents path.
            string docPath = pathServices.DirectoryGrafanaJSON;
            Console.WriteLine(docPath);

            var dirPrograms = new DirectoryInfo(docPath);

            var dirs = dirPrograms.EnumerateDirectories()
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

        public static dynamic GetDefaultValueOfDynamic(string type)
        {
            if (type == "bool")
            {
                return 0;
            }

            if (type == "DateTime")
            {
                return GrafanaHelpers.GetTimeGrafana(DateTime.MinValue);
            }


            return string.Empty;
        }

        public static dynamic GetValueOfDynamic(dynamic value)
        {
            if (value.Value is bool b)
            {
                return b ? 1 : 0;
            }

            if (value.Value is DateTime c)
            {
                return GrafanaHelpers.GetTimeGrafana(c);
            }
            return value.Value;
        }


        [Produces("application/json")]
        [Route("query")]
        [HttpPost]
        public ActionResult<dynamic> Query([FromBody] dynamic value)
        {
            string docPath = pathServices.DirectoryGrafanaJSON;
            dynamic data = value;

            var response = new List<QueryResponse>();

            if (data.targets[0].target == null || string.IsNullOrEmpty(data.targets[0].target.Value))
            {
                // geen target meegegeven, die alle dirs timeseries
                var dirPrograms = new DirectoryInfo(docPath);

                foreach (var enumerateDirectory in dirPrograms.EnumerateDirectories()
                    .Where(b => !IsDirectoryEmpty(b.FullName)).OrderBy(b => b.Name))
                {
                    if (Directory.Exists(enumerateDirectory.FullName) &&
                        System.IO.File.Exists($"{enumerateDirectory.FullName}/info.json"))
                    {
                        response.Add(GetTimeSerie(GetDescription(enumerateDirectory.FullName), enumerateDirectory.FullName));
                    }
                }
            }else
            {
                foreach (var target in data.targets)
                {
                    string name = target.target;
                    var dir = $"{docPath}/{name}";

                    var typeData_target = GetTypeData(dir);

                    switch (typeData_target)
                    {
                        case TypeData.Default:
                            if (target.type == "table")
                            {
                                response.Add(GetTableDefault(dir));
                            }
                            else
                            {
                                response.Add(GetTimeSerie(GetDescription(dir), dir));
                            }                            
                            break;
                        case TypeData.KeyValue:
                            if (target.type == "table")
                            {
                                response.Add(GetTableKeyValue(dir));
                            }
                            // TODO
                            break;
                    }
                }
            }

            return new ActionResult<dynamic>(response);
        }

        private bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        private TypeData GetTypeData(string dir)
        {
            if (System.IO.File.Exists($"{dir}/info.json"))
            {
                dynamic info = FileHelper.GetJson<dynamic>($"{dir}/info.json");
                if (DynamicHelper.HasProperty(info, "Name"))
                {
                    return TypeData.Default; // voor oude data
                }

                var infoJSON = FileHelper.GetJson<GetInfoJson>($"{dir}/info.json");
                return infoJSON.Type;
            }

            return TypeData.Default;
        }

        private Table GetTableKeyValue(string dir)
        {
            FileHelper.CheckDataFiles(dir);
            var table = new Table {Type = "table", Rows = new List<dynamic>(), Columns = new List<dynamic>()};
            var columns = FileHelper.GetJson<List<GetInfoTableJsonColumn>>($"{dir}/table.json");
            //We geven alleen dag standen!
            var dateData = DateTime.Today;
            var keys = new List<string>();
            var todayDir = dateData.ToString("yyyy-MM-dd");
            foreach (var column in columns)
            {
                dynamic expandoObject = new ExpandoObject();
                expandoObject.text = column.Text;
                // We kennen geen bools in grafana
                expandoObject.type = column.Type == "bool" ? "number" : column.Type;
                expandoObject.jsonvalue = column.JsonValue;
                table.Columns.Add(expandoObject);
                                
                var filePath = $"{dir}/{column.JsonValue}/{todayDir}/data.json";
                var items = new Dictionary<string, string>();
                if (System.IO.File.Exists(filePath))
                {
                    items = FileHelper.GetJson<Dictionary<string, string>>(filePath);
                }

                foreach (var item in items)
                {
                    if (!keys.Contains(item.Key.ToLower()))
                    {
                        keys.Add(item.Key.ToLower());
                    }
                }
            }

            // op volgorde zetten
            keys = keys.OrderBy(b => b).ToList();

            foreach (var key in keys)
            {
                var values = new List<dynamic>();
                foreach (var column in columns)
                {
                    if (column.JsonValue.ToLower() == "key")
                    {
                        values.Add(key);
                    }
                    else
                    {                        
                        var filePath = $"{dir}/{column.JsonValue}/{todayDir}/data.json";
                        var items = new JObject();
                        if (System.IO.File.Exists(filePath))
                        {
                            items = FileHelper.GetJson<JObject>(filePath);
                        }

                        if (items.ContainsKey(key))
                        {
                            values.Add(GetValueOfDynamic(items[key]));
                        }
                        else
                        {
                            values.Add(GetDefaultValueOfDynamic(column.Type));
                        }
                    }
                }

                table.Rows.Add(values);
            }
            return table;
        }

        private Table GetTableDefault(string dir)
        {
            FileHelper.CheckDataFiles(dir);
            var table = new Table { Type = "table", Rows = new List<dynamic>(), Columns = new List<dynamic>() };
            // We geven alleen het eerste query object terug bij type = table
            // We weten niet of alle targets wel de zelfde kolommen hebben                        
            // Eerste kolom is altijd een Time kolom
            dynamic timeColum = new ExpandoObject();
            timeColum.text = "Time";
            timeColum.type = "time";
            timeColum.jsonvalue = "Time";
            table.Columns.Add(timeColum);
            var columns = FileHelper.GetJson<List<GetInfoTableJsonColumn>>($"{dir}/table.json");
            
            foreach (var column in columns)
            {
                dynamic boolColumn = new ExpandoObject();
                boolColumn.text = column.Text;
                // We kennen geen bools in grafana
                boolColumn.type = column.Type == "bool" ? "number" : column.Type;
                boolColumn.jsonvalue = column.JsonValue;
                table.Columns.Add(boolColumn);
            }
            

            var dirPrograms = new DirectoryInfo(dir);
            // laatste gegevens alleen weergeven in table
            var enumerateDirectory = dirPrograms.EnumerateDirectories().Where(b => !IsDirectoryEmpty(b.FullName)).OrderByDescending(b => b.Name).First();
            var dateData = GetDateTime(enumerateDirectory.Name);

            // JObject, we weten niet hoe de kolomen heten real-time
            var items = FileHelper.GetJson<List<JObject>>($"{enumerateDirectory.FullName}/data.json");
            if (items != null)
            {
                foreach (var item in items)
                {
                    var values = new List<dynamic>();
                    foreach (var tableColumn in table.Columns)
                    {
                        // Time kan je halen uit de directory naam
                        values.Add(
                            tableColumn.jsonvalue == "Time"
                                ? GrafanaHelpers.GetTimeGrafana(dateData)
                                : GetValueOfDynamic(item.GetValue(tableColumn.jsonvalue)));
                    }

                    table.Rows.Add(values);
                }                    
            }
            

            return table;
        }

        private DateTime GetDateTime(string directoryName)
        {
            // van directory naam weer terug naar een datetime
            var datetimeSplit = directoryName.Split(" ");
            var dateStrings = datetimeSplit[0].Split("-");
            var timeStrings = datetimeSplit[1].Split("_");
            var dateData = new DateTime(Int32.Parse(dateStrings[0]), Int32.Parse(dateStrings[1]),
                Int32.Parse(dateStrings[2]), Int32.Parse(timeStrings[0]), Int32.Parse(timeStrings[1]), Int32.Parse(timeStrings[2]));
            return dateData;
        }

        private string GetDescription(string dir)
        {
            dynamic info = FileHelper.GetJson<dynamic>($"{dir}/info.json");
            if (DynamicHelper.HasProperty(info, "Name"))
            {
                return info.Name; // voor oude data
            }
            return info.Description;
        }

        private TimeSerie GetTimeSerie(string name, string dir)
        {
            Console.WriteLine(dir);
            var dirPrograms = new DirectoryInfo(dir);
            var floatList = new List<float[]>();
            foreach (var enumerateDirectory in dirPrograms.EnumerateDirectories().Where(b => !IsDirectoryEmpty(b.FullName)).OrderBy(b => b.Name))
            {
                var dateData = GetDateTime(enumerateDirectory.Name);
                var items = FileHelper.GetJson<List<dynamic>>($"{dir}/data.json");

                if (items.Count != 0)
                {
                    floatList.Add(new[] {items.Count, GrafanaHelpers.GetTimeGrafana(dateData) });
                }
                else
                {
                    floatList.Add(new[] { 0, GrafanaHelpers.GetTimeGrafana(dateData) });
                }
            }

            return new TimeSerie {Target = name, Datapoints = floatList};
        }
    }
}
