using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_parser.Models.Requirements.ObjectRequirements.SubObjects
{
    public class EnemyDamage : AbstractObjectLogicalElement
    {
        [JsonPropertyName("enemy")]
        public string EnemyName { get; set; }

        [JsonPropertyName("type")]
        public string AttackName { get; set; }

        public int Hits { get; set; }
    }
}
