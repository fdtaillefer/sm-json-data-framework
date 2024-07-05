using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Arrays;
using sm_json_data_framework.Models.Items;
using System.Reflection;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules.InitialState;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.Arrays
{
    public class ResourceCapacityLogicalElementTest
    {
        private static SuperMetroidModel ReusableModel() => StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel NewModelForOptions() => StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation
            SuperMetroidModel model = ReusableModel();

            // Expect
            ResourceCapacityLogicalElement resourceCapacityLogicalElement = model.Locks["Crocomire Fight"].UnlockStrats["Missiles"].Requires
                .LogicalElement<ResourceCapacityLogicalElement>(0);
            Assert.Equal(1, resourceCapacityLogicalElement.ResourceCapacities.Count);
            Assert.Equal(RechargeableResourceEnum.Missile, resourceCapacityLogicalElement.ResourceCapacities[RechargeableResourceEnum.Missile].Resource);
            Assert.Equal(10, resourceCapacityLogicalElement.ResourceCapacities[RechargeableResourceEnum.Missile].Count);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_Fulfilled_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ResourceCapacityLogicalElement resourceCapacityLogicalElement = model.Locks["Crocomire Fight"].UnlockStrats["Missiles"].Requires
                .LogicalElement<ResourceCapacityLogicalElement>(0, capacityElement => capacityElement.ResourceCapacities[RechargeableResourceEnum.Missile].Count == 10);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME)
                .ApplyAddItem(SuperMetroidModel.MISSILE_NAME);

            // When
            ExecutionResult result = resourceCapacityLogicalElement.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_NotFulfilled_Fails()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            ResourceCount baseResouces = ResourceCount.CreateVanillaBaseResourceMaximums()
                .ApplyAmount(RechargeableResourceEnum.Missile, 9);
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .BaseResourceMaximums(baseResouces)
                .StartingResources(baseResouces)
                .Build();
            model.ApplyLogicalOptions(logicalOptions);

            ResourceCapacityLogicalElement resourceCapacityLogicalElement = model.Locks["Crocomire Fight"].UnlockStrats["Missiles"].Requires
                .LogicalElement<ResourceCapacityLogicalElement>(0, capacityElement => capacityElement.ResourceCapacities[RechargeableResourceEnum.Missile].Count == 10);
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = resourceCapacityLogicalElement.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_Possible_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            ResourceCapacityLogicalElement resourceCapacityLogicalElement = model.Locks["Crocomire Fight"].UnlockStrats["Missiles"].Requires
                .LogicalElement<ResourceCapacityLogicalElement>(0, capacityElement => capacityElement.ResourceCapacities[RechargeableResourceEnum.Missile].Count == 10);
            Assert.True(resourceCapacityLogicalElement.LogicallyRelevant);
            Assert.False(resourceCapacityLogicalElement.LogicallyNever);
            Assert.False(resourceCapacityLogicalElement.LogicallyAlways);
            Assert.False(resourceCapacityLogicalElement.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_Impossible_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)model.Items[SuperMetroidModel.ENERGY_TANK_NAME], 14)
                .ApplyAddExpansionItem((ExpansionItem)model.Items[SuperMetroidModel.RESERVE_TANK_NAME], 4)
                .ApplyAddExpansionItem((ExpansionItem)model.Items[SuperMetroidModel.SUPER_NAME], 10)
                .ApplyAddExpansionItem((ExpansionItem)model.Items[SuperMetroidModel.POWER_BOMB_NAME], 10);

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            ResourceCapacityLogicalElement resourceCapacityLogicalElement = model.Locks["Crocomire Fight"].UnlockStrats["Missiles"].Requires
                .LogicalElement<ResourceCapacityLogicalElement>(0, capacityElement => capacityElement.ResourceCapacities[RechargeableResourceEnum.Missile].Count == 10);
            Assert.True(resourceCapacityLogicalElement.LogicallyRelevant);
            Assert.True(resourceCapacityLogicalElement.LogicallyNever);
            Assert.False(resourceCapacityLogicalElement.LogicallyAlways);
            Assert.False(resourceCapacityLogicalElement.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_AlwaysPossible_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items[SuperMetroidModel.MISSILE_NAME])
                    .ApplyAddItem(model.Items[SuperMetroidModel.MISSILE_NAME])
                )
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            ResourceCapacityLogicalElement resourceCapacityLogicalElement = model.Locks["Crocomire Fight"].UnlockStrats["Missiles"].Requires
                .LogicalElement<ResourceCapacityLogicalElement>(0, capacityElement => capacityElement.ResourceCapacities[RechargeableResourceEnum.Missile].Count == 10);
            Assert.True(resourceCapacityLogicalElement.LogicallyRelevant);
            Assert.False(resourceCapacityLogicalElement.LogicallyNever);
            Assert.True(resourceCapacityLogicalElement.LogicallyAlways);
            Assert.True(resourceCapacityLogicalElement.LogicallyFree);
        }

        #endregion
    }
}
