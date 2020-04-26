using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Navigation
{
    /// <summary>
    /// A class that takes an InGameState and offers functionality to navigate around, and interact with, the game.
    /// </summary>
    public class GameNavigator
    {
        // Ok so how do I want this to work? We'll have operations that will have results.
        // What are things you can do:
        // - Follow a link
        // - Interact with current node
        //   - Remember that interacting with a door means you change rooms
        // - Needs some kind of support for a remote exit.
        // - In the case of door locks, you could absolutely attempt to open the lock without interacting with it
        // - Farm enemies
        //   - Optionally you could provide a door to use to reset the room and keep farming (mandatory if the enemies don't respawn)
        //   - Optionally you could indicate specific enemies you want to farm
        //   - Optionally you could indicate a weapon to use (only worth doing if no effective free weapon available)
        // Anything else?
        // - There could be some more complex operations like "go to THIS node", but then that can lead to evaluating a lot of things to find the cheapest option.
        //   Still, I'm sure it could be worth.
        //
        // This would maintain the new current InGameState, while possibly leaving previous ones in memory to allow some undo/redo shenanigans.
        // 
        // Failure to do an action would leave the state unchanged and return something to indicate failure.
        //
        // We could possibly also have a log of previous actions in here, maintained alongside the previous states.
        //
        // Some kind of system to customize/override the items at in-game locations could be good, to navigate an actual randomized seed and grab items.















        /// <summary>
        /// Constructor that initializes a GameNavigator with the provided initial state.
        /// </summary>
        /// <param name="initialState">The starting inGameState for this navigator.</param>
        public GameNavigator(InGameState initialState)
        {
            InGameState = initialState;
        }

        private InGameState InGameState {get; set;}


        #region Action methods

        #endregion

        #region Consultation methods

        /// <summary>
        /// Returns the node the player is currently standing at.
        /// </summary>
        /// <returns></returns>
        public RoomNode GetCurrentPosition()
        {
            return InGameState.GetCurrentNode();
        }

        /// <summary>
        /// Returns a sequence of IDs of nodes that have been visited in the current room since entering, in order, 
        /// starting with the node through which the room was entered.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> GetVisitedNodeIds()
        {
            return InGameState.GetVisitedNodeIds();
        }

        /// <summary>
        /// Returns a sequence of IDs of obstacles that have been destroyed in the current room since entering.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetDestroyedObstacleIds()
        {
            return InGameState.GetDestroyedObstacleIds();
        }

        /// <summary>
        /// Returns the current amount of energy, excluding reserve tanks.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentNonReserveEnergy()
        {
            return InGameState.GetCurrentAmount(RechargeableResourceEnum.RegularEnergy);
        }

        /// <summary>
        /// Returns the current amount of energy in reserve tanks.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentReserveEnergy()
        {
            return InGameState.GetCurrentAmount(RechargeableResourceEnum.ReserveEnergy);
        }

        /// <summary>
        /// Returns the current amount of total energy, including reserve tanks.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentTotalEnergy()
        {
            return InGameState.GetCurrentAmount(ConsumableResourceEnum.ENERGY);
        }

        /// <summary>
        /// Returns the current amount of missiles.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentMissiles()
        {
            return InGameState.GetCurrentAmount(RechargeableResourceEnum.Missile);
        }

        /// <summary>
        /// Returns the current amount of super missiles.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentSuperMissiles()
        {
            return InGameState.GetCurrentAmount(RechargeableResourceEnum.Super);
        }

        /// <summary>
        /// Returns the current amount of power bombs.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentPowerBombs()
        {
            return InGameState.GetCurrentAmount(RechargeableResourceEnum.PowerBomb);
        }

        /// <summary>
        /// Returns the current amount of the provided rechargeable resource.
        /// </summary>
        /// <param name="resource">Resource to get the amount of.</param>
        /// <returns></returns>
        public int GetCurrentAmount(RechargeableResourceEnum resource)
        {
            return InGameState.GetCurrentAmount(resource);
        }

        /// <summary>
        /// Returns the current amount of the provided consumable resource. This is almost the same as getting the current amount of a rechargeable resource,
        /// except both types of energy are grouped together.
        /// </summary>
        /// <param name="resource">Resource to get the amount of.</param>
        /// <returns></returns>
        public int GetCurrentAmount(ConsumableResourceEnum resource)
        {
            return InGameState.GetCurrentAmount(resource);
        }

        /// <summary>
        /// Returns the max amount of energy, excluding reserve tanks.
        /// </summary>
        /// <returns></returns>
        public int GetMaxNonReserveEnergy()
        {
            return InGameState.GetMaxAmount(RechargeableResourceEnum.RegularEnergy);
        }

        /// <summary>
        /// Returns the max amount of energy in reserve tanks.
        /// </summary>
        /// <returns></returns>
        public int GetMaxReserveEnergy()
        {
            return InGameState.GetMaxAmount(RechargeableResourceEnum.ReserveEnergy);
        }

        /// <summary>
        /// Returns the max amount of missiles.
        /// </summary>
        /// <returns></returns>
        public int GetMaxMissiles()
        {
            return InGameState.GetMaxAmount(RechargeableResourceEnum.Missile);
        }

        /// <summary>
        /// Returns the max amount of super missiles.
        /// </summary>
        /// <returns></returns>
        public int GetMaxSuperMissiles()
        {
            return InGameState.GetMaxAmount(RechargeableResourceEnum.Super);
        }

        /// <summary>
        /// Returns the max amount of power bombs.
        /// </summary>
        /// <returns></returns>
        public int GetMaxPowerBombs()
        {
            return InGameState.GetMaxAmount(RechargeableResourceEnum.PowerBomb);
        }

        /// <summary>
        /// Returns the max amount of the provided rechargeable resource.
        /// </summary>
        /// <param name="resource">Resource to get the amount of.</param>
        /// <returns></returns>
        public int GetMaxAmount(RechargeableResourceEnum resource)
        {
            return InGameState.GetMaxAmount(resource);
        }

        /// <summary>
        /// Returns a Dictionary of the non-consumable items that are available, mapped by name.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, Item> GetItemsDictionary()
        {
            return InGameState.GetNonConsumableItemsDictionary();
        }

        /// <summary>
        /// Returns a Dictionary of the locks that are opened, mapped by name.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, NodeLock> GetOpenedLocksDictionary()
        {
            return InGameState.GetOpenedLocksDictionary();
        }

        /// <summary>
        /// Returns a Dictionary of the game flags that are active, mapped by name.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, GameFlag> GetActiveGameFlagsDictionary()
        {
            return InGameState.GetActiveGameFlagsDictionary();
        }

        #endregion
    }
}
