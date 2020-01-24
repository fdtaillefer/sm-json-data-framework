using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.InGameStates;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Rules
{
    /// <summary>
    /// A repository of game rules. This offers some base values, as well as calculations based on game rules.
    /// </summary>
    public class SuperMetroidRules
    {
        public virtual int ThornDamage { get => 16; }

        public virtual int SpikeDamage { get => 60; }

        public virtual int HibashiDamage { get => 30; }

        /// <summary>
        /// Calculates and returns the environmental damage Samus would take for the provided in-game state and base environmental damage. This method is intended
        /// for environment-based punctual hits, not damage over time.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="baseDamage">The base damage</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateEnvironmentalDamage(InGameState inGameState, int baseDamage)
        {
            bool hasVaria = inGameState.HasVariaSuit();
            bool hasGravity = inGameState.HasGravitySuit();
            if (hasGravity)
            {
                return baseDamage / 4;
            }
            else if (hasVaria)
            {
                return baseDamage / 2;
            }
            else
            {
                return baseDamage;
            }
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in a heated room, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="heatFrames">The duration (in frames) of the heat exposure whose damage to calculate.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateHeatDamage(InGameState inGameState, int heatFrames)
        {
            bool hasVaria = inGameState.HasVariaSuit();
            bool hasGravity = inGameState.HasGravitySuit();
            if (hasGravity || hasVaria)
            {
                return 0;
            }
            else
            {
                return heatFrames / 4;
            }
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in lava, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="lavaFrames">The duration (in frames) of the lava exposure whose damage to calculate.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateLavaDamage(InGameState inGameState, int lavaFrames)
        {
            bool hasVaria = inGameState.HasVariaSuit();
            bool hasGravity = inGameState.HasGravitySuit();
            if (hasGravity)
            {
                return 0;
            }
            else if (hasVaria)
            {
                return lavaFrames / 4;
            }
            else
            {
                return lavaFrames / 2;
            }
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration in acid, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="acidFrames">The duration (in frames) of the acid exposure whose damage to calculate.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateAcidDamage(InGameState inGameState, int acidFrames)
        {
            bool hasVaria = inGameState.HasVariaSuit();
            bool hasGravity = inGameState.HasGravitySuit();
            if (hasGravity)
            {
                return acidFrames * 3 / 8;
            }
            else if (hasVaria)
            {
                return acidFrames * 3 / 4;
            }
            else
            {
                return acidFrames * 6 / 4;
            }
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for spending the provided duration grappled to a broken Draygon turret, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="electricityFrames">The duration (in frames) of the electricity exposure whose damage to calculate.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateElectricityGrappleDamage(InGameState inGameState, int electricityFrames)
        {
            bool hasVaria = inGameState.HasVariaSuit();
            bool hasGravity = inGameState.HasGravitySuit();
            if (hasGravity)
            {
                return electricityFrames / 4;
            }
            else if (hasVaria)
            {
                return electricityFrames / 2;
            }
            else
            {
                return electricityFrames;
            }
        }

        /// <summary>
        /// Calculates and returns the damage Samus would take for exeucting a shinespark of the provided duration, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="shinesparkFrames">The duration (in frames) of the shinespark whose damage to calculate.</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateShinesparkDamage(InGameState inGameState, int shinesparkFrames)
        {
            return shinesparkFrames;
        }

        /// <summary>
        /// Calculates and returns how much damage the provided enemy attack would do to Samus, given the provided in-game state.
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="attack">The enemy attack whose damage to calculate</param>
        /// <returns>The calculated damage</returns>
        public virtual int CalculateEnemyDamage(InGameState inGameState, EnemyAttack attack)
        {
            bool hasVaria = inGameState.HasVariaSuit();
            bool hasGravity = inGameState.HasGravitySuit();

            if (hasGravity && attack.AffectedByGravity)
            {
                return attack.BaseDamage / 4;
            }
            else if (hasVaria && attack.AffectedByVaria)
            {
                return attack.BaseDamage / 2;
            }
            else
            {
                return attack.BaseDamage;
            }
        }
    }
}
