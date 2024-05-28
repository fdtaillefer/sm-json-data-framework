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
        public static IEnumerable<UnfinalizedWeapon> NamesToWeapons(this IEnumerable<string> names, UnfinalizedSuperMetroidModel model)
        {
            return names.SelectMany(n => n.NameToWeapons(model)).Distinct(ObjectReferenceEqualityComparer<UnfinalizedWeapon>.Default);
        }

        /// <summary>
        /// Converts a string, which represents a weapon name or weapon category, to a sequence of weapons
        /// (based on the weapons in the provided SuperMetroidModel).
        /// </summary>
        /// <param name="name">The name to convert</param>
        /// <param name="model">A SuperMetroidModel that contains existing weapons</param>
        /// <returns>A sequence of Weapons, or null if the string matches no weapon or category</returns>
        public static IEnumerable<UnfinalizedWeapon> NameToWeapons(this string name, UnfinalizedSuperMetroidModel model)
        {
            if(model.Weapons.TryGetValue(name, out UnfinalizedWeapon weapon))
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
        public static IList<RawTech> SelectAllTechs(this RawTechContainer rawTechContainer)
        {
            return rawTechContainer.TechCategories.SelectMany(category => category.Techs).SelectMany(tech => tech.SelectWithExtensions()).ToList();
        }

        // We put this as an extension rather than in RawTechContainer because we want RawTechContainer to stay a basic model with no logic as much as possible
        /// <summary>
        /// Builds and returns a list of all techs found at the top level of a TechCategory inside this RawTechContainer.
        /// </summary>
        /// <returns></returns>
        public static IList<RawTech> SelectTopLevelTechs(this RawTechContainer rawTechContainer)
        {
            return rawTechContainer.TechCategories.SelectMany(category => category.Techs).ToList();
        }

        // We put this as an extension rather than in RawTech because we want RawTech to stay a basic model with no logic as much as possible
        /// <summary>
        /// Returns a list containing this RawTech and all its extension raw techs (and all their own extension raw techs, and so on).
        /// </summary>
        /// <returns></returns>
        public static IList<RawTech> SelectWithExtensions(this RawTech rawTech)
        {
            return rawTech.ExtensionTechs.SelectMany(tech => tech.SelectWithExtensions()).Prepend(rawTech).ToList();
        }

        /// <summary>
        /// Returns an enumeration of the provided IModelElement objects, except those that are logically irrelevant and can always be ignored in a logic context.
        /// </summary>
        /// <param name="elements">Enumeration of elements to filter</param>
        /// <returns></returns>
        public static IEnumerable<T> WhereLogicallyRelevant<T>(this IEnumerable<T> elements) where T : IModelElement
        {
            return elements.Where(element => element.LogicallyRelevant);
        }

        /// <summary>
        /// Returns an enumeration of the provided ILogicalExecutionPreProcessable objects, containing only those that are logically always possible to execute
        /// given the current logical options, regardless of in-game state.
        /// </summary>
        /// <param name="elements">Enumeration of elements to filter</param>
        /// <returns></returns>
        public static IEnumerable<T> WhereLogicallyAlways<T>(this IEnumerable<T> elements) where T : ILogicalExecutionPreProcessable
        {
            return elements.Where(element => element.LogicallyAlways);
        }

        /// <summary>
        /// Returns an enumeration of the provided ILogicalExecutionPreProcessable objects, containing only those that are logically always possible to execute for free
        /// given the current logical options, regardless of in-game state.
        /// </summary>
        /// <param name="elements">Enumeration of elements to filter</param>
        /// <returns></returns>
        public static IEnumerable<T> WhereLogicallyFree<T>(this IEnumerable<T> elements) where T : ILogicalExecutionPreProcessable
        {
            return elements.Where(element => element.LogicallyFree);
        }

        public static bool ContainsFlag(this IReadOnlyDictionary<string, UnfinalizedGameFlag> flagDictionary, string gameFlagName)
        {
            return flagDictionary.ContainsKey(gameFlagName);
        }

        public static bool ContainsFlag(this IReadOnlyDictionary<string, UnfinalizedGameFlag> flagDictionary, UnfinalizedGameFlag gameFlag)
        {
            return flagDictionary.ContainsKey(gameFlag.Name);
        }

        public static bool ContainsLock(this IReadOnlyDictionary<string, UnfinalizedNodeLock> lockDictionary, string lockName)
        {
            return lockDictionary.ContainsKey(lockName);
        }

        public static bool ContainsLock(this IReadOnlyDictionary<string, UnfinalizedNodeLock> lockDictionary, UnfinalizedNodeLock nodeLock)
        {
            return lockDictionary.ContainsKey(nodeLock.Name);
        }

        public static bool ContainsNode(this IReadOnlyDictionary<string, UnfinalizedRoomNode> nodeDictionary, string nodeName)
        {
            return nodeDictionary.ContainsKey(nodeName);
        }

        public static bool ContainsNode(this IReadOnlyDictionary<string, UnfinalizedRoomNode> nodeDictionary, UnfinalizedRoomNode node)
        {
            return nodeDictionary.ContainsKey(node.Name);
        }

        public static bool ContainsItem(this IReadOnlyDictionary<string, UnfinalizedItem> itemDictionary, string itemName)
        {
            return itemDictionary.ContainsKey(itemName);
        }

        public static bool ContainsItem(this IReadOnlyDictionary<string, UnfinalizedItem> itemDictionary, UnfinalizedItem item)
        {
            return itemDictionary.ContainsKey(item.Name);
        }

        public static bool ContainsVariaSuit(this IReadOnlyDictionary<string, UnfinalizedItem> itemDictionary, UnfinalizedItem item)
        {
            return itemDictionary.ContainsItem(SuperMetroidModel.VARIA_SUIT_NAME);
        }

        public static bool ContainsGravitySuit(this IReadOnlyDictionary<string, UnfinalizedItem> itemDictionary, UnfinalizedItem item)
        {
            return itemDictionary.ContainsItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
        }

        public static bool ContainsSpeedBooster(this IReadOnlyDictionary<string, UnfinalizedItem> itemDictionary, UnfinalizedItem item)
        {
            return itemDictionary.ContainsItem(SuperMetroidModel.SPEED_BOOSTER_NAME);
        }

        public static bool ContainsFlag(this IReadOnlyDictionary<string, GameFlag> flagDictionary, string gameFlagName)
        {
            return flagDictionary.ContainsKey(gameFlagName);
        }

        public static bool ContainsFlag(this IReadOnlyDictionary<string, GameFlag> flagDictionary, GameFlag gameFlag)
        {
            return flagDictionary.ContainsKey(gameFlag.Name);
        }

        public static bool ContainsLock(this IReadOnlyDictionary<string, NodeLock> lockDictionary, string lockName)
        {
            return lockDictionary.ContainsKey(lockName);
        }

        public static bool ContainsLock(this IReadOnlyDictionary<string, NodeLock> lockDictionary, NodeLock nodeLock)
        {
            return lockDictionary.ContainsKey(nodeLock.Name);
        }

        public static bool ContainsNode(this IReadOnlyDictionary<string, RoomNode> nodeDictionary, string nodeName)
        {
            return nodeDictionary.ContainsKey(nodeName);
        }

        public static bool ContainsNode(this IReadOnlyDictionary<string, RoomNode> nodeDictionary, RoomNode node)
        {
            return nodeDictionary.ContainsKey(node.Name);
        }

        public static bool ContainsItem(this IReadOnlyDictionary<string, Item> itemDictionary, string itemName)
        {
            return itemDictionary.ContainsKey(itemName);
        }

        public static bool ContainsItem(this IReadOnlyDictionary<string, Item> itemDictionary, Item item)
        {
            return itemDictionary.ContainsKey(item.Name);
        }

        public static bool ContainsVariaSuit(this IReadOnlyDictionary<string, Item> itemDictionary)
        {
            return itemDictionary.ContainsItem(SuperMetroidModel.VARIA_SUIT_NAME);
        }

        public static bool ContainsGravitySuit(this IReadOnlyDictionary<string, Item> itemDictionary)
        {
            return itemDictionary.ContainsItem(SuperMetroidModel.GRAVITY_SUIT_NAME);
        }

        public static bool ContainsSpeedBooster(this IReadOnlyDictionary<string, Item> itemDictionary)
        {
            return itemDictionary.ContainsItem(SuperMetroidModel.SPEED_BOOSTER_NAME);
        }

        public static bool ContainsItem(this IReadOnlySet<string> itemNameSet, string itemName)
        {
            return itemNameSet.Contains(itemName);
        }

        public static bool ContainsItem(this IReadOnlySet<string> itemNameSet, Item item)
        {
            return itemNameSet.Contains(item.Name);
        }

        public static bool ContainsVariaSuit(this IReadOnlySet<string> itemNameSet)
        {
            return itemNameSet.Contains(SuperMetroidModel.VARIA_SUIT_NAME);
        }

        public static bool ContainsGravitySuit(this IReadOnlySet<string> itemNameSet)
        {
            return itemNameSet.Contains(SuperMetroidModel.GRAVITY_SUIT_NAME);
        }

        public static bool ContainsSpeedBooster(this IReadOnlySet<string> itemNameSet)
        {
            return itemNameSet.Contains(SuperMetroidModel.SPEED_BOOSTER_NAME);
        }
    }
}
