namespace Json.Grafana.DataSources.Models
{
    using System.Collections.Generic;

    public class Table : QueryResponse
    {
        public List<dynamic> Columns { get; set; }

        public List<dynamic> Rows { get; set; }

        public string Type { get; set; }
    }
}