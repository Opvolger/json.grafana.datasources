namespace Json.Grafana.DataSources.Logic
{
    using System;
    using System.IO;

    public class PathServices : IPathServices
    {
        public static AppSettings Settings;
        public string DirectoryGrafanaJSON => Settings.DirectoryGrafanaJSON;


        public PathServices(AppSettings settings)
        {
            Settings = settings;
        }

        public string GetNamePath(string name, bool create)
        {
            if (name.Contains('.') || name.Contains('/'))
            {
                throw new Exception("invalid name");
            }
            string docPath = Path.Combine(DirectoryGrafanaJSON, name);
            if (create)
            {
                if (!Directory.Exists(docPath))
                {
                    Directory.CreateDirectory(docPath);
                }
            }

            return docPath;
        }

        public string CreateDateTimePath(string name, bool create = false)
        {
            string docPath = GetNamePath(name, true);
            // create dir met data
            var datetimeDir = DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss");
            var fullPath = Path.Combine(docPath, datetimeDir);
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }
    }

    public interface IPathServices
    {
        string DirectoryGrafanaJSON { get; }

        string GetNamePath(string name, bool create = false);

        string CreateDateTimePath(string name, bool create = false);
    }
}
