using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Enemies
{
    public class RawEnemy
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<EnemyAttack> Attacks { get; set; } = new List<EnemyAttack>();

        public int Hp { get; set; }

        public int AmountOfDrops { get; set; }

        public RawEnemyDrops Drops { get; set; }

        public RawEnemyDrops FarmableDrops { get; set; }

        public RawEnemyDimensions Dims { get; set; }

        public bool Freezable { get; set; }

        public bool Grapplable { get; set; }

        public ISet<string> Invul { get; set; } = new HashSet<string>();

        public IList<RawEnemyDamageMultiplier> DamageMultipliers { get; set; } = new List<RawEnemyDamageMultiplier>();

        public ISet<string> Areas { get; set; } = new HashSet<string>();
    }
}
