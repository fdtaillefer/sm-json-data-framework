using sm_json_data_framework.Models.Raw.Enemies;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Enemies
{
    public class EnemyAttack: RawEnemyAttack
    {
        public EnemyAttack()
        {

        }

        public EnemyAttack(RawEnemyAttack rawAttack)
        {
            Name = rawAttack.Name;
            BaseDamage = rawAttack.BaseDamage;
            AffectedByVaria = rawAttack.AffectedByVaria;
            AffectedByGravity = rawAttack.AffectedByGravity;
        }
    }
}
