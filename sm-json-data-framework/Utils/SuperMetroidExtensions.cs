using sm_json_data_framework.Models;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Raw.Techs;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
            return names.SelectMany(n => n.NameToWeapons(model)).Distinct(ObjectReferenceEqualityComparer<Weapon>.Default);
        }

        /// <summary>
        /// Converts a string, which represents a weapon name or weapon category, to a sequence of weapons
        /// (based on the weapons in the provided SuperMetroidModel).
        /// </summary>
        /// <param name="name">The name to convert</param>
        /// <param name="model">A SuperMetroidModel that contains existing weapons</param>
        /// <returns>A sequence of Weapons, or null if the string matches no weapon or category</returns>
        public static IEnumerable<Weapon> NameToWeapons(this string name, SuperMetroidModel model)
        {
            if(model.Weapons.TryGetValue(name, out Weapon weapon))
            {
                return new[] { weapon };
            }
            else
            {
                try
                {
                    WeaponCategoryEnum category = (WeaponCategoryEnum)Enum.Parse(typeof(WeaponCategoryEnum), name);
                    return model.WeaponsByCategory[category];
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
        }

        // We put this as an extension rather than in RawTechContainer because we want RawTechContainer to stay a basic model with no logic as much as possible
        /// <summary>
        /// Builds and returns a list of all techs found inside this RawTechContainer (at any level).
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<RawTech> SelectAllTechs(this RawTechContainer rawTechContainer)
        {
            return rawTechContainer.TechCategories.SelectMany(category => category.Techs).SelectMany(tech => tech.SelectWithExtensions()).ToList();
        }

        // We put this as an extension rather than in RawTech because we want RawTech to stay a basic model with no logic as much as possible
        /// <summary>
        /// Returns a list containing this RawTech and all its extension raw techs (and all their own extension raw techs, and so on).
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<RawTech> SelectWithExtensions(this RawTech rawTech)
        {
            return rawTech.ExtensionTechs.SelectMany(tech => tech.SelectWithExtensions()).Prepend(rawTech).ToList();
        }

        /// <summary>
        /// Returns a dictionary containing the strats in this dictionary that are enabled.
        /// </summary>
        /// <param name="strats">This dictionary of strats</param>
        /// <param name="model">A model which can be used to figure out whether a strat is enabled</param>
        /// <returns></returns>
        public static IDictionary<string, Strat> WhereEnabled(this IDictionary<string, Strat> strats, SuperMetroidModel model)
        {
            return strats.Where(kvp => model.LogicalOptions.IsStratEnabled(kvp.Value)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static bool ContainsFlag(this IDictionary<string, GameFlag> flagDictionary, string gameFlagName)
        {
            return flagDictionary.ContainsKey(gameFlagName);
        }

        public static bool ContainsFlag(this IDictionary<string, GameFlag> flagDictionary, GameFlag gameFlag)
        {
            return flagDictionary.ContainsKey(gameFlag.Name);
        }

        public static bool ContainsLock(this IDictionary<string, NodeLock> lockDictionary, string lockName)
        {
            return lockDictionary.ContainsKey(lockName);
        }

        public static bool ContainsLock(this IDictionary<string, NodeLock> lockDictionary, NodeLock nodeLock)
        {
            return lockDictionary.ContainsKey(nodeLock.Name);
        }

        public static bool ContainsNode(this IDictionary<string, RoomNode> nodeDictionary, string nodeName)
        {
            return nodeDictionary.ContainsKey(nodeName);
        }

        public static bool ContainsNode(this IDictionary<string, RoomNode> nodeDictionary, RoomNode node)
        {
            return nodeDictionary.ContainsKey(node.Name);
        }

        public static bool ContainsItem(this IDictionary<string, Item> itemDictionary, string itemName)
        {
            return itemDictionary.ContainsKey(itemName);
        }

        public static bool ContainsItem(this IDictionary<string, Item> itemDictionary, Item item)
        {
            return itemDictionary.ContainsKey(item.Name);
        }

        public static bool ContainsVariaSuit(this IDictionary<string, Item> itemDictionary, Item item)
        {
            return itemDictionary.ContainsItem(SuperMetroidModel.VARIA_SUIT_NAME);
        }

        public static bool ContainsGravitySuit(this IDictionary<string, Item> itemDictionary, Item item)
        {
            return itemDictionary.ContainsItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
        }

        public static bool ContainsSpeedBooster(this IDictionary<string, Item> itemDictionary, Item item)
        {
            return itemDictionary.ContainsItem(SuperMetroidModel.SPEED_BOOSTER_NAME);
        }
    }
}
