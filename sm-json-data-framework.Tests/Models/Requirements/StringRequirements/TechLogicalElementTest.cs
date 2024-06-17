using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Items;

namespace sm_json_data_framework.Tests.Models.Requirements.StringRequirements
{
    public class TechLogicalElementTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            TechLogicalElement techElement = Model.Techs["canDelayedWalljump"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canPreciseWalljump");
            Assert.Same(Model.Techs["canPreciseWalljump"], techElement.Tech);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_RequirementsNotMet_Fails()
        {
            // Given
            TechLogicalElement techLogicalElement = Model.Techs["canWaterBreakFree"].Requires
                .LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canSunkenDualWallClimb");
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            ExecutionResult result = techLogicalElement.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RequirementsMet_Succeeds()
        {
            // Given
            TechLogicalElement techLogicalElement = Model.Techs["canWaterBreakFree"].Requires
                .LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canSunkenDualWallClimb");
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddItem("HiJump");

            // When
            ExecutionResult result = techLogicalElement.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .ExpectItemInvolved("HiJump")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_RequirementsMetButTechDisabled_Fails()
        {
            // Given
            TechLogicalElement techLogicalElement = ModelWithOptions.Techs["canWaterBreakFree"].Requires
                .LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canSunkenDualWallClimb");
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canSunkenDualWallClimb");
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = ModelWithOptions.CreateInitialGameState()
                .ApplyAddItem("HiJump");

            // When
            ExecutionResult result = techLogicalElement.Execute(ModelWithOptions, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_ConfiguredWithMultipleTries_ConsumesMoreResources()
        {
            // Given
            TechLogicalElement techLogicalElement = ModelWithOptions.Rooms["Draygon's Room"].Links[3].To[2].Strats["Draygon Grapple Jump"].Requires
                .LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canDraygonTurretGrappleJump");
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterTechTries("canDraygonTurretGrappleJump", 3);
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = ModelWithOptions.CreateInitialGameState()
                .ApplyAddItem("Grapple")
                .ApplyAddItem("HiJump")
                .ApplyAddItem("Morph")
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources();

            // When
            ExecutionResult result = techLogicalElement.Execute(ModelWithOptions, inGameState);

            // Expect
            new ExecutionResultValidator(ModelWithOptions, inGameState)
                .ExpectItemInvolved("Grapple")
                .ExpectItemInvolved("HiJump")
                .ExpectItemInvolved("Morph")
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -180)
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledTech("canPreciseWalljump");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(ModelWithOptions)
                    .ApplyAddItem(ModelWithOptions.Items["Morph"])
                    .ApplyAddItem(ModelWithOptions.Items["Bombs"])
                )
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            TechLogicalElement disabledTechElement = ModelWithOptions.Techs["canDelayedWalljump"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canPreciseWalljump");
            Assert.True(disabledTechElement.LogicallyRelevant);
            Assert.False(disabledTechElement.LogicallyAlways);
            Assert.False(disabledTechElement.LogicallyFree);
            Assert.True(disabledTechElement.LogicallyNever);

            TechLogicalElement impossibleSubTechElement = ModelWithOptions.Techs["canInsaneWalljump"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canDelayedWalljump");
            Assert.True(impossibleSubTechElement.LogicallyRelevant);
            Assert.False(impossibleSubTechElement.LogicallyAlways);
            Assert.False(impossibleSubTechElement.LogicallyFree);
            Assert.True(impossibleSubTechElement.LogicallyNever);

            TechLogicalElement nonFreeTechElement = ModelWithOptions.Locks["West Ocean Ship Exit Grey Lock (to Gravity Suit Room)"].BypassStrats["Bowling Skip"]
                .Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canGrappleClip");
            Assert.True(nonFreeTechElement.LogicallyRelevant);
            Assert.False(nonFreeTechElement.LogicallyAlways);
            Assert.False(nonFreeTechElement.LogicallyFree);
            Assert.False(nonFreeTechElement.LogicallyNever);

            TechLogicalElement freeTechElement = ModelWithOptions.Techs["canPreciseWalljump"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canWalljump");
            Assert.True(freeTechElement.LogicallyRelevant);
            Assert.True(freeTechElement.LogicallyAlways);
            Assert.True(freeTechElement.LogicallyFree);
            Assert.False(freeTechElement.LogicallyNever);

            TechLogicalElement freeByStartItemTechElement = ModelWithOptions.Techs["canJumpIntoIBJ"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canIBJ");
            Assert.True(freeByStartItemTechElement.LogicallyRelevant);
            Assert.True(freeByStartItemTechElement.LogicallyAlways);
            Assert.True(freeByStartItemTechElement.LogicallyFree);
            Assert.False(freeByStartItemTechElement.LogicallyNever);
        }

        #endregion
    }
}
