﻿using Newtonsoft.Json;

namespace MxIO.ApiClient.Abstractions
{
    public class CollectionDto<T>
    {
        [JsonProperty]
        public int TotalRecords { get; set; }

        [JsonProperty]
        public int FilteredRecords { get; set; }

        [JsonProperty]
        public List<T> Entries { get; set; } = new List<T>();
    }
}
