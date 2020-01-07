using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_parser.Models.Enemies
{
    public class EnemyDimensions
    {
        [JsonPropertyName("h")]
        public int Height { get; set; }

        [JsonPropertyName("w")]
        public int Width { get; set; }

        // STITCHME Note?
    }
}
