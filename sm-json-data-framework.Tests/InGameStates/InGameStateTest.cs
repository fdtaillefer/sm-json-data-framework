using sm_json_data_framework.Models;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Reading;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.InGameStates
{
    public class InGameStateTest
    {
        /// <summary>
        /// Returns all values of <see cref="ConsumableResourceEnum"/> in a format that can be used by <see cref="MemberDataAttribute"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> ConsumableResourceValues()
        {

            foreach (ConsumableResourceEnum resource in Enum.GetValues(typeof(ConsumableResourceEnum)))
            {
                yield return new object[] { resource };
            }
        }

        // Use a static model to read it only once.
        private static SuperMetroidModel Model { get; set; } = ModelReader.ReadModel();

        [Fact]
        public void ConstructorWithStartConditions_InitializesProperly()
        {
            string startingRoomName = "Business Center";
            int startingNodeId = 7;
            string startingLockName = "Business Center Top Left Green Lock (to Ice Beam Gate)";
            int startingEnergy = 50;
            string maridiaTubeFlag = "f_MaridiaTubeBroken";
            RoomNode variaNode = Model.GetNodeInRoom("Varia Suit Room", 2);
            StartConditions startConditions = new StartConditions
            {
                StartingGameFlags = new GameFlag[] { Model.GameFlags[maridiaTubeFlag] },
                StartingInventory = ItemInventory.CreateVanillaStartingInventory(Model).ApplyAddItem(Model.Items[SuperMetroidModel.VARIA_SUIT_NAME]),
                StartingNode = Model.GetNodeInRoom(startingRoomName, startingNodeId),
                StartingOpenLocks = new NodeLock[] { Model.Locks[startingLockName] },
                StartingTakenItemLocations = new RoomNode[] { variaNode },
                StartingResources = new ResourceCount().ApplyAmountIncrease(RechargeableResourceEnum.RegularEnergy, startingEnergy)
            };

            InGameState inGameState = new InGameState(startConditions);

            Assert.Equal(startingRoomName, inGameState.CurrentRoom.Name);
            Assert.Equal(startingNodeId, inGameState.GetCurrentNode().Id);
            Assert.Contains(startingLockName, inGameState.OpenedLocks.Keys);
            Assert.True(inGameState.Inventory.HasVariaSuit());
            Assert.True(inGameState.TakenItemLocations.ContainsNode(variaNode));
            Assert.Equal(startConditions.BaseResourceMaximums.GetAmount(RechargeableResourceEnum.RegularEnergy),
                inGameState.ResourceMaximums.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(startingEnergy, inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.True(inGameState.ActiveGameFlags.ContainsFlag(maridiaTubeFlag));
        }

        [Theory]
        [MemberData(nameof(ConsumableResourceValues))]
        public void IsResourceAvailable_Requesting0_ReturnsTrue(ConsumableResourceEnum resource)
        {
            ResourceCount resourceCount = new ResourceCount();
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            InGameState inGameState = new InGameState(startConditions);

            Assert.True(inGameState.IsResourceAvailable(resource, 0));
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingExactPresentAmount_Ammo_ReturnsTrue(RechargeableResourceEnum resource)
        {
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amount);
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            InGameState inGameState = new InGameState(startConditions);

            Assert.True(inGameState.IsResourceAvailable(resource.ToConsumableResource(), amount));
        }

        [Fact]
        public void IsResourceAvailable_RequestingExactPresentAmount_Energy_ReturnsFalse()
        {
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amount);
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            InGameState inGameState = new InGameState(startConditions);

            // X energy is not available to spend if you have exactly X energy, because you'd die
            Assert.False(inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, amount));
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_Ammo_ReturnsTrue(RechargeableResourceEnum resource)
        {
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amountToRequest + 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            InGameState inGameState = new InGameState(startConditions);

            Assert.True(inGameState.IsResourceAvailable(resource.ToConsumableResource(), amountToRequest));
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_Energy_ReturnsTrue()
        {
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amountToRequest + 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            InGameState inGameState = new InGameState(startConditions);

            Assert.True(inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, amountToRequest));
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_EnergyMixOfReserveAndNormal_ReturnsTrue()
        {
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 3);
            resourceCount.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            InGameState inGameState = new InGameState(startConditions);

            Assert.True(inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, 5));
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingMoreThanPresentAmount_Ammo_ReturnsFalse(RechargeableResourceEnum resource)
        {
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amountToRequest - 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            InGameState inGameState = new InGameState(startConditions);

            Assert.False(inGameState.IsResourceAvailable(resource.ToConsumableResource(), amountToRequest));
        }

        [Fact]
        public void IsResourceAvailable_RequestingMoreThanPresentAmount_Energy_ReturnsFalse()
        {
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amountToRequest - 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            InGameState inGameState = new InGameState(startConditions);

            Assert.False(inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, amountToRequest));
        }

        [Fact]
        public void Clone_CopiesCorrectly()
        {
            string startingRoomName = "Business Center";
            int startingNodeId = 7;
            string startingLockName = "Business Center Top Left Green Lock (to Ice Beam Gate)";
            int startingEnergy = 50;
            string maridiaTubeFlag = "f_MaridiaTubeBroken";
            RoomNode variaNode = Model.GetNodeInRoom("Varia Suit Room", 2);
            StartConditions startConditions = new StartConditions
            {
                StartingGameFlags = new GameFlag[] { Model.GameFlags[maridiaTubeFlag] },
                StartingInventory = ItemInventory.CreateVanillaStartingInventory(Model).ApplyAddItem(Model.Items[SuperMetroidModel.VARIA_SUIT_NAME]),
                StartingNode = Model.GetNodeInRoom(startingRoomName, startingNodeId),
                StartingOpenLocks = new NodeLock[] { Model.Locks[startingLockName] },
                StartingTakenItemLocations = new RoomNode[] { variaNode },
                StartingResources = new ResourceCount().ApplyAmountIncrease(RechargeableResourceEnum.RegularEnergy, startingEnergy)
            };

            InGameState inGameState = new InGameState(startConditions).Clone();

            Assert.Equal(startingRoomName, inGameState.CurrentRoom.Name);
            Assert.Equal(startingNodeId, inGameState.GetCurrentNode().Id);
            Assert.Contains(startingLockName, inGameState.OpenedLocks.Keys);
            Assert.True(inGameState.Inventory.HasVariaSuit());
            Assert.True(inGameState.TakenItemLocations.ContainsNode(variaNode));
            Assert.Equal(startConditions.BaseResourceMaximums.GetAmount(RechargeableResourceEnum.RegularEnergy),
                inGameState.ResourceMaximums.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(startingEnergy, inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.True(inGameState.ActiveGameFlags.ContainsFlag(maridiaTubeFlag));
        }

        [Fact]
        public void Clone_SeparatesState()
        {
            string startingRoomName = "Business Center";
            int startingNodeId = 7;
            string startingLockName = "Business Center Top Left Green Lock (to Ice Beam Gate)";
            string secondLockName = "Business Center Bottom Left Red Lock (to HiJump E-Tank)";
            int startingEnergy = 50;
            string maridiaTubeFlag = "f_MaridiaTubeBroken";
            RoomNode variaNode = Model.GetNodeInRoom("Varia Suit Room", 2);
            StartConditions startConditions = new StartConditions
            {
                StartingInventory = ItemInventory.CreateVanillaStartingInventory(Model).ApplyAddItem(Model.Items["Missile"]),
                StartingNode = Model.GetNodeInRoom(startingRoomName, startingNodeId),
                StartingOpenLocks = new NodeLock[] { Model.Locks[startingLockName] },
                StartingResources = new ResourceCount().ApplyAmountIncrease(RechargeableResourceEnum.RegularEnergy, startingEnergy)
                    .ApplyAmountIncrease(RechargeableResourceEnum.Missile, 5)
            };

            InGameState inGameState = new InGameState(startConditions);

            // Create and modify a clone
            InGameState clone = inGameState.Clone();
            clone.ApplyVisitNode(Model.GetNodeInRoom(startingRoomName, 8), clone.GetCurrentNode().Links[8].Strats["Base"]);
            clone.ApplyVisitNode(Model.GetNodeInRoom(startingRoomName, 3), clone.GetCurrentNode().Links[3].Strats["Base"]);
            clone.ApplyOpenLock(Model.Locks[secondLockName]);
            clone.ApplyTakeLocation(variaNode);
            clone.ApplyAddItem(Model.Items[SuperMetroidModel.VARIA_SUIT_NAME]);
            clone.ApplyAddItem(Model.Items[SuperMetroidModel.MISSILE_NAME]);
            clone.ApplyAddResource(RechargeableResourceEnum.Missile, 2);
            clone.ApplyAddGameFlag(Model.GameFlags[maridiaTubeFlag]);

            // Make sure the original is unchanged
            Assert.Equal(startingRoomName, inGameState.CurrentRoom.Name);
            Assert.Equal(startingNodeId, inGameState.GetCurrentNode().Id);
            Assert.DoesNotContain(secondLockName, inGameState.OpenedLocks.Keys);
            Assert.False(inGameState.Inventory.HasVariaSuit());
            Assert.False(inGameState.TakenItemLocations.ContainsNode(variaNode));
            Assert.Equal(startConditions.StartingInventory.ResourceMaximums.GetAmount(RechargeableResourceEnum.Missile),
                inGameState.ResourceMaximums.GetAmount(RechargeableResourceEnum.Missile));
            Assert.Equal(5, inGameState.Resources.GetAmount(RechargeableResourceEnum.Missile));
            Assert.False(inGameState.ActiveGameFlags.ContainsFlag(maridiaTubeFlag));
        }
    }
}
