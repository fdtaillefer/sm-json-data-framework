using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_parser.Models.Enemies
{
    public class Enemy
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<EnemyAttack> Attacks { get; set; } = Enumerable.Empty<EnemyAttack>();

        public int Hp { get; set; }

        public int AmountOfDrops { get; set; }

        // STITCHME Note?

        public EnemyDrops Drops { get; set; }

        public EnemyDrops FarmableDrops { get; set; }

        [JsonPropertyName("dims")]
        public EnemyDimensions Dimensions { get; set; }

        public bool Freezable { get; set; }

        public bool Grapplable { get; set; }

        public bool Farmable { get; set; }

        [JsonPropertyName("invul")]
        public IEnumerable<string> InvulnerabilityStrings { get; set; } = Enumerable.Empty<string>();

        [JsonPropertyName("damageMultipliers")]
        public IEnumerable<RawEnemyDamageMultiplier> RawDamageMultipliers { get; set; } = Enumerable.Empty<RawEnemyDamageMultiplier>();

        public IEnumerable<string> Areas { get; set; } = Enumerable.Empty<string>();

        // STITCHME Something about putting down all weapons this is vulnerable to and the number of shots?
        public IDictionary<string, WeaponSusceptibility> WeaponSusceptibilities { get; set; }

        public void Initialize(SuperMetroidModel model)
        {
            // STITCHME Model should provide weapons by category. Now it does, see what I can do with that.
        }

    }
}
