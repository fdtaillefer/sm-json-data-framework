using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Enemies
{
    public class RawEnemyAttack
    {
        public string Name { get; set; }

        public int BaseDamage { get; set; }

        public bool AffectedByVaria { get; set; } = true;

        public bool AffectedByGravity { get; set; } = true;
    }
}
