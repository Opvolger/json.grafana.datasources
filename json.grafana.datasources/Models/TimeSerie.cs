namespace Json.Grafana.DataSources.Models
{
    using System.Collections.Generic;

    public class TimeSerie : QueryResponse
    {
        public string Target { get; set; }

        public List<float[]> Datapoints { get; set; }
    }
}
