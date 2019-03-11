namespace Json.Grafana.DataSources.Models
{
    using System;
    using System.Collections.Generic;

    public class Table
    {
        public List<dynamic> Columns { get; set; }

        public List<dynamic> Rows { get; set; }

        public string Type { get; set; }
    }

    public class TimeSerie
    {
        public string Target { get; set; }
        public List<float[]> Datapoints { get; set; }
    }
}
