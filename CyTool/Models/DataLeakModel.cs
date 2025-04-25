using System;
using System.Text.Json.Serialization;

namespace CyTool.Models
{
    public class DataLeakModel
    {
        [JsonPropertyName("systemid")]
        public string SystemId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("bucket")]
        public string Bucket { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonIgnore]
        public string SizeDisplay => $"{Size} bytes";
    }
}