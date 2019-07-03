﻿namespace Json.Grafana.DataSources.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;


    [ApiController]
    public class SimpelJsonController : ControllerBase
    {
        public static AppSettings Settings;

        [Produces("application/json")]
        [Route("search")]
        [HttpPost]
        public ActionResult<IEnumerable<string>> Search()
        {
            // Set a variable to the Documents path.
            string docPath = Settings.DirectoryGrafanaJSON;
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

        public static dynamic GetValueOfDynamic(dynamic value)
        {
            if (value.Value is bool b)
            {
                return b ? 1 : 0;
            }

            if (value.Value is DateTime c)
            {
                return GetTimeGrafana(c);
            }
            return value.Value;

        }

        private static float GetTimeGrafana(DateTime dateTime)
        {
            float unixTimestamp = (int)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp * 1000; // grafana werkt met ms
        }

        [Produces("application/json")]
        [Route("query")]
        [HttpPost]
        public ActionResult<dynamic> Query([FromBody] dynamic value)
        {
            string docPath = Settings.DirectoryGrafanaJSON;
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
                        response.Add(GetTimeSerie(GetName(enumerateDirectory.FullName), enumerateDirectory.FullName));
                    }
                }
            }else
            {
                foreach (var target in data.targets)
                {
                    string name = target.target;
                    var dir = $"{docPath}/{name}";
                    if (Directory.Exists(dir))
                    {
                        if (target.type == "table")
                        {
                            response.Add(GetTable(dir));
                        }
                        else
                        {
                            response.Add(GetTimeSerie(GetName(dir), dir));
                        }
                    }
                }
            }

            return new ActionResult<dynamic>(response);
        }

        private bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        private Table GetTable(string dir)
        {
            var table = new Table { Type = "table", Rows = new List<dynamic>(), Columns = new List<dynamic>() };
            // We geven alleen het eerste query object terug bij type = table
            // We weten niet of alle targets wel de zelfde kolommen hebben            
            if (Directory.Exists(dir) && System.IO.File.Exists($"{dir}/table.json"))
            {
                dynamic timeColum = new System.Dynamic.ExpandoObject();
                timeColum.text = "Time";
                timeColum.type = "time";
                timeColum.jsonvalue = "Time";
                table.Columns.Add(timeColum);
                using (StreamReader r = new StreamReader($"{dir}/table.json"))
                {
                    string json = r.ReadToEnd();
                    List<dynamic> columns = JsonConvert.DeserializeObject<List<dynamic>>(json);
                    foreach (var column in columns)
                    {
                        // We kennen geen bools in grafana
                        if (column.type == "bool")
                        {
                            dynamic boolColumn = new System.Dynamic.ExpandoObject();
                            boolColumn.text = column.Text;
                            boolColumn.type = "number";
                            boolColumn.jsonvalue = column.JsonValue;
                            table.Columns.Add(boolColumn);
                        }
                        else
                        {
                            dynamic column2 = new System.Dynamic.ExpandoObject();
                            column2.text = column.Text;
                            column2.type = column.Type;
                            column2.jsonvalue = column.JsonValue;
                            table.Columns.Add(column2);
                        }
                    }
                }

                var dirPrograms = new DirectoryInfo(dir);
                // laatste gegevens alleen weergeven in table
                var enumerateDirectory = dirPrograms.EnumerateDirectories().Where(b => !IsDirectoryEmpty(b.FullName)).OrderByDescending(b => b.Name).First();
                var dateData = GetDateTime(enumerateDirectory.Name);
                using (StreamReader r = new StreamReader($"{enumerateDirectory.FullName}/data.json"))
                {
                    string json = r.ReadToEnd();
                    List<JObject> items = JsonConvert.DeserializeObject<List<JObject>>(json);
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var values = new List<dynamic>();
                            foreach (var tableColumn in table.Columns)
                            {
                                if (tableColumn.jsonvalue == "Time")
                                {
                                    values.Add(GetTimeGrafana(dateData));
                                }
                                else
                                {
                                    // ophalen van item uit json
                                    values.Add(GetValueOfDynamic(item.GetValue(tableColumn.jsonvalue.Value)));
                                }
                            }

                            table.Rows.Add(values);
                        }
                    }
                }
            }

            return table;
        }

        private DateTime GetDateTime(string x)
        {
            var datetimeSplit = x.Split(" ");
            var dateStrings = datetimeSplit[0].Split("-");
            var timeStrings = datetimeSplit[1].Split("_");
            var dateData = new DateTime(Int32.Parse(dateStrings[0]), Int32.Parse(dateStrings[1]),
                Int32.Parse(dateStrings[2]), Int32.Parse(timeStrings[0]), Int32.Parse(timeStrings[1]), Int32.Parse(timeStrings[2]));
            return dateData;
        }

        private string GetName(string dir)
        {
            using (StreamReader r = new StreamReader($"{dir}/info.json"))
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
            foreach (var enumerateDirectory in dirPrograms.EnumerateDirectories().Where(b => !IsDirectoryEmpty(b.FullName)).OrderBy(b => b.Name))
            {
                var dateData = GetDateTime(enumerateDirectory.Name);
                using (StreamReader r = new StreamReader($"{enumerateDirectory.FullName}/data.json"))
                {
                    string json = r.ReadToEnd();
                    List<dynamic> items = JsonConvert.DeserializeObject<List<dynamic>>(json);
                    if (!string.IsNullOrEmpty(json))
                    {
                        floatList.Add(new float[] {items.Count, GetTimeGrafana(dateData) });
                    }
                    else
                    {
                        floatList.Add(new float[] { 0, GetTimeGrafana(dateData) });
                    }
                }
            }

            return new TimeSerie
                {Target = name, Datapoints = floatList};
        }
    }
}