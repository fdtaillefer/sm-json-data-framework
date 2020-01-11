using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Enemies
{
    /// <summary>
    /// Combines a Weapon with a damage multiplier, which results in a specific damage taken per shot.
    /// </summary>
    public class WeaponMultiplier
    {
        public WeaponMultiplier() { }

        public WeaponMultiplier(Weapon weapon, decimal multiplier)
        {
            Weapon = weapon;
            Multiplier = multiplier;
            DamagePerShot = decimal.ToInt32(weapon.Damage * multiplier);
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
            return (enemyHp + DamagePerShot - 1) / DamagePerShot;
        }
    }
}
