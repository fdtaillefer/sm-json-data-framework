using Nito.Collections;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
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
        // Failure to do an action would leave the state unchanged and return something to indicate failure.
        //
        // We could possibly also have a log of previous actions in here, maintained alongside the previous states.
        //
        // Some kind of system to customize/override the items at in-game locations could be good, to navigate an actual randomized seed and grab items.







        /// <summary>
        /// Constructor that initializes a GameNavigator with the provided initial state.
        /// </summary>
        /// <param name="initialState">The starting inGameState for this navigator.</param>
        /// <param name="maxPreviousStatesSize">The maximum number of previous states that this navigator should keep in memory.</param>
        public GameNavigator(InGameState initialState, int maxPreviousStatesSize)
        {
            CurrentInGameState = initialState;
            MaxPreviousStatesSize = maxPreviousStatesSize;
        }

        /// <summary>
        /// The InGameState describing the current in-game situation of this GameNavigator.
        /// </summary>
        public InGameState CurrentInGameState {get; private set;}

        private int MaxPreviousStatesSize { get; set; }

        /// <summary>
        /// Contains previous in-game states, paired with the action that was performed on them to obtain the next state.
        /// More recent actions are at the front of the Deque.
        /// </summary>
        private Deque<(INavigationAction action, InGameState initialState)> PreviousStates { get; } = new Deque<(INavigationAction action, InGameState inGameState)>();

        /// <summary>
        /// Contains actions that have been undone, paired with the in-game state that resulted from the action.
        /// </summary>
        private Stack<(INavigationAction action, InGameState resultingState)> UndoneActions { get; } = new Stack<(INavigationAction action, InGameState inGameState)>();

        /// <summary>
        /// Given a performed action and its resulting state, moves this navigator forward,
        /// remembering its current state as a previous one and adopting the resulting state as its current one.
        /// </summary>
        /// <param name="action">The action that was performed</param>
        /// <param name="resultingState">The InGameState that resulted from performing the action</param>
        private void DoAction(INavigationAction action, InGameState resultingState)
        {
            // Can't move forward on a failure
            if(!action.Succeeded)
            {
                return;
            }

            UndoneActions.Clear();
            PreviousStates.AddToFront((action, CurrentInGameState));
            CurrentInGameState = resultingState;

            // If we exceeded the allowed number of previous states, remove the oldest one
            if(PreviousStates.Count > MaxPreviousStatesSize)
            {
                PreviousStates.RemoveFromBack();
            }
        }

        /// <summary>
        /// Undoes the most recent previous action, rolling back to the state before that action.
        /// </summary>
        /// <returns>An action representing the effects of undoing the previous action.
        /// If there are no actions to undo, returns a failure action.</returns>
        public INavigationAction Undo()
        {
            if (PreviousStates.Any())
            {
                (var action, var initialState) = PreviousStates.RemoveFromFront();
                UndoneActions.Push((action, CurrentInGameState));
                CurrentInGameState = initialState;
                return action.Reverse();
            }
            // No actions to undo
            else
            {
                return Failure.Instance;
            }
        }

        /// <summary>
        /// Redoes the last action that was undone, moving forward to the state after that action.
        /// </summary>
        /// <returns>The action that was redone, or a failure if there are no actions to redo.</returns>
        public INavigationAction Redo()
        {
            if(UndoneActions.Count > 0)
            {
                (var action, var resultingState)  = UndoneActions.Pop();
                PreviousStates.AddToFront((action, CurrentInGameState));
                CurrentInGameState = resultingState;
                return action;
            }
            // No actions to redo
            else
            {
                return Failure.Instance;
            }
        }

        #region Action methods

        #endregion

        #region Consultation methods

        /// <summary>
        /// Returns the node the player is currently standing at.
        /// </summary>
        /// <returns></returns>
        public RoomNode GetCurrentPosition()
        {
            return CurrentInGameState.GetCurrentNode();
        }

        /// <summary>
        /// Returns a sequence of IDs of nodes that have been visited in the current room since entering, in order, 
        /// starting with the node through which the room was entered.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> GetVisitedNodeIds()
        {
            return CurrentInGameState.GetVisitedNodeIds();
        }

        /// <summary>
        /// Returns a sequence of IDs of obstacles that have been destroyed in the current room since entering.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetDestroyedObstacleIds()
        {
            return CurrentInGameState.GetDestroyedObstacleIds();
        }

        /// <summary>
        /// Returns the current amount of energy, excluding reserve tanks.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentNonReserveEnergy()
        {
            return CurrentInGameState.GetCurrentAmount(RechargeableResourceEnum.RegularEnergy);
        }

        /// <summary>
        /// Returns the current amount of energy in reserve tanks.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentReserveEnergy()
        {
            return CurrentInGameState.GetCurrentAmount(RechargeableResourceEnum.ReserveEnergy);
        }

        /// <summary>
        /// Returns the current amount of total energy, including reserve tanks.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentTotalEnergy()
        {
            return CurrentInGameState.GetCurrentAmount(ConsumableResourceEnum.ENERGY);
        }

        /// <summary>
        /// Returns the current amount of missiles.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentMissiles()
        {
            return CurrentInGameState.GetCurrentAmount(RechargeableResourceEnum.Missile);
        }

        /// <summary>
        /// Returns the current amount of super missiles.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentSuperMissiles()
        {
            return CurrentInGameState.GetCurrentAmount(RechargeableResourceEnum.Super);
        }

        /// <summary>
        /// Returns the current amount of power bombs.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentPowerBombs()
        {
            return CurrentInGameState.GetCurrentAmount(RechargeableResourceEnum.PowerBomb);
        }

        /// <summary>
        /// Returns the current amount of the provided rechargeable resource.
        /// </summary>
        /// <param name="resource">Resource to get the amount of.</param>
        /// <returns></returns>
        public int GetCurrentAmount(RechargeableResourceEnum resource)
        {
            return CurrentInGameState.GetCurrentAmount(resource);
        }

        /// <summary>
        /// Returns the current amount of the provided consumable resource. This is almost the same as getting the current amount of a rechargeable resource,
        /// except both types of energy are grouped together.
        /// </summary>
        /// <param name="resource">Resource to get the amount of.</param>
        /// <returns></returns>
        public int GetCurrentAmount(ConsumableResourceEnum resource)
        {
            return CurrentInGameState.GetCurrentAmount(resource);
        }

        /// <summary>
        /// Returns the max amount of energy, excluding reserve tanks.
        /// </summary>
        /// <returns></returns>
        public int GetMaxNonReserveEnergy()
        {
            return CurrentInGameState.GetMaxAmount(RechargeableResourceEnum.RegularEnergy);
        }

        /// <summary>
        /// Returns the max amount of energy in reserve tanks.
        /// </summary>
        /// <returns></returns>
        public int GetMaxReserveEnergy()
        {
            return CurrentInGameState.GetMaxAmount(RechargeableResourceEnum.ReserveEnergy);
        }

        /// <summary>
        /// Returns the max amount of missiles.
        /// </summary>
        /// <returns></returns>
        public int GetMaxMissiles()
        {
            return CurrentInGameState.GetMaxAmount(RechargeableResourceEnum.Missile);
        }

        /// <summary>
        /// Returns the max amount of super missiles.
        /// </summary>
        /// <returns></returns>
        public int GetMaxSuperMissiles()
        {
            return CurrentInGameState.GetMaxAmount(RechargeableResourceEnum.Super);
        }

        /// <summary>
        /// Returns the max amount of power bombs.
        /// </summary>
        /// <returns></returns>
        public int GetMaxPowerBombs()
        {
            return CurrentInGameState.GetMaxAmount(RechargeableResourceEnum.PowerBomb);
        }

        /// <summary>
        /// Returns the max amount of the provided rechargeable resource.
        /// </summary>
        /// <param name="resource">Resource to get the amount of.</param>
        /// <returns></returns>
        public int GetMaxAmount(RechargeableResourceEnum resource)
        {
            return CurrentInGameState.GetMaxAmount(resource);
        }

        /// <summary>
        /// Returns a Dictionary of the non-consumable items that are available, mapped by name.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, Item> GetItemsDictionary()
        {
            return CurrentInGameState.GetNonConsumableItemsDictionary();
        }

        /// <summary>
        /// Returns a Dictionary of the locks that are opened, mapped by name.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, NodeLock> GetOpenedLocksDictionary()
        {
            return CurrentInGameState.GetOpenedLocksDictionary();
        }

        /// <summary>
        /// Returns a Dictionary of the game flags that are active, mapped by name.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, GameFlag> GetActiveGameFlagsDictionary()
        {
            return CurrentInGameState.GetActiveGameFlagsDictionary();
        }

        #endregion
    }
}
