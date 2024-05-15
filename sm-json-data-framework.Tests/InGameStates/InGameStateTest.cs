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
using sm_json_data_framework.Tests.TestTools;
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

        // Use a static model to build it only once.
        private static UnfinalizedSuperMetroidModel Model { get; set; } = new UnfinalizedSuperMetroidModel(StaticTestObjects.RawModel);

        #region Tests for ctor(StartConditions)
        [Fact]
        public void ConstructorWithStartConditions_InitializesProperly()
        {
            // Given
            string startingRoomName = "Business Center";
            int startingNodeId = 7;
            string startingLockName = "Business Center Top Left Green Lock (to Ice Beam Gate)";
            int startingEnergy = 50;
            string maridiaTubeFlag = "f_MaridiaTubeBroken";
            UnfinalizedRoomNode variaNode = Model.GetNodeInRoom("Varia Suit Room", 2);
            UnfinalizedStartConditions startConditions = new UnfinalizedStartConditions
            {
                StartingGameFlags = new UnfinalizedGameFlag[] { Model.GameFlags[maridiaTubeFlag] },
                StartingInventory = UnfinalizedItemInventory.CreateVanillaStartingInventory(Model).ApplyAddItem(Model.Items[SuperMetroidModel.VARIA_SUIT_NAME]),
                StartingNode = Model.GetNodeInRoom(startingRoomName, startingNodeId),
                StartingOpenLocks = new UnfinalizedNodeLock[] { Model.Locks[startingLockName] },
                StartingTakenItemLocations = new UnfinalizedRoomNode[] { variaNode },
                StartingResources = new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, startingEnergy)
            };

            // When
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // Expect
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
        #endregion

        #region Tests for IsResourceAvailable()
        [Theory]
        [MemberData(nameof(ConsumableResourceValues))]
        public void IsResourceAvailable_Requesting0_ReturnsTrue(ConsumableResourceEnum resource)
        {
            // Given
            ResourceCount resourceCount = new ResourceCount();
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            bool result = inGameState.IsResourceAvailable(resource, 0);

            // Expect
            Assert.True(result);
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingExactPresentAmount_Ammo_ReturnsTrue(RechargeableResourceEnum resource)
        {
            // Given
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amount);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            bool result = inGameState.IsResourceAvailable(resource.ToConsumableResource(), amount);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingExactPresentAmount_Energy_ReturnsFalse()
        {
            // Given
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amount);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            bool result = inGameState.IsResourceAvailable(ConsumableResourceEnum.Energy, amount);

            // Expect
            // X energy is not available to spend if you have exactly X energy, because you'd die
            Assert.False(result);
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_Ammo_ReturnsTrue(RechargeableResourceEnum resource)
        {
            // Given
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amountToRequest + 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            bool result = inGameState.IsResourceAvailable(resource.ToConsumableResource(), amountToRequest);
            
            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_Energy_ReturnsTrue()
        {
            // Given
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amountToRequest + 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            bool result = inGameState.IsResourceAvailable(ConsumableResourceEnum.Energy, amountToRequest);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_EnergyMixOfReserveAndNormal_ReturnsTrue()
        {
            // Given
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 3);
            resourceCount.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            bool result = inGameState.IsResourceAvailable(ConsumableResourceEnum.Energy, 5);

            // Expect
            Assert.True(result);
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void IsResourceAvailable_RequestingMoreThanPresentAmount_Ammo_ReturnsFalse(RechargeableResourceEnum resource)
        {
            // Given
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amountToRequest - 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            bool result = inGameState.IsResourceAvailable(resource.ToConsumableResource(), amountToRequest);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingMoreThanPresentAmount_Energy_ReturnsFalse()
        {
            // Given
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amountToRequest - 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceCount);
            startConditions.StartingResources = resourceCount;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            bool result = inGameState.IsResourceAvailable(ConsumableResourceEnum.Energy, amountToRequest);
             // Expect
            Assert.False(result);
        }
        #endregion

        #region Tests for ApplyAddResource()
        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void ApplyAddResource_AddsAmount(RechargeableResourceEnum resource)
        {
            // Given
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
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyAddResource(resource, addedAmount);

            // Expect
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
            // Given
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
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyAddResource(resource, addedAmount);

            // Expect
            Assert.Equal(expectedamount, inGameState.Resources.GetAmount(resource));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                if (otherResource != resource)
                {
                    Assert.Equal(initialAmount, inGameState.Resources.GetAmount(otherResource));
                }
            }
        }
        #endregion

        #region Tests for ApplyConsumeResource()
        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void ApplyConsumeResource_Ammo_SetsAmount(RechargeableResourceEnum resource)
        {
            // Given
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
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyConsumeResource(resource.ToConsumableResource(), removedAmount);

            // Expect
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
            // Given
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
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyConsumeResource(ConsumableResourceEnum.Energy, removedAmount);

            // Expect
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
            // Given
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
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyConsumeResource(ConsumableResourceEnum.Energy, removedAmount);

            // Expect
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
            // Given
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
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyConsumeResource(ConsumableResourceEnum.Energy, reductionAmount);

            // Expect
            Assert.Equal(expectedRegularAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(expectedReserveAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }

        [Fact]
        public void ApplyConsumeResource_MixedEnergy_ConsumesReservesBeforeGoingTo0Regular()
        {
            // Given
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
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyConsumeResource(ConsumableResourceEnum.Energy, reductionAmount);

            // Expect
            Assert.Equal(expectedRegularAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(expectedReserveAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }

        [Fact]
        public void ApplyConsumeResource_MixedEnergy_ConsumesReservesBeforeGoingToNegativeRegular()
        {
            // Given
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
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyConsumeResource(ConsumableResourceEnum.Energy, reductionAmount);

            // Expect
            Assert.Equal(expectedRegularAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(expectedReserveAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }
        #endregion

        #region Tests for ApplyRefillResource(RechargeableResourceEnum)
        [Theory]
        [MemberData(nameof(RechargeableResourceValues))]
        public void ApplyRefillResource_RechargeableResource_SetsToMax(RechargeableResourceEnum resource)
        {
            // Given
            int initialAmount = 5;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount()
                .ApplyAmount(resource, initialAmount);
            ResourceCount maxResources = startResources.Clone()
                .ApplyAmount(resource, maxAmount);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyRefillResource(resource);

            // Expect
            Assert.Equal(maxAmount, inGameState.Resources.GetAmount(resource));
        }
        #endregion

        #region Tests for ApplyRefillResource(ConsumableResourceEnum)
        [Theory]
        [InlineData(RechargeableResourceEnum.Missile)]
        [InlineData(RechargeableResourceEnum.Super)]
        [InlineData(RechargeableResourceEnum.PowerBomb)]
        public void ApplyRefillResource_ConsumableAmmo_SetsToMax(RechargeableResourceEnum resource)
        {
            // Given
            int initialAmount = 5;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount()
                .ApplyAmount(resource, initialAmount);
            ResourceCount maxResources = startResources.Clone()
                .ApplyAmount(resource, maxAmount);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyRefillResource(resource.ToConsumableResource());

            // Expect
            Assert.Equal(maxAmount, inGameState.Resources.GetAmount(resource));
        }

        [Fact]
        public void ApplyRefillResource_ConsumableEnergy_SetsBothTypesToMax()
        {
            // Given
            int initialAmount = 5;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, initialAmount)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, initialAmount);
            ResourceCount maxResources = startResources.Clone()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, maxAmount)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, maxAmount);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyRefillResource(ConsumableResourceEnum.Energy);

            // Expect
            Assert.Equal(maxAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(maxAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }
        #endregion

        #region Tests for GetResourceVariationWith()
        [Fact]
        public void GetResourceVariationWith_ReturnsPositiveAndNegativeAnd0()
        {
            // Given
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

            UnfinalizedStartConditions startConditions1 = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions1.StartingInventory = startConditions1.StartingInventory.WithBaseResourceMaximums(resourceMaximums);
            startConditions1.StartingResources = resources1;
            UnfinalizedInGameState inGameState1 = new UnfinalizedInGameState(startConditions1);

            UnfinalizedStartConditions startConditions2 = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions2.StartingInventory = startConditions2.StartingInventory.WithBaseResourceMaximums(resourceMaximums);
            startConditions2.StartingResources = resources2;
            UnfinalizedInGameState inGameState2 = new UnfinalizedInGameState(startConditions2);

            // When
            ResourceCount result = inGameState1.GetResourceVariationWith(inGameState2);

            // Expect
            Assert.Equal(5, result.GetAmount(RechargeableResourceEnum.Missile));
            Assert.Equal(-5, result.GetAmount(RechargeableResourceEnum.Super));
            Assert.Equal(10, result.GetAmount(RechargeableResourceEnum.PowerBomb));
            Assert.Equal(-10, result.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(0, result.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }
        #endregion

        #region Tests for GetFullRechargeableResources()
        [Fact]
        public void GetFullRechargeableResources_ReturnsFullResources()
        {
            // Given
            ResourceCount resourceMaximums = new ResourceCount();
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceMaximums.ApplyAmount(resource, 100);
            }
            ResourceCount resources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.Super, 100)
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 100);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceMaximums);
            startConditions.StartingResources = resources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            IEnumerable<RechargeableResourceEnum> result = inGameState.GetFullRechargeableResources();

            // Expect
            Assert.Equal(2, result.Count());
            Assert.Contains(RechargeableResourceEnum.Super, result);
            Assert.Contains(RechargeableResourceEnum.RegularEnergy, result);
        }
        #endregion

        #region Tests for GetFullConsumableResources()
        [Fact]
        public void GetFullConsumableResources_ReturnsFullResources()
        {
            // Given
            ResourceCount resourceMaximums = new ResourceCount();
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceMaximums.ApplyAmount(resource, 100);
            }
            ResourceCount resources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.Super, 100)
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 100)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 100);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceMaximums);
            startConditions.StartingResources = resources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            IEnumerable<ConsumableResourceEnum> result = inGameState.GetFullConsumableResources();

            // Expect
            Assert.Equal(2, result.Count());
            Assert.Contains(ConsumableResourceEnum.Super, result);
            Assert.Contains(ConsumableResourceEnum.Energy, result);
        }

        [Fact]
        public void GetFullConsumableResources_OneEnergyTypeFull_DoesNotReturnEnergy()
        {
            // Given
            ResourceCount resourceMaximums = new ResourceCount();
            foreach (RechargeableResourceEnum resource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                resourceMaximums.ApplyAmount(resource, 100);
            }
            ResourceCount resources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.Super, 100)
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 100);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(resourceMaximums);
            startConditions.StartingResources = resources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            IEnumerable<ConsumableResourceEnum> result = inGameState.GetFullConsumableResources();

            // Expect
            Assert.Single(result);
            Assert.Contains(ConsumableResourceEnum.Super, result);
        }
        #endregion

        #region Tests for GetUnneededDrops()
        [Fact]
        public void GetUnneededDrops_SamusIsFull_ReturnsAllButNoDrops()
        {
            // Given
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model));

            // When
            IEnumerable<EnemyDropEnum> result = inGameState.GetUnneededDrops(Model);

            // Expect
            IEnumerable<EnemyDropEnum> expected = Enum.GetValues<EnemyDropEnum>().Except(new EnemyDropEnum[] { EnemyDropEnum.NoDrop });
            Assert.Equal(expected.Count(), result.Count());
            foreach (EnemyDropEnum drop in expected)
            {
                Assert.Contains(drop, result);
            }
        }

        [Fact]
        public void GetUnneededDrops_SamusNeedsEverything_ReturnsEmpty()
        {
            // Given
            int initialAmount = 5;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues(typeof(RechargeableResourceEnum)))
            {
                startResources.ApplyAmount(loopResource, initialAmount);
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            IEnumerable<EnemyDropEnum> result = inGameState.GetUnneededDrops(Model);

            // Expect
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.RegularEnergy)]
        [InlineData(RechargeableResourceEnum.ReserveEnergy)]
        public void GetUnneededDrops_OnlyOneTypeOfEnergyNotFull_DoesNotReturnEnergyDrops(RechargeableResourceEnum energyResource)
        {
            // Given
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
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = startConditions.StartingInventory.WithBaseResourceMaximums(maxResources);
            startConditions.StartingResources = startResources;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            IEnumerable<EnemyDropEnum> result = inGameState.GetUnneededDrops(Model);

            // Expect
            // Although one type of energy is full, the other isn't, so energy drops are needed and should NOT be returned
            Assert.DoesNotContain(EnemyDropEnum.SmallEnergy, result);
            Assert.DoesNotContain(EnemyDropEnum.BigEnergy, result);
        }
        #endregion

        #region Tests for ApplyAddGameFlag()
        [Fact]
        public void ApplyAddGameFlag_AddsIt()
        {
            // Given
            UnfinalizedGameFlag flag1 = Model.GameFlags["f_ZebesAwake"];
            UnfinalizedGameFlag flag2 = Model.GameFlags["f_DefeatedBombTorizo"];
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model));

            // When
            inGameState
                .ApplyAddGameFlag(flag1)
                .ApplyAddGameFlag(flag2);

            // Expect
            Assert.Equal(2, inGameState.ActiveGameFlags.Count);
            Assert.Contains(flag1, inGameState.ActiveGameFlags.Values, ObjectReferenceEqualityComparer<UnfinalizedGameFlag>.Default);
            Assert.Contains(flag2, inGameState.ActiveGameFlags.Values, ObjectReferenceEqualityComparer<UnfinalizedGameFlag>.Default);
        }
        #endregion

        #region Tests for GetActiveGameFlagsExceptIn()
        [Fact]
        public void GetActiveGameFlagsExceptIn_ReturnsDifference()
        {
            // Given
            UnfinalizedGameFlag flagIn1 = Model.GameFlags["f_ZebesAwake"];
            UnfinalizedGameFlag flagIn2 = Model.GameFlags["f_DefeatedBombTorizo"];
            UnfinalizedGameFlag flagInBoth = Model.GameFlags["f_DefeatedCeresRidley"];
            UnfinalizedInGameState inGameState1 = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model))
                .ApplyAddGameFlag(flagIn1)
                .ApplyAddGameFlag(flagInBoth);
            UnfinalizedInGameState inGameState2 = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model))
                .ApplyAddGameFlag(flagIn2)
                .ApplyAddGameFlag(flagInBoth);

            // When
            Dictionary<string, UnfinalizedGameFlag> result = inGameState1.GetActiveGameFlagsExceptIn(inGameState2);

            // Expect
            Assert.Single(result);
            Assert.Same(flagIn1, result[flagIn1.Name]);
        }
        #endregion

        #region Tests for ApplyOpenLock()
        [Fact]
        public void ApplyOpenLock_AddsIt()
        {
            // Given
            UnfinalizedNodeLock lock1 = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            UnfinalizedNodeLock lock2 = Model.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"];
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model));

            // When
            inGameState
                .ApplyOpenLock(lock1, applyToRoomState: false)
                .ApplyOpenLock(lock2, applyToRoomState: false);

            // Expect
            Assert.Equal(2, inGameState.OpenedLocks.Count);
            Assert.Contains(lock1, inGameState.OpenedLocks.Values, ObjectReferenceEqualityComparer<UnfinalizedNodeLock>.Default);
            Assert.Contains(lock2, inGameState.OpenedLocks.Values, ObjectReferenceEqualityComparer<UnfinalizedNodeLock>.Default);
            Assert.Empty(inGameState.InRoomState.CurrentNodeState.OpenedLocks);
        }

        [Fact]
        public void ApplyOpenLock_ApplyingToRoomStateWhileNotOnNode_ThrowsArgumentException()
        {
            // Given
            UnfinalizedNodeLock nodeLock = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyOpenLock(nodeLock, applyToRoomState: true));
        }

        [Fact]
        public void ApplyOpenLock_ApplyingToRoomStateWhileOnNode_SucceedsAndAltersNodeState()
        {
            // Given
            UnfinalizedNodeLock nodeLock = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 3);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyOpenLock(nodeLock, applyToRoomState: true);

            // Expect
            Assert.Single(inGameState.OpenedLocks);
            Assert.Contains(nodeLock, inGameState.OpenedLocks.Values, ObjectReferenceEqualityComparer<UnfinalizedNodeLock>.Default);
            Assert.Single(inGameState.InRoomState.CurrentNodeState.OpenedLocks);
            Assert.Contains(nodeLock, inGameState.InRoomState.CurrentNodeState.OpenedLocks, ObjectReferenceEqualityComparer<UnfinalizedNodeLock>.Default);
        }
        #endregion

        #region Tests for GetOpenedNodeLocksExceptIn()
        [Fact]
        public void GetOpenedNodeLocksExceptIn_ReturnsDifference()
        {
            // Given
            UnfinalizedNodeLock lockIn1 = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            UnfinalizedNodeLock lockIn2 = Model.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"];
            UnfinalizedNodeLock lockInBoth = Model.Locks["Parlor Bottom Right Red Lock (to Pre-Map)"];
            UnfinalizedInGameState inGameState1 = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model))
                .ApplyOpenLock(lockIn1, applyToRoomState: false)
                .ApplyOpenLock(lockInBoth, applyToRoomState: false);
            UnfinalizedInGameState inGameState2 = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model))
                .ApplyOpenLock(lockIn2, applyToRoomState: false)
                .ApplyOpenLock(lockInBoth, applyToRoomState: false);

            // When
            Dictionary<string, UnfinalizedNodeLock> result = inGameState1.GetOpenedNodeLocksExceptIn(inGameState2);

            // Expect
            Assert.Single(result);
            Assert.Same(lockIn1, result[lockIn1.Name]);
        }
        #endregion

        #region Tests for ApplyBypassLock()
        [Fact]
        public void ApplyBypassLock_NotOnNode_AltersInNodeState()
        {
            // Given
            UnfinalizedNodeLock nodeLock = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 3);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyBypassLock(nodeLock);

            // Expect
            Assert.Single(inGameState.InRoomState.CurrentNodeState.BypassedLocks);
            Assert.Contains(nodeLock, inGameState.InRoomState.CurrentNodeState.BypassedLocks, ObjectReferenceEqualityComparer<UnfinalizedNodeLock>.Default);
        }

        [Fact]
        public void ApplyBypassLock_NotOnNode_ThrowsArgumentException()
        {
            // Given
            UnfinalizedNodeLock nodeLock = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyBypassLock(nodeLock));
        }
        #endregion

        #region Tests for GetBypassedExitLocks()
        [Fact]
        public void GetBypassedExitLocks_CurrentRoom_ReturnsBypassedLocks()
        {
            // Given
            UnfinalizedNodeLock nodeLock = Model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 3);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyBypassLock(nodeLock);

            // When
            IEnumerable<UnfinalizedNodeLock> result = inGameState.GetBypassedExitLocks();

            // Expect
            Assert.Single(result);
            Assert.Contains(nodeLock, result, ObjectReferenceEqualityComparer<UnfinalizedNodeLock>.Default);
        }

        [Fact]
        public void GetBypassedExitLocks_PreviousRoom_ReturnsBypassedLocksOnlyFromCorrectRoom()
        {
            // Given
            UnfinalizedNodeLock previousRoomLock = Model.Locks["Red Brinstar Elevator Yellow Lock (to Kihunters)"];
            UnfinalizedNodeLock currentRoomLock = Model.Locks["Crateria Kihunter Room Bottom Yellow Lock (to Elevator)"];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Red Brinstar Elevator Room", 1);

            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyBypassLock(previousRoomLock);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Crateria Kihunter Room", 3));
            inGameState.ApplyBypassLock(currentRoomLock);

            // When
            IEnumerable<UnfinalizedNodeLock> result = inGameState.GetBypassedExitLocks(1);

            // Expect
            Assert.Single(result);
            Assert.Contains(previousRoomLock, result, ObjectReferenceEqualityComparer<UnfinalizedNodeLock>.Default);
        }
        #endregion

        #region Tests for ApplyTakeLocation()
        [Fact]
        public void ApplyTakeLocation_AddsIt()
        {
            // Given
            UnfinalizedRoomNode node1 = Model.GetNodeInRoom("Varia Suit Room", 2);
            UnfinalizedRoomNode node2 = Model.GetNodeInRoom("Spazer Room", 2);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model));

            // When
            inGameState
                .ApplyTakeLocation(node1)
                .ApplyTakeLocation(node2);

            // Expect
            Assert.Equal(2, inGameState.TakenItemLocations.Count);
            Assert.Contains(node1, inGameState.TakenItemLocations.Values, ObjectReferenceEqualityComparer<UnfinalizedRoomNode>.Default);
            Assert.Contains(node2, inGameState.TakenItemLocations.Values, ObjectReferenceEqualityComparer<UnfinalizedRoomNode>.Default);
        }
        #endregion

        #region Tests for GetTakenItemLocationsExceptIn()
        [Fact]
        public void GetTakenItemLocationsExceptIn_ReturnsDifference()
        {
            // Given
            UnfinalizedRoomNode nodeIn1 = Model.GetNodeInRoom("Varia Suit Room", 2);
            UnfinalizedRoomNode nodeIn2 = Model.GetNodeInRoom("Spazer Room", 2);
            UnfinalizedRoomNode nodeInBoth = Model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 3);

            UnfinalizedInGameState inGameState1 = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model))
                .ApplyTakeLocation(nodeIn1)
                .ApplyTakeLocation(nodeInBoth);
            UnfinalizedInGameState inGameState2 = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model))
                .ApplyTakeLocation(nodeIn2)
                .ApplyTakeLocation(nodeInBoth);

            // When
            Dictionary<string, UnfinalizedRoomNode> result = inGameState1.GetTakenItemLocationsExceptIn(inGameState2);

            // Expect
            Assert.Single(result);
            Assert.Same(nodeIn1, result[nodeIn1.Name]);
        }
        #endregion

        #region Tests for ApplyAddItem()
        [Fact]
        public void ApplyAddItem_NonConsumableItem_AddsIt()
        {
            // Given
            UnfinalizedItem item1 = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            UnfinalizedItem item2 = Model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = new UnfinalizedItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState
                .ApplyAddItem(item1)
                .ApplyAddItem(item2);

            // Expect
            Assert.Equal(2, inGameState.Inventory.NonConsumableItems.Count);
            Assert.Same(item1, inGameState.Inventory.NonConsumableItems[item1.Name]);
            Assert.Same(item2, inGameState.Inventory.NonConsumableItems[item2.Name]);
        }

        [Fact]
        public void ApplyAddItem_ExpansionItem_AddsItAndIncreasesCount()
        {
            // Given
            UnfinalizedItem item1 = Model.Items[SuperMetroidModel.MISSILE_NAME];
            UnfinalizedItem item2 = Model.Items[SuperMetroidModel.SUPER_NAME];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = new UnfinalizedItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState
                .ApplyAddItem(item1)
                .ApplyAddItem(item1)
                .ApplyAddItem(item2);

            // Expect
            Assert.Equal(2, inGameState.Inventory.ExpansionItems.Count);
            Assert.Same(item1, inGameState.Inventory.ExpansionItems[item1.Name].item);
            Assert.Equal(2, inGameState.Inventory.ExpansionItems[item1.Name].count);
            Assert.Same(item2, inGameState.Inventory.ExpansionItems[item2.Name].item);
            Assert.Equal(1, inGameState.Inventory.ExpansionItems[item2.Name].count);
        }
        #endregion

        #region Tests for ApplyDisableItem()
        [Fact]
        public void ApplyDisableItem_NonConsumableItem_DisablesIt()
        {
            // Given
            UnfinalizedItem item1 = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            UnfinalizedItem item2 = Model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME];
            UnfinalizedItem notPresentItem = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = new UnfinalizedItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState
                .ApplyAddItem(item1)
                .ApplyAddItem(item2);

            // When
            inGameState
                .ApplyDisableItem(item1)
                .ApplyDisableItem(notPresentItem);

            // Expect
            Assert.True(inGameState.Inventory.IsItemDisabled(item1));
            Assert.False(inGameState.Inventory.IsItemDisabled(item2));
            Assert.False(inGameState.Inventory.IsItemDisabled(notPresentItem));
            Assert.False(inGameState.Inventory.HasItem(item1));
            Assert.True(inGameState.Inventory.HasItem(item2));
        }

        [Fact]
        public void ApplyDisableItem_ExpansionItem_DoesNothing()
        {
            // Given
            UnfinalizedItem item1 = Model.Items[SuperMetroidModel.MISSILE_NAME];
            UnfinalizedItem item2 = Model.Items[SuperMetroidModel.SUPER_NAME];
            UnfinalizedItem notPresentItem = Model.Items[SuperMetroidModel.POWER_BOMB_NAME];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = new UnfinalizedItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState
                .ApplyAddItem(item1)
                .ApplyAddItem(item2);

            // When
            inGameState
                .ApplyDisableItem(item1)
                .ApplyDisableItem(notPresentItem);

            // Expect
            Assert.False(inGameState.Inventory.IsItemDisabled(item1));
            Assert.False(inGameState.Inventory.IsItemDisabled(item2));
            Assert.False(inGameState.Inventory.IsItemDisabled(notPresentItem));
            Assert.True(inGameState.Inventory.HasItem(item1));
            Assert.True(inGameState.Inventory.HasItem(item2));
        }
        #endregion

        #region Tests for ApplyEnableItem
        [Fact]
        public void ApplyEnableItem_NonConsumableItem_EnablesIt()
        {
            // Given
            UnfinalizedItem item = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingInventory = new UnfinalizedItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums());
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyAddItem(item);
            inGameState.ApplyDisableItem(item);

            // When
            inGameState.ApplyEnableItem(item);

            // Expect
            Assert.False(inGameState.Inventory.IsItemDisabled(item));
            Assert.True(inGameState.Inventory.HasItem(item));
        }
        #endregion

        #region Tests for GetInventoryExceptIn()
        [Fact]
        public void GetInventoryExceptIn_ReturnsDifference()
        {
            // Given
            UnfinalizedItem nonConsumableItemIn1 = Model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            UnfinalizedItem nonConsumableItemIn2 = Model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME];
            UnfinalizedItem nonConsumableItemInBoth = Model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];

            UnfinalizedItem expansionItemIn1 = Model.Items[SuperMetroidModel.MISSILE_NAME];
            UnfinalizedItem expansionItemIn2 = Model.Items[SuperMetroidModel.SUPER_NAME];
            UnfinalizedItem expansionItemInBoth = Model.Items[SuperMetroidModel.POWER_BOMB_NAME];
            UnfinalizedItem expansionItemMoreIn1 = Model.Items[SuperMetroidModel.ENERGY_TANK_NAME];
            UnfinalizedItem expansionItemMoreIn2 = Model.Items[SuperMetroidModel.RESERVE_TANK_NAME];

            UnfinalizedInGameState inGameState1 = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model))
                .ApplyAddItem(nonConsumableItemIn1)
                .ApplyAddItem(nonConsumableItemInBoth)
                .ApplyAddItem(expansionItemIn1)
                .ApplyAddItem(expansionItemInBoth)
                .ApplyAddItem(expansionItemMoreIn1)
                .ApplyAddItem(expansionItemMoreIn1)
                .ApplyAddItem(expansionItemMoreIn1)
                .ApplyAddItem(expansionItemMoreIn2);
            UnfinalizedInGameState inGameState2 = new UnfinalizedInGameState(UnfinalizedStartConditions.CreateVanillaStartConditions(Model))
                .ApplyAddItem(nonConsumableItemIn2)
                .ApplyAddItem(nonConsumableItemInBoth)
                .ApplyAddItem(expansionItemIn2)
                .ApplyAddItem(expansionItemInBoth)
                .ApplyAddItem(expansionItemMoreIn1)
                .ApplyAddItem(expansionItemMoreIn2)
                .ApplyAddItem(expansionItemMoreIn2)
                .ApplyAddItem(expansionItemMoreIn2);

            // When
            UnfinalizedItemInventory result = inGameState1.GetInventoryExceptIn(inGameState2);

            // Expect
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
        #endregion

        #region Tests for ApplyDestroyObstacle()
        [Fact]
        public void ApplyDestroyObstacle_AddsItToDestroyedObstacles()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 5);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyDestroyObstacle(inGameState.CurrentRoom.Obstacles["A"]);

            // Expect
            Assert.Single(inGameState.InRoomState.DestroyedObstacleIds);
            Assert.Contains("A", inGameState.InRoomState.DestroyedObstacleIds);
        }

        [Fact]
        public void ApplyDestroyObstacle_ObstacleNotInRoom_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyDestroyObstacle(Model.Rooms["Landing Site"].Obstacles["A"]));
        }
        #endregion

        #region Tests for ApplyEnterRoom()
        [Fact]
        public void ApplyEnterRoom_ChangesCurrentNode()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Red Tower", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyEnterRoom(expectedNode);

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }

        [Fact]
        public void ApplyEnterRoom_ClearsPreviousRoomState()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 5);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyDestroyObstacle(inGameState.CurrentRoom.Obstacles["A"]);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Landing Site", 2), inGameState.CurrentNode.LinksTo[2].Strats["Base"]);

            // When
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Parlor and Alcatraz", 4));

            // Expect
            Assert.Empty(inGameState.InRoomState.DestroyedObstacleIds);
            Assert.Single(inGameState.InRoomState.VisitedRoomPath);
        }

        [Fact]
        public void ApplyEnterRoom_AddsCurrentRoomStateCopyToRememberedRooms()
        {
            // Given
            UnfinalizedRoom initialRoom = Model.Rooms["Sloaters Refill"];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom(initialRoom.Name, 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));
            
            // Expect
            Assert.Same(initialRoom, inGameState.PreviousRoomStates[0].CurrentRoom);
            Assert.NotSame(inGameState.InRoomState, inGameState.PreviousRoomStates[0]);
        }

        [Fact]
        public void ApplyEnterRoom_SpawnsAtDifferentNode_GoesToCorrectNode()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Ice Beam Gate Room", 6);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Ice Beam Tutorial Room", 2);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Ice Beam Gate Room", 1));

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }

        [Fact]
        public void ApplyEnterRoom_GoingBeyondRememberedRooms_EliminatesOldestRoom()
        {
            // Given
            string initialRoomName = "Sloaters Refill";
            UnfinalizedRoomNode node1 = Model.GetNodeInRoom("Red Tower", 4);
            UnfinalizedRoomNode node2 = Model.GetNodeInRoom("Bat Room", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom(initialRoomName, 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Red Tower", 8), inGameState.CurrentNode.LinksTo[8].Strats["Base"]);
            inGameState.ApplyVisitNode(node1, inGameState.CurrentNode.LinksTo[4].Strats["Base"]);

            // When
            for (int i = 0; i < UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // Expect
            Assert.NotEqual(initialRoomName, inGameState.PreviousRoomStates.Last().CurrentRoom.Name);
        }
        #endregion

        #region Tests for ApplyExitRoom()
        [Fact]
        public void ApplyExitRoom_ChangesCurrentNode()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Red Tower", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyExitRoom(Model);

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }

        [Fact]
        public void ApplyExitRoom_ClearsPreviousRoomState()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 5);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyDestroyObstacle(inGameState.CurrentRoom.Obstacles["A"]);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Landing Site", 2), inGameState.CurrentNode.LinksTo[2].Strats["Base"]);

            // When
            inGameState.ApplyExitRoom(Model);

            // Expect
            Assert.Empty(inGameState.InRoomState.DestroyedObstacleIds);
            Assert.Single(inGameState.InRoomState.VisitedRoomPath);
        }

        [Fact]
        public void ApplyExitRoom_AddsCurrentRoomStateCopyToRememberedRooms()
        {
            // Given
            UnfinalizedRoom initialRoom = Model.Rooms["Sloaters Refill"];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom(initialRoom.Name, 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyExitRoom(Model);

            // Expect
            Assert.Same(initialRoom, inGameState.PreviousRoomStates[0].CurrentRoom);
            Assert.NotSame(inGameState.InRoomState, inGameState.PreviousRoomStates[0]);
        }

        [Fact]
        public void ApplyExitRoom_SpawnsAtDifferentNode_GoesToCorrectNode()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Ice Beam Gate Room", 6);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Ice Beam Tutorial Room", 2);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyExitRoom(Model);

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }

        [Fact]
        public void ApplyExitRoom_GoingBeyondRememberedRooms_EliminatesOldestRoom()
        {
            // Given
            string initialRoomName = "Sloaters Refill";
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom(initialRoomName, 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Red Tower", 8), inGameState.CurrentNode.LinksTo[8].Strats["Base"]);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Red Tower", 4), inGameState.CurrentNode.LinksTo[4].Strats["Base"]);

            // When
            for (int i = 0; i < UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                inGameState.ApplyExitRoom(Model);
            }

            // Expect
            Assert.NotEqual(initialRoomName, inGameState.PreviousRoomStates.Last().CurrentRoom.Name);
        }
        [Fact]
        public void ApplyExitRoom_NonDoorNode_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 7);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When and expect
            Assert.Throws<InvalidOperationException>(() => inGameState.ApplyExitRoom(Model));
        }

        [Fact]
        public void ApplyExitRoom_OneWayEntranceDoorNode_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("West Sand Hall", 3);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When and expect
            Assert.Throws<InvalidOperationException>(() => inGameState.ApplyExitRoom(Model));
        }

        [Fact]
        public void ApplyExitRoom_LockedDoorNode_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 4);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When and expect
            Assert.Throws<InvalidOperationException>(() => inGameState.ApplyExitRoom(Model));
        }

        [Fact]
        public void ApplyExitRoom_UnlockedDoorNode_Succeeds()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Crateria Tube", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 4);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyOpenLock(inGameState.CurrentNode.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"]);

            // When
            inGameState.ApplyExitRoom(Model);

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }

        [Fact]
        public void ApplyExitRoom_BypassedLock_Succeeds()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Crateria Tube", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 4);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyBypassLock(inGameState.CurrentNode.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"]);

            // When
            inGameState.ApplyExitRoom(Model);

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }
        #endregion

        #region Tests for ApplyVisitNode()
        [Fact]
        public void ApplyVisitNode_AccumulatesVisitedNodesAndStrats()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 8), inGameState.CurrentNode.LinksTo[8].Strats["Base"])
                .ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 1), inGameState.CurrentNode.LinksTo[1].Strats["Parlor Quick Charge"]);

            // Expect
            Assert.Equal("Parlor and Alcatraz", inGameState.CurrentRoom.Name);
            Assert.Equal(1, inGameState.CurrentNode.Id);
            Assert.Equal("Parlor Quick Charge", inGameState.InRoomState.LastStrat.Name);
            Assert.Equal(3, inGameState.GetVisitedPath().Count);

            Assert.Equal(4, inGameState.GetVisitedPath()[0].nodeState.Node.Id);
            Assert.Null(inGameState.GetVisitedPath()[0].strat);

            Assert.Equal(8, inGameState.GetVisitedPath()[1].nodeState.Node.Id);
            Assert.Equal("Base", inGameState.GetVisitedPath()[1].strat.Name);

            Assert.Equal(1, inGameState.GetVisitedPath()[2].nodeState.Node.Id);
            Assert.Equal("Parlor Quick Charge", inGameState.GetVisitedPath()[2].strat.Name);
        }

        [Fact]
        public void ApplyVisitNode_LinkDoesntExist_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 1), inGameState.CurrentNode.LinksTo[8].Strats["Base"]));
        }

        [Fact]
        public void ApplyVisitNode_StratNotOnOriginLink_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Parlor and Alcatraz", 4);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            UnfinalizedStrat wrongStrat = Model.GetNodeInRoom("Parlor and Alcatraz", 8).LinksTo[1].Strats["Base"];

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyVisitNode(Model.GetNodeInRoom("Parlor and Alcatraz", 8), wrongStrat));
        }
        #endregion

        #region Tests for GetInRoomState()
        [Fact]
        public void GetInRoomState_CurrentRoom_ReturnsCurrentRoomState()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Red Tower", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            // When
            ReadOnlyUnfinalizedInRoomState result = inGameState.GetInRoomState(0);

            // Expect
            Assert.Same(expectedNode.Room, result.CurrentRoom);
        }

        [Fact]
        public void GetInRoomState_PreviousRoom_ReturnsPreviousRoomState()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = expectedNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When
            ReadOnlyUnfinalizedInRoomState result = inGameState.GetInRoomState(1);

            // Expect
            Assert.Same(expectedNode.Room, result.CurrentRoom);
        }

        [Fact]
        public void GetInRoomState_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Oasis", 4);
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Oasis", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(expectedNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            // When
            ReadOnlyUnfinalizedInRoomState result = inGameState.GetInRoomState(1);

            // Expect
            Assert.Same(expectedNode.Room, result.CurrentRoom);
        }

        [Fact]
        public void GetInRoomState_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetInRoomState(-1));
        }

        [Fact]
        public void GetInRoomState_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            UnfinalizedRoomNode node1 = Model.GetNodeInRoom("Red Tower", 4);
            UnfinalizedRoomNode node2 = Model.GetNodeInRoom("Bat Room", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = node1;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            for(int i = 0; i <= UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            ReadOnlyUnfinalizedInRoomState result = inGameState.GetInRoomState(UnfinalizedInGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for GetCurrentOrPreviousRoom()
        [Fact]
        public void GetCurrentOrPreviousRoom_CurrentRoom_ReturnsCurrentRoom()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Red Tower", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            // When
            UnfinalizedRoom result = inGameState.GetCurrentOrPreviousRoom(0);

            // Expect
            Assert.Same(expectedNode.Room, result);
        }

        [Fact]
        public void GetCurrentOrPreviousRoom_PreviousRoom_ReturnsPreviousRoom()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = expectedNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When
            UnfinalizedRoom result = inGameState.GetCurrentOrPreviousRoom(1);

            // Expect
            Assert.Same(expectedNode.Room, result);
        }

        [Fact]
        public void GetCurrentOrPreviousRoom_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Oasis", 4);
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Oasis", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(expectedNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            // When
            UnfinalizedRoom result = inGameState.GetCurrentOrPreviousRoom(1);

            // Expect
            Assert.Same(expectedNode.Room, result);
        }

        [Fact]
        public void GetCurrentOrPreviousRoom_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetCurrentOrPreviousRoom(-1));
        }

        [Fact]
        public void GetCurrentOrPreviousRoom_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            UnfinalizedRoomNode node1 = Model.GetNodeInRoom("Red Tower", 4);
            UnfinalizedRoomNode node2 = Model.GetNodeInRoom("Bat Room", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = node1;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            for (int i = 0; i <= UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            UnfinalizedRoom result = inGameState.GetCurrentOrPreviousRoom(UnfinalizedInGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for GetCurrentOrPreviousRoomEnvironment()
        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_CurrentRoom_ReturnsCurrentRoomEnvironment()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Cathedral Entrance", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Business Center", 6);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            // When
            UnfinalizedRoomEnvironment result = inGameState.GetCurrentOrPreviousRoomEnvironment(0);

            // Expect
            Assert.Same(expectedNode.Room, result.Room);
            Assert.True(result.Heated);
        }

        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_PreviousRoom_ReturnsPreviousRoomEnvironment()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Business Center", 6);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = expectedNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Cathedral Entrance", 1));

            // When
            UnfinalizedRoomEnvironment result = inGameState.GetCurrentOrPreviousRoomEnvironment(1);

            // Expect
            Assert.Same(expectedNode.Room, result.Room);
            Assert.False(result.Heated);
        }

        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Oasis", 4);
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Oasis", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(expectedNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            // When
            UnfinalizedRoomEnvironment result = inGameState.GetCurrentOrPreviousRoomEnvironment(1);

            // Expect
            Assert.Same(expectedNode.Room, result.Room);
        }

        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetCurrentOrPreviousRoomEnvironment(-1));
        }

        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            UnfinalizedRoomNode node1 = Model.GetNodeInRoom("Red Tower", 4);
            UnfinalizedRoomNode node2 = Model.GetNodeInRoom("Bat Room", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = node1;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            for (int i = 0; i <= UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            UnfinalizedRoomEnvironment result = inGameState.GetCurrentOrPreviousRoomEnvironment(UnfinalizedInGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for GetLastStrat()
        [Fact]
        public void GetLastStrat_CurrentRoom_ReturnsCurrentRoomLastStrat()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Red Tower", 8), inGameState.CurrentNode.LinksTo[8].Strats["Base"]);
            UnfinalizedStrat expectedStrat = inGameState.CurrentNode.LinksTo[4].Strats["Base"];
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Red Tower", 4), expectedStrat);

            // Expect
            UnfinalizedStrat result = inGameState.GetLastStrat(0);

            // When
            Assert.Same(expectedStrat, result);
        }

        [Fact]
        public void GetLastStrat_NoLastStrat_ReturnsNull()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When
            UnfinalizedStrat result = inGameState.GetLastStrat(0);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void GetLastStrat_PreviousRoom_ReturnsPreviousRoomData()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Business Center", 8);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            UnfinalizedStrat expectedStrat = inGameState.CurrentNode.LinksTo[6].Strats["Base"];
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Business Center", 6), expectedStrat);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Cathedral Entrance", 1));

            // When
            UnfinalizedStrat result = inGameState.GetLastStrat(1);

            // Expect
            Assert.Same(expectedStrat, result);
        }

        [Fact]
        public void GetLastStrat_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Oasis", 4);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            UnfinalizedStrat expectedStrat = inGameState.CurrentNode.LinksTo[3].Strats["Base"];
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Oasis", 3), expectedStrat);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            // When
            UnfinalizedStrat result = inGameState.GetLastStrat(1);

            // Expect
            Assert.Same(expectedStrat, result);
        }

        [Fact]
        public void GetLastStrat_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetLastStrat(-1));
        }

        [Fact]
        public void GetLastStrat_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            UnfinalizedRoomNode room1DoorNode = Model.GetNodeInRoom("Red Tower", 4);
            UnfinalizedRoomNode room1OtherNode = Model.GetNodeInRoom("Red Tower", 8);
            UnfinalizedRoomNode room2DoorNode = Model.GetNodeInRoom("Bat Room", 1);
            UnfinalizedRoomNode room2OtherNode = Model.GetNodeInRoom("Bat Room", 2);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = room1OtherNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(room1DoorNode, inGameState.CurrentNode.LinksTo[room1DoorNode.Id].Strats["Base"]);
            for (int i = 0; i <= UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode doorNode = i % 2 == 0 ? room2DoorNode : room1DoorNode;
                UnfinalizedRoomNode otherNode = i % 2 == 0 ? room2OtherNode : room1OtherNode;
                inGameState.ApplyEnterRoom(doorNode);
                inGameState.ApplyVisitNode(otherNode, inGameState.CurrentNode.LinksTo[otherNode.Id].Strats["Base"]);
                inGameState.ApplyVisitNode(doorNode, inGameState.CurrentNode.LinksTo[doorNode.Id].Strats["Base"]);
            }

            // When
            UnfinalizedStrat result = inGameState.GetLastStrat(UnfinalizedInGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for GetVisitedNodeIds()
        [Fact]
        public void GetVisitedNodeIds_CurrentRoom_ReturnsCurrentRoomVisitedNodeIds()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Red Tower", 8), inGameState.CurrentNode.LinksTo[8].Strats["Base"]);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Red Tower", 4), inGameState.CurrentNode.LinksTo[4].Strats["Base"]);

            // When
            IReadOnlyList<int> result = inGameState.GetVisitedNodeIds(0);

            // Expect
            Assert.Equal(3, result.Count);
            Assert.Equal(3, result[0]);
            Assert.Equal(8, result[1]);
            Assert.Equal(4, result[2]);
        }

        [Fact]
        public void GetVisitedNodeIds_PreviousRoom_ReturnsPreviousRoomData()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Business Center", 8);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Business Center", 6), inGameState.CurrentNode.LinksTo[6].Strats["Base"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Cathedral Entrance", 1));

            // When
            IReadOnlyList<int> result = inGameState.GetVisitedNodeIds(1);

            // Expect
            Assert.Equal(2, result.Count);
            Assert.Equal(8, result[0]);
            Assert.Equal(6, result[1]);
        }

        [Fact]
        public void GetVisitedNodeIds_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Oasis", 4);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Oasis", 3), inGameState.CurrentNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            // When
            IReadOnlyList<int> result = inGameState.GetVisitedNodeIds(1);

            // Expect
            Assert.Equal(2, result.Count);
            Assert.Equal(4, result[0]);
            Assert.Equal(3, result[1]);
        }

        [Fact]
        public void GetVisitedNodeIds_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetVisitedNodeIds(-1));
        }

        [Fact]
        public void GetVisitedNodeIds_GoingBeyondRememberedRooms_ReturnsEmpty()
        {
            // Given
            UnfinalizedRoomNode room1DoorNode = Model.GetNodeInRoom("Red Tower", 4);
            UnfinalizedRoomNode room1OtherNode = Model.GetNodeInRoom("Red Tower", 8);
            UnfinalizedRoomNode room2DoorNode = Model.GetNodeInRoom("Bat Room", 1);
            UnfinalizedRoomNode room2OtherNode = Model.GetNodeInRoom("Bat Room", 2);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = room1OtherNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(room1DoorNode, inGameState.CurrentNode.LinksTo[room1DoorNode.Id].Strats["Base"]);
            for (int i = 0; i <= UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode doorNode = i % 2 == 0 ? room2DoorNode : room1DoorNode;
                UnfinalizedRoomNode otherNode = i % 2 == 0 ? room2OtherNode : room1OtherNode;
                inGameState.ApplyEnterRoom(doorNode);
                inGameState.ApplyVisitNode(otherNode, inGameState.CurrentNode.LinksTo[otherNode.Id].Strats["Base"]);
                inGameState.ApplyVisitNode(doorNode, inGameState.CurrentNode.LinksTo[doorNode.Id].Strats["Base"]);
            }

            // When
            IReadOnlyList<int> result = inGameState.GetVisitedNodeIds(UnfinalizedInGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Empty(result);
        }
        #endregion

        #region Tests for GetVisitedPath()
        [Fact]
        public void GetVisitedPath_CurrentRoom_ReturnsCurrentRoomVisitedPath()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            UnfinalizedRoomNode expectedNode1 = Model.GetNodeInRoom("Red Tower", 3);
            inGameState.ApplyEnterRoom(expectedNode1);
            UnfinalizedRoomNode expectedNode2 = Model.GetNodeInRoom("Red Tower", 8);
            UnfinalizedStrat expectedStrat2 = inGameState.CurrentNode.LinksTo[8].Strats["Base"];
            inGameState.ApplyVisitNode(expectedNode2, expectedStrat2);
            UnfinalizedRoomNode expectedNode3 = Model.GetNodeInRoom("Red Tower", 4);
            UnfinalizedStrat expectedStrat3 = inGameState.CurrentNode.LinksTo[4].Strats["Base"];
            inGameState.ApplyVisitNode(expectedNode3, expectedStrat3);

            // When
            var result = inGameState.GetVisitedPath(0);

            // Expect
            Assert.Equal(3, result.Count);
            Assert.Equal(expectedNode1, result[0].nodeState.Node);
            Assert.Null(result[0].strat);
            Assert.Equal(expectedNode2, result[1].nodeState.Node);
            Assert.Equal(expectedStrat2, result[1].strat);
            Assert.Equal(expectedNode3, result[2].nodeState.Node);
            Assert.Equal(expectedStrat3, result[2].strat);
        }

        [Fact]
        public void GetVisitedPath_PreviousRoom_ReturnsPreviousRoomData()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            UnfinalizedRoomNode expectedNode1 = Model.GetNodeInRoom("Business Center", 8);
            startConditions.StartingNode = expectedNode1;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            UnfinalizedRoomNode expectedNode2 = Model.GetNodeInRoom("Business Center", 6);
            UnfinalizedStrat expectedStrat2 = inGameState.CurrentNode.LinksTo[6].Strats["Base"];
            inGameState.ApplyVisitNode(expectedNode2, expectedStrat2);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Cathedral Entrance", 1));

            // When
            var result = inGameState.GetVisitedPath(1);

            // Expect
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedNode1, result[0].nodeState.Node);
            Assert.Null(result[0].strat);
            Assert.Equal(expectedNode2, result[1].nodeState.Node);
            Assert.Equal(expectedStrat2, result[1].strat);
        }

        [Fact]
        public void GetVisitedPath_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            UnfinalizedRoomNode expectedNode1 = Model.GetNodeInRoom("Oasis", 4);
            startConditions.StartingNode = expectedNode1;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            UnfinalizedRoomNode expectedNode2 = Model.GetNodeInRoom("Oasis", 3);
            UnfinalizedStrat expectedStrat2 = inGameState.CurrentNode.LinksTo[3].Strats["Base"];
            inGameState.ApplyVisitNode(expectedNode2, expectedStrat2);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            // When
            var result = inGameState.GetVisitedPath(1);

            // Expect
            Assert.Equal(2, result.Count);
            Assert.Equal(expectedNode1, result[0].nodeState.Node);
            Assert.Null(result[0].strat);
            Assert.Equal(expectedNode2, result[1].nodeState.Node);
            Assert.Equal(expectedStrat2, result[1].strat);
        }

        [Fact]
        public void GetVisitedPath_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetVisitedPath(-1));
        }

        [Fact]
        public void GetVisitedPath_GoingBeyondRememberedRooms_ReturnsEmpty()
        {
            // Given
            UnfinalizedRoomNode room1DoorNode = Model.GetNodeInRoom("Red Tower", 4);
            UnfinalizedRoomNode room1OtherNode = Model.GetNodeInRoom("Red Tower", 8);
            UnfinalizedRoomNode room2DoorNode = Model.GetNodeInRoom("Bat Room", 1);
            UnfinalizedRoomNode room2OtherNode = Model.GetNodeInRoom("Bat Room", 2);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = room1OtherNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(room1DoorNode, inGameState.CurrentNode.LinksTo[room1DoorNode.Id].Strats["Base"]);
            for (int i = 0; i <= UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode doorNode = i % 2 == 0 ? room2DoorNode : room1DoorNode;
                UnfinalizedRoomNode otherNode = i % 2 == 0 ? room2OtherNode : room1OtherNode;
                inGameState.ApplyEnterRoom(doorNode);
                inGameState.ApplyVisitNode(otherNode, inGameState.CurrentNode.LinksTo[otherNode.Id].Strats["Base"]);
                inGameState.ApplyVisitNode(doorNode, inGameState.CurrentNode.LinksTo[doorNode.Id].Strats["Base"]);
            }

            // When
            var result = inGameState.GetVisitedPath(UnfinalizedInGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Empty(result);
        }
        #endregion

        #region Tests for GetDestroyedObstacleIds()
        [Fact]
        public void GetDestroyedObstacleIds_CurrentRoom_ReturnsCurrentRoomData()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Hellway", 2);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Caterpillar Room", 2));
            inGameState.ApplyDestroyObstacle(inGameState.CurrentRoom.Obstacles["A"]);

            // When
            IEnumerable<string> result = inGameState.GetDestroyedObstacleIds(0);

            // Expect
            Assert.Single(result);
            Assert.Equal("A", result.First());
        }

        [Fact]
        public void GetDestroyedObstacleIds_PreviousRoom_ReturnsPreviousRoomData()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Caterpillar Room", 2);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyDestroyObstacle(inGameState.CurrentRoom.Obstacles["A"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Hellway", 2));

            // When
            IEnumerable<string> result = inGameState.GetDestroyedObstacleIds(1);

            // Expect
            Assert.Single(result);
            Assert.Equal("A", result.First());
        }

        [Fact]
        public void GetDestroyedObstacleIds_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Caterpillar Room", 2);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyDestroyObstacle(inGameState.CurrentRoom.Obstacles["A"]);
            //Note: This is an invalid connection, but it's not ApplyEnterRoom()'s job to validate that
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            // When
            IEnumerable<string> result = inGameState.GetDestroyedObstacleIds(1);

            // Expect
            Assert.Single(result);
            Assert.Equal("A", result.First());
        }

        [Fact]
        public void GetDestroyedObstacleIds_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetDestroyedObstacleIds(-1));
        }

        [Fact]
        public void GetDestroyedObstacleIds_GoingBeyondRememberedRooms_ReturnsEmpty()
        {
            // Given
            UnfinalizedRoomNode node1 = Model.GetNodeInRoom("Caterpillar Room", 2);
            UnfinalizedRoomObstacle obstacle1 = node1.Room.Obstacles["A"];
            UnfinalizedRoomNode node2 = Model.GetNodeInRoom("Beta Power Bomb Room", 1);
            UnfinalizedRoomObstacle obstacle2 = node2.Room.Obstacles["B"];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = node1;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyDestroyObstacle(obstacle1);
            for (int i = 0; i <= UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode node = i % 2 == 0 ? node2 : node1;
                UnfinalizedRoomObstacle obstacle = i % 2 == 0 ? obstacle2 : obstacle1;
                inGameState.ApplyEnterRoom(node);
                inGameState.ApplyDestroyObstacle(obstacle);
            }

            // When
            IEnumerable<string> result = inGameState.GetDestroyedObstacleIds(UnfinalizedInGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Empty(result);
        }
        #endregion

        #region Tests for IsHeatedRoom()
        [Fact]
        public void IsHeatedRoom_CurrentRoom_ReturnsCurrentRoom()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Business Center", 6);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Cathedral Entrance", 1));

            // When
            bool result = inGameState.IsHeatedRoom(0);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsHeatedRoom_ConditionalEnteringFromHeatedNode_ReturnsTrue()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Volcano Room", 2);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            bool result = inGameState.IsHeatedRoom(0);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsHeatedRoom_ConditionalEnteringFromNonHeatedNode_ReturnsTrue()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Volcano Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            bool result = inGameState.IsHeatedRoom(0);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsHeatedRoom_PreviousRoom_ReturnsPreviousRoom()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Business Center", 6);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Cathedral Entrance", 1));

            // When
            bool result = inGameState.IsHeatedRoom(1);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsHeatedRoom_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Cathedral Entrance", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            // Not a valid connection but it's not ApplyEnterRoom()'s job to know that
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            // When
            bool result = inGameState.IsHeatedRoom(1);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsHeatedRoom_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.IsHeatedRoom(-1));
        }

        [Fact]
        public void IsHeatedRoom_GoingBeyondRememberedRooms_ReturnsFalse()
        {
            // Given
            UnfinalizedRoomNode node1 = Model.GetNodeInRoom("Bat Cave", 2);
            UnfinalizedRoomNode node2 = Model.GetNodeInRoom("Speed Booster Hall", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = node1;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            for (int i = 0; i <= UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            bool result = inGameState.IsHeatedRoom(UnfinalizedInGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.False(result);
        }
        #endregion

        #region Tests for GetCurrentDoorEnvironment()
        [Fact]
        public void GetCurrentDoorEnvironment_CurrentRoom_ReturnsCurrentRoomData()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Cathedral Entrance", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Business Center", 6);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            // When
            UnfinalizedDoorEnvironment result = inGameState.GetCurrentDoorEnvironment(0);

            // Expect
            Assert.Same(expectedNode, result.Node);
        }

        [Fact]
        public void GetCurrentDoorEnvironment_ConditionalFromSameEntranceNode_ReturnsCorrectEnvironment()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Volcano Room", 2);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            UnfinalizedDoorEnvironment result = inGameState.GetCurrentDoorEnvironment(0);

            // Expect
            Assert.Equal(PhysicsEnum.Normal, result.Physics);
        }

        [Fact]
        public void GetCurrentDoorEnvironment_ConditionalFromDifferentEntranceNode_ReturnsCorrectEnvironment()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Volcano Room", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Volcano Room", 2), startNode.LinksTo[2].Strats["Base"]);

            // When
            UnfinalizedDoorEnvironment result = inGameState.GetCurrentDoorEnvironment(0);

            // Expect
            Assert.Equal(PhysicsEnum.Lava, result.Physics);
        }

        [Fact]
        public void GetCurrentDoorEnvironment_PreviousRoom_ReturnsPreviousRoomData()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Crab Hole", 2);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = expectedNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Boyon Gate Hall", 3));

            // When
            UnfinalizedDoorEnvironment result = inGameState.GetCurrentDoorEnvironment(1);

            // Expect
            Assert.Same(expectedNode, result.Node);
            Assert.Equal(PhysicsEnum.Water, result.Physics);
        }

        [Fact]
        public void GetCurrentDoorEnvironment_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Oasis", 4);
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Oasis", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(expectedNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            // When
            UnfinalizedDoorEnvironment result = inGameState.GetCurrentDoorEnvironment(1);

            // Expect
            Assert.Same(expectedNode, result.Node);
            Assert.Equal(PhysicsEnum.Water, result.Physics);
        }

        [Fact]
        public void GetCurrentDoorEnvironment_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetCurrentDoorEnvironment(-1));
        }

        [Fact]
        public void GetCurrentDoorEnvironment_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            UnfinalizedRoomNode node1 = Model.GetNodeInRoom("Crab Hole", 2);
            UnfinalizedRoomNode node2 = Model.GetNodeInRoom("Boyon Gate Hall", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = node1;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            for (int i = 0; i <= UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            UnfinalizedDoorEnvironment result = inGameState.GetCurrentDoorEnvironment(UnfinalizedInGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for GetCurrentDoorPhysics()
        [Fact]
        public void GetCurrentDoorPhysics_CurrentRoom_ReturnsCurrentRoomData()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Boyon Gate Hall", 3);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Crab Hole", 2));

            // When
            PhysicsEnum? result = inGameState.GetCurrentDoorPhysics(0);

            // Expect
            Assert.Equal(PhysicsEnum.Water, result);
        }

        [Fact]
        public void GetCurrentDoorPhysics_ConditionalFromSameEntranceNode_ReturnsCorrectEnvironment()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Volcano Room", 2);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            PhysicsEnum? result = inGameState.GetCurrentDoorPhysics(0);

            // Expect
            Assert.Equal(PhysicsEnum.Normal, result);
        }

        [Fact]
        public void GetCurrentDoorPhysics_ConditionalFromDifferentEntranceNode_ReturnsCorrectEnvironment()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Volcano Room", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Volcano Room", 2), startNode.LinksTo[2].Strats["Base"]);

            // When
            PhysicsEnum? result = inGameState.GetCurrentDoorPhysics(0);

            // Expect
            Assert.Equal(PhysicsEnum.Lava, result);
        }

        [Fact]
        public void GetCurrentDoorPhysics_PreviousRoom_ReturnsPreviousRoomData()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Crab Hole", 2);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Boyon Gate Hall", 3));

            // When
            PhysicsEnum? result = inGameState.GetCurrentDoorPhysics(1);

            // Expect
            Assert.Equal(PhysicsEnum.Water, result);
        }

        [Fact]
        public void GetCurrentDoorPhysics_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Oasis", 4);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Oasis", 3), startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            // When
            PhysicsEnum? result = inGameState.GetCurrentDoorPhysics(1);

            // Expect
            Assert.Equal(PhysicsEnum.Water, result);
        }

        [Fact]
        public void GetCurrentDoorPhysics_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetCurrentDoorPhysics(-1));
        }

        [Fact]
        public void GetCurrentDoorPhysics_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            UnfinalizedRoomNode node1 = Model.GetNodeInRoom("Crab Hole", 2);
            UnfinalizedRoomNode node2 = Model.GetNodeInRoom("Boyon Gate Hall", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = node1;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            for (int i = 0; i <= UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            PhysicsEnum? result = inGameState.GetCurrentDoorPhysics(UnfinalizedInGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for GetCurrentNode()
        [Fact]
        public void GetCurrentNode_CurrentRoom_ReturnsCurrentNode()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Red Tower", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            // When
            UnfinalizedRoomNode result = inGameState.GetCurrentNode(0);

            // Expect
            Assert.Same(expectedNode, result);
        }

        [Fact]
        public void GetCurrentNode_PreviousRoom_ReturnsLastNodeOfPreviousRoom()
        {
            // Given
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = expectedNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When
            UnfinalizedRoomNode result = inGameState.GetCurrentNode(1);

            // Expect
            Assert.Same(expectedNode, result);
        }

        [Fact]
        public void GetCurrentNode_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Oasis", 4);
            UnfinalizedRoomNode expectedNode = Model.GetNodeInRoom("Oasis", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(expectedNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            // When
            UnfinalizedRoomNode result = inGameState.GetCurrentNode(1);

            // Expect
            Assert.Same(expectedNode, result);
        }

        [Fact]
        public void GetCurrentNode_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetCurrentNode(-1));
        }

        [Fact]
        public void GetCurrentNode_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            UnfinalizedRoomNode node1 = Model.GetNodeInRoom("Red Tower", 4);
            UnfinalizedRoomNode node2 = Model.GetNodeInRoom("Bat Room", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = node1;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            for (int i = 0; i <= UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            UnfinalizedRoomNode result = inGameState.GetCurrentNode(UnfinalizedInGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for BypassingExitLock()
        [Fact]
        public void BypassingExitLock_CurrentRoomNotBypassing_ReturnsFalse()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Business Center", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            bool result = inGameState.BypassingExitLock(0);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void BypassingExitLock_CurrentRoomBypassing_ReturnsTrue()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Business Center", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyBypassLock(startNode.Locks["Business Center Top Left Green Lock (to Ice Beam Gate)"]);

            // When
            bool result = inGameState.BypassingExitLock(0);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void BypassingExitLock_PreviousRoom_ReturnsLastRoomData()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Business Center", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyBypassLock(startNode.Locks["Business Center Top Left Green Lock (to Ice Beam Gate)"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Ice Beam Gate Room", 4));

            // When
            bool result = inGameState.BypassingExitLock(1);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void BypassingExitLock_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Oasis", 4);
            UnfinalizedRoomNode exitNode = Model.GetNodeInRoom("Oasis", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(exitNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyBypassLock(exitNode.Locks["Oasis Green Lock (to Toilet)"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            // When
            bool result = inGameState.BypassingExitLock(1);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void BypassingExitLock_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.BypassingExitLock(-1));
        }

        [Fact]
        public void BypassingExitLock_GoingBeyondRememberedRooms_ReturnsFalse()
        {
            // Given
            UnfinalizedRoomNode node1 = Model.GetNodeInRoom("Red Brinstar Elevator Room", 1);
            UnfinalizedNodeLock lock1 = node1.Locks["Red Brinstar Elevator Yellow Lock (to Kihunters)"];
            UnfinalizedRoomNode node2 = Model.GetNodeInRoom("Crateria Kihunter Room", 3);
            UnfinalizedNodeLock lock2 = node2.Locks["Crateria Kihunter Room Bottom Yellow Lock (to Elevator)"];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = node1;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyBypassLock(lock1);
            for (int i = 0; i <= UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode node = i % 2 == 0 ? node2 : node1;
                UnfinalizedNodeLock nodeLock = i % 2 == 0 ? lock2 : lock1;
                inGameState.ApplyEnterRoom(node);
                inGameState.ApplyBypassLock(nodeLock);
            }

            // When
            bool result = inGameState.BypassingExitLock(UnfinalizedInGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.False(result);
        }
        #endregion

        #region Tests for OpeningExitLock()
        [Fact]
        public void OpeningExitLock_CurrentRoomNotOpening_ReturnsFalse()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Business Center", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            bool result = inGameState.OpeningExitLock(0);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void OpeningExitLock_CurrentRoomOpening_ReturnsTrue()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Business Center", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyOpenLock(startNode.Locks["Business Center Top Left Green Lock (to Ice Beam Gate)"]);

            // When
            bool result = inGameState.OpeningExitLock(0);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void OpeningExitLock_PreviousRoom_ReturnsLastRoomData()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Business Center", 1);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyOpenLock(startNode.Locks["Business Center Top Left Green Lock (to Ice Beam Gate)"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Ice Beam Gate Room", 4));

            // When
            bool result = inGameState.OpeningExitLock(1);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void OpeningExitLock_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            UnfinalizedRoomNode startNode = Model.GetNodeInRoom("Oasis", 4);
            UnfinalizedRoomNode exitNode = Model.GetNodeInRoom("Oasis", 3);
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = startNode;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(exitNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyOpenLock(exitNode.Locks["Oasis Green Lock (to Toilet)"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Toilet Bowl", 2)); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Plasma Spark Room", 1));

            // When
            bool result = inGameState.OpeningExitLock(1);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void OpeningExitLock_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Sloaters Refill", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Red Tower", 3));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.OpeningExitLock(-1));
        }

        [Fact]
        public void OpeningExitLock_GoingBeyondRememberedRooms_ReturnsFalse()
        {
            // Given
            UnfinalizedRoomNode node1 = Model.GetNodeInRoom("Red Brinstar Elevator Room", 1);
            UnfinalizedNodeLock lock1 = node1.Locks["Red Brinstar Elevator Yellow Lock (to Kihunters)"];
            UnfinalizedRoomNode node2 = Model.GetNodeInRoom("Crateria Kihunter Room", 3);
            UnfinalizedNodeLock lock2 = node2.Locks["Crateria Kihunter Room Bottom Yellow Lock (to Elevator)"];
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = node1;
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyOpenLock(lock1);
            for (int i = 0; i <= UnfinalizedInGameState.MaxPreviousRooms; i++)
            {
                UnfinalizedRoomNode node = i % 2 == 0 ? node2 : node1;
                UnfinalizedNodeLock nodeLock = i % 2 == 0 ? lock2 : lock1;
                inGameState.ApplyEnterRoom(node);
                inGameState.ApplyOpenLock(nodeLock);
            }

            // When
            bool result = inGameState.OpeningExitLock(UnfinalizedInGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.False(result);
        }
        #endregion

        #region Tests for GetRetroactiveRunways()
        [Fact]
        public void GetRetroactiveRunways_NoPreviousRoom_ReturnsEmpty()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 5);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            IEnumerable<UnfinalizedRunway> result = inGameState.GetRetroactiveRunways(new int[] { 5 }, acceptablePhysics: null);

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveRunways_ReturnsRunways()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Seaweed Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            IEnumerable<UnfinalizedRunway> expected = inGameState.CurrentNode.Runways.Values;
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Big Boy Room", 1));

            // When
            IEnumerable<UnfinalizedRunway> result = inGameState.GetRetroactiveRunways(new int[] { inGameState.CurrentNode.Id }, acceptablePhysics: null);

            // Expect
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.Count(), result.Intersect(expected, ObjectReferenceEqualityComparer<UnfinalizedRunway>.Default).Count());
        }

        [Fact]
        public void GetRetroactiveRunways_PreviousRoomNodeUnconnected_ReturnsEmpty()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Seaweed Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Big Boy Room", 2));

            // When
            IEnumerable<UnfinalizedRunway> result = inGameState.GetRetroactiveRunways(new int[] { inGameState.CurrentNode.Id }, acceptablePhysics: null);

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveRunways_VisitedPathMismatch_ReturnsEmpty()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Seaweed Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Big Boy Room", 1));
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Big Boy Room", 2), inGameState.CurrentNode.LinksTo[2].Strats["Base"]);

            // When
            IEnumerable<UnfinalizedRunway> result = inGameState.GetRetroactiveRunways(new int[] { inGameState.GetVisitedNodeIds()[0]}, acceptablePhysics: null);

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveRunways_MultiNodePathMatch_ReturnsRunways()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Seaweed Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            IEnumerable<UnfinalizedRunway> expected = inGameState.CurrentNode.Runways.Values;
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Big Boy Room", 1));
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Big Boy Room", 2), inGameState.CurrentNode.LinksTo[2].Strats["Base"]);

            // When
            IEnumerable<UnfinalizedRunway> result = inGameState.GetRetroactiveRunways(inGameState.GetVisitedNodeIds(), acceptablePhysics: null);

            // Expect
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.Count(), result.Intersect(expected, ObjectReferenceEqualityComparer<UnfinalizedRunway>.Default).Count());
        }

        [Fact]
        public void GetRetroactiveRunways_PhysicsMatch_ReturnsRunways()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Seaweed Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            IEnumerable<UnfinalizedRunway> expected = inGameState.CurrentNode.Runways.Values;
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Big Boy Room", 1));

            // When
            IEnumerable<UnfinalizedRunway> result = inGameState.GetRetroactiveRunways(new int[] { inGameState.CurrentNode.Id }, acceptablePhysics: new HashSet<PhysicsEnum> { PhysicsEnum.Normal });

            // Expect
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.Count(), result.Intersect(expected, ObjectReferenceEqualityComparer<UnfinalizedRunway>.Default).Count());
        }

        [Fact]
        public void GetRetroactiveRunways_PhysicsMismatch_ReturnsEmpty()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Seaweed Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Big Boy Room", 1));

            // When
            IEnumerable<UnfinalizedRunway> result = inGameState.GetRetroactiveRunways(new int[] { inGameState.CurrentNode.Id }, acceptablePhysics: new HashSet<PhysicsEnum> { PhysicsEnum.Water });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveRunways_BypassingLock_ReturnsEmpty()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 4);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyBypassLock(inGameState.CurrentNode.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Crateria Tube", 1));

            // When
            IEnumerable<UnfinalizedRunway> result = inGameState.GetRetroactiveRunways(new int[] { inGameState.CurrentNode.Id }, acceptablePhysics: null);

            // Expect
            Assert.Empty(result);
        }
        #endregion

        #region Tests for GetRetroactiveCanLeaveChargeds()
        [Fact]
        public void GetRetroactiveCanLeaveChargeds_NoPreviousRoom_ReturnsEmpty()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 5);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { 5 });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_ReturnsCanLeaveChargeds()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Green Hill Zone", 3);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            IEnumerable<UnfinalizedCanLeaveCharged> expected = inGameState.CurrentNode.CanLeaveCharged;
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Noob Bridge aka A Bridge Too Far", 1));

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.Count(), result.Intersect(expected, ObjectReferenceEqualityComparer<UnfinalizedCanLeaveCharged>.Default).Count());
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_PreviousRoomNodeUnconnected_ReturnsEmpty()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Green Hill Zone", 3);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Noob Bridge aka A Bridge Too Far", 2));

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_VisitedPathMismatch_ReturnsEmpty()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Green Hill Zone", 3);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Noob Bridge aka A Bridge Too Far", 1));
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Noob Bridge aka A Bridge Too Far", 2), inGameState.CurrentNode.LinksTo[2].Strats["Base"]);

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { inGameState.GetVisitedNodeIds()[0] });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_MultiNodePathMatch_ReturnsCanLeaveChargeds()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Green Hill Zone", 3);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            IEnumerable<UnfinalizedCanLeaveCharged> expected = inGameState.CurrentNode.CanLeaveCharged;
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Noob Bridge aka A Bridge Too Far", 1));
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Noob Bridge aka A Bridge Too Far", 2), inGameState.CurrentNode.LinksTo[2].Strats["Base"]);

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, inGameState.GetVisitedNodeIds());

            // Expect
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.Count(), result.Intersect(expected, ObjectReferenceEqualityComparer<UnfinalizedCanLeaveCharged>.Default).Count());
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_BypassingLock_ReturnsEmpty()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Crab Shaft", 2);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyBypassLock(inGameState.CurrentNode.Locks["Crab Shaft Green Lock (to Aqueduct)"]);
            inGameState.ApplyEnterRoom(Model.GetNodeInRoom("Aqueduct", 1));

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_FollowingPathToDoor_ReturnsCanLeaveCharged()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Early Supers Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Early Supers Room", 2), inGameState.CurrentNode.LinksTo[2].Strats["Speed Through"]);
            IEnumerable<UnfinalizedCanLeaveCharged> expected = inGameState.CurrentNode.CanLeaveCharged;
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.Count(), result.Intersect(expected, ObjectReferenceEqualityComparer<UnfinalizedCanLeaveCharged>.Default).Count());
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_FollowingPathToDoor_BypassingLock_ReturnsEmpty()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Early Supers Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Early Supers Room", 2), inGameState.CurrentNode.LinksTo[2].Strats["Speed Through"]);
            inGameState.ApplyBypassLock(inGameState.CurrentNode.Locks["Early Supers Red Lock (to Brinstar Reserve)"]);
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_NotFollowingPathToDoor_ReturnsEmpty()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Early Supers Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Early Supers Room", 2), inGameState.CurrentNode.LinksTo[2].Strats["Speed Through"]);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Early Supers Room", 4), inGameState.CurrentNode.LinksTo[4].Strats["Base"]);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Early Supers Room", 2), inGameState.CurrentNode.LinksTo[2].Strats["Base"]);
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_FollowingPathToDoorWithWrongStrat_ReturnsEmpty()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Early Supers Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Early Supers Room", 2), inGameState.CurrentNode.LinksTo[2].Strats["Early Supers Mockball"]);
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_RequiringOpenDoorButNotVisited_ExcludesCanLeaveCharged()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 3);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Landing Site", 1), inGameState.CurrentNode.LinksTo[1].Strats["Shinespark"]);
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result.Where(clc => clc.InitiateRemotely.MustOpenDoorFirst));
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_RequiringOpenDoorAndWasVisited_IncludesCanLeaveCharged()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Landing Site", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Landing Site", 4), inGameState.CurrentNode.LinksTo[4].Strats["Shinespark"]);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Landing Site", 3), inGameState.CurrentNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Landing Site", 1), inGameState.CurrentNode.LinksTo[1].Strats["Shinespark"]);
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Single(result.Where(clc => clc.InitiateRemotely.MustOpenDoorFirst));
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_RequiringOpenLockedDoorAndWasVisitedButNotUnlocked_ExcludesCanLeaveCharged()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Red Brinstar Fireflea Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Red Brinstar Fireflea Room", 2), inGameState.CurrentNode.LinksTo[2].Strats["Base"]);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Red Brinstar Fireflea Room", 1), inGameState.CurrentNode.LinksTo[1].Strats["In-Room Shinespark"]);
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result.Where(clc => clc.InitiateRemotely.MustOpenDoorFirst));
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_RequiringOpenLockedDoorAndWasVisitedButNotUnlockedUntilLeaving_ExcludesCanLeaveCharged()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Red Brinstar Fireflea Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Red Brinstar Fireflea Room", 2), inGameState.CurrentNode.LinksTo[2].Strats["Base"]);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Red Brinstar Fireflea Room", 1), inGameState.CurrentNode.LinksTo[1].Strats["In-Room Shinespark"]);
            inGameState.ApplyOpenLock(inGameState.CurrentNode.Locks["Red Firefleas Red Lock (to X-Ray)"]);
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result.Where(clc => clc.InitiateRemotely.MustOpenDoorFirst));
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_RequiringOpenDoorLockedAndWasUnlocked_IncludesCanLeaveCharged()
        {
            // Given
            UnfinalizedStartConditions startConditions = UnfinalizedStartConditions.CreateVanillaStartConditions(Model);
            startConditions.StartingNode = Model.GetNodeInRoom("Red Brinstar Fireflea Room", 1);
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);
            inGameState.ApplyOpenLock(inGameState.CurrentNode.Locks["Red Firefleas Red Lock (to X-Ray)"]);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Red Brinstar Fireflea Room", 2), inGameState.CurrentNode.LinksTo[2].Strats["Base"]);
            inGameState.ApplyVisitNode(Model.GetNodeInRoom("Red Brinstar Fireflea Room", 1), inGameState.CurrentNode.LinksTo[1].Strats["In-Room Shinespark"]);
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<UnfinalizedCanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(Model, new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Single(result.Where(clc => clc.InitiateRemotely.MustOpenDoorFirst));
        }
        #endregion

        #region Tests for Clone()
        [Fact]
        public void Clone_CopiesCorrectly()
        {
            // Given
            string startingRoomName = "Business Center";
            int startingNodeId = 7;
            string startingLockName = "Business Center Top Left Green Lock (to Ice Beam Gate)";
            int startingEnergy = 50;
            string maridiaTubeFlag = "f_MaridiaTubeBroken";
            UnfinalizedRoomNode variaNode = Model.GetNodeInRoom("Varia Suit Room", 2);
            UnfinalizedStartConditions startConditions = new UnfinalizedStartConditions
            {
                StartingGameFlags = new UnfinalizedGameFlag[] { Model.GameFlags[maridiaTubeFlag] },
                StartingInventory = UnfinalizedItemInventory.CreateVanillaStartingInventory(Model).ApplyAddItem(Model.Items[SuperMetroidModel.VARIA_SUIT_NAME]),
                StartingNode = Model.GetNodeInRoom(startingRoomName, startingNodeId),
                StartingOpenLocks = new UnfinalizedNodeLock[] { Model.Locks[startingLockName] },
                StartingTakenItemLocations = new UnfinalizedRoomNode[] { variaNode },
                StartingResources = new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, startingEnergy)
            };

            // When
            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions).Clone();

            // Expect
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
            // Given
            string startingRoomName = "Business Center";
            int startingNodeId = 7;
            string startingLockName = "Business Center Top Left Green Lock (to Ice Beam Gate)";
            string secondLockName = "Business Center Bottom Left Red Lock (to HiJump E-Tank)";
            int startingEnergy = 50;
            string maridiaTubeFlag = "f_MaridiaTubeBroken";
            UnfinalizedRoomNode variaNode = Model.GetNodeInRoom("Varia Suit Room", 2);
            UnfinalizedStartConditions startConditions = new UnfinalizedStartConditions
            {
                StartingInventory = UnfinalizedItemInventory.CreateVanillaStartingInventory(Model).ApplyAddItem(Model.Items["Missile"]),
                StartingNode = Model.GetNodeInRoom(startingRoomName, startingNodeId),
                StartingOpenLocks = new UnfinalizedNodeLock[] { Model.Locks[startingLockName] },
                StartingResources = new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, startingEnergy)
                    .ApplyAmount(RechargeableResourceEnum.Missile, 5)
            };

            UnfinalizedInGameState inGameState = new UnfinalizedInGameState(startConditions);

            // When
            UnfinalizedInGameState clone = inGameState.Clone();

            // Subsequently given
            // Modify the clone
            clone.ApplyVisitNode(Model.GetNodeInRoom(startingRoomName, 8), clone.GetCurrentNode().LinksTo[8].Strats["Base"]);
            clone.ApplyVisitNode(Model.GetNodeInRoom(startingRoomName, 3), clone.GetCurrentNode().LinksTo[3].Strats["Base"]);
            clone.ApplyOpenLock(Model.Locks[secondLockName]);
            clone.ApplyTakeLocation(variaNode);
            clone.ApplyAddItem(Model.Items[SuperMetroidModel.VARIA_SUIT_NAME]);
            clone.ApplyAddItem(Model.Items[SuperMetroidModel.MISSILE_NAME]);
            clone.ApplyAddResource(RechargeableResourceEnum.Missile, 2);
            clone.ApplyAddGameFlag(Model.GameFlags[maridiaTubeFlag]);

            // Expect
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
        #endregion
    }
}
