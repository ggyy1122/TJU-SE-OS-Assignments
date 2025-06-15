using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace FileSystem.Models
{
    public enum EntryType
    {
        File = 0,
        Directory = 1
    }

    public class FileEntry
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Type")]
        public EntryType Type { get; set; }

        [JsonProperty("Size")]
        public long Size { get; set; }

        [JsonProperty("CreatedTime")]
        public DateTime CreatedTime { get; set; }

        [JsonProperty("ModifiedTime")]
        public DateTime ModifiedTime { get; set; }

        [JsonProperty("IndexBlock")]
        public IndexBlock IndexBlock { get; set; }

        [JsonIgnore]
        public DirectoryEntry Parent { get; set; }

        public FileEntry() { }

        public FileEntry(string name, EntryType type)
        {
            Name = name;
            Type = type;
            Size = 0;
            CreatedTime = DateTime.Now;
            ModifiedTime = DateTime.Now;
            IndexBlock = null;
            Parent = null;
        }
    }
    public class FileSystemConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(FileEntry);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var type = (EntryType)jo["Type"].Value<int>();
            FileEntry entry = type == EntryType.Directory ? new DirectoryEntry() : new FileEntry();
            serializer.Populate(jo.CreateReader(), entry);
            return entry;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // 默认逻辑，不重写
            throw new NotImplementedException("This converter is only for deserialization");
        }
    }
}