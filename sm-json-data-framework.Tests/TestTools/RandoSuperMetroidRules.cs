using sm_json_data_framework.InGameStates;
using sm_json_data_framework.InGameStates.EnergyManagement;
using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.TestTools
{
    /// <summary>
    /// A subclass of SuperMetroidRules which has typical randomizer adjustments, to be used intests as an alternate when relevant.
    /// </summary>
    public class RandoSuperMetroidRules : SuperMetroidRules
    {
        public override decimal GetDamageOverTimeReductionMultiplier(ReadOnlyInGameState inGameState, DamageOverTimeEnum dotEnum)
        {
            return GetDamageOverTimeReductionMultiplier(dotEnum, inGameState.Inventory.HasVariaSuit(), inGameState.Inventory.HasGravitySuit());
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

        public override int CalculateEnvironmentalDamage(ReadOnlyInGameState inGameState, int baseDamage)
        {
            // Make Varia the only suit that reduces environmental damage
            if (inGameState.Inventory.HasVariaSuit())
            {
                return baseDamage / 4;
            }
            else
            {
                return baseDamage;
            }
        }

        private IEnumerable<Item> ReturnVariaIfPresent(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            if (inGameState.Inventory.HasVariaSuit())
            {
                return new[] { model.Items[SuperMetroidModel.VARIA_SUIT_NAME] };
            }
            else
            {
                return new Item[] { };
            }
        }

        public override IEnumerable<Item> GetEnvironmentalDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            // Report Varia as being the only suit that reduces environmental damage
            return ReturnVariaIfPresent(model, inGameState);
        }
    }
}
