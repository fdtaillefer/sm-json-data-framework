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
        private UnfinalizedWeaponMultiplier InnerElement { get; set; }

        public WeaponMultiplier(UnfinalizedWeaponMultiplier innerElement, Action<WeaponMultiplier> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            Weapon = innerElement.Weapon.Finalize(mappings);
        }

        /// <summary>
        /// The Weapon whose damage is multiplied
        /// </summary>
        public Weapon Weapon { get; }

        /// <summary>
        /// The value by which to multiply base weapon damage
        /// </summary>
        public decimal Multiplier => InnerElement.Multiplier;

        /// <summary>
        /// The damage this WeaponMultiplier's weapon will inflict with each shot.
        /// </summary>
        public int DamagePerShot => InnerElement.DamagePerShot;

        /// <summary>
        /// Calculates the number of hits an enemy with the provided hp will take to die if this WeaponMultiplier is applied to it.
        /// </summary>
        /// <param name="enemyHp">Hp of the enemy</param>
        /// <returns>The number of hits to kill the enemy</returns>
        public int NumberOfHits(int enemyHp)
        {
            return InnerElement.NumberOfHits(enemyHp);
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
            // enemyHp / DamagePerShot will floor. Adding DamagePerShot - 1 is like forcing the division to ceiling instead
            return (enemyHp + DamagePerShot - 1) / DamagePerShot;
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing to do here
            return false;
        }
    }
}
