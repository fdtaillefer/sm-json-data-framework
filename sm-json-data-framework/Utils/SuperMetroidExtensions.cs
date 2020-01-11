using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Utils
{
    public static class SuperMetroidExtensions
    {
        /// <summary>
        /// Converts a sequence of strings, which represent weapon names or weapon categories, to a sequence of weapons
        /// (based on the weapons in the provided SuperMetroidModel).
        /// </summary>
        /// <param name="names">The names to convert</param>
        /// <param name="model">A SuperMetroidModel that contains existing weapons</param>
        /// <returns>A sequence of Weapons</returns>
        public static IEnumerable<Weapon> NamesToWeapons(this IEnumerable<string> names, SuperMetroidModel model)
        {
            return names.SelectMany(n => n.NameToWeapons(model)).Distinct();
        }

        /// <summary>
        /// Converts a string, which represents a weapon name or weapon category, to a sequence of weapons
        /// (based on the weapons in the provided SuperMetroidModel).
        /// </summary>
        /// <param name="name">The name to convert</param>
        /// <param name="model">A SuperMetroidModel that contains existing weapons</param>
        /// <returns>A sequence of Weapons</returns>
        public static IEnumerable<Weapon> NameToWeapons(this string name, SuperMetroidModel model)
        {
            return model.Weapons.ContainsKey(name) ?
                    new[] { model.Weapons[name] } : model.WeaponsByCategory[(WeaponCategoryEnum)Enum.Parse(typeof(WeaponCategoryEnum), name)];
        }
    }
}
