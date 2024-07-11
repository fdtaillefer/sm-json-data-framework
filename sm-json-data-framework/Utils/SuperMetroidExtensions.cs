using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Raw.Techs;
using sm_json_data_framework.Models.Requirements;
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
        /// <para>Among the executables in this IEnumerable, attempts to find the least costly one that can be successfully executed using the provided model.
        /// Returns the associated execution result.
        /// If a no-cost executable is found, its result is returned immediately.</para>
        /// <para>If there are no executables, this is an automatic failure.</para>
        /// </summary>
        /// <param name="executables">This enumeration of executables.</param>
        /// <param name="model">The SuperMetroidModel to use to execute the executables</param>
        /// <param name="initialInGameState">The initial in-game state. Will not be modified by this method.</param>
        /// <param name="times">The number of consecutive times the executables should be executed.
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// <param name="acceptationCondition">An optional Predicate that is checked against the resulting in-game state of executions.
        /// Executions whose resulting state does not respect the predicate are rejected.</param>
        /// <returns>The best executable, alongside its ExecutionResult, or default values if none succeeded</returns>
        public static (T bestExecutable, ExecutionResult result) ExecuteBest<T>(this IEnumerable<T> executables, SuperMetroidModel model, 
            ReadOnlyInGameState initialInGameState, int times = 1,
            int previousRoomCount = 0, Predicate<ReadOnlyInGameState> acceptationCondition = null) where T : IExecutable
        {
            // Try to execute all executables, returning whichever spends the lowest amount of resources
            (T bestExecutable, ExecutionResult result) bestResult = (default(T), null);
            foreach (T currentExecutable in executables)
            {
                ExecutionResult currentResult = currentExecutable.Execute(model, initialInGameState, times: times, previousRoomCount: previousRoomCount);

                // If the fulfillment was successful
                if (currentResult != null && (acceptationCondition == null || acceptationCondition.Invoke(currentResult.ResultingState)))
                {

                    // If the fulfillment did not reduce the amount of resources, return immediately
                    if (model.CompareInGameStates(currentResult.ResultingState, initialInGameState) == 0)
                    {
                        return (currentExecutable, currentResult);
                    }

                    // If the resulting state is the best we've found yet, retain it
                    if (bestResult.result == null
                        || model.CompareInGameStates(currentResult.ResultingState, bestResult.result.ResultingState) > 0)
                    {
                        bestResult = (currentExecutable, currentResult);
                    }
                }
            }

            return bestResult;
        }

        /// <summary>
        /// <para>Executes all executables in this IEnumerable successively using the provided model, starting from the provided initialGameState.</para>
        /// <para>This method will give up at the first failed execution and return null.</para>
        /// <para>If there are no executables, this is an automatic success.</para>
        /// </summary>
        /// <typeparam name="T">The type of the executables to execute.</typeparam>
        /// <param name="executables">This enumeration of executables.</param>
        /// <param name="model">The SuperMetroidModel to use to execute the executables</param>
        /// <param name="initialInGameState">The initial in-game state. Will not be modified by this method.</param>
        /// <param name="times">The number of consecutive times the executables should be executed.
        /// <param name="previousRoomCount">The number of playable rooms to go back by (whenever in-room state is relevant). 
        /// 0 means current room, 3 means go back 3 rooms (using last known state), negative values are invalid. Non-playable rooms are skipped.</param>
        /// Only really impacts resource cost, since most items are non-consumable.</param>
        /// <returns>The InGameState obtained by executing all executables, or null if any execution failed.
        /// This will never return the initialInGameState instance.</returns>
        public static ExecutionResult ExecuteAll(this IEnumerable<IExecutable> executables, SuperMetroidModel model, ReadOnlyInGameState initialInGameState, int times = 1, int previousRoomCount = 0)
        {
            // If there are no executables, this is an instant success. Clone the inGameState to respect the contract.
            if (!executables.Any())
            {
                return new ExecutionResult(initialInGameState.Clone());
            }

            // Iterate over all executables, attempting to fulfill them
            ExecutionResult result = null;
            foreach (IExecutable currentExecutable in executables)
            {
                // If this is the first execution, generate an initial result
                if (result == null)
                {
                    result = currentExecutable.Execute(model, initialInGameState, times: times, previousRoomCount: previousRoomCount);
                }
                // If this is not the first execution, apply this execution on top of previous result
                else
                {
                    result = result.AndThen(currentExecutable, model, times: times, previousRoomCount: previousRoomCount);
                }

                // If we failed to execute, give up immediately
                if (result == null)
                {
                    return null;
                }
            }
            return result;
        }

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
