using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Enemies
{
    /// <summary>
    /// Combines a Weapon with a damage multiplier, which results in a specific damage taken per shot.
    /// </summary>
    public class WeaponMultiplier : AbstractModelElement<UnfinalizedWeaponMultiplier, WeaponMultiplier>
    {
        public WeaponMultiplier(UnfinalizedWeaponMultiplier innerElement, Action<WeaponMultiplier> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            Multiplier = innerElement.Multiplier;
            DamagePerShot = innerElement.DamagePerShot;
            Weapon = innerElement.Weapon.Finalize(mappings);
        }

        /// <summary>
        /// The Weapon whose damage is multiplied
        /// </summary>
        public Weapon Weapon { get; }

        /// <summary>
        /// The value by which to multiply base weapon damage
        /// </summary>
        public decimal Multiplier { get; }

        /// <summary>
        /// The damage this WeaponMultiplier's weapon will inflict with each shot.
        /// </summary>
        public int DamagePerShot { get; }

        /// <summary>
        /// Calculates the number of hits an enemy with the provided hp will take to die if this WeaponMultiplier is applied to it.
        /// </summary>
        /// <param name="enemyHp">Hp of the enemy</param>
        /// <returns>The number of hits to kill the enemy</returns>
        public int NumberOfHits(int enemyHp)
        {
            return WeaponMultiplierCalculations.NumberOfHits(enemyHp, DamagePerShot);
        }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // The state of the weapon itself is useful to calculate logical relevance, so propagate to it to be sure it's up-to-date when we calculate.
            Weapon.ApplyLogicalOptions(logicalOptions);
        }

        public override bool CalculateLogicallyRelevant()
        {
            // If our weapon isn't relevant, neither is a multiplier involving it.
            return Weapon.LogicallyRelevant;
        }
    }

    public class UnfinalizedWeaponMultiplier : AbstractUnfinalizedModelElement<UnfinalizedWeaponMultiplier, WeaponMultiplier>
    {
        public UnfinalizedWeaponMultiplier() { }

        public UnfinalizedWeaponMultiplier(UnfinalizedWeapon weapon, decimal multiplier)
        {
            Weapon = weapon;
            Multiplier = multiplier;
            DamagePerShot = decimal.ToInt32(weapon.Damage * multiplier);
        }

        protected override WeaponMultiplier CreateFinalizedElement(UnfinalizedWeaponMultiplier sourceElement, Action<WeaponMultiplier> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new WeaponMultiplier(sourceElement, mappingsInsertionCallback, mappings);
        }

        /// <summary>
        /// The Weapon whose damage is multiplied
        /// </summary>
        public UnfinalizedWeapon Weapon { get; }

        /// <summary>
        /// The value by which to multiply base weapon damage
        /// </summary>
        public decimal Multiplier { get; }

        /// <summary>
        /// The damage this WeaponMultiplier's weapon will inflict with each shot.
        /// </summary>
        public int DamagePerShot { get; }

        /// <summary>
        /// Calculates the number of hits an enemy with the provided hp will take to die if this WeaponMultiplier is applied to it.
        /// </summary>
        /// <param name="enemyHp">Hp of the enemy</param>
        /// <returns>The number of hits to kill the enemy</returns>
        public int NumberOfHits(int enemyHp)
        {
            return WeaponMultiplierCalculations.NumberOfHits(enemyHp, DamagePerShot);
        }
    }

    internal static class WeaponMultiplierCalculations
    {
        internal static int NumberOfHits(int enemyHp, int damagePerShot)
        {
            // enemyHp / DamagePerShot will floor. Adding DamagePerShot - 1 is like forcing the division to ceiling instead
            return (enemyHp + damagePerShot - 1) / damagePerShot;
        }
    }
}
