namespace Json.Grafana.DataSources.Models
{
    using System.Collections.Generic;

    public class SendData
    {
        public string Name { get; set; }

        public dynamic Json_data { get; set; }
    }

    public class SendKeyValueData : SendData
    {
        public string Subject { get; set; }
    }

    public class GetInfo
    {
        public string Name { get; set; }

        public GetInfoJson Info { get; set; }

        public List<GetInfoTableJsonColumn> Table { get; set; }
    }

    public class JsonExport
    {
        public string Name { get; set; }

        public Table Json_data { get; set; }

        public GetInfoJson Info { get; set; }
    }

    public class GetInfoJson
    {        
        public string Description { get; set; }

        public TypeData Type { get; set; }
    }

    public enum TypeData
    {
        Default,
        KeyValue
    }

    public class GetInfoTableJsonColumn
    {
        public string JsonValue { get; set; }

        public string Type { get; set; }

        public string Text { get; set; }
    }
}
