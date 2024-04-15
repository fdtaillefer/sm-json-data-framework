using sm_json_data_framework.Models;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Rooms;
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
        /// Returns all values of <see cref="RechargeableResourceEnum"/> in a format that can be used by <see cref="MemberDataAttribute"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> RechargeableResourceValues()
        {

            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                yield return new object[] { resource };
            }
        }

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
                StartingResources = new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, startingEnergy)
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
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            InGameState inGameState = new InGameState(startConditions);

            Assert.False(inGameState.IsResourceAvailable(ConsumableResourceEnum.ENERGY, amountToRequest));
        }

        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void ApplyAddResource_AddsAmount(RechargeableResourceEnum resource)
        {
            int initialAmount = 2;
            int addedAmount = 5;
            int expectedamount = 7;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                startResources.ApplyAmount(loopResource, initialAmount);
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            InGameState inGameState = new InGameState(startConditions);

            inGameState.ApplyAddResource(resource, addedAmount);

            Assert.Equal(expectedamount, inGameState.Resources.GetAmount(resource));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != resource)
                {
                    Assert.Equal(initialAmount, inGameState.Resources.GetAmount(otherResource));
                }
            }
        }

        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void ApplyAddResource_DoesNotExceedMax(RechargeableResourceEnum resource)
        {
            int initialAmount = 2;
            int addedAmount = 150;
            int expectedamount = 100;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                startResources.ApplyAmount(loopResource, initialAmount);
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            InGameState inGameState = new InGameState(startConditions);

            inGameState.ApplyAddResource(resource, addedAmount);

            Assert.Equal(expectedamount, inGameState.Resources.GetAmount(resource));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != resource)
                {
                    Assert.Equal(initialAmount, inGameState.Resources.GetAmount(otherResource));
                }
            }
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void ApplyConsumeResource_Ammo_SetsAmount(RechargeableResourceEnum resource)
        {
            int initialAmount = 5;
            int removedAmount = 2;
            int expectedAmount = 3;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                startResources.ApplyAmount(loopResource, initialAmount);
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            InGameState inGameState = new InGameState(startConditions);



            inGameState.ApplyConsumeResource(resource.ToConsumableResource(), removedAmount);

            Assert.Equal(expectedAmount, inGameState.Resources.GetAmount(resource));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != resource)
                {
                    Assert.Equal(initialAmount, inGameState.Resources.GetAmount(otherResource));
                }
            }
        }

        [Fact]
        public void ApplyConsumeResource_RegularEnergy_SetsAmount()
        {
            int initialAmount = 5;
            int removedAmount = 2;
            int expectedAmount = 3;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                // Don't put any reserve energy in, we're checking regular energy
                if (loopResource != RechargeableResourceEnum.ReserveEnergy)
                {
                    startResources.ApplyAmount(loopResource, initialAmount);
                }
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            InGameState inGameState = new InGameState(startConditions);

            inGameState.ApplyConsumeResource(ConsumableResourceEnum.ENERGY, removedAmount);



            Assert.Equal(expectedAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != RechargeableResourceEnum.RegularEnergy && otherResource != RechargeableResourceEnum.ReserveEnergy)
                {
                    Assert.Equal(initialAmount, inGameState.Resources.GetAmount(otherResource));
                }
            }
        }

        [Fact]
        public void ApplyConsumeResource_ReserveEnergy_SetsAmount()
        {
            int initialAmount = 5;
            int removedAmount = 2;
            int expectedAmount = 3;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                // Don't put any regular energy in, we're checking reserve energy
                if (loopResource != RechargeableResourceEnum.RegularEnergy)
                {
                    startResources.ApplyAmount(loopResource, initialAmount);
                }
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            InGameState inGameState = new InGameState(startConditions);

            inGameState.ApplyConsumeResource(ConsumableResourceEnum.ENERGY, removedAmount);

            Assert.Equal(expectedAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.ReserveEnergy));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != RechargeableResourceEnum.RegularEnergy && otherResource != RechargeableResourceEnum.ReserveEnergy)
                {
                    Assert.Equal(initialAmount, inGameState.Resources.GetAmount(otherResource));
                }
            }
        }

        [Fact]
        public void ApplyConsumeResource_MixedEnergy_ConsumesRegularEnergyFirst()
        {
            int initialRegularAmount = 10;
            int initialReserveAmount = 10;
            int reductionAmount = 12;
            int expectedRegularAmount = 1;
            int expectedReserveAmount = 7;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, initialRegularAmount)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, initialReserveAmount);
            ResourceCount maxResources = startResources.Clone()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, maxAmount)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, maxAmount);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            InGameState inGameState = new InGameState(startConditions);

            inGameState.ApplyConsumeResource(ConsumableResourceEnum.ENERGY, reductionAmount);

            Assert.Equal(expectedRegularAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(expectedReserveAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }

        [Fact]
        public void ApplyConsumeResource_MixedEnergy_ConsumesReservesBeforeGoingTo0Regular()
        {
            int initialRegularAmount = 10;
            int initialReserveAmount = 10;
            int reductionAmount = 19;
            int expectedRegularAmount = 1;
            int expectedReserveAmount = 0;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, initialRegularAmount)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, initialReserveAmount);
            ResourceCount maxResources = startResources.Clone()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, maxAmount)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, maxAmount);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            InGameState inGameState = new InGameState(startConditions);

            inGameState.ApplyConsumeResource(ConsumableResourceEnum.ENERGY, reductionAmount);

            Assert.Equal(expectedRegularAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(expectedReserveAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }

        [Fact]
        public void ApplyConsumeResource_MixedEnergy_ConsumesReservesBeforeGoingToNegativeRegular()
        {
            int initialRegularAmount = 10;
            int initialReserveAmount = 10;
            int reductionAmount = 22;
            int expectedRegularAmount = -2;
            int expectedReserveAmount = 0;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, initialRegularAmount)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, initialReserveAmount);
            ResourceCount maxResources = startResources.Clone()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, maxAmount)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, maxAmount);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            InGameState inGameState = new InGameState(startConditions);

            inGameState.ApplyConsumeResource(ConsumableResourceEnum.ENERGY, reductionAmount);

            Assert.Equal(expectedRegularAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(expectedReserveAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }

        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void ApplyRefillResource_RechargeableResource_SetsToMax(RechargeableResourceEnum resource)
        {
            int initialAmount = 5;
            int maxAmount = 100;

            ResourceCount startResources = new ResourceCount()
                .ApplyAmount(resource, initialAmount);
            ResourceCount maxResources = startResources.Clone()
                .ApplyAmount(resource, maxAmount);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            InGameState inGameState = new InGameState(startConditions);

            inGameState.ApplyRefillResource(resource);

            Assert.Equal(maxAmount, inGameState.Resources.GetAmount(resource));
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void ApplyRefillResource_ConsumableAmmo_SetsToMax(RechargeableResourceEnum resource)
        {
            int initialAmount = 5;
            int maxAmount = 100;

            ResourceCount startResources = new ResourceCount()
                .ApplyAmount(resource, initialAmount);
            ResourceCount maxResources = startResources.Clone()
                .ApplyAmount(resource, maxAmount);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            InGameState inGameState = new InGameState(startConditions);

            inGameState.ApplyRefillResource(resource.ToConsumableResource());

            Assert.Equal(maxAmount, inGameState.Resources.GetAmount(resource));
        }

        [Fact]
        public void ApplyRefillResource_ConsumableEnergy_SetsBothTypesToMax()
        {
            int initialAmount = 5;
            int maxAmount = 100;

            ResourceCount startResources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, initialAmount)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, initialAmount);
            ResourceCount maxResources = startResources.Clone()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, maxAmount)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, maxAmount);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            InGameState inGameState = new InGameState(startConditions);

            inGameState.ApplyRefillResource(ConsumableResourceEnum.ENERGY);

            Assert.Equal(maxAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(maxAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }

        [Fact]
        public void GetResourceVariationWith_ReturnsPositiveAndNegativeAnd0()
        {
            ResourceCount resourceMaximums = new ResourceCount();
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceMaximums.ApplyAmount(resource, 100);
            }

            ResourceCount resources1 = new ResourceCount();
            ResourceCount resources2 = new ResourceCount();

            resources1.ApplyAmount(RechargeableResourceEnum.Missile, 50);
            resources2.ApplyAmount(RechargeableResourceEnum.Missile, 45);

            resources1.ApplyAmount(RechargeableResourceEnum.Super, 45);
            resources2.ApplyAmount(RechargeableResourceEnum.Super, 50);

            resources1.ApplyAmount(RechargeableResourceEnum.PowerBomb, 10);
            resources2.ApplyAmount(RechargeableResourceEnum.PowerBomb, 0);

            resources1.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 0);
            resources2.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 10);

            resources1.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 40);
            resources2.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 40);

            StartConditions startConditions1 = StartConditions.CreateVanillaStartConditions(Model);
            startConditions1.StartingInventory = startConditions1.StartingInventory.WithBaseResourceMaximums(resourceMaximums);
            startConditions1.StartingResources = resources1;
            InGameState inGameState1 = new InGameState(startConditions1);

            StartConditions startConditions2 = StartConditions.CreateVanillaStartConditions(Model);
            startConditions2.StartingInventory = startConditions2.StartingInventory.WithBaseResourceMaximums(resourceMaximums);
            startConditions2.StartingResources = resources2;
            InGameState inGameState2 = new InGameState(startConditions2);

            ResourceCount result = inGameState1.GetResourceVariationWith(inGameState2);

            Assert.Equal(5, result.GetAmount(RechargeableResourceEnum.Missile));
            Assert.Equal(-5, result.GetAmount(RechargeableResourceEnum.Super));
            Assert.Equal(10, result.GetAmount(RechargeableResourceEnum.PowerBomb));
            Assert.Equal(-10, result.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(0, result.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }

        [Fact]
        public void GetFullRechargeableResources_ReturnsFullResources()
        {
            ResourceCount resourceMaximums = new ResourceCount();
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceMaximums.ApplyAmount(resource, 100);
            }
            ResourceCount resources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.Super, 100)
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 100);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceMaximums);
            startConditions.StartingResources = resources;
            InGameState inGameState = new InGameState(startConditions);

            IEnumerable<RechargeableResourceEnum> result = inGameState.GetFullRechargeableResources();

            Assert.Equal(2, result.Count());
            Assert.Contains(RechargeableResourceEnum.Super, result);
            Assert.Contains(RechargeableResourceEnum.RegularEnergy, result);
        }

        [Fact]
        public void GetFullConsumableResources_ReturnsFullResources()
        {
            ResourceCount resourceMaximums = new ResourceCount();
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceMaximums.ApplyAmount(resource, 100);
            }
            ResourceCount resources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.Super, 100)
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 100)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 100);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceMaximums);
            startConditions.StartingResources = resources;
            InGameState inGameState = new InGameState(startConditions);

            IEnumerable<ConsumableResourceEnum> result = inGameState.GetFullConsumableResources();

            Assert.Equal(2, result.Count());
            Assert.Contains(ConsumableResourceEnum.SUPER, result);
            Assert.Contains(ConsumableResourceEnum.ENERGY, result);
        }

        [Fact]
        public void GetFullConsumableResources_OneEnergyTypeFull_DoesNotReturnEnergy()
        {
            ResourceCount resourceMaximums = new ResourceCount();
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceMaximums.ApplyAmount(resource, 100);
            }
            ResourceCount resources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.Super, 100)
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 100);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceMaximums);
            startConditions.StartingResources = resources;
            InGameState inGameState = new InGameState(startConditions);

            IEnumerable<ConsumableResourceEnum> result = inGameState.GetFullConsumableResources();

            Assert.Single(result);
            Assert.Contains(ConsumableResourceEnum.SUPER, result);
        }

        [Fact]
        public void GetUnneededDrops_SamusIsFull_ReturnsAllButNoDrops()
        {
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(Model));

            IEnumerable<EnemyDropEnum> result = inGameState.GetUnneededDrops(Model);

            IEnumerable<EnemyDropEnum> expected = Enum.GetValues<EnemyDropEnum>().Except(new EnemyDropEnum[] { EnemyDropEnum.NO_DROP });
            Assert.Equal(expected.Count(), result.Count());
            foreach (EnemyDropEnum drop in expected)
            {
                Assert.Contains(drop, result);
            }
        }

        [Fact]
        public void GetUnneededDrops_SamusNeedsEverything_ReturnsEmpty()
        {
            int initialAmount = 5;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                startResources.ApplyAmount(loopResource, initialAmount);
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            InGameState inGameState = new InGameState(startConditions);

            IEnumerable<EnemyDropEnum> result = inGameState.GetUnneededDrops(Model);

            Assert.Empty(result);
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.RegularEnergy)]
        [InlineData(RechargeableResourceEnum.ReserveEnergy)]
        public void GetUnneededDrops_OnlyOneTypeOfEnergyNotFull_DoesNotReturnEnergyDrops(RechargeableResourceEnum energyResource)
        {
            int initialAmount = 5;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (loopResource == energyResource)
                {
                    startResources.ApplyAmount(loopResource, initialAmount);
                }
                else
                {
                    startResources.ApplyAmount(loopResource, maxAmount);
                }
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            InGameState inGameState = new InGameState(startConditions);

            IEnumerable<EnemyDropEnum> result = inGameState.GetUnneededDrops(Model);

            // Although one type of energy is full, the other isn't, so energy drops are needed and should NOT be returned
            Assert.DoesNotContain(EnemyDropEnum.SMALL_ENERGY, result);
            Assert.DoesNotContain(EnemyDropEnum.BIG_ENERGY, result);
        }

        [Fact]
        public void ApplyAddGameFlag_AddsIt()
        {
            GameFlag flag1 = Model.GameFlags["f_ZebesAwake"];
            GameFlag flag2 = Model.GameFlags["f_DefeatedBombTorizo"];

            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(Model));

            inGameState
                .ApplyAddGameFlag(flag1)
                .ApplyAddGameFlag(flag2);

            Assert.Equal(2, inGameState.ActiveGameFlags.Count);
            Assert.Contains(flag1, inGameState.ActiveGameFlags.Values, ObjectReferenceEqualityComparer<GameFlag>.Default);
            Assert.Contains(flag2, inGameState.ActiveGameFlags.Values, ObjectReferenceEqualityComparer<GameFlag>.Default);
        }

        [Fact]
        public void GetActiveGameFlagsExceptIn_ReturnsDifference()
        {
            GameFlag flagIn1 = Model.GameFlags["f_ZebesAwake"];
            GameFlag flagIn2 = Model.GameFlags["f_DefeatedBombTorizo"];
            GameFlag flagInBoth = Model.GameFlags["f_DefeatedCeresRidley"];

            InGameState inGameState1 = new InGameState(StartConditions.CreateVanillaStartConditions(Model))
                .ApplyAddGameFlag(flagIn1)
                .ApplyAddGameFlag(flagInBoth);
            InGameState inGameState2 = new InGameState(StartConditions.CreateVanillaStartConditions(Model))
                .ApplyAddGameFlag(flagIn2)
                .ApplyAddGameFlag(flagInBoth);

            Dictionary<string, GameFlag> result = inGameState1.GetActiveGameFlagsExceptIn(inGameState2);
            Assert.Single(result);
            Assert.Same(flagIn1, result[flagIn1.Name]);
        }

        [Fact]
        public void ApplyOpenLock_AddsIt()
        {
            NodeLock lock1 = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            NodeLock lock2 = Model.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"];
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(Model));

            inGameState
                .ApplyOpenLock(lock1, applyToRoomState: false)
                .ApplyOpenLock(lock2, applyToRoomState: false);

            Assert.Equal(2, inGameState.OpenedLocks.Count);
            Assert.Contains(lock1, inGameState.OpenedLocks.Values, ObjectReferenceEqualityComparer<NodeLock>.Default);
            Assert.Contains(lock2, inGameState.OpenedLocks.Values, ObjectReferenceEqualityComparer<NodeLock>.Default);
            Assert.Empty(inGameState.InRoomState.CurrentNodeState.OpenedLocks);
        }

        [Fact]
        public void ApplyOpenLock_ApplyingToRoomStateWhileNotOnNode_ThrowsArgumentException()
        {
            NodeLock nodeLock = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(Model));
            Assert.Throws<ArgumentException>(() => inGameState.ApplyOpenLock(nodeLock, applyToRoomState: true));
        }

        [Fact]
        public void ApplyOpenLock_ApplyingToRoomStateWhileOnNode_SucceedsAndAltersNodeState()
        {
            NodeLock nodeLock = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 3);
            InGameState inGameState = new InGameState(startConditions);

            inGameState.ApplyOpenLock(nodeLock, applyToRoomState: true);

            Assert.Single(inGameState.OpenedLocks);
            Assert.Contains(nodeLock, inGameState.OpenedLocks.Values, ObjectReferenceEqualityComparer<NodeLock>.Default);
            Assert.Single(inGameState.InRoomState.CurrentNodeState.OpenedLocks);
            Assert.Contains(nodeLock, inGameState.InRoomState.CurrentNodeState.OpenedLocks, ObjectReferenceEqualityComparer<NodeLock>.Default);
        }

        [Fact]
        public void GetOpenedNodeLocksExceptIn_ReturnsDifference()
        {
            NodeLock lockIn1 = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            NodeLock lockIn2 = Model.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"];
            NodeLock lockInBoth = Model.Locks["Parlor Bottom Right Red Lock (to Pre-Map)"];

            InGameState inGameState1 = new InGameState(StartConditions.CreateVanillaStartConditions(Model))
                .ApplyOpenLock(lockIn1, applyToRoomState: false)
                .ApplyOpenLock(lockInBoth, applyToRoomState: false);
            InGameState inGameState2 = new InGameState(StartConditions.CreateVanillaStartConditions(Model))
                .ApplyOpenLock(lockIn2, applyToRoomState: false)
                .ApplyOpenLock(lockInBoth, applyToRoomState: false);

            Dictionary<string, NodeLock> result = inGameState1.GetOpenedNodeLocksExceptIn(inGameState2);
            Assert.Single(result);
            Assert.Same(lockIn1, result[lockIn1.Name]);
        }

        [Fact]
        public void ApplyBypassLock_NotOnNode_AltersInNodeState()
        {
            NodeLock nodeLock = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 3);
            InGameState inGameState = new InGameState(startConditions);

            inGameState.ApplyBypassLock(nodeLock);

            Assert.Single(inGameState.InRoomState.CurrentNodeState.BypassedLocks);
            Assert.Contains(nodeLock, inGameState.InRoomState.CurrentNodeState.BypassedLocks, ObjectReferenceEqualityComparer<NodeLock>.Default);
        }

        [Fact]
        public void ApplyBypassLock_NotOnNode_ThrowsArgumentException()
        {
            NodeLock nodeLock = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(Model));
            Assert.Throws<ArgumentException>(() => inGameState.ApplyBypassLock(nodeLock));
        }

        [Fact]
        public void GetBypassedExitLocks_CurrentRoom_ReturnsBypassedLocks()
        {
            NodeLock nodeLock = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 3);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyBypassLock(nodeLock);

            IEnumerable<NodeLock> result = inGameState.GetBypassedExitLocks();

            Assert.Single(result);
            Assert.Contains(nodeLock, result, ObjectReferenceEqualityComparer<NodeLock>.Default);
        }

        [Fact]
        public void GetBypassedExitLocks_PreviousRoom_ReturnsBypassedLocksOnlyFromCorrectRoom()
        {
            NodeLock previousRoomLock = Model.Locks["Red Brinstar Elevator Yellow Lock (to Kihunters)"];
            NodeLock currentRoomLock = Model.Locks["Crateria Kihunter Room Bottom Yellow Lock (to Elevator)"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Red Brinstar Elevator Room", 1);

            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyBypassLock(previousRoomLock);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Crateria Kihunter Room", 3));
            inGameState.ApplyBypassLock(currentRoomLock);

            IEnumerable<NodeLock> result = inGameState.GetBypassedExitLocks(1);

            Assert.Single(result);
            Assert.Contains(previousRoomLock, result, ObjectReferenceEqualityComparer<NodeLock>.Default);
        }

        [Fact]
        public void ApplyTakeLocation_AddsIt()
        {
            RoomNode node1 = Model.GetNodeInRoom("Varia Suit Room", 2);
            RoomNode node2 = Model.GetNodeInRoom("Spazer Room", 2);
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(Model));

            inGameState
                .ApplyTakeLocation(node1)
                .ApplyTakeLocation(node2);

            Assert.Equal(2, inGameState.TakenItemLocations.Count);
            Assert.Contains(node1, inGameState.TakenItemLocations.Values, ObjectReferenceEqualityComparer<RoomNode>.Default);
            Assert.Contains(node2, inGameState.TakenItemLocations.Values, ObjectReferenceEqualityComparer<RoomNode>.Default);
        }

        [Fact]
        public void GetTakenItemLocationsExceptIn_ReturnsDifference()
        {
            RoomNode nodeIn1 = Model.GetNodeInRoom("Varia Suit Room", 2);
            RoomNode nodeIn2 = Model.GetNodeInRoom("Spazer Room", 2);
            RoomNode nodeInBoth = Model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 3);

            InGameState inGameState1 = new InGameState(StartConditions.CreateVanillaStartConditions(Model))
                .ApplyTakeLocation(nodeIn1)
                .ApplyTakeLocation(nodeInBoth);
            InGameState inGameState2 = new InGameState(StartConditions.CreateVanillaStartConditions(Model))
                .ApplyTakeLocation(nodeIn2)
                .ApplyTakeLocation(nodeInBoth);

            Dictionary<string, RoomNode> result = inGameState1.GetTakenItemLocationsExceptIn(inGameState2);
            Assert.Single(result);
            Assert.Same(nodeIn1, result[nodeIn1.Name]);
        }

        [Fact]
        public void ApplyAddItem_NonConsumableItem_AddsIt()
        {
            Item item1 = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item item2 = Model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            InGameState inGameState = new InGameState(startConditions);

            inGameState
                .ApplyAddItem(item1)
                .ApplyAddItem(item2);

            Assert.Equal(2, inGameState.Inventory.NonConsumableItems.Count);
            Assert.Same(item1, inGameState.Inventory.NonConsumableItems[item1.Name]);
            Assert.Same(item2, inGameState.Inventory.NonConsumableItems[item2.Name]);
        }

        [Fact]
        public void ApplyAddItem_ExpansionItem_AddsItAndIncreasesCount()
        {
            Item item1 = Model.Items[SuperMetroidModel.MISSILE_NAME];
            Item item2 = Model.Items[SuperMetroidModel.SUPER_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            InGameState inGameState = new InGameState(startConditions);

            inGameState
                .ApplyAddItem(item1)
                .ApplyAddItem(item1)
                .ApplyAddItem(item2);

            Assert.Equal(2, inGameState.Inventory.ExpansionItems.Count);
            Assert.Same(item1, inGameState.Inventory.ExpansionItems[item1.Name].item);
            Assert.Equal(2, inGameState.Inventory.ExpansionItems[item1.Name].count);
            Assert.Same(item2, inGameState.Inventory.ExpansionItems[item2.Name].item);
            Assert.Equal(1, inGameState.Inventory.ExpansionItems[item2.Name].count);
        }

        [Fact]
        public void ApplyDisableItem_NonConsumableItem_DisablesIt()
        {
            Item item1 = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item item2 = Model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME];
            Item notPresentItem = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            InGameState inGameState = new InGameState(startConditions);
            inGameState
                .ApplyAddItem(item1)
                .ApplyAddItem(item2);

            inGameState
                .ApplyDisableItem(item1)
                .ApplyDisableItem(notPresentItem);

            Assert.True(inGameState.Inventory.IsItemDisabled(item1));
            Assert.False(inGameState.Inventory.IsItemDisabled(item2));
            Assert.False(inGameState.Inventory.IsItemDisabled(notPresentItem));
            Assert.False(inGameState.Inventory.HasItem(item1));
            Assert.True(inGameState.Inventory.HasItem(item2));
        }

        [Fact]
        public void ApplyDisableItem_ExpansionItem_DoesNothing()
        {
            Item item1 = Model.Items[SuperMetroidModel.MISSILE_NAME];
            Item item2 = Model.Items[SuperMetroidModel.SUPER_NAME];
            Item notPresentItem = Model.Items[SuperMetroidModel.POWER_BOMB_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            InGameState inGameState = new InGameState(startConditions);
            inGameState
                .ApplyAddItem(item1)
                .ApplyAddItem(item2);

            inGameState
                .ApplyDisableItem(item1)
                .ApplyDisableItem(notPresentItem);

            Assert.False(inGameState.Inventory.IsItemDisabled(item1));
            Assert.False(inGameState.Inventory.IsItemDisabled(item2));
            Assert.False(inGameState.Inventory.IsItemDisabled(notPresentItem));
            Assert.True(inGameState.Inventory.HasItem(item1));
            Assert.True(inGameState.Inventory.HasItem(item2));
        }

        [Fact]
        public void ApplyDisableItem_NonConsumableItem_EnablesIt()
        {
            Item item1 = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyAddItem(item1);
            inGameState.ApplyDisableItem(item1);

            inGameState.ApplyEnableItem(item1);

            Assert.False(inGameState.Inventory.IsItemDisabled(item1));
            Assert.True(inGameState.Inventory.HasItem(item1));
        }

        [Fact]
        public void GetInventoryExceptIn_ReturnsDifference()
        {
            Item nonConsumableItemIn1 = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item nonConsumableItemIn2 = Model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME];
            Item nonConsumableItemInBoth = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];

            Item expansionItemIn1 = Model.Items[SuperMetroidModel.MISSILE_NAME];
            Item expansionItemIn2 = Model.Items[SuperMetroidModel.SUPER_NAME];
            Item expansionItemInBoth = Model.Items[SuperMetroidModel.POWER_BOMB_NAME];
            Item expansionItemMoreIn1 = Model.Items[SuperMetroidModel.ENERGY_TANK_NAME];
            Item expansionItemMoreIn2 = Model.Items[SuperMetroidModel.RESERVE_TANK_NAME];

            InGameState inGameState1 = new InGameState(StartConditions.CreateVanillaStartConditions(Model))
                .ApplyAddItem(nonConsumableItemIn1)
                .ApplyAddItem(nonConsumableItemInBoth)
                .ApplyAddItem(expansionItemIn1)
                .ApplyAddItem(expansionItemInBoth)
                .ApplyAddItem(expansionItemMoreIn1)
                .ApplyAddItem(expansionItemMoreIn1)
                .ApplyAddItem(expansionItemMoreIn1)
                .ApplyAddItem(expansionItemMoreIn2);
            InGameState inGameState2 = new InGameState(StartConditions.CreateVanillaStartConditions(Model))
                .ApplyAddItem(nonConsumableItemIn2)
                .ApplyAddItem(nonConsumableItemInBoth)
                .ApplyAddItem(expansionItemIn2)
                .ApplyAddItem(expansionItemInBoth)
                .ApplyAddItem(expansionItemMoreIn1)
                .ApplyAddItem(expansionItemMoreIn2)
                .ApplyAddItem(expansionItemMoreIn2)
                .ApplyAddItem(expansionItemMoreIn2);

            ItemInventory result = inGameState1.GetInventoryExceptIn(inGameState2);

            Assert.True(result.HasItem(nonConsumableItemIn1));
            Assert.True(result.HasItem(expansionItemIn1));
            Assert.Equal(1, result.ExpansionItems[expansionItemIn1.Name].count);
            Assert.True(result.HasItem(expansionItemMoreIn1));
            Assert.Equal(2, result.ExpansionItems[expansionItemMoreIn1.Name].count);
            Assert.False(result.HasItem(nonConsumableItemIn2));
            Assert.False(result.HasItem(nonConsumableItemInBoth));
            Assert.False(result.HasItem(expansionItemIn2));
            Assert.False(result.HasItem(expansionItemInBoth));
            Assert.False(result.HasItem(expansionItemMoreIn2));
        }

        [Fact]
        public void GetInRoomState_CurrentRoom_ReturnsCurrentRoomState()
        {
            RoomNode expectedNode = Model.GetNodeInRoom("Red Tower", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            ReadOnlyInRoomState result = inGameState.GetInRoomState(0);

            Assert.Same(expectedNode.Room, result.CurrentRoom);
        }

        [Fact]
        public void GetInRoomState_PreviousRoom_ReturnsPreviousRoomState()
        {
            RoomNode expectedNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = expectedNode;
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            ReadOnlyInRoomState result = inGameState.GetInRoomState(1);

            Assert.Same(expectedNode.Room, result.CurrentRoom);
        }

        [Fact]
        public void GetInRoomState_PreviousRoom_SkipsNonPlayableRooms()
        {
            RoomNode expectedNode = Model.GetNodeInRoom("Oasis", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = expectedNode;
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            ReadOnlyInRoomState result = inGameState.GetInRoomState(1);

            Assert.Same(expectedNode.Room, result.CurrentRoom);
        }

        [Fact]
        public void GetInRoomState_NegativePreviousRoomCount_ThrowsException()
        {
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            Assert.Throws<ArgumentException>(() => inGameState.GetInRoomState(-1));
        }

        [Fact]
        public void GetInRoomState_GoingBeyondRememberedRooms_ReturnsNull()
        {
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));
            RoomNode entryNode = Model.GetNodeInRoom("Bat Room", 1);
            inGameState.ApplyEnterRoom(entryNode);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Bat Room", 2), entryNode.Links[2].Strats["Base"]);
            entryNode = Model.GetNodeInRoom("Below Spazer", 1);
            inGameState.ApplyEnterRoom(entryNode);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Below Spazer", 2), entryNode.Links[2].Strats["Base"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("West (Glass Tube) Tunnel", 1));

            Assert.Null(inGameState.GetInRoomState(4));
        }

        [Fact]
        public void GetCurrentOrPreviousRoom_CurrentRoom_ReturnsCurrentRoom()
        {
            RoomNode expectedNode = Model.GetNodeInRoom("Red Tower", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            Room result = inGameState.GetCurrentOrPreviousRoom(0);

            Assert.Same(expectedNode.Room, result);
        }

        [Fact]
        public void GetCurrentOrPreviousRoom_PreviousRoom_ReturnsPreviousRoom()
        {
            RoomNode expectedNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = expectedNode;
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            Room result = inGameState.GetCurrentOrPreviousRoom(1);

            Assert.Same(expectedNode.Room, result);
        }

        [Fact]
        public void GetCurrentOrPreviousRoom_PreviousRoom_SkipsNonPlayableRooms()
        {
            RoomNode expectedNode = Model.GetNodeInRoom("Oasis", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = expectedNode;
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            Room result = inGameState.GetCurrentOrPreviousRoom(1);

            Assert.Same(expectedNode.Room, result);
        }

        [Fact]
        public void GetCurrentOrPreviousRoom_NegativePreviousRoomCount_ThrowsException()
        {
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            Assert.Throws<ArgumentException>(() => inGameState.GetCurrentOrPreviousRoom(-1));
        }

        [Fact]
        public void GetCurrentOrPreviousRoom_GoingBeyondRememberedRooms_ReturnsNull()
        {
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));
            RoomNode entryNode = Model.GetNodeInRoom("Bat Room", 1);
            inGameState.ApplyEnterRoom(entryNode);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Bat Room", 2), entryNode.Links[2].Strats["Base"]);
            entryNode = Model.GetNodeInRoom("Below Spazer", 1);
            inGameState.ApplyEnterRoom(entryNode);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Below Spazer", 2), entryNode.Links[2].Strats["Base"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("West (Glass Tube) Tunnel", 1));

            Assert.Null(inGameState.GetCurrentOrPreviousRoom(4));
        }

        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_CurrentRoom_ReturnsCurrentRoomEnvironment()
        {
            RoomNode expectedNode = Model.GetNodeInRoom("Cathedral Entrance", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Business Center", 6);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            RoomEnvironment result = inGameState.GetCurrentOrPreviousRoomEnvironment(0);

            Assert.Same(expectedNode.Room, result.Room);
            Assert.True(result.Heated);
        }

        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_PreviousRoom_ReturnsPreviousRoomEnvironment()
        {
            RoomNode expectedNode = Model.GetNodeInRoom("Business Center", 6);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = expectedNode;
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Cathedral Entrance", 1));

            RoomEnvironment result = inGameState.GetCurrentOrPreviousRoomEnvironment(1);

            Assert.Same(expectedNode.Room, result.Room);
            Assert.False(result.Heated);
        }

        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_PreviousRoom_SkipsNonPlayableRooms()
        {
            RoomNode expectedNode = Model.GetNodeInRoom("Oasis", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = expectedNode;
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            RoomEnvironment result = inGameState.GetCurrentOrPreviousRoomEnvironment(1);

            Assert.Same(expectedNode.Room, result.Room);
        }

        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_NegativePreviousRoomCount_ThrowsException()
        {
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            Assert.Throws<ArgumentException>(() => inGameState.GetCurrentOrPreviousRoomEnvironment(-1));
        }

        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_GoingBeyondRememberedRooms_ReturnsNull()
        {
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));
            RoomNode entryNode = Model.GetNodeInRoom("Bat Room", 1);
            inGameState.ApplyEnterRoom(entryNode);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Bat Room", 2), entryNode.Links[2].Strats["Base"]);
            entryNode = Model.GetNodeInRoom("Below Spazer", 1);
            inGameState.ApplyEnterRoom(entryNode);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Below Spazer", 2), entryNode.Links[2].Strats["Base"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("West (Glass Tube) Tunnel", 1));

            Assert.Null(inGameState.GetCurrentOrPreviousRoomEnvironment(4));
        }

        [Fact]
        public void IsHeated_CurrentRoom_ReturnsCurrentRoom()
        {
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Business Center", 6);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Cathedral Entrance", 1));

            bool result = inGameState.IsHeatedRoom(0);

            Assert.True(result);
        }

        [Fact]
        public void IsHeated_PreviousRoom_ReturnsPreviousRoom()
        {
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Business Center", 6);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Cathedral Entrance", 1));

            bool result = inGameState.IsHeatedRoom(1);

            Assert.False(result);
        }

        [Fact]
        public void IsHeated_PreviousRoom_SkipsNonPlayableRooms()
        {
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Cathedral Entrance", 1);
            InGameState inGameState = new InGameState(startConditions);
            // Not a valid connection but it's not InGameState's job to know that
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            bool result = inGameState.IsHeatedRoom(1);

            Assert.True(result);
        }

        [Fact]
        public void IsHeated_NegativePreviousRoomCount_ThrowsException()
        {
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            Assert.Throws<ArgumentException>(() => inGameState.IsHeatedRoom(-1));
        }

        [Fact]
        public void IsHeated_GoingBeyondRememberedRooms_ReturnsFalse()
        {
            RoomNode node1 = Model.GetNodeInRoom("Bat Cave", 2);
            RoomNode node2 = Model.GetNodeInRoom("Speed Booster Hall", 1);

            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = node1;
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(node2);
            inGameState.ApplyEnterRoom(node1);
            inGameState.ApplyEnterRoom(node1);
            inGameState.ApplyEnterRoom(node2);

            Assert.False(inGameState.IsHeatedRoom(4));
        }

        [Fact]
        public void GetCurrentNode_CurrentRoom_ReturnsCurrentNode()
        {
            RoomNode expectedNode = Model.GetNodeInRoom("Red Tower", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            RoomNode result = inGameState.GetCurrentNode(0);

            Assert.Same(expectedNode, result);
        }

        [Fact]
        public void GetCurrentNode_PreviousRoom_ReturnsLastNodeOfPreviousRoom()
        {
            RoomNode expectedNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = expectedNode;
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            RoomNode result = inGameState.GetCurrentNode(1);

            Assert.Same(expectedNode, result);
        }

        [Fact]
        public void GetCurrentNode_PreviousRoom_SkipsNonPlayableRooms()
        {
            RoomNode expectedNode = Model.GetNodeInRoom("Oasis", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = expectedNode;
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            RoomNode result = inGameState.GetCurrentNode(1);

            Assert.Same(expectedNode, result);
        }

        [Fact]
        public void GetCurrentNode_NegativePreviousRoomCount_ThrowsException()
        {
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            Assert.Throws<ArgumentException>(() => inGameState.GetCurrentNode(-1));
        }

        [Fact]
        public void GetCurrentNode_GoingBeyondRememberedRooms_ReturnsNull()
        {
            StartConditions startConditions = StartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));
            RoomNode entryNode = Model.GetNodeInRoom("Bat Room", 1);
            inGameState.ApplyEnterRoom(entryNode);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Bat Room", 2), entryNode.Links[2].Strats["Base"]);
            entryNode = Model.GetNodeInRoom("Below Spazer", 1);
            inGameState.ApplyEnterRoom(entryNode);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Below Spazer", 2), entryNode.Links[2].Strats["Base"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("West (Glass Tube) Tunnel", 1));

            Assert.Null(inGameState.GetCurrentNode(4));
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
                StartingResources = new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, startingEnergy)
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
                StartingResources = new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, startingEnergy)
                    .ApplyAmount(RechargeableResourceEnum.Missile, 5)
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
