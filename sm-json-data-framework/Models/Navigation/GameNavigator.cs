using Nito.Collections;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Rules;
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
        // Some kind of system to customize/override the items at in-game locations could be good, to navigate an actual randomized seed and grab items.
        // Though this one might be out of scope for GameNavigator. It should probably happen upstream.







        /// <summary>
        /// Constructor that initializes a GameNavigator with the provided initial state.
        /// </summary>
        /// <param name="model">A model that can be used to obtain data about the current game configuration.</param>
        /// <param name="initialState">The starting inGameState for this navigator.</param>
        /// <param name="maxPreviousStatesSize">The maximum number of previous states that this navigator should keep in memory.</param>
        /// <param name="options">Optional game navigation options. If left null, default options will be used.</param>
        public GameNavigator(SuperMetroidModel model, InGameState initialState, int maxPreviousStatesSize, GameNavigatorOptions options = null)
        {
            GameModel = model;
            CurrentInGameState = initialState;
            MaxPreviousStatesSize = maxPreviousStatesSize;
            if (options == null)
            {
                options = new GameNavigatorOptions();
            }
            Options = options;
        }

        private GameNavigatorOptions Options { get; set; }

        /// <summary>
        /// The InGameState describing the current in-game situation of this GameNavigator.
        /// </summary>
        public InGameState CurrentInGameState {get; private set;}

        /// <summary>
        /// A model that can be used to obtain data about the current game configuration.
        /// </summary>
        private SuperMetroidModel GameModel { get; set; }

        private int MaxPreviousStatesSize { get; set; }

        /// <summary>
        /// Contains previous in-game states, paired with the action that was performed on them to obtain the next state.
        /// More recent actions are at the front of the Deque.
        /// </summary>
        private Deque<(AbstractNavigationAction action, InGameState initialState)> PreviousStates { get; } = new Deque<(AbstractNavigationAction action, InGameState inGameState)>();

        /// <summary>
        /// Contains actions that have been undone, paired with the in-game state that resulted from the action.
        /// </summary>
        private Stack<(AbstractNavigationAction action, InGameState resultingState)> UndoneActions { get; } = new Stack<(AbstractNavigationAction action, InGameState inGameState)>();

        #region Action methods

        /// <summary>
        /// Given a performed action and its resulting state, moves this navigator forward,
        /// remembering its current state as a previous one and adopting the resulting state as its current one.
        /// </summary>
        /// <param name="action">The action that was performed</param>
        /// <param name="resultingState">The InGameState that resulted from performing the action</param>
        private void DoAction(AbstractNavigationAction action, InGameState resultingState)
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
        public AbstractNavigationAction Undo()
        {
            if (PreviousStates.Any())
            {
                (var action, var initialState) = PreviousStates.RemoveFromFront();
                UndoneActions.Push((action, CurrentInGameState));
                CurrentInGameState = initialState;
                return action.Reverse(GameModel);
            }
            // No actions to undo
            else
            {
                return new Failure("Undo an action, but there are no actions to undo in the queue");
            }
        }

        /// <summary>
        /// Redoes the last action that was undone, moving forward to the state after that action.
        /// </summary>
        /// <returns>The action that was redone, or a failure if there are no actions to redo.</returns>
        public AbstractNavigationAction Redo()
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
                return new Failure("Redo an action, but there are no actions to redo in the queue");
            }
        }

        /// <summary>
        /// Attempts to move from current node to the node with the provided ID in the current room.
        /// This requires a direct link.
        /// </summary>
        /// <param name="nodeId">The ID of the node to move to</param>
        /// <returns>The resulting action. If moving fails, returns a failure action.</returns>
        public AbstractNavigationAction MoveToNode(int nodeId)
        {
            string intent = $"Move to node {nodeId}";

            // Does that node exist?
            if (!CurrentInGameState.GetCurrentRoom().Nodes.TryGetValue(nodeId, out RoomNode destinationNode))
            {
                intent = intent + $", but that node doesn't exist in {CurrentInGameState.GetCurrentRoom().Name}";
                return new Failure(intent);
            }

            // Find a link from current node to that node
            LinkTo linkTo = CurrentInGameState.GetCurrentLinkTo(nodeId);
            if (linkTo == null)
            {
                intent = intent + $", but no links found to that node from node {CurrentInGameState.GetCurrentNode().Id}";
                return new Failure(intent);
            }

            // We found a link, try to follow it
            var(strat, result) = GameModel.ExecuteBest(linkTo.Strats, CurrentInGameState);
            
            // If no strat of the link was successful, this is a failure
            if (strat == null)
            {
                intent = intent + $", but could not execute any strats";
                return new Failure(intent);
            }

            // A strat succeeded, update the in-game position
            result.ResultingState.ApplyVisitNode(linkTo.TargetNode, strat);

            // If a strat succeeded, create a corresponding action
            var action = new MoveToNodeAction(intent, GameModel, CurrentInGameState, strat, result);

            // Register the action as done and return it
            DoAction(action, result.ResultingState);
            return action;
        }

        /// <summary>
        /// Attempts to interact with the current node. If the node is locked, attempts to open or bypass the lock beforehand.
        /// </summary>
        /// <returns>The resulting action. If interaction fails, returns a failure action.</returns>
        public AbstractNavigationAction InteractWithNode()
        {
            RoomNode node = CurrentInGameState.GetCurrentNode();

            string intent = node.NodeType switch
            {
                NodeTypeEnum.Door => $"Exit through node {node.Name}",
                NodeTypeEnum.Exit => $"Exit through node {node.Name}",
                NodeTypeEnum.Event => $"Activate event {node.Name}",
                NodeTypeEnum.Item => $"Pick up item at {node.Name}",
                NodeTypeEnum.Entrance => $"Interact with {node.Name}",
                NodeTypeEnum.Junction => $"Interact with {node.Name}"
            };

            ExecutionResult result = null;

            bool nodeAccessible = true;
            bool bypassedLock = false;
            foreach(NodeLock currentLock in node.Locks.Where(l => l.IsActive(GameModel, CurrentInGameState)))
            {
                // Try to open lock
                var currentResult = currentLock.OpenExecution.Execute(GameModel, result?.ResultingState ?? CurrentInGameState);

                // If opening failed, try to bypass
                if (currentResult == null)
                {
                    currentResult = currentLock.BypassExecution.Execute(GameModel, result?.ResultingState ?? CurrentInGameState);

                    // If bypass also failed, we can't interact with the node because of this lock
                    if (currentResult == null)
                    {
                        nodeAccessible = false;
                    }
                    else
                    {
                        bypassedLock = true;
                    }
                }

                if (result == null)
                {
                    result = currentResult;
                }
                else
                {
                    result.ApplySubsequentResult(currentResult);
                }
            }

            // We couldn't get through a lock, return a failure
            if (!nodeAccessible)
            {
                intent = intent + ", but a lock is preventing access";
                return new Failure(intent);
            }

            // Locks are taken care of, but maybe interaction with the node has straight requirements as well
            var interactionResult = node.InteractionRequires.Execute(GameModel, result?.ResultingState ?? CurrentInGameState);
            if(result == null)
            {
                result = interactionResult;
            }
            else
            {
                result.ApplySubsequentResult(interactionResult);
            }
            // Interaction failed. Drop the lock phase altogether.
            // If the goal is to unlock the node regardless of interaction, there is a method for that.
            if (result == null)
            {
                intent = intent + ", but couldn't fulfill interaction requirements";
                return new Failure(intent);
            }

            // At this point, the interaction can happen. Process everything on the node regardless of type.

            // Activate game flags
            foreach(GameFlag flag in node.Yields)
            {
                result.ResultingState.ApplyAddGameFlag(flag);
            }

            // Take item at location
            if(node.NodeItem != null && !CurrentInGameState.IsItemLocationTaken(node.Name))
            {
                result.ResultingState.ApplyTakeLocation(node);
                result.ResultingState.ApplyAddItem(node.NodeItem);

                // If we need to adjust resources according to normal game behavior...
                if (Options.AddResourcesOnExpansionPickup && node.NodeItem is ExpansionItem expansionItem)
                {
                    switch (GameModel.Rules.GetExpansionPickupRestoreBehavior(expansionItem.Resource))
                    {
                        case ExpansionPickupRestoreBehaviorEnum.ADD_PICKED_UP:
                            result.ResultingState.ApplyAddResource(expansionItem.Resource, expansionItem.ResourceAmount);
                            break;
                        case ExpansionPickupRestoreBehaviorEnum.REFILL:
                            result.ResultingState.ApplyRefillResource(expansionItem.Resource);
                            break;
                        // If we don't know or if the pickup has no effect, do nothing
                        case ExpansionPickupRestoreBehaviorEnum.NOTHING:
                        default:
                            break;
                    }
                }
            }

            // Use any refill utility
            foreach(UtilityEnum utility in node.Utility)
            {
                switch(utility)
                {
                    case UtilityEnum.Energy:
                        result.ResultingState.ApplyRefillResource(RechargeableResourceEnum.RegularEnergy);
                        break;
                    case UtilityEnum.Reserve:
                        result.ResultingState.ApplyRefillResource(RechargeableResourceEnum.ReserveEnergy);
                        break;
                    case UtilityEnum.Missile:
                        result.ResultingState.ApplyRefillResource(RechargeableResourceEnum.Missile);
                        break;
                    case UtilityEnum.Super:
                        result.ResultingState.ApplyRefillResource(RechargeableResourceEnum.Super);
                        break;
                    case UtilityEnum.PowerBomb:
                        result.ResultingState.ApplyRefillResource(RechargeableResourceEnum.PowerBomb);
                        break;
                    // Other utilities don't do anything for us
                    default:
                        break;
                }
            }

            // Use node to exit the room
            if (node.OutNode != null)
            {
                result.ResultingState.ApplyEnterRoom(node.OutNode, bypassedLock);
            }

            // Create an action to return
            InteractWithNodeAction action = new InteractWithNodeAction(intent, GameModel, CurrentInGameState, result);

            // Register the action as done and return it
            DoAction(action, result.ResultingState);
            return action;
        }

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
