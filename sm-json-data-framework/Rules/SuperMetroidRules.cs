using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Rooms.Node;
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
        /// The energy at which a shinespark is interrupted
        /// </summary>
        public virtual int ShinesparkEnergyLimit { get => 29; }

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
        /// <para>Returns the energy Samus must have when initiating a shinespark, for that shinespark to complete to the end.
        /// By default, that is the energy cost of the shinespark + the energy at which a shinespark is interrupted.</para>
        /// <para>This does return 0 if shinesparkFrames is 0, interpreting that as meaning there is no shinespark.</para>
        /// </summary>
        /// <param name="inGameState">An in-game state describing the current player situation, notably knowing what items the player has.</param>
        /// <param name="shinesparkFrames">The duration (in frames) of the shinespark. 0 means no shinespark is being executed.</param>
        /// <returns></returns>
        public virtual int CalculateEnergyNeededForShinespark(InGameState inGameState, int shinesparkFrames)
        {
            return shinesparkFrames == 0 ? 0 : shinesparkFrames + ShinesparkEnergyLimit;
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

        /// <summary>
        /// Calculates the effective length of the provided runway, in flat tiles.
        /// </summary>
        /// <param name="runway">The runway to calculate</param>
        /// <returns></returns>
        public virtual decimal CalculateEffectiveRunwayLength(IRunway runway)
        {
            // I don't know how diagonal tiles work, so ignore them for now
            // Up and down tiles are both worth a bit more than 1 tile, but seem to be worth a different amount each.
            return runway.OpenEnds * 0.75M + runway.Length;
        }

        /// <summary>
        /// Calculates the effective length of the provided runway, used in the opposite direction, in flat tiles.
        /// </summary>
        /// <param name="runway">The runway to calculate in reverse</param>
        /// <returns></returns>
        public decimal CalculateEffectiveReversedRunwayLength(IRunway runway)
        {
            return CalculateEffectiveRunwayLength(ReverseRunway(runway));
        }

        /// <summary>
        /// Creates and returns the equivalent of the provided runway, but used in the opposite direction.
        /// </summary>
        /// <param name="runway">The runway to reverse</param>
        /// <returns></returns>
        public virtual IRunway ReverseRunway(IRunway runway)
        {
            return new Runway {
                Length = runway.Length,
                EndingUpTiles = runway.StartingDownTiles,
                GentleDownTiles = runway.GentleUpTiles,
                GentleUpTiles = runway.GentleDownTiles,
                OpenEnds = runway.OpenEnds,
                StartingDownTiles = runway.EndingUpTiles,
                SteepDownTiles = runway.SteepUpTiles,
                SteepUpTiles = runway.SteepDownTiles
            };
        }
    }
}
