using sm_json_data_framework.Models;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
