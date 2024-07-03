using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
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
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Tests.TestTools;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.InGameStates
{
    public class InGameStateTest
    {
        /// <summary>
        /// Returns all values of <see cref="RechargeableResourceEnum"/> in a format that can be used by <see cref="MemberDataAttribute"/>.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> RechargeableResourceValues()
        {

            foreach (RechargeableResourceEnum resource in Enum.GetValues<RechargeableResourceEnum>())
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

            foreach (ConsumableResourceEnum resource in Enum.GetValues< ConsumableResourceEnum>())
            {
                yield return new object[] { resource };
            }
        }

        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel _otherReusableModel = StaticTestObjects.UnfinalizedModel.Finalize();
        private static SuperMetroidModel OtherReusableModel() => _otherReusableModel;

        #region Tests for ctor(StartConditions)
        [Fact]
        public void ConstructorWithStartConditions_InitializesProperly()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            string startingRoomName = "Business Center";
            int startingNodeId = 7;
            string startingLockName = "Business Center Top Left Green Lock (to Ice Beam Gate)";
            int startingEnergy = 50;
            string maridiaTubeFlag = "f_MaridiaTubeBroken";
            RoomNode variaNode = model.GetNodeInRoom("Varia Suit Room", 2);
            StartConditions startConditions = new StartConditions
            (
                model: model,
                startingGameFlags: new GameFlag[] { model.GameFlags[maridiaTubeFlag] },
                startingInventory: ItemInventory.CreateVanillaStartingInventory(model).ApplyAddItem(model.Items[SuperMetroidModel.VARIA_SUIT_NAME]),
                startingNode: model.GetNodeInRoom(startingRoomName, startingNodeId),
                startingOpenLocks: new NodeLock[] { model.Locks[startingLockName] },
                startingTakenItemLocations: new RoomNode[] { variaNode },
                startingResources: new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, startingEnergy)
            );

            // When
            InGameState inGameState = new InGameState(startConditions);

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
            SuperMetroidModel model = ReusableModel();
            ResourceCount resourceCount = new ResourceCount();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(resourceCount)
                .StartingResources(resourceCount)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

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
            SuperMetroidModel model = ReusableModel();
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amount);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(resourceCount)
                .StartingResources(resourceCount)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            bool result = inGameState.IsResourceAvailable(resource.ToConsumableResource(), amount);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingExactPresentAmount_Energy_ReturnsFalse()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            int amount = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amount);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(resourceCount)
                .StartingResources(resourceCount)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

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
            SuperMetroidModel model = ReusableModel();
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amountToRequest + 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(resourceCount)
                .StartingResources(resourceCount)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            bool result = inGameState.IsResourceAvailable(resource.ToConsumableResource(), amountToRequest);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_Energy_ReturnsTrue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amountToRequest + 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(resourceCount)
                .StartingResources(resourceCount)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            bool result = inGameState.IsResourceAvailable(ConsumableResourceEnum.Energy, amountToRequest);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingLessThanPresentAmount_EnergyMixOfReserveAndNormal_ReturnsTrue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, 3);
            resourceCount.ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(resourceCount)
                .StartingResources(resourceCount)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

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
            SuperMetroidModel model = ReusableModel();
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(resource, amountToRequest - 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(resourceCount)
                .StartingResources(resourceCount)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            bool result = inGameState.IsResourceAvailable(resource.ToConsumableResource(), amountToRequest);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsResourceAvailable_RequestingMoreThanPresentAmount_Energy_ReturnsFalse()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            int amountToRequest = 5;
            ResourceCount resourceCount = new ResourceCount();
            resourceCount.ApplyAmount(RechargeableResourceEnum.RegularEnergy, amountToRequest - 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(resourceCount)
                .StartingResources(resourceCount)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

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
            SuperMetroidModel model = ReusableModel();
            int initialAmount = 2;
            int addedAmount = 5;
            int expectedamount = 7;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues<RechargeableResourceEnum>())
            {
                startResources.ApplyAmount(loopResource, initialAmount);
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyAddResource(resource, addedAmount);

            // Expect
            Assert.Equal(expectedamount, inGameState.Resources.GetAmount(resource));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues<RechargeableResourceEnum>())
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
            SuperMetroidModel model = ReusableModel();
            int initialAmount = 2;
            int addedAmount = 150;
            int expectedamount = 100;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues<RechargeableResourceEnum>())
            {
                startResources.ApplyAmount(loopResource, initialAmount);
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyAddResource(resource, addedAmount);

            // Expect
            Assert.Equal(expectedamount, inGameState.Resources.GetAmount(resource));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues<RechargeableResourceEnum>())
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
            SuperMetroidModel model = ReusableModel();
            int initialAmount = 5;
            int removedAmount = 2;
            int expectedAmount = 3;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues<RechargeableResourceEnum>())
            {
                startResources.ApplyAmount(loopResource, initialAmount);
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyConsumeResource(resource.ToConsumableResource(), removedAmount);

            // Expect
            Assert.Equal(expectedAmount, inGameState.Resources.GetAmount(resource));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues<RechargeableResourceEnum>())
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
            SuperMetroidModel model = ReusableModel();
            int initialAmount = 5;
            int removedAmount = 2;
            int expectedAmount = 3;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues<RechargeableResourceEnum>())
            {
                // Don't put any reserve energy in, we're checking regular energy
                if (loopResource != RechargeableResourceEnum.ReserveEnergy)
                {
                    startResources.ApplyAmount(loopResource, initialAmount);
                }
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyConsumeResource(ConsumableResourceEnum.Energy, removedAmount);

            // Expect
            Assert.Equal(expectedAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues<RechargeableResourceEnum>())
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
            SuperMetroidModel model = ReusableModel();
            int initialAmount = 5;
            int removedAmount = 2;
            int expectedAmount = 3;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues<RechargeableResourceEnum>())
            {
                // Don't put any regular energy in, we're checking reserve energy
                if (loopResource != RechargeableResourceEnum.RegularEnergy)
                {
                    startResources.ApplyAmount(loopResource, initialAmount);
                }
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyConsumeResource(ConsumableResourceEnum.Energy, removedAmount);

            // Expect
            Assert.Equal(expectedAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.ReserveEnergy));
            foreach (RechargeableResourceEnum otherResource in Enum.GetValues<RechargeableResourceEnum>())
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
            SuperMetroidModel model = ReusableModel();
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
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

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
            SuperMetroidModel model = ReusableModel();
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
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

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
            SuperMetroidModel model = ReusableModel();
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
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

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
            SuperMetroidModel model = ReusableModel();
            int initialAmount = 5;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount()
                .ApplyAmount(resource, initialAmount);
            ResourceCount maxResources = startResources.Clone()
                .ApplyAmount(resource, maxAmount);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

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
            SuperMetroidModel model = ReusableModel();
            int initialAmount = 5;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount()
                .ApplyAmount(resource, initialAmount);
            ResourceCount maxResources = startResources.Clone()
                .ApplyAmount(resource, maxAmount);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyRefillResource(resource.ToConsumableResource());

            // Expect
            Assert.Equal(maxAmount, inGameState.Resources.GetAmount(resource));
        }

        [Fact]
        public void ApplyRefillResource_ConsumableEnergy_SetsBothTypesToMax()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            int initialAmount = 5;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, initialAmount)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, initialAmount);
            ResourceCount maxResources = startResources.Clone()
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, maxAmount)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, maxAmount);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyRefillResource(ConsumableResourceEnum.Energy);

            // Expect
            Assert.Equal(maxAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.RegularEnergy));
            Assert.Equal(maxAmount, inGameState.Resources.GetAmount(RechargeableResourceEnum.ReserveEnergy));
        }
        #endregion

        #region Tests for ApplyRefillResources()

        [Fact]
        public void ApplyRefillResources_RefillsAllResources()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            int initialAmount = 5;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach(RechargeableResourceEnum resource in Enum.GetValues<RechargeableResourceEnum>())
            {
                startResources.ApplyAmount(resource, initialAmount);
                maxResources.ApplyAmount(resource, maxAmount);
            }

            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyRefillResources();

            // Expect
            foreach (RechargeableResourceEnum resource in Enum.GetValues<RechargeableResourceEnum>())
            { 
                Assert.Equal(maxAmount, inGameState.Resources.GetAmount(resource));
            }
        }

        #endregion

        #region Tests for GetResourceVariationWith()
        [Fact]
        public void GetResourceVariationWith_ReturnsPositiveAndNegativeAnd0()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ResourceCount resourceMaximums = new ResourceCount();
            foreach (RechargeableResourceEnum resource in Enum.GetValues<RechargeableResourceEnum>())
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

            StartConditions startConditions1 = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(resourceMaximums)
                .StartingResources(resources1)
                .Build();

            InGameState inGameState1 = new InGameState(startConditions1);

            StartConditions startConditions2 = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(resourceMaximums)
                .StartingResources(resources2)
                .Build();

            InGameState inGameState2 = new InGameState(startConditions2);

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
            SuperMetroidModel model = ReusableModel();
            ResourceCount resourceMaximums = new ResourceCount();
            foreach (RechargeableResourceEnum resource in Enum.GetValues<RechargeableResourceEnum>())
            {
                resourceMaximums.ApplyAmount(resource, 100);
            }
            ResourceCount resources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.Super, 100)
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 100);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(resourceMaximums)
                .StartingResources(resources)
                .Build();

            InGameState inGameState = new InGameState(startConditions);

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
            SuperMetroidModel model = ReusableModel();
            ResourceCount resourceMaximums = new ResourceCount();
            foreach (RechargeableResourceEnum resource in Enum.GetValues<RechargeableResourceEnum>())
            {
                resourceMaximums.ApplyAmount(resource, 100);
            }
            ResourceCount resources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.Super, 100)
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 100)
                .ApplyAmount(RechargeableResourceEnum.ReserveEnergy, 100);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(resourceMaximums)
                .StartingResources(resources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

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
            SuperMetroidModel model = ReusableModel();
            ResourceCount resourceMaximums = new ResourceCount();
            foreach (RechargeableResourceEnum resource in Enum.GetValues<RechargeableResourceEnum>())
            {
                resourceMaximums.ApplyAmount(resource, 100);
            }
            ResourceCount resources = new ResourceCount()
                .ApplyAmount(RechargeableResourceEnum.Super, 100)
                .ApplyAmount(RechargeableResourceEnum.RegularEnergy, 100);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(resourceMaximums)
                .StartingResources(resources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

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
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When
            ISet<EnemyDropEnum> result = inGameState.GetUnneededDrops();

            // Expect
            ISet<EnemyDropEnum> expected = Enum.GetValues<EnemyDropEnum>().Except(new EnemyDropEnum[] { EnemyDropEnum.NoDrop }).ToHashSet();
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
            SuperMetroidModel model = ReusableModel();
            int initialAmount = 5;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues<RechargeableResourceEnum>())
            {
                startResources.ApplyAmount(loopResource, initialAmount);
                maxResources.ApplyAmount(loopResource, maxAmount);
            }
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            ISet<EnemyDropEnum> result = inGameState.GetUnneededDrops();

            // Expect
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(RechargeableResourceEnum.RegularEnergy)]
        [InlineData(RechargeableResourceEnum.ReserveEnergy)]
        public void GetUnneededDrops_OnlyOneTypeOfEnergyNotFull_DoesNotReturnEnergyDrops(RechargeableResourceEnum energyResource)
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            int initialAmount = 5;
            int maxAmount = 100;
            ResourceCount startResources = new ResourceCount();
            ResourceCount maxResources = startResources.Clone();
            foreach (RechargeableResourceEnum loopResource in Enum.GetValues<RechargeableResourceEnum>())
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
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(maxResources)
                .StartingResources(startResources)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            ISet<EnemyDropEnum> result = inGameState.GetUnneededDrops();

            // Expect
            // Although one type of energy is full, the other isn't, so energy drops are needed and should NOT be returned
            Assert.DoesNotContain(EnemyDropEnum.SmallEnergy, result);
            Assert.DoesNotContain(EnemyDropEnum.BigEnergy, result);
        }
        #endregion

        #region Tests for ApplyAddGameFlag(GameFlag)
        [Fact]
        public void ApplyAddGameFlag_AddsIt()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            GameFlag flag1 = model.GameFlags["f_ZebesAwake"];
            GameFlag flag2 = model.GameFlags["f_DefeatedBombTorizo"];
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When
            inGameState
                .ApplyAddGameFlag(flag1)
                .ApplyAddGameFlag(flag2);

            // Expect
            Assert.Equal(2, inGameState.ActiveGameFlags.Count);
            Assert.Contains(flag1, inGameState.ActiveGameFlags.Values, ObjectReferenceEqualityComparer<GameFlag>.Default);
            Assert.Contains(flag2, inGameState.ActiveGameFlags.Values, ObjectReferenceEqualityComparer<GameFlag>.Default);
        }

        [Fact]
        public void ApplyAddGameFlag_FromWrongModel_ThrowsModelElementMismatchException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            SuperMetroidModel otherModel = OtherReusableModel();
            GameFlag flag1 = otherModel.GameFlags["f_ZebesAwake"];
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When and expect
            Assert.Throws<ModelElementMismatchException>(() => inGameState.ApplyAddGameFlag(flag1));
        }

        #endregion

        #region Tests for ApplyAddGameFlag(string)
        [Fact]
        public void ApplyAddGameFlagByName_AddsIt()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            GameFlag flag1 = model.GameFlags["f_ZebesAwake"];
            GameFlag flag2 = model.GameFlags["f_DefeatedBombTorizo"];
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When
            inGameState
                .ApplyAddGameFlag("f_ZebesAwake")
                .ApplyAddGameFlag("f_DefeatedBombTorizo");

            // Expect
            Assert.Equal(2, inGameState.ActiveGameFlags.Count);
            Assert.Contains(flag1, inGameState.ActiveGameFlags.Values, ObjectReferenceEqualityComparer<GameFlag>.Default);
            Assert.Contains(flag2, inGameState.ActiveGameFlags.Values, ObjectReferenceEqualityComparer<GameFlag>.Default);
        }
        #endregion

        #region Tests for GetActiveGameFlagsExceptIn()
        [Fact]
        public void GetActiveGameFlagsExceptIn_ReturnsDifference()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            GameFlag flagIn1 = model.GameFlags["f_ZebesAwake"];
            GameFlag flagIn2 = model.GameFlags["f_DefeatedBombTorizo"];
            GameFlag flagInBoth = model.GameFlags["f_DefeatedCeresRidley"];
            InGameState inGameState1 = new InGameState(StartConditions.CreateVanillaStartConditions(model))
                .ApplyAddGameFlag(flagIn1)
                .ApplyAddGameFlag(flagInBoth);
            InGameState inGameState2 = new InGameState(StartConditions.CreateVanillaStartConditions(model))
                .ApplyAddGameFlag(flagIn2)
                .ApplyAddGameFlag(flagInBoth);

            // When
            Dictionary<string, GameFlag> result = inGameState1.GetActiveGameFlagsExceptIn(inGameState2);

            // Expect
            Assert.Single(result);
            Assert.Same(flagIn1, result[flagIn1.Name]);
        }
        #endregion

        #region Tests for ApplyOpenLock(NodeLock)

        [Fact]
        public void ApplyOpenLock_AddsIt()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NodeLock lock1 = model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            NodeLock lock2 = model.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"];
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When
            inGameState
                .ApplyOpenLock(lock1, applyToRoomState: false)
                .ApplyOpenLock(lock2, applyToRoomState: false);

            // Expect
            Assert.Equal(2, inGameState.OpenedLocks.Count);
            Assert.Contains(lock1, inGameState.OpenedLocks.Values, ObjectReferenceEqualityComparer<NodeLock>.Default);
            Assert.Contains(lock2, inGameState.OpenedLocks.Values, ObjectReferenceEqualityComparer<NodeLock>.Default);
            Assert.Empty(inGameState.InRoomState.CurrentNodeState.OpenedLocks);
        }

        [Fact]
        public void ApplyOpenLock_FromWrongModel_ThrowsModelElementMismatchException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            SuperMetroidModel otherModel = OtherReusableModel();
            NodeLock lock1 = otherModel.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When and expect
            Assert.Throws<ModelElementMismatchException>(() => inGameState.ApplyOpenLock(lock1, applyToRoomState: false));
        }

        [Fact]
        public void ApplyOpenLock_ApplyingToRoomStateWhileNotOnNode_ThrowsArgumentException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NodeLock nodeLock = model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyOpenLock(nodeLock, applyToRoomState: true));
        }

        [Fact]
        public void ApplyOpenLock_ApplyingToRoomStateWhileOnNode_SucceedsAndAltersNodeState()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NodeLock nodeLock = model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 3)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyOpenLock(nodeLock, applyToRoomState: true);

            // Expect
            Assert.Single(inGameState.OpenedLocks);
            Assert.Contains(nodeLock, inGameState.OpenedLocks.Values, ObjectReferenceEqualityComparer<NodeLock>.Default);
            Assert.Single(inGameState.InRoomState.CurrentNodeState.OpenedLocks);
            Assert.Contains(nodeLock, inGameState.InRoomState.CurrentNodeState.OpenedLocks, ObjectReferenceEqualityComparer<NodeLock>.Default);
        }

        #endregion

        #region Tests for ApplyOpenLock(string)

        [Fact]
        public void ApplyOpenLockByName_AddsIt()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NodeLock lock1 = model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            NodeLock lock2 = model.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"];
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When
            inGameState
                .ApplyOpenLock(lock1.Name, applyToRoomState: false)
                .ApplyOpenLock(lock2.Name, applyToRoomState: false);

            // Expect
            Assert.Equal(2, inGameState.OpenedLocks.Count);
            Assert.Contains(lock1, inGameState.OpenedLocks.Values, ObjectReferenceEqualityComparer<NodeLock>.Default);
            Assert.Contains(lock2, inGameState.OpenedLocks.Values, ObjectReferenceEqualityComparer<NodeLock>.Default);
            Assert.Empty(inGameState.InRoomState.CurrentNodeState.OpenedLocks);
        }

        [Fact]
        public void ApplyOpenLockByName_ApplyingToRoomStateWhileNotOnNode_ThrowsArgumentException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NodeLock nodeLock = model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyOpenLock(nodeLock.Name, applyToRoomState: true));
        }

        [Fact]
        public void ApplyOpenLockByName_ApplyingToRoomStateWhileOnNode_SucceedsAndAltersNodeState()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NodeLock nodeLock = model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 3)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyOpenLock(nodeLock.Name, applyToRoomState: true);

            // Expect
            Assert.Single(inGameState.OpenedLocks);
            Assert.Contains(nodeLock, inGameState.OpenedLocks.Values, ObjectReferenceEqualityComparer<NodeLock>.Default);
            Assert.Single(inGameState.InRoomState.CurrentNodeState.OpenedLocks);
            Assert.Contains(nodeLock, inGameState.InRoomState.CurrentNodeState.OpenedLocks, ObjectReferenceEqualityComparer<NodeLock>.Default);
        }

        #endregion

        #region Tests for GetOpenedNodeLocksExceptIn()
        [Fact]
        public void GetOpenedNodeLocksExceptIn_ReturnsDifference()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NodeLock lockIn1 = model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            NodeLock lockIn2 = model.Locks["Landing Site Bottom Right Green Lock (to Crateria Tube)"];
            NodeLock lockInBoth = model.Locks["Parlor Bottom Right Red Lock (to Pre-Map)"];
            InGameState inGameState1 = new InGameState(StartConditions.CreateVanillaStartConditions(model))
                .ApplyOpenLock(lockIn1, applyToRoomState: false)
                .ApplyOpenLock(lockInBoth, applyToRoomState: false);
            InGameState inGameState2 = new InGameState(StartConditions.CreateVanillaStartConditions(model))
                .ApplyOpenLock(lockIn2, applyToRoomState: false)
                .ApplyOpenLock(lockInBoth, applyToRoomState: false);

            // When
            Dictionary<string, NodeLock> result = inGameState1.GetOpenedNodeLocksExceptIn(inGameState2);

            // Expect
            Assert.Single(result);
            Assert.Same(lockIn1, result[lockIn1.Name]);
        }
        #endregion

        #region Tests for ApplyBypassLock(NodeLock)

        [Fact]
        public void ApplyBypassLock_AltersInNodeState()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NodeLock nodeLock = model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 3)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyBypassLock(nodeLock);

            // Expect
            Assert.Single(inGameState.InRoomState.CurrentNodeState.BypassedLocks);
            Assert.Contains(nodeLock, inGameState.InRoomState.CurrentNodeState.BypassedLocks, ObjectReferenceEqualityComparer<NodeLock>.Default);
        }

        [Fact]
        public void ApplyBypassLock_FromWrongModel_ThrowsModelElementMismatchException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            SuperMetroidModel otherModel = OtherReusableModel();
            NodeLock nodeLock = otherModel.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 3)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            Assert.Throws<ModelElementMismatchException>(() => inGameState.ApplyBypassLock(nodeLock));
        }

        [Fact]
        public void ApplyBypassLock_NotOnNode_ThrowsArgumentException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NodeLock nodeLock = model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyBypassLock(nodeLock));
        }

        #endregion

        #region Tests for ApplyBypassLock(string)

        [Fact]
        public void ApplyBypassLockByName_AltersInNodeState()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NodeLock nodeLock = model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 3)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyBypassLock(nodeLock.Name);

            // Expect
            Assert.Single(inGameState.InRoomState.CurrentNodeState.BypassedLocks);
            Assert.Contains(nodeLock, inGameState.InRoomState.CurrentNodeState.BypassedLocks, ObjectReferenceEqualityComparer<NodeLock>.Default);
        }

        [Fact]
        public void ApplyBypassLockByName_NotOnNode_ThrowsArgumentException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NodeLock nodeLock = model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyBypassLock(nodeLock.Name));
        }

        #endregion

        #region Tests for GetBypassedExitLocks()
        [Fact]
        public void GetBypassedExitLocks_CurrentRoom_ReturnsBypassedLocks()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NodeLock nodeLock = model.Locks["Landing Site Top Right Yellow Lock (to Power Bombs)"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 3)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyBypassLock(nodeLock);

            // When
            IEnumerable<NodeLock> result = inGameState.GetBypassedExitLocks();

            // Expect
            Assert.Single(result);
            Assert.Contains(nodeLock, result, ObjectReferenceEqualityComparer<NodeLock>.Default);
        }

        [Fact]
        public void GetBypassedExitLocks_PreviousRoom_ReturnsBypassedLocksOnlyFromCorrectRoom()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            NodeLock previousRoomLock = model.Locks["Red Brinstar Elevator Yellow Lock (to Kihunters)"];
            NodeLock currentRoomLock = model.Locks["Crateria Kihunter Room Bottom Yellow Lock (to Elevator)"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Red Brinstar Elevator Room", 1)
                .Build();

            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyBypassLock(previousRoomLock);
            inGameState.ApplyEnterRoom("Crateria Kihunter Room", 3);
            inGameState.ApplyBypassLock(currentRoomLock);

            // When
            IEnumerable<NodeLock> result = inGameState.GetBypassedExitLocks(1);

            // Expect
            Assert.Single(result);
            Assert.Contains(previousRoomLock, result, ObjectReferenceEqualityComparer<NodeLock>.Default);
        }
        #endregion

        #region Tests for ApplyTakeLocation(RoomNode)

        [Fact]
        public void ApplyTakeLocation_AddsIt()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode node1 = model.GetNodeInRoom("Varia Suit Room", 2);
            RoomNode node2 = model.GetNodeInRoom("Spazer Room", 2);
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When
            inGameState
                .ApplyTakeLocation(node1)
                .ApplyTakeLocation(node2);

            // Expect
            Assert.Equal(2, inGameState.TakenItemLocations.Count);
            Assert.Contains(node1, inGameState.TakenItemLocations.Values, ObjectReferenceEqualityComparer<RoomNode>.Default);
            Assert.Contains(node2, inGameState.TakenItemLocations.Values, ObjectReferenceEqualityComparer<RoomNode>.Default);
        }

        [Fact]
        public void ApplyTakeLocation_FromWrongModel_ThrowsModelElementMismatchException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            SuperMetroidModel otherModel = OtherReusableModel();
            RoomNode node1 = otherModel.GetNodeInRoom("Varia Suit Room", 2);
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When and expect
            Assert.Throws<ModelElementMismatchException>(() => inGameState.ApplyTakeLocation(node1));
        }

        #endregion

        #region Tests for ApplyTakeLocation(string, int)

        [Fact]
        public void ApplyTakeLocationByRoomNameAndNodeId_AddsIt()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode node1 = model.GetNodeInRoom("Varia Suit Room", 2);
            RoomNode node2 = model.GetNodeInRoom("Spazer Room", 2);
            InGameState inGameState = new InGameState(StartConditions.CreateVanillaStartConditions(model));

            // When
            inGameState
                .ApplyTakeLocation("Varia Suit Room", 2)
                .ApplyTakeLocation("Spazer Room", 2);

            // Expect
            Assert.Equal(2, inGameState.TakenItemLocations.Count);
            Assert.Contains(node1, inGameState.TakenItemLocations.Values, ObjectReferenceEqualityComparer<RoomNode>.Default);
            Assert.Contains(node2, inGameState.TakenItemLocations.Values, ObjectReferenceEqualityComparer<RoomNode>.Default);
        }

        #endregion

        #region Tests for GetTakenItemLocationsExceptIn()
        [Fact]
        public void GetTakenItemLocationsExceptIn_ReturnsDifference()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode nodeIn1 = model.GetNodeInRoom("Varia Suit Room", 2);
            RoomNode nodeIn2 = model.GetNodeInRoom("Spazer Room", 2);
            RoomNode nodeInBoth = model.GetNodeInRoom("Blue Brinstar Energy Tank Room", 3);

            InGameState inGameState1 = new InGameState(StartConditions.CreateVanillaStartConditions(model))
                .ApplyTakeLocation(nodeIn1)
                .ApplyTakeLocation(nodeInBoth);
            InGameState inGameState2 = new InGameState(StartConditions.CreateVanillaStartConditions(model))
                .ApplyTakeLocation(nodeIn2)
                .ApplyTakeLocation(nodeInBoth);

            // When
            Dictionary<string, RoomNode> result = inGameState1.GetTakenItemLocationsExceptIn(inGameState2);

            // Expect
            Assert.Single(result);
            Assert.Same(nodeIn1, result[nodeIn1.Name]);
        }
        #endregion

        #region Tests for ApplyAddItem(Item)

        [Fact]
        public void ApplyAddItem_NonConsumableItem_AddsIt()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Item item1 = model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item item2 = model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()))
                .Build();
            InGameState inGameState = new InGameState(startConditions);

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
        public void ApplyAddItem_FromWrongModel_ThrowsModelElementMismatchException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            SuperMetroidModel otherModel = OtherReusableModel();
            Item item1 = otherModel.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()))
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            Assert.Throws<ModelElementMismatchException>(() => inGameState.ApplyAddItem(item1));
        }

        [Fact]
        public void ApplyAddItem_ExpansionItem_AddsItAndIncreasesCount()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Item item1 = model.Items[SuperMetroidModel.MISSILE_NAME];
            Item item2 = model.Items[SuperMetroidModel.SUPER_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()))
                .Build();
            InGameState inGameState = new InGameState(startConditions);

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

        #region Tests for ApplyAddItem(string)

        [Fact]
        public void ApplyAddItemByName_NonConsumableItem_AddsIt()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Item item1 = model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item item2 = model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()))
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState
                .ApplyAddItem(item1.Name)
                .ApplyAddItem(item2.Name);

            // Expect
            Assert.Equal(2, inGameState.Inventory.NonConsumableItems.Count);
            Assert.Same(item1, inGameState.Inventory.NonConsumableItems[item1.Name]);
            Assert.Same(item2, inGameState.Inventory.NonConsumableItems[item2.Name]);
        }

        [Fact]
        public void ApplyAddItemByName_ExpansionItem_AddsItAndIncreasesCount()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Item item1 = model.Items[SuperMetroidModel.MISSILE_NAME];
            Item item2 = model.Items[SuperMetroidModel.SUPER_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()))
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState
                .ApplyAddItem(item1.Name)
                .ApplyAddItem(item1.Name)
                .ApplyAddItem(item2.Name);

            // Expect
            Assert.Equal(2, inGameState.Inventory.ExpansionItems.Count);
            Assert.Same(item1, inGameState.Inventory.ExpansionItems[item1.Name].item);
            Assert.Equal(2, inGameState.Inventory.ExpansionItems[item1.Name].count);
            Assert.Same(item2, inGameState.Inventory.ExpansionItems[item2.Name].item);
            Assert.Equal(1, inGameState.Inventory.ExpansionItems[item2.Name].count);
        }

        #endregion

        #region Tests for ApplyDisableItem(Item)

        [Fact]
        public void ApplyDisableItem_NonConsumableItem_DisablesIt()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Item item1 = model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item item2 = model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME];
            Item notPresentItem = model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()))
                .Build();
            InGameState inGameState = new InGameState(startConditions);
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
        public void ApplyDisableItem_FromWrongModel_ThrowsModelElementMismatchException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            SuperMetroidModel otherModel = OtherReusableModel();
            Item item1 = model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item wrongItem1 = otherModel.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item notPresentItem = model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()))
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState
                .ApplyAddItem(item1);

            // When and expect
            Assert.Throws<ModelElementMismatchException>(() => inGameState.ApplyDisableItem(wrongItem1));
        }

        [Fact]
        public void ApplyDisableItem_ExpansionItem_DoesNothing()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Item item1 = model.Items[SuperMetroidModel.MISSILE_NAME];
            Item item2 = model.Items[SuperMetroidModel.SUPER_NAME];
            Item notPresentItem = model.Items[SuperMetroidModel.POWER_BOMB_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()))
                .Build();
            InGameState inGameState = new InGameState(startConditions);
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

        #region Tests for ApplyDisableItem(string)

        [Fact]
        public void ApplyDisableItemByName_NonConsumableItem_DisablesIt()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Item item1 = model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item item2 = model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME];
            Item notPresentItem = model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()))
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState
                .ApplyAddItem(item1)
                .ApplyAddItem(item2);

            // When
            inGameState
                .ApplyDisableItem(item1.Name)
                .ApplyDisableItem(notPresentItem.Name);

            // Expect
            Assert.True(inGameState.Inventory.IsItemDisabled(item1));
            Assert.False(inGameState.Inventory.IsItemDisabled(item2));
            Assert.False(inGameState.Inventory.IsItemDisabled(notPresentItem));
            Assert.False(inGameState.Inventory.HasItem(item1));
            Assert.True(inGameState.Inventory.HasItem(item2));
        }

        [Fact]
        public void ApplyDisableItemByName_ExpansionItem_DoesNothing()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Item item1 = model.Items[SuperMetroidModel.MISSILE_NAME];
            Item item2 = model.Items[SuperMetroidModel.SUPER_NAME];
            Item notPresentItem = model.Items[SuperMetroidModel.POWER_BOMB_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()))
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState
                .ApplyAddItem(item1.Name)
                .ApplyAddItem(item2.Name);

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

        #region Tests for ApplyEnableItem(Item)

        [Fact]
        public void ApplyEnableItem_NonConsumableItem_EnablesIt()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Item item = model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()))
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyAddItem(item);
            inGameState.ApplyDisableItem(item);

            // When
            inGameState.ApplyEnableItem(item);

            // Expect
            Assert.False(inGameState.Inventory.IsItemDisabled(item));
            Assert.True(inGameState.Inventory.HasItem(item));
        }

        [Fact]
        public void ApplyEnableItem_FromWrongModel_ThrowsModelElementMismatchException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            SuperMetroidModel otherModel = OtherReusableModel();
            Item item = model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item wrongItem = otherModel.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()))
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyAddItem(item);
            inGameState.ApplyDisableItem(item);

            // When and expect
            Assert.Throws<ModelElementMismatchException>(() => inGameState.ApplyEnableItem(wrongItem));
        }

        #endregion

        #region Tests for ApplyEnableItem(string)

        [Fact]
        public void ApplyEnableItemByName_NonConsumableItem_EnablesIt()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Item item = model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingInventory(new ItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums()))
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyAddItem(item);
            inGameState.ApplyDisableItem(item);

            // When
            inGameState.ApplyEnableItem(item.Name);

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
            SuperMetroidModel model = ReusableModel();
            Item nonConsumableItemIn1 = model.Items[SuperMetroidModel.VARIA_SUIT_NAME];
            Item nonConsumableItemIn2 = model.Items[SuperMetroidModel.GRAVITY_SUIT_NAME];
            Item nonConsumableItemInBoth = model.Items[SuperMetroidModel.SPEED_BOOSTER_NAME];

            Item expansionItemIn1 = model.Items[SuperMetroidModel.MISSILE_NAME];
            Item expansionItemIn2 = model.Items[SuperMetroidModel.SUPER_NAME];
            Item expansionItemInBoth = model.Items[SuperMetroidModel.POWER_BOMB_NAME];
            Item expansionItemMoreIn1 = model.Items[SuperMetroidModel.ENERGY_TANK_NAME];
            Item expansionItemMoreIn2 = model.Items[SuperMetroidModel.RESERVE_TANK_NAME];

            InGameState inGameState1 = new InGameState(StartConditions.CreateVanillaStartConditions(model))
                .ApplyAddItem(nonConsumableItemIn1)
                .ApplyAddItem(nonConsumableItemInBoth)
                .ApplyAddItem(expansionItemIn1)
                .ApplyAddItem(expansionItemInBoth)
                .ApplyAddItem(expansionItemMoreIn1)
                .ApplyAddItem(expansionItemMoreIn1)
                .ApplyAddItem(expansionItemMoreIn1)
                .ApplyAddItem(expansionItemMoreIn2);
            InGameState inGameState2 = new InGameState(StartConditions.CreateVanillaStartConditions(model))
                .ApplyAddItem(nonConsumableItemIn2)
                .ApplyAddItem(nonConsumableItemInBoth)
                .ApplyAddItem(expansionItemIn2)
                .ApplyAddItem(expansionItemInBoth)
                .ApplyAddItem(expansionItemMoreIn1)
                .ApplyAddItem(expansionItemMoreIn2)
                .ApplyAddItem(expansionItemMoreIn2)
                .ApplyAddItem(expansionItemMoreIn2);

            // When
            ItemInventory result = inGameState1.GetInventoryExceptIn(inGameState2);

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

        #region Tests for ApplyDestroyObstacle(RoomObstacle)

        [Fact]
        public void ApplyDestroyObstacle_AddsItToDestroyedObstacles()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 5)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyDestroyObstacle(inGameState.CurrentRoom.Obstacles["A"]);

            // Expect
            Assert.Single(inGameState.InRoomState.DestroyedObstacleIds);
            Assert.Contains("A", inGameState.InRoomState.DestroyedObstacleIds);
        }

        [Fact]
        public void ApplyDestroyObstacle_FromWrongModel_ThrowsModelElementMismatchException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            SuperMetroidModel otherModel = OtherReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 5)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            Assert.Throws<ModelElementMismatchException>(() => inGameState.ApplyDestroyObstacle(otherModel.Rooms["Landing Site"].Obstacles["A"]));
        }

        [Fact]
        public void ApplyDestroyObstacle_ObstacleNotInRoom_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Parlor and Alcatraz", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyDestroyObstacle(model.Rooms["Landing Site"].Obstacles["A"]));
        }

        #endregion

        #region Tests for ApplyDestroyObstacle(string)

        [Fact]
        public void ApplyDestroyObstacleById_AddsItToDestroyedObstacles()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 5)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyDestroyObstacle("A");

            // Expect
            Assert.Single(inGameState.InRoomState.DestroyedObstacleIds);
            Assert.Contains("A", inGameState.InRoomState.DestroyedObstacleIds);
        }

        [Fact]
        public void ApplyDestroyObstacleById_ObstacleNotInRoom_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Parlor and Alcatraz", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyDestroyObstacle("A"));
        }

        #endregion

        #region Tests for ApplyEnterRoom(RoomNode)

        [Fact]
        public void ApplyEnterRoom_ChangesCurrentNode()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Red Tower", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyEnterRoom(expectedNode);

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }

        [Fact]
        public void ApplyEnterRoom_FromWrongModel_ThrowsModelElementMismatchException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            SuperMetroidModel otherModel = OtherReusableModel();
            RoomNode node = otherModel.GetNodeInRoom("Red Tower", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            Assert.Throws<ModelElementMismatchException>(() => inGameState.ApplyEnterRoom(node));
        }

        [Fact]
        public void ApplyEnterRoom_ClearsPreviousRoomState()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 5)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyDestroyObstacle(inGameState.CurrentRoom.Obstacles["A"]);
            inGameState.ApplyVisitNode(2, "Base");

            // When
            inGameState.ApplyEnterRoom(model.GetNodeInRoom("Parlor and Alcatraz", 4));

            // Expect
            Assert.Empty(inGameState.InRoomState.DestroyedObstacleIds);
            Assert.Single(inGameState.InRoomState.VisitedRoomPath);
        }

        [Fact]
        public void ApplyEnterRoom_AddsCurrentRoomStateCopyToRememberedRooms()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Room initialRoom = model.Rooms["Sloaters Refill"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(initialRoom.Name, 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyEnterRoom(model.GetNodeInRoom("Red Tower", 3));

            // Expect
            Assert.Same(initialRoom, inGameState.PreviousRoomStates[0].CurrentRoom);
            Assert.NotSame(inGameState.InRoomState, inGameState.PreviousRoomStates[0]);
        }

        [Fact]
        public void ApplyEnterRoom_SpawnsAtDifferentNode_GoesToCorrectNode()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Ice Beam Gate Room", 6);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Ice Beam Tutorial Room", 2)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyEnterRoom(model.GetNodeInRoom("Ice Beam Gate Room", 1));

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }

        [Fact]
        public void ApplyEnterRoom_GoingBeyondRememberedRooms_EliminatesOldestRoom()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            string initialRoomName = "Sloaters Refill";
            RoomNode node1 = model.GetNodeInRoom("Red Tower", 4);
            RoomNode node2 = model.GetNodeInRoom("Bat Room", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(initialRoomName, 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(model.GetNodeInRoom("Red Tower", 3));
            inGameState.ApplyVisitNode(8, "Base");
            inGameState.ApplyVisitNode(node1, inGameState.CurrentNode.LinksTo[4].Strats["Base"]);

            // When
            for (int i = 0; i < InGameState.MaxPreviousRooms; i++)
            {
                RoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // Expect
            Assert.NotEqual(initialRoomName, inGameState.PreviousRoomStates.Last().CurrentRoom.Name);
        }

        #endregion

        #region Tests for ApplyEnterRoom(string, int)

        [Fact]
        public void ApplyEnterRoomByRoomNameAndId_ChangesCurrentNode()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Red Tower", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }

        [Fact]
        public void ApplyEnterRoomByRoomNameAndId_ClearsPreviousRoomState()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 5)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyDestroyObstacle(inGameState.CurrentRoom.Obstacles["A"]);
            inGameState.ApplyVisitNode(2, "Base");

            // When
            inGameState.ApplyEnterRoom("Parlor and Alcatraz", 4);

            // Expect
            Assert.Empty(inGameState.InRoomState.DestroyedObstacleIds);
            Assert.Single(inGameState.InRoomState.VisitedRoomPath);
        }

        [Fact]
        public void ApplyEnterRoomByRoomNameAndId_AddsCurrentRoomStateCopyToRememberedRooms()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Room initialRoom = model.Rooms["Sloaters Refill"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(initialRoom.Name, 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // Expect
            Assert.Same(initialRoom, inGameState.PreviousRoomStates[0].CurrentRoom);
            Assert.NotSame(inGameState.InRoomState, inGameState.PreviousRoomStates[0]);
        }

        [Fact]
        public void ApplyEnterRoomByRoomNameAndId_SpawnsAtDifferentNode_GoesToCorrectNode()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Ice Beam Gate Room", 6);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Ice Beam Tutorial Room", 2)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyEnterRoom("Ice Beam Gate Room", 1);

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }

        [Fact]
        public void ApplyEnterRoomByRoomNameAndId_GoingBeyondRememberedRooms_EliminatesOldestRoom()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            string initialRoomName = "Sloaters Refill";
            RoomNode node1 = model.GetNodeInRoom("Red Tower", 4);
            RoomNode node2 = model.GetNodeInRoom("Bat Room", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(initialRoomName, 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(model.GetNodeInRoom("Red Tower", 3));
            inGameState.ApplyVisitNode(8, "Base");
            inGameState.ApplyVisitNode(node1, inGameState.CurrentNode.LinksTo[4].Strats["Base"]);

            // When
            for (int i = 0; i < InGameState.MaxPreviousRooms; i++)
            {
                RoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node.Room.Name, node.Id);
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
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Red Tower", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyExitRoom();

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }

        [Fact]
        public void ApplyExitRoom_ClearsPreviousRoomState()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 5)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyDestroyObstacle(inGameState.CurrentRoom.Obstacles["A"]);
            inGameState.ApplyVisitNode(2, "Base");

            // When
            inGameState.ApplyExitRoom();

            // Expect
            Assert.Empty(inGameState.InRoomState.DestroyedObstacleIds);
            Assert.Single(inGameState.InRoomState.VisitedRoomPath);
        }

        [Fact]
        public void ApplyExitRoom_AddsCurrentRoomStateCopyToRememberedRooms()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Room initialRoom = model.Rooms["Sloaters Refill"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(initialRoom.Name, 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyExitRoom();

            // Expect
            Assert.Same(initialRoom, inGameState.PreviousRoomStates[0].CurrentRoom);
            Assert.NotSame(inGameState.InRoomState, inGameState.PreviousRoomStates[0]);
        }

        [Fact]
        public void ApplyExitRoom_SpawnsAtDifferentNode_GoesToCorrectNode()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Ice Beam Gate Room", 6);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Ice Beam Tutorial Room", 2)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyExitRoom();

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }

        [Fact]
        public void ApplyExitRoom_GoingBeyondRememberedRooms_EliminatesOldestRoom()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            string initialRoomName = "Sloaters Refill";
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(initialRoomName, 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);
            inGameState.ApplyVisitNode(8, "Base");
            inGameState.ApplyVisitNode(4, "Base");

            // When
            for (int i = 0; i < InGameState.MaxPreviousRooms; i++)
            {
                inGameState.ApplyExitRoom();
            }

            // Expect
            Assert.NotEqual(initialRoomName, inGameState.PreviousRoomStates.Last().CurrentRoom.Name);
        }

        [Fact]
        public void ApplyExitRoom_NonDoorNode_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 7)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            Assert.Throws<InvalidOperationException>(() => inGameState.ApplyExitRoom());
        }

        [Fact]
        public void ApplyExitRoom_OneWayEntranceDoorNode_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("West Sand Hall", 3)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            Assert.Throws<InvalidOperationException>(() => inGameState.ApplyExitRoom());
        }

        [Fact]
        public void ApplyExitRoom_LockedDoorNode_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            Assert.Throws<InvalidOperationException>(() => inGameState.ApplyExitRoom());
        }

        [Fact]
        public void ApplyExitRoom_UnlockedDoorNode_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Crateria Tube", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyOpenLock("Landing Site Bottom Right Green Lock (to Crateria Tube)");

            // When
            inGameState.ApplyExitRoom();

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }

        [Fact]
        public void ApplyExitRoom_BypassedLock_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Crateria Tube", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyBypassLock("Landing Site Bottom Right Green Lock (to Crateria Tube)");

            // When
            inGameState.ApplyExitRoom();

            // Expect
            Assert.Same(expectedNode, inGameState.CurrentNode);
        }
        #endregion

        #region Tests for ApplyVisitNode(RoomNode, Strat)

        [Fact]
        public void ApplyVisitNode_AccumulatesVisitedNodesAndStrats()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Parlor and Alcatraz", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyVisitNode(model.GetNodeInRoom("Parlor and Alcatraz", 8), inGameState.CurrentNode.LinksTo[8].Strats["Base"])
                .ApplyVisitNode(model.GetNodeInRoom("Parlor and Alcatraz", 1), inGameState.CurrentNode.LinksTo[1].Strats["Parlor Quick Charge"]);

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
        public void ApplyVisitNode_NodeFromWrongModel_ThrowsModelElementMismatchException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            SuperMetroidModel otherModel = OtherReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Parlor and Alcatraz", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            RoomNode node = otherModel.GetNodeInRoom("Parlor and Alcatraz", 8);
            Assert.Throws<ModelElementMismatchException>(() => inGameState.ApplyVisitNode(node, inGameState.CurrentNode.LinksTo[8].Strats["Base"]));
        }

        [Fact]
        public void ApplyVisitNode_StratFromWrongModel_ThrowsModelElementMismatchException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            SuperMetroidModel otherModel = OtherReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Parlor and Alcatraz", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            Strat strat = otherModel.GetNodeInRoom("Parlor and Alcatraz", 4).LinksTo[8].Strats["Base"];
            Assert.Throws<ModelElementMismatchException>(() => inGameState.ApplyVisitNode(model.GetNodeInRoom("Parlor and Alcatraz", 8), strat));
        }

        [Fact]
        public void ApplyVisitNode_LinkDoesntExist_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Parlor and Alcatraz", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyVisitNode(model.GetNodeInRoom("Parlor and Alcatraz", 1), inGameState.CurrentNode.LinksTo[8].Strats["Base"]));
        }

        [Fact]
        public void ApplyVisitNode_StratNotOnOriginLink_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Parlor and Alcatraz", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            Strat wrongStrat = model.GetNodeInRoom("Parlor and Alcatraz", 8).LinksTo[1].Strats["Base"];

            // When and expect
            Assert.Throws<ModelElementMismatchException>(() => inGameState.ApplyVisitNode(model.GetNodeInRoom("Parlor and Alcatraz", 8), wrongStrat));
        }

        #endregion

        #region Tests for ApplyVisitNode(int, string)

        [Fact]
        public void ApplyVisitNodeByNodeIdAndStratName_AccumulatesVisitedNodesAndStrats()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Parlor and Alcatraz", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            inGameState.ApplyVisitNode(8, "Base")
                .ApplyVisitNode(1, "Parlor Quick Charge");

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
        public void ApplyVisitNodeByNodeIdAndStratName_LinkDoesntExist_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Parlor and Alcatraz", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyVisitNode(1, "Base"));
        }

        [Fact]
        public void ApplyVisitNodeByNodeIdAndStratName_StratNotOnOriginLink_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Parlor and Alcatraz", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.ApplyVisitNode(8, "wrongStrat"));
        }

        #endregion

        #region Tests for GetInRoomState()
        [Fact]
        public void GetInRoomState_CurrentRoom_ReturnsCurrentRoomState()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Red Tower", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            // When
            ReadOnlyInRoomState result = inGameState.GetInRoomState(0);

            // Expect
            Assert.Same(expectedNode.Room, result.CurrentRoom);
        }

        [Fact]
        public void GetInRoomState_PreviousRoom_ReturnsPreviousRoomState()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Sloaters Refill", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(expectedNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When
            ReadOnlyInRoomState result = inGameState.GetInRoomState(1);

            // Expect
            Assert.Same(expectedNode.Room, result.CurrentRoom);
        }

        [Fact]
        public void GetInRoomState_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Oasis", 4);
            RoomNode expectedNode = model.GetNodeInRoom("Oasis", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(expectedNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyEnterRoom("Toilet Bowl", 2); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom("Plasma Spark Room", 1);

            // When
            ReadOnlyInRoomState result = inGameState.GetInRoomState(1);

            // Expect
            Assert.Same(expectedNode.Room, result.CurrentRoom);
        }

        [Fact]
        public void GetInRoomState_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetInRoomState(-1));
        }

        [Fact]
        public void GetInRoomState_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode node1 = model.GetNodeInRoom("Red Tower", 4);
            RoomNode node2 = model.GetNodeInRoom("Bat Room", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(node1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            for (int i = 0; i <= InGameState.MaxPreviousRooms; i++)
            {
                RoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            ReadOnlyInRoomState result = inGameState.GetInRoomState(InGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for GetCurrentOrPreviousRoom()
        [Fact]
        public void GetCurrentOrPreviousRoom_CurrentRoom_ReturnsCurrentRoom()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Red Tower", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            // When
            Room result = inGameState.GetCurrentOrPreviousRoom(0);

            // Expect
            Assert.Same(expectedNode.Room, result);
        }

        [Fact]
        public void GetCurrentOrPreviousRoom_PreviousRoom_ReturnsPreviousRoom()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Sloaters Refill", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(expectedNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When
            Room result = inGameState.GetCurrentOrPreviousRoom(1);

            // Expect
            Assert.Same(expectedNode.Room, result);
        }

        [Fact]
        public void GetCurrentOrPreviousRoom_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Oasis", 4);
            RoomNode expectedNode = model.GetNodeInRoom("Oasis", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(expectedNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyEnterRoom("Toilet Bowl", 2); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom("Plasma Spark Room", 1);

            // When
            Room result = inGameState.GetCurrentOrPreviousRoom(1);

            // Expect
            Assert.Same(expectedNode.Room, result);
        }

        [Fact]
        public void GetCurrentOrPreviousRoom_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetCurrentOrPreviousRoom(-1));
        }

        [Fact]
        public void GetCurrentOrPreviousRoom_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode node1 = model.GetNodeInRoom("Red Tower", 4);
            RoomNode node2 = model.GetNodeInRoom("Bat Room", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(node1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            for (int i = 0; i <= InGameState.MaxPreviousRooms; i++)
            {
                RoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            Room result = inGameState.GetCurrentOrPreviousRoom(InGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for GetCurrentOrPreviousRoomEnvironment()
        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_CurrentRoom_ReturnsCurrentRoomEnvironment()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Cathedral Entrance", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Business Center", 6)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            // When
            RoomEnvironment result = inGameState.GetCurrentOrPreviousRoomEnvironment(0);

            // Expect
            Assert.Same(expectedNode.Room, result.Room);
            Assert.True(result.Heated);
        }

        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_PreviousRoom_ReturnsPreviousRoomEnvironment()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Business Center", 6);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(expectedNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Cathedral Entrance", 1);

            // When
            RoomEnvironment result = inGameState.GetCurrentOrPreviousRoomEnvironment(1);

            // Expect
            Assert.Same(expectedNode.Room, result.Room);
            Assert.False(result.Heated);
        }

        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Oasis", 4);
            RoomNode expectedNode = model.GetNodeInRoom("Oasis", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(expectedNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyEnterRoom("Toilet Bowl", 2); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom("Plasma Spark Room", 1);

            // When
            RoomEnvironment result = inGameState.GetCurrentOrPreviousRoomEnvironment(1);

            // Expect
            Assert.Same(expectedNode.Room, result.Room);
        }

        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetCurrentOrPreviousRoomEnvironment(-1));
        }

        [Fact]
        public void GetCurrentOrPreviousRoomEnvironment_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode node1 = model.GetNodeInRoom("Red Tower", 4);
            RoomNode node2 = model.GetNodeInRoom("Bat Room", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(node1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            for (int i = 0; i <= InGameState.MaxPreviousRooms; i++)
            {
                RoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            RoomEnvironment result = inGameState.GetCurrentOrPreviousRoomEnvironment(InGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for GetLastStrat()
        [Fact]
        public void GetLastStrat_CurrentRoom_ReturnsCurrentRoomLastStrat()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);
            inGameState.ApplyVisitNode(8, "Base");
            Strat expectedStrat = inGameState.CurrentNode.LinksTo[4].Strats["Base"];
            inGameState.ApplyVisitNode(model.GetNodeInRoom("Red Tower", 4), expectedStrat);

            // Expect
            Strat result = inGameState.GetLastLinkStrat(0);

            // When
            Assert.Same(expectedStrat, result);
        }

        [Fact]
        public void GetLastStrat_NoLastStrat_ReturnsNull()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When
            Strat result = inGameState.GetLastLinkStrat(0);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void GetLastStrat_PreviousRoom_ReturnsPreviousRoomData()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Business Center", 8)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            Strat expectedStrat = inGameState.CurrentNode.LinksTo[6].Strats["Base"];
            inGameState.ApplyVisitNode(model.GetNodeInRoom("Business Center", 6), expectedStrat);
            inGameState.ApplyEnterRoom("Cathedral Entrance", 1);

            // When
            Strat result = inGameState.GetLastLinkStrat(1);

            // Expect
            Assert.Same(expectedStrat, result);
        }

        [Fact]
        public void GetLastStrat_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Oasis", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            Strat expectedStrat = inGameState.CurrentNode.LinksTo[3].Strats["Base"];
            inGameState.ApplyVisitNode(model.GetNodeInRoom("Oasis", 3), expectedStrat);
            inGameState.ApplyEnterRoom("Toilet Bowl", 2); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom("Plasma Spark Room", 1);

            // When
            Strat result = inGameState.GetLastLinkStrat(1);

            // Expect
            Assert.Same(expectedStrat, result);
        }

        [Fact]
        public void GetLastStrat_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetLastLinkStrat(-1));
        }

        [Fact]
        public void GetLastStrat_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode room1DoorNode = model.GetNodeInRoom("Red Tower", 4);
            RoomNode room1OtherNode = model.GetNodeInRoom("Red Tower", 8);
            RoomNode room2DoorNode = model.GetNodeInRoom("Bat Room", 1);
            RoomNode room2OtherNode = model.GetNodeInRoom("Bat Room", 2);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(room1OtherNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(room1DoorNode, inGameState.CurrentNode.LinksTo[room1DoorNode.Id].Strats["Base"]);
            for (int i = 0; i <= InGameState.MaxPreviousRooms; i++)
            {
                RoomNode doorNode = i % 2 == 0 ? room2DoorNode : room1DoorNode;
                RoomNode otherNode = i % 2 == 0 ? room2OtherNode : room1OtherNode;
                inGameState.ApplyEnterRoom(doorNode);
                inGameState.ApplyVisitNode(otherNode, inGameState.CurrentNode.LinksTo[otherNode.Id].Strats["Base"]);
                inGameState.ApplyVisitNode(doorNode, inGameState.CurrentNode.LinksTo[doorNode.Id].Strats["Base"]);
            }

            // When
            Strat result = inGameState.GetLastLinkStrat(InGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for GetVisitedNodeIds()
        [Fact]
        public void GetVisitedNodeIds_CurrentRoom_ReturnsCurrentRoomVisitedNodeIds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);
            inGameState.ApplyVisitNode(8, "Base");
            inGameState.ApplyVisitNode(4, "Base");

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
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Business Center", 8)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(6, "Base");
            inGameState.ApplyEnterRoom("Cathedral Entrance", 1);

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
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Oasis", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(3, "Base");
            inGameState.ApplyEnterRoom("Toilet Bowl", 2); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom("Plasma Spark Room", 1);

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
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetVisitedNodeIds(-1));
        }

        [Fact]
        public void GetVisitedNodeIds_GoingBeyondRememberedRooms_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode room1DoorNode = model.GetNodeInRoom("Red Tower", 4);
            RoomNode room1OtherNode = model.GetNodeInRoom("Red Tower", 8);
            RoomNode room2DoorNode = model.GetNodeInRoom("Bat Room", 1);
            RoomNode room2OtherNode = model.GetNodeInRoom("Bat Room", 2);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(room1OtherNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(room1DoorNode, inGameState.CurrentNode.LinksTo[room1DoorNode.Id].Strats["Base"]);
            for (int i = 0; i <= InGameState.MaxPreviousRooms; i++)
            {
                RoomNode doorNode = i % 2 == 0 ? room2DoorNode : room1DoorNode;
                RoomNode otherNode = i % 2 == 0 ? room2OtherNode : room1OtherNode;
                inGameState.ApplyEnterRoom(doorNode);
                inGameState.ApplyVisitNode(otherNode, inGameState.CurrentNode.LinksTo[otherNode.Id].Strats["Base"]);
                inGameState.ApplyVisitNode(doorNode, inGameState.CurrentNode.LinksTo[doorNode.Id].Strats["Base"]);
            }

            // When
            IReadOnlyList<int> result = inGameState.GetVisitedNodeIds(InGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Empty(result);
        }
        #endregion

        #region Tests for GetVisitedPath()
        [Fact]
        public void GetVisitedPath_CurrentRoom_ReturnsCurrentRoomVisitedPath()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            RoomNode expectedNode1 = model.GetNodeInRoom("Red Tower", 3);
            inGameState.ApplyEnterRoom(expectedNode1);
            RoomNode expectedNode2 = model.GetNodeInRoom("Red Tower", 8);
            Strat expectedStrat2 = inGameState.CurrentNode.LinksTo[8].Strats["Base"];
            inGameState.ApplyVisitNode(expectedNode2, expectedStrat2);
            RoomNode expectedNode3 = model.GetNodeInRoom("Red Tower", 4);
            Strat expectedStrat3 = inGameState.CurrentNode.LinksTo[4].Strats["Base"];
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
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode1 = model.GetNodeInRoom("Business Center", 8);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(expectedNode1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            RoomNode expectedNode2 = model.GetNodeInRoom("Business Center", 6);
            Strat expectedStrat2 = inGameState.CurrentNode.LinksTo[6].Strats["Base"];
            inGameState.ApplyVisitNode(expectedNode2, expectedStrat2);
            inGameState.ApplyEnterRoom("Cathedral Entrance", 1);

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
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode1 = model.GetNodeInRoom("Oasis", 4);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(expectedNode1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            RoomNode expectedNode2 = model.GetNodeInRoom("Oasis", 3);
            Strat expectedStrat2 = inGameState.CurrentNode.LinksTo[3].Strats["Base"];
            inGameState.ApplyVisitNode(expectedNode2, expectedStrat2);
            inGameState.ApplyEnterRoom("Toilet Bowl", 2); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom("Plasma Spark Room", 1);

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
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetVisitedPath(-1));
        }

        [Fact]
        public void GetVisitedPath_GoingBeyondRememberedRooms_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode room1DoorNode = model.GetNodeInRoom("Red Tower", 4);
            RoomNode room1OtherNode = model.GetNodeInRoom("Red Tower", 8);
            RoomNode room2DoorNode = model.GetNodeInRoom("Bat Room", 1);
            RoomNode room2OtherNode = model.GetNodeInRoom("Bat Room", 2);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(room1OtherNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(room1DoorNode, inGameState.CurrentNode.LinksTo[room1DoorNode.Id].Strats["Base"]);
            for (int i = 0; i <= InGameState.MaxPreviousRooms; i++)
            {
                RoomNode doorNode = i % 2 == 0 ? room2DoorNode : room1DoorNode;
                RoomNode otherNode = i % 2 == 0 ? room2OtherNode : room1OtherNode;
                inGameState.ApplyEnterRoom(doorNode);
                inGameState.ApplyVisitNode(otherNode, inGameState.CurrentNode.LinksTo[otherNode.Id].Strats["Base"]);
                inGameState.ApplyVisitNode(doorNode, inGameState.CurrentNode.LinksTo[doorNode.Id].Strats["Base"]);
            }

            // When
            var result = inGameState.GetVisitedPath(InGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Empty(result);
        }
        #endregion

        #region Tests for GetDestroyedObstacleIds()
        [Fact]
        public void GetDestroyedObstacleIds_CurrentRoom_ReturnsCurrentRoomData()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Hellway", 2)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Caterpillar Room", 2);
            inGameState.ApplyDestroyObstacle("A");

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
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Caterpillar Room", 2)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyDestroyObstacle("A");
            inGameState.ApplyEnterRoom("Hellway", 2);

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
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Caterpillar Room", 2)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyDestroyObstacle(inGameState.CurrentRoom.Obstacles["A"]);
            //Note: This is an invalid connection, but it's not ApplyEnterRoom()'s job to validate that
            inGameState.ApplyEnterRoom("Toilet Bowl", 2); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom("Plasma Spark Room", 1);

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
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetDestroyedObstacleIds(-1));
        }

        [Fact]
        public void GetDestroyedObstacleIds_GoingBeyondRememberedRooms_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode node1 = model.GetNodeInRoom("Caterpillar Room", 2);
            RoomObstacle obstacle1 = node1.Room.Obstacles["A"];
            RoomNode node2 = model.GetNodeInRoom("Beta Power Bomb Room", 1);
            RoomObstacle obstacle2 = node2.Room.Obstacles["B"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(node1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyDestroyObstacle(obstacle1);
            for (int i = 0; i <= InGameState.MaxPreviousRooms; i++)
            {
                RoomNode node = i % 2 == 0 ? node2 : node1;
                RoomObstacle obstacle = i % 2 == 0 ? obstacle2 : obstacle1;
                inGameState.ApplyEnterRoom(node);
                inGameState.ApplyDestroyObstacle(obstacle);
            }

            // When
            IEnumerable<string> result = inGameState.GetDestroyedObstacleIds(InGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Empty(result);
        }
        #endregion

        #region Tests for IsHeatedRoom()
        [Fact]
        public void IsHeatedRoom_CurrentRoom_ReturnsCurrentRoom()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Business Center", 6)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Cathedral Entrance", 1);

            // When
            bool result = inGameState.IsHeatedRoom(0);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsHeatedRoom_ConditionalEnteringFromHeatedNode_ReturnsTrue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Volcano Room", 2)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            bool result = inGameState.IsHeatedRoom(0);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsHeatedRoom_ConditionalEnteringFromNonHeatedNode_ReturnsTrue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Volcano Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            bool result = inGameState.IsHeatedRoom(0);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsHeatedRoom_PreviousRoom_ReturnsPreviousRoom()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Business Center", 6)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Cathedral Entrance", 1);

            // When
            bool result = inGameState.IsHeatedRoom(1);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsHeatedRoom_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Cathedral Entrance", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            // Not a valid connection but it's not ApplyEnterRoom()'s job to know that
            inGameState.ApplyEnterRoom("Toilet Bowl", 2); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom("Plasma Spark Room", 1);

            // When
            bool result = inGameState.IsHeatedRoom(1);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void IsHeatedRoom_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.IsHeatedRoom(-1));
        }

        [Fact]
        public void IsHeatedRoom_GoingBeyondRememberedRooms_ReturnsFalse()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode node1 = model.GetNodeInRoom("Bat Cave", 2);
            RoomNode node2 = model.GetNodeInRoom("Speed Booster Hall", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(node1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            for (int i = 0; i <= InGameState.MaxPreviousRooms; i++)
            {
                RoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            bool result = inGameState.IsHeatedRoom(InGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.False(result);
        }
        #endregion

        #region Tests for GetCurrentDoorEnvironment()
        [Fact]
        public void GetCurrentDoorEnvironment_CurrentRoom_ReturnsCurrentRoomData()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Cathedral Entrance", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Business Center", 6)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            // When
            DoorEnvironment result = inGameState.GetCurrentDoorEnvironment(0);

            // Expect
            Assert.Same(expectedNode, result.Node);
        }

        [Fact]
        public void GetCurrentDoorEnvironment_ConditionalFromSameEntranceNode_ReturnsCorrectEnvironment()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Volcano Room", 2)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            DoorEnvironment result = inGameState.GetCurrentDoorEnvironment(0);

            // Expect
            Assert.Equal(PhysicsEnum.Normal, result.Physics);
        }

        [Fact]
        public void GetCurrentDoorEnvironment_ConditionalFromDifferentEntranceNode_ReturnsCorrectEnvironment()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Volcano Room", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(2, "Base");

            // When
            DoorEnvironment result = inGameState.GetCurrentDoorEnvironment(0);

            // Expect
            Assert.Equal(PhysicsEnum.Lava, result.Physics);
        }

        [Fact]
        public void GetCurrentDoorEnvironment_PreviousRoom_ReturnsPreviousRoomData()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Crab Hole", 2);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(expectedNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Boyon Gate Hall", 3);

            // When
            DoorEnvironment result = inGameState.GetCurrentDoorEnvironment(1);

            // Expect
            Assert.Same(expectedNode, result.Node);
            Assert.Equal(PhysicsEnum.Water, result.Physics);
        }

        [Fact]
        public void GetCurrentDoorEnvironment_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Oasis", 4);
            RoomNode expectedNode = model.GetNodeInRoom("Oasis", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(expectedNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyEnterRoom("Toilet Bowl", 2); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom("Plasma Spark Room", 1);

            // When
            DoorEnvironment result = inGameState.GetCurrentDoorEnvironment(1);

            // Expect
            Assert.Same(expectedNode, result.Node);
            Assert.Equal(PhysicsEnum.Water, result.Physics);
        }

        [Fact]
        public void GetCurrentDoorEnvironment_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetCurrentDoorEnvironment(-1));
        }

        [Fact]
        public void GetCurrentDoorEnvironment_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode node1 = model.GetNodeInRoom("Crab Hole", 2);
            RoomNode node2 = model.GetNodeInRoom("Boyon Gate Hall", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(node1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            for (int i = 0; i <= InGameState.MaxPreviousRooms; i++)
            {
                RoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            DoorEnvironment result = inGameState.GetCurrentDoorEnvironment(InGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for GetCurrentDoorPhysics()
        [Fact]
        public void GetCurrentDoorPhysics_CurrentRoom_ReturnsCurrentRoomData()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Boyon Gate Hall", 3)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Crab Hole", 2);

            // When
            PhysicsEnum? result = inGameState.GetCurrentDoorPhysics(0);

            // Expect
            Assert.Equal(PhysicsEnum.Water, result);
        }

        [Fact]
        public void GetCurrentDoorPhysics_ConditionalFromSameEntranceNode_ReturnsCorrectEnvironment()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Volcano Room", 2)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            PhysicsEnum? result = inGameState.GetCurrentDoorPhysics(0);

            // Expect
            Assert.Equal(PhysicsEnum.Normal, result);
        }

        [Fact]
        public void GetCurrentDoorPhysics_ConditionalFromDifferentEntranceNode_ReturnsCorrectEnvironment()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Volcano Room", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(2, "Base");

            // When
            PhysicsEnum? result = inGameState.GetCurrentDoorPhysics(0);

            // Expect
            Assert.Equal(PhysicsEnum.Lava, result);
        }

        [Fact]
        public void GetCurrentDoorPhysics_PreviousRoom_ReturnsPreviousRoomData()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Crab Hole", 2)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Boyon Gate Hall", 3);

            // When
            PhysicsEnum? result = inGameState.GetCurrentDoorPhysics(1);

            // Expect
            Assert.Equal(PhysicsEnum.Water, result);
        }

        [Fact]
        public void GetCurrentDoorPhysics_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Oasis", 4);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(3, "Base");
            inGameState.ApplyEnterRoom("Toilet Bowl", 2); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom("Plasma Spark Room", 1);

            // When
            PhysicsEnum? result = inGameState.GetCurrentDoorPhysics(1);

            // Expect
            Assert.Equal(PhysicsEnum.Water, result);
        }

        [Fact]
        public void GetCurrentDoorPhysics_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetCurrentDoorPhysics(-1));
        }

        [Fact]
        public void GetCurrentDoorPhysics_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode node1 = model.GetNodeInRoom("Crab Hole", 2);
            RoomNode node2 = model.GetNodeInRoom("Boyon Gate Hall", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(node1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            for (int i = 0; i <= InGameState.MaxPreviousRooms; i++)
            {
                RoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            PhysicsEnum? result = inGameState.GetCurrentDoorPhysics(InGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for GetCurrentNode()
        [Fact]
        public void GetCurrentNode_CurrentRoom_ReturnsCurrentNode()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Red Tower", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom(expectedNode);

            // When
            RoomNode result = inGameState.GetCurrentNode(0);

            // Expect
            Assert.Same(expectedNode, result);
        }

        [Fact]
        public void GetCurrentNode_PreviousRoom_ReturnsLastNodeOfPreviousRoom()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode expectedNode = model.GetNodeInRoom("Sloaters Refill", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(expectedNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When
            RoomNode result = inGameState.GetCurrentNode(1);

            // Expect
            Assert.Same(expectedNode, result);
        }

        [Fact]
        public void GetCurrentNode_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Oasis", 4);
            RoomNode expectedNode = model.GetNodeInRoom("Oasis", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(expectedNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyEnterRoom("Toilet Bowl", 2); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom("Plasma Spark Room", 1);

            // When
            RoomNode result = inGameState.GetCurrentNode(1);

            // Expect
            Assert.Same(expectedNode, result);
        }

        [Fact]
        public void GetCurrentNode_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.GetCurrentNode(-1));
        }

        [Fact]
        public void GetCurrentNode_GoingBeyondRememberedRooms_ReturnsNull()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode node1 = model.GetNodeInRoom("Red Tower", 4);
            RoomNode node2 = model.GetNodeInRoom("Bat Room", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(node1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            for (int i = 0; i <= InGameState.MaxPreviousRooms; i++)
            {
                RoomNode node = i % 2 == 0 ? node2 : node1;
                inGameState.ApplyEnterRoom(node);
            }

            // When
            RoomNode result = inGameState.GetCurrentNode(InGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.Null(result);
        }
        #endregion

        #region Tests for BypassingExitLock()
        [Fact]
        public void BypassingExitLock_CurrentRoomNotBypassing_ReturnsFalse()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Business Center", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            bool result = inGameState.BypassingExitLock(0);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void BypassingExitLock_CurrentRoomBypassing_ReturnsTrue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Business Center", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyBypassLock("Business Center Top Left Green Lock (to Ice Beam Gate)");

            // When
            bool result = inGameState.BypassingExitLock(0);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void BypassingExitLock_PreviousRoom_ReturnsLastRoomData()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Business Center", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyBypassLock("Business Center Top Left Green Lock (to Ice Beam Gate)");
            inGameState.ApplyEnterRoom("Ice Beam Gate Room", 4);

            // When
            bool result = inGameState.BypassingExitLock(1);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void BypassingExitLock_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Oasis", 4);
            RoomNode exitNode = model.GetNodeInRoom("Oasis", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(exitNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyBypassLock("Oasis Green Lock (to Toilet)");
            inGameState.ApplyEnterRoom("Toilet Bowl", 2); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom("Plasma Spark Room", 1);

            // When
            bool result = inGameState.BypassingExitLock(1);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void BypassingExitLock_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.BypassingExitLock(-1));
        }

        [Fact]
        public void BypassingExitLock_GoingBeyondRememberedRooms_ReturnsFalse()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode node1 = model.GetNodeInRoom("Red Brinstar Elevator Room", 1);
            NodeLock lock1 = node1.Locks["Red Brinstar Elevator Yellow Lock (to Kihunters)"];
            RoomNode node2 = model.GetNodeInRoom("Crateria Kihunter Room", 3);
            NodeLock lock2 = node2.Locks["Crateria Kihunter Room Bottom Yellow Lock (to Elevator)"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(node1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyBypassLock(lock1);
            for (int i = 0; i <= InGameState.MaxPreviousRooms; i++)
            {
                RoomNode node = i % 2 == 0 ? node2 : node1;
                NodeLock nodeLock = i % 2 == 0 ? lock2 : lock1;
                inGameState.ApplyEnterRoom(node);
                inGameState.ApplyBypassLock(nodeLock);
            }

            // When
            bool result = inGameState.BypassingExitLock(InGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.False(result);
        }
        #endregion

        #region Tests for OpeningExitLock()
        [Fact]
        public void OpeningExitLock_CurrentRoomNotOpening_ReturnsFalse()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Business Center", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            bool result = inGameState.OpeningExitLock(0);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void OpeningExitLock_CurrentRoomOpening_ReturnsTrue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Business Center", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyOpenLock("Business Center Top Left Green Lock (to Ice Beam Gate)");

            // When
            bool result = inGameState.OpeningExitLock(0);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void OpeningExitLock_PreviousRoom_ReturnsLastRoomData()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Business Center", 1);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyOpenLock("Business Center Top Left Green Lock (to Ice Beam Gate)");
            inGameState.ApplyEnterRoom("Ice Beam Gate Room", 4);

            // When
            bool result = inGameState.OpeningExitLock(1);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void OpeningExitLock_PreviousRoom_SkipsNonPlayableRooms()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode startNode = model.GetNodeInRoom("Oasis", 4);
            RoomNode exitNode = model.GetNodeInRoom("Oasis", 3);
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(startNode)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(exitNode, startNode.LinksTo[3].Strats["Base"]);
            inGameState.ApplyOpenLock("Oasis Green Lock (to Toilet)");
            inGameState.ApplyEnterRoom("Toilet Bowl", 2); // Toilet Bowl is non-playable
            inGameState.ApplyEnterRoom("Plasma Spark Room", 1);

            // When
            bool result = inGameState.OpeningExitLock(1);

            // Expect
            Assert.True(result);
        }

        [Fact]
        public void OpeningExitLock_NegativePreviousRoomCount_ThrowsException()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Sloaters Refill", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Red Tower", 3);

            // When and expect
            Assert.Throws<ArgumentException>(() => inGameState.OpeningExitLock(-1));
        }

        [Fact]
        public void OpeningExitLock_GoingBeyondRememberedRooms_ReturnsFalse()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            RoomNode node1 = model.GetNodeInRoom("Red Brinstar Elevator Room", 1);
            NodeLock lock1 = node1.Locks["Red Brinstar Elevator Yellow Lock (to Kihunters)"];
            RoomNode node2 = model.GetNodeInRoom("Crateria Kihunter Room", 3);
            NodeLock lock2 = node2.Locks["Crateria Kihunter Room Bottom Yellow Lock (to Elevator)"];
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode(node1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyOpenLock(lock1);
            for (int i = 0; i <= InGameState.MaxPreviousRooms; i++)
            {
                RoomNode node = i % 2 == 0 ? node2 : node1;
                NodeLock nodeLock = i % 2 == 0 ? lock2 : lock1;
                inGameState.ApplyEnterRoom(node);
                inGameState.ApplyOpenLock(nodeLock);
            }

            // When
            bool result = inGameState.OpeningExitLock(InGameState.MaxPreviousRooms + 1);

            // Expect
            Assert.False(result);
        }
        #endregion

        #region Tests for GetRetroactiveRunways()
        [Fact]
        public void GetRetroactiveRunways_NoPreviousRoom_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 5)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            IEnumerable<Runway> result = inGameState.GetRetroactiveRunways(new int[] { 5 }, acceptablePhysics: null);

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveRunways_ReturnsRunways()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Seaweed Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            IEnumerable<Runway> expected = inGameState.CurrentNode.Runways.Values;
            inGameState.ApplyEnterRoom("Big Boy Room", 1);

            // When
            IEnumerable<Runway> result = inGameState.GetRetroactiveRunways(new int[] { inGameState.CurrentNode.Id }, acceptablePhysics: null);

            // Expect
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.Count(), result.Intersect(expected, ObjectReferenceEqualityComparer<Runway>.Default).Count());
        }

        [Fact]
        public void GetRetroactiveRunways_PreviousRoomNodeUnconnected_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Seaweed Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Big Boy Room", 2);

            // When
            IEnumerable<Runway> result = inGameState.GetRetroactiveRunways(new int[] { inGameState.CurrentNode.Id }, acceptablePhysics: null);

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveRunways_VisitedPathMismatch_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Seaweed Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Big Boy Room", 1);
            inGameState.ApplyVisitNode(2, "Base");

            // When
            IEnumerable<Runway> result = inGameState.GetRetroactiveRunways(new int[] { inGameState.GetVisitedNodeIds()[0] }, acceptablePhysics: null);

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveRunways_MultiNodePathMatch_ReturnsRunways()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Seaweed Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            IEnumerable<Runway> expected = inGameState.CurrentNode.Runways.Values;
            inGameState.ApplyEnterRoom("Big Boy Room", 1);
            inGameState.ApplyVisitNode(2, "Base");

            // When
            IEnumerable<Runway> result = inGameState.GetRetroactiveRunways(inGameState.GetVisitedNodeIds(), acceptablePhysics: null);

            // Expect
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.Count(), result.Intersect(expected, ObjectReferenceEqualityComparer<Runway>.Default).Count());
        }

        [Fact]
        public void GetRetroactiveRunways_PhysicsMatch_ReturnsRunways()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Seaweed Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            IEnumerable<Runway> expected = inGameState.CurrentNode.Runways.Values;
            inGameState.ApplyEnterRoom("Big Boy Room", 1);

            // When
            IEnumerable<Runway> result = inGameState.GetRetroactiveRunways(new int[] { inGameState.CurrentNode.Id }, acceptablePhysics: new HashSet<PhysicsEnum> { PhysicsEnum.Normal });

            // Expect
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.Count(), result.Intersect(expected, ObjectReferenceEqualityComparer<Runway>.Default).Count());
        }

        [Fact]
        public void GetRetroactiveRunways_PhysicsMismatch_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Seaweed Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Big Boy Room", 1);

            // When
            IEnumerable<Runway> result = inGameState.GetRetroactiveRunways(new int[] { inGameState.CurrentNode.Id }, acceptablePhysics: new HashSet<PhysicsEnum> { PhysicsEnum.Water });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveRunways_BypassingLock_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 4)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyBypassLock("Landing Site Bottom Right Green Lock (to Crateria Tube)");
            inGameState.ApplyEnterRoom("Crateria Tube", 1);

            // When
            IEnumerable<Runway> result = inGameState.GetRetroactiveRunways(new int[] { inGameState.CurrentNode.Id }, acceptablePhysics: null);

            // Expect
            Assert.Empty(result);
        }
        #endregion

        #region Tests for GetRetroactiveCanLeaveChargeds()
        [Fact]
        public void GetRetroactiveCanLeaveChargeds_NoPreviousRoom_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 5)
                .Build();
            InGameState inGameState = new InGameState(startConditions);

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { 5 });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_ReturnsCanLeaveChargeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Green Hill Zone", 3)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            IEnumerable<CanLeaveCharged> expected = inGameState.CurrentNode.CanLeaveCharged;
            inGameState.ApplyEnterRoom("Noob Bridge aka A Bridge Too Far", 1);

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.Count(), result.Intersect(expected, ObjectReferenceEqualityComparer<CanLeaveCharged>.Default).Count());
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_PreviousRoomNodeUnconnected_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Green Hill Zone", 3)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Noob Bridge aka A Bridge Too Far", 2);

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_VisitedPathMismatch_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Green Hill Zone", 3)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyEnterRoom("Noob Bridge aka A Bridge Too Far", 1);
            inGameState.ApplyVisitNode(2, "Base");

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { inGameState.GetVisitedNodeIds()[0] });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_MultiNodePathMatch_ReturnsCanLeaveChargeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Green Hill Zone", 3)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            IEnumerable<CanLeaveCharged> expected = inGameState.CurrentNode.CanLeaveCharged;
            inGameState.ApplyEnterRoom("Noob Bridge aka A Bridge Too Far", 1);
            inGameState.ApplyVisitNode(2, "Base");

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(inGameState.GetVisitedNodeIds());

            // Expect
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.Count(), result.Intersect(expected, ObjectReferenceEqualityComparer<CanLeaveCharged>.Default).Count());
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_BypassingLock_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Crab Shaft", 2)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyBypassLock("Crab Shaft Green Lock (to Aqueduct)");
            inGameState.ApplyEnterRoom("Aqueduct", 1);

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_FollowingPathToDoor_ReturnsCanLeaveCharged()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Early Supers Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(2, "Speed Through");
            IEnumerable<CanLeaveCharged> expected = inGameState.CurrentNode.CanLeaveCharged;
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Equal(expected.Count(), result.Count());
            Assert.Equal(expected.Count(), result.Intersect(expected, ObjectReferenceEqualityComparer<CanLeaveCharged>.Default).Count());
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_FollowingPathToDoor_BypassingLock_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Early Supers Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(2, "Speed Through");
            inGameState.ApplyBypassLock("Early Supers Red Lock (to Brinstar Reserve)");
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_NotFollowingPathToDoor_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Early Supers Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(2, "Speed Through");
            inGameState.ApplyVisitNode(4, "Base");
            inGameState.ApplyVisitNode(2, "Base");
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_FollowingPathToDoorWithWrongStrat_ReturnsEmpty()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Early Supers Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(2, "Early Supers Mockball");
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result);
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_RequiringOpenDoorButNotVisited_ExcludesCanLeaveCharged()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 3)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(1, "Shinespark");
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result.Where(clc => clc.InitiateRemotely.MustOpenDoorFirst));
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_RequiringOpenDoorAndWasVisited_IncludesCanLeaveCharged()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Landing Site", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(4, "Shinespark");
            inGameState.ApplyVisitNode(3, "Base");
            inGameState.ApplyVisitNode(1, "Shinespark");
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Single(result.Where(clc => clc.InitiateRemotely.MustOpenDoorFirst));
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_RequiringOpenLockedDoorAndWasVisitedButNotUnlocked_ExcludesCanLeaveCharged()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Red Brinstar Fireflea Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(2, "Base");
            inGameState.ApplyVisitNode(1, "In-Room Shinespark");
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result.Where(clc => clc.InitiateRemotely.MustOpenDoorFirst));
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_RequiringOpenLockedDoorAndWasVisitedButNotUnlockedUntilLeaving_ExcludesCanLeaveCharged()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Red Brinstar Fireflea Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyVisitNode(2, "Base");
            inGameState.ApplyVisitNode(1, "In-Room Shinespark");
            inGameState.ApplyOpenLock("Red Firefleas Red Lock (to X-Ray)");
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Empty(result.Where(clc => clc.InitiateRemotely.MustOpenDoorFirst));
        }

        [Fact]
        public void GetRetroactiveCanLeaveChargeds_RemoteCanLeaveCharged_RequiringOpenDoorLockedAndWasUnlocked_IncludesCanLeaveCharged()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            StartConditions startConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingNode("Red Brinstar Fireflea Room", 1)
                .Build();
            InGameState inGameState = new InGameState(startConditions);
            inGameState.ApplyOpenLock("Red Firefleas Red Lock (to X-Ray)");
            inGameState.ApplyVisitNode(2, "Base");
            inGameState.ApplyVisitNode(1, "In-Room Shinespark");
            inGameState.ApplyEnterRoom(inGameState.CurrentNode.OutNode);

            // When
            IEnumerable<CanLeaveCharged> result = inGameState.GetRetroactiveCanLeaveChargeds(new int[] { inGameState.CurrentNode.Id });

            // Expect
            Assert.Single(result.Where(clc => clc.InitiateRemotely.MustOpenDoorFirst));
        }
        #endregion

        #region Tests for Clone()
        [Fact]
        public void Clone_CopiesCorrectly()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            string startingRoomName = "Business Center";
            int startingNodeId = 7;
            string startingLockName = "Business Center Top Left Green Lock (to Ice Beam Gate)";
            int startingEnergy = 50;
            string maridiaTubeFlag = "f_MaridiaTubeBroken";
            RoomNode variaNode = model.GetNodeInRoom("Varia Suit Room", 2);
            StartConditions startConditions = new StartConditionsBuilder(model)
                .StartingGameFlags(new string[] { maridiaTubeFlag })
                .StartingInventory(ItemInventory.CreateVanillaStartingInventory(model).ApplyAddItem(model.Items[SuperMetroidModel.VARIA_SUIT_NAME]))
                .StartingNode(startingRoomName, startingNodeId)
                .StartingOpenLocks(new string[] { startingLockName })
                .StartingTakenItemLocations(new RoomNode[] { variaNode })
                .StartingResources(new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, startingEnergy))
                .Build();

            // When
            InGameState inGameState = new InGameState(startConditions).Clone();

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
            SuperMetroidModel model = ReusableModel();
            string startingRoomName = "Business Center";
            int startingNodeId = 7;
            string startingLockName = "Business Center Top Left Green Lock (to Ice Beam Gate)";
            string secondLockName = "Business Center Bottom Left Red Lock (to HiJump E-Tank)";
            int startingEnergy = 50;
            string maridiaTubeFlag = "f_MaridiaTubeBroken";
            RoomNode variaNode = model.GetNodeInRoom("Varia Suit Room", 2);
            StartConditions startConditions = new StartConditionsBuilder(model)
                .StartingInventory(ItemInventory.CreateVanillaStartingInventory(model).ApplyAddItem(model.Items["Missile"]))
                .StartingNode(startingRoomName, startingNodeId)
                .StartingOpenLocks(new string[] { startingLockName })
                .StartingResources(new ResourceCount().ApplyAmount(RechargeableResourceEnum.RegularEnergy, startingEnergy)
                    .ApplyAmount(RechargeableResourceEnum.Missile, 5))
                .Build();

            InGameState inGameState = new InGameState(startConditions);

            // When
            InGameState clone = inGameState.Clone();

            // Subsequently given
            // Modify the clone
            clone.ApplyVisitNode(8, "Base");
            clone.ApplyVisitNode(3, "Base");
            clone.ApplyOpenLock(model.Locks[secondLockName]);
            clone.ApplyTakeLocation(variaNode);
            clone.ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME);
            clone.ApplyAddItem(SuperMetroidModel.MISSILE_NAME);
            clone.ApplyAddResource(RechargeableResourceEnum.Missile, 2);
            clone.ApplyAddGameFlag(maridiaTubeFlag);

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
