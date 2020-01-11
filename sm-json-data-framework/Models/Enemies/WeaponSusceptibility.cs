using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Enemies
{
    /// <summary>
    /// Describes an implicit enemy's susceptibility to a weapon, and the number of perfectly-aimed shots it would take
    /// from this weapon to kill the enemy.
    /// </summary>
    public class WeaponSusceptibility
    {
        public WeaponSusceptibility() { }

        public WeaponSusceptibility(Weapon weapon, int shots)
        {
            Weapon = weapon;
            Shots = shots;
        }

        /// <summary>
        /// The weapon the implicit enemy is susceptible to
        /// </summary>
        public Weapon Weapon { get; set; }

        /// <summary>
        /// The number of perfectly-aimed shots of the weapon it would take to kill the implicit enemy
        /// </summary>
        public int Shots { get; set; }
    }
}
