using sm_json_data_framework.Models;
using sm_json_data_framework.Models.InGameStates;
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

        private IEnumerable<UnfinalizedItem> ReturnVariaIfPresent(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            if (inGameState.Inventory.HasVariaSuit())
            {
                return new[] { model.Items[SuperMetroidModel.VARIA_SUIT_NAME] };
            }
            else
            {
                return new UnfinalizedItem[] { };
            }
        }

        public override IEnumerable<UnfinalizedItem> GetEnvironmentalDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            // Report Varia as being the only suit that reduces environmental damage
            return ReturnVariaIfPresent(model, inGameState);
        }

        public override int CalculateHeatDamage(ReadOnlyInGameState inGameState, int heatFrames)
        {
            // Make Varia the only suit that reduces heat damage
            if (inGameState.Inventory.HasVariaSuit())
            {
                return 0;
            }
            else
            {
                return heatFrames / 4;
            }
        }

        public override IEnumerable<UnfinalizedItem> GetHeatDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            // Report Varia as being the only suit that reduces heat damage
            return ReturnVariaIfPresent(model, inGameState);
        }

        public override int CalculateLavaDamage(ReadOnlyInGameState inGameState, int lavaFrames)
        {
            // Make Varia the only suit that reduces lava damage
            bool hasVaria = inGameState.Inventory.HasVariaSuit();
            if (hasVaria)
            {
                return 0;
            }
            else
            {
                return lavaFrames / 2;
            }
        }

        public override int CalculateLavaPhysicsDamage(ReadOnlyInGameState inGameState, int lavaPhysicsFrames)
        {
            // Turning off Gravity does nothing, use standard calculation
            return CalculateLavaDamage(inGameState, lavaPhysicsFrames);
        }

        public override IEnumerable<UnfinalizedItem> GetLavaDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            // Report Varia as being the only suit that reduces lava damage
            return ReturnVariaIfPresent(model, inGameState);
        }

        public override int CalculateAcidDamage(ReadOnlyInGameState inGameState, int acidFrames)
        {
            // Make Varia the only suit that reduces acid damage
            if (inGameState.Inventory.HasVariaSuit())
            {
                return acidFrames * 3 / 8;
            }
            else
            {
                return acidFrames * 6 / 4;
            }
        }

        public override IEnumerable<UnfinalizedItem> GetAcidDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            // Report Varia as being the only suit that reduces acid damage
            return ReturnVariaIfPresent(model, inGameState);
        }

        public override int CalculateElectricityGrappleDamage(ReadOnlyInGameState inGameState, int electricityFrames)
        {
            // Make Varia the only suit that reduces electricity grapple damage
            if (inGameState.Inventory.HasVariaSuit())
            {
                return electricityFrames / 4;
            }
            else
            {
                return electricityFrames;
            }
        }

        public override IEnumerable<UnfinalizedItem> GetElectricityGrappleDamageReducingItems(SuperMetroidModel model, ReadOnlyInGameState inGameState)
        {
            // Report Varia as being the only suit that reduces electricity grapple damage
            return ReturnVariaIfPresent(model, inGameState);
        }

    }
}
