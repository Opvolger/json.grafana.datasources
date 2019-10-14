using System;

namespace Json.Grafana.DataSources.Logic
{
    public static class GrafanaHelpers
    {
        public static float GetTimeGrafana(DateTime dateTime)
        {
            float unixTimestamp = (int)(dateTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp * 1000; // grafana werkt met ms
        }
    }
}
