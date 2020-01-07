using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_parser.Models.Enemies
{
    public class EnemyAttack
    {
        public string Name { get; set; }

        public int BaseDamage { get; set; }

        public bool AffectedByVaria { get; set; } = true;

        public bool AffectedByGravity { get; set; } = true;
    }
}
