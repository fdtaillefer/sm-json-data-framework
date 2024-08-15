using sm_json_data_framework.EnergyManagement;
using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.TestTools.AlteredRules
{
    public class UselessSuitsSuperMetroidRules : SuperMetroidRules
    {
        public override decimal GetDamageOverTimeReductionMultiplier(DamageOverTimeEnum dotEnum, ReadOnlyItemInventory inventory)
        {
            return 1;
        }

        public override decimal GetEnemyDamageReductionMultiplier(EnemyAttack attack, ReadOnlyItemInventory inventory)
        {
            return 1;
        }

        public override decimal GetPunctualEnvironmentDamageReductionMultiplier(PunctualEnvironmentDamageEnum environmentDamageEnum, ReadOnlyItemInventory inventory)
        {
            return 1;
        }
    }
}
