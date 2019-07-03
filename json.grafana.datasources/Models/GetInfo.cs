namespace Json.Grafana.DataSources.Models
{
    using System.Collections.Generic;

    public class GetInfo
    {
        public string Name { get; set; }

        public GetInfoInfo Info { get; set; }

        public List<GetInfoTableColumn> Table { get; set; }
    }

    public class GetInfoInfo
    {
        public string Name { get; set; }
    }

    public class GetInfoTableColumn
    {
        public string JsonValue { get; set; }

        public string Type { get; set; }

        public string Text { get; set; }
    }
}
