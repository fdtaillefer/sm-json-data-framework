using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_parser.Models.Requirements.ObjectRequirements.SubObjects
{
    public class EnemyKill : AbstractObjectLogicalElement
    {
        [JsonPropertyName("enemies")]
        public IEnumerable<IEnumerable<string>> GroupedEnemyNames { get; set; } = Enumerable.Empty<IEnumerable<string>>();

        [JsonPropertyName("explicitWeapons")]
        public IEnumerable<string> ExplicitWeaponNames { get; set; } = Enumerable.Empty<string>();

        [JsonPropertyName("excludedWeapons")]
        public IEnumerable<string> ExcludedWeaponsNames { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<AmmoEnum> FarmableAmmo { get; set; } = Enumerable.Empty<AmmoEnum>();
    }
}
