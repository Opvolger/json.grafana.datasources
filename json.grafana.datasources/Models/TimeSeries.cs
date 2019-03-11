namespace Json.Grafana.DataSources.Models
{
    using System;
    using System.Collections.Generic;

    public class TimeSeries
    {

    }

    public class TimeSerie
    {
        public string Target { get; set; }
        public List<float[]> Datapoints { get; set; }
    }
}
