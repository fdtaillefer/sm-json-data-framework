﻿using sm_json_data_framework.Models;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.TestSubClasses
{
    /// <summary>
    /// A subclass of SuperMetroidRules which has typical randomizer adjustments, to be used intests as an alternate when relevant.
    /// </summary>
    public class RandoSuperMetroidRules : SuperMetroidRules
    {
        public override int CalculateEnvironmentalDamage(InGameState inGameState, int baseDamage)
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

        private IEnumerable<Item> ReturnVariaIfPresent(SuperMetroidModel model, InGameState inGameState)
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

        public override IEnumerable<Item> GetEnvironmentalDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            // Report Varia as being the only suit that reduces environmental damage
            return ReturnVariaIfPresent(model, inGameState);
        }

        public override int CalculateHeatDamage(InGameState inGameState, int heatFrames)
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

        public override IEnumerable<Item> GetHeatDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            // Report Varia as being the only suit that reduces heat damage
            return ReturnVariaIfPresent(model, inGameState);
        }

        public override int CalculateLavaDamage(InGameState inGameState, int lavaFrames)
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

        public override int CalculateLavaPhysicsDamage(InGameState inGameState, int lavaPhysicsFrames)
        {
            // Turning off Gravity does nothing, use standard calculation
            return CalculateLavaDamage(inGameState, lavaPhysicsFrames);
        }

        public override IEnumerable<Item> GetLavaDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            // Report Varia as being the only suit that reduces lava damage
            return ReturnVariaIfPresent(model, inGameState);
        }

        public override int CalculateAcidDamage(InGameState inGameState, int acidFrames)
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

        public override IEnumerable<Item> GetAcidDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            // Report Varia as being the only suit that reduces acid damage
            return ReturnVariaIfPresent(model, inGameState);
        }

        public override int CalculateElectricityGrappleDamage(InGameState inGameState, int electricityFrames)
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

        public override IEnumerable<Item> GetElectricityGrappleDamageReducingItems(SuperMetroidModel model, InGameState inGameState)
        {
            // Report Varia as being the only suit that reduces electricity grapple damage
            return ReturnVariaIfPresent(model, inGameState);
        }

    }
}
