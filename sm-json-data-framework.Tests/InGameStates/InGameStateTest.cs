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
                maxResources.ApplyAmountIncrease(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
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
                maxResources.ApplyAmountIncrease(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
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
                maxResources.ApplyAmountIncrease(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
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
                maxResources.ApplyAmountIncrease(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
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
                maxResources.ApplyAmountIncrease(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
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

            StartConditions startConditions1 = StartConditions.CreateVanillaStartconditions(Model);
            startConditions1.StartingInventory = startConditions1.StartingInventory.WithBaseResourceMaximums(resourceMaximums);
            startConditions1.StartingResources = resources1;
            InGameState inGameState1 = new InGameState(startConditions1);

            StartConditions startConditions2 = StartConditions.CreateVanillaStartconditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
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
            StartConditions startConditions = StartConditions.CreateVanillaStartconditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceMaximums);
            startConditions.StartingResources = resources;
            InGameState inGameState = new InGameState(startConditions);

            IEnumerable<ConsumableResourceEnum> result = inGameState.GetFullConsumableResources();
            Assert.Single(result);
            Assert.Contains(ConsumableResourceEnum.SUPER, result);
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
