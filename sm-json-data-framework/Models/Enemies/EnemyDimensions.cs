using sm_json_data_framework.Models.Raw.Enemies;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Enemies
{
    public class EnemyDimensions
    {
        [JsonPropertyName("h")]
        public int Height { get; set; }

        [JsonPropertyName("w")]
        public int Width { get; set; }

        public EnemyDimensions()
        {

        }

        public EnemyDimensions(RawEnemyDimensions dimensions)
        {
            Height = dimensions.H;
            Width = dimensions.W;
        }
    }
}
