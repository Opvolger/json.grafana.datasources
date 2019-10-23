namespace Json.Grafana.DataSources.Models
{
    using System.Collections.Generic;

    public class SendData
    {
        public string Name { get; set; }

        public dynamic Json_Data { get; set; }
    }

    public class SendKeyValueData
    {
        public string Name { get; set; }

        public string Subject { get; set; }

        public dynamic Json_Data { get; set; }
    }

    public class GetInfo
    {
        public string Name { get; set; }

        public GetInfoJson Info { get; set; }

        public List<GetInfoTableJsonColumn> Table { get; set; }
    }

    public class GetInfoJson
    {
        // Vroeger stond hier Name!
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
