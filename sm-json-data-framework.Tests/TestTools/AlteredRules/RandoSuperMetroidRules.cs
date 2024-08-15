using sm_json_data_framework.EnergyManagement;
using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.TestTools.AlteredRules
{
    /// <summary>
    /// A subclass of SuperMetroidRules which has typical randomizer adjustments, to be used intests as an alternate when relevant.
    /// </summary>
    public class RandoSuperMetroidRules : SuperMetroidRules
    {
        public override decimal GetDamageOverTimeReductionMultiplier(DamageOverTimeEnum dotEnum, ReadOnlyItemInventory inventory)
        {
            return GetDamageOverTimeReductionMultiplier(dotEnum, inventory.HasVariaSuit(), inventory.HasGravitySuit());
        }

        private decimal GetDamageOverTimeReductionMultiplier(DamageOverTimeEnum dotEnum, bool hasVaria, bool hasGravity)
        {
            // We're making Varia the only suit that reduces heat, acid, lava, and electricity
            if (hasGravity)
            {
                switch (dotEnum)
                {
                    case DamageOverTimeEnum.SamusEater:
                        return 0.25M;
                }
            }

            if (hasVaria)
            {
                // Shinespark damage is unaffected by having Varia
                switch (dotEnum)
                {
                    case DamageOverTimeEnum.Acid:
                    case DamageOverTimeEnum.GrappleElectricity:
                    case DamageOverTimeEnum.SamusEater:
                    case DamageOverTimeEnum.Lava:
                    case DamageOverTimeEnum.LavaPhysics:
                        return 0.5M;
                    case DamageOverTimeEnum.Heat:
                        return 0;
                }
            }

            switch (dotEnum)
            {
                case DamageOverTimeEnum.Acid:
                case DamageOverTimeEnum.GrappleElectricity:
                case DamageOverTimeEnum.SamusEater:
                case DamageOverTimeEnum.Lava:
                case DamageOverTimeEnum.LavaPhysics:
                case DamageOverTimeEnum.Heat:
                case DamageOverTimeEnum.Shinespark:
                    return 1;
                default:
                    throw new NotImplementedException($"DamageOverTime enum {dotEnum} not supported here");
            }
        }

        public override decimal GetPunctualEnvironmentDamageReductionMultiplier(PunctualEnvironmentDamageEnum environmentDamageEnum, ReadOnlyItemInventory inventory)
        {
            // Make Varia the only suit that reduces environment damage
            if (inventory.HasVariaSuit())
            {
                return 0.25M;
            }
            else
            {
                return 1;
            }
        }
    }
}
