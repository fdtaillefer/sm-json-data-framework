using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Items;
using System.Reflection;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Requirements.StringRequirements
{
    public class TechLogicalElementTest
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
            TechLogicalElement techElement = model.Techs["canDelayedWalljump"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canPreciseWalljump");
            Assert.Same(model.Techs["canPreciseWalljump"], techElement.Tech);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_RequirementsNotMet_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            TechLogicalElement techLogicalElement = model.Techs["canWaterBreakFree"].Requires
                .LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canSunkenDualWallClimb");
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = techLogicalElement.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RequirementsMet_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            TechLogicalElement techLogicalElement = model.Techs["canWaterBreakFree"].Requires
                .LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canSunkenDualWallClimb");
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("HiJump");

            // When
            ExecutionResult result = techLogicalElement.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved("HiJump")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_RequirementsMetButTechDisabled_Fails()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            TechLogicalElement techLogicalElement = model.Techs["canWaterBreakFree"].Requires
                .LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canSunkenDualWallClimb");
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledTech("canSunkenDualWallClimb");
            model.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("HiJump");

            // When
            ExecutionResult result = techLogicalElement.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_ConfiguredWithMultipleTries_ConsumesMoreResources()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            TechLogicalElement techLogicalElement = model.Rooms["Draygon's Room"].Links[3].To[2].Strats["Draygon Grapple Jump"].Requires
                .LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canDraygonTurretGrappleJump");
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterTechTries("canDraygonTurretGrappleJump", 3);
            model.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Grapple")
                .ApplyAddItem("HiJump")
                .ApplyAddItem("Morph")
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources();

            // When
            ExecutionResult result = techLogicalElement.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
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
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledTech("canPreciseWalljump");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model).StartingInventory(
                ItemInventory.CreateVanillaStartingInventory(model)
                    .ApplyAddItem(model.Items["Morph"])
                    .ApplyAddItem(model.Items["Bombs"])
                )
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            TechLogicalElement disabledTechElement = model.Techs["canDelayedWalljump"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canPreciseWalljump");
            Assert.True(disabledTechElement.LogicallyRelevant);
            Assert.False(disabledTechElement.LogicallyAlways);
            Assert.False(disabledTechElement.LogicallyFree);
            Assert.True(disabledTechElement.LogicallyNever);

            TechLogicalElement impossibleSubTechElement = model.Techs["canInsaneWalljump"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canDelayedWalljump");
            Assert.True(impossibleSubTechElement.LogicallyRelevant);
            Assert.False(impossibleSubTechElement.LogicallyAlways);
            Assert.False(impossibleSubTechElement.LogicallyFree);
            Assert.True(impossibleSubTechElement.LogicallyNever);

            TechLogicalElement nonFreeTechElement = model.Locks["West Ocean Ship Exit Grey Lock (to Gravity Suit Room)"].BypassStrats["Bowling Skip"]
                .Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canGrappleClip");
            Assert.True(nonFreeTechElement.LogicallyRelevant);
            Assert.False(nonFreeTechElement.LogicallyAlways);
            Assert.False(nonFreeTechElement.LogicallyFree);
            Assert.False(nonFreeTechElement.LogicallyNever);

            TechLogicalElement freeTechElement = model.Techs["canPreciseWalljump"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canWalljump");
            Assert.True(freeTechElement.LogicallyRelevant);
            Assert.True(freeTechElement.LogicallyAlways);
            Assert.True(freeTechElement.LogicallyFree);
            Assert.False(freeTechElement.LogicallyNever);

            TechLogicalElement freeByStartItemTechElement = model.Techs["canJumpIntoIBJ"].Requires.LogicalElement<TechLogicalElement>(0, element => element.Tech.Name == "canIBJ");
            Assert.True(freeByStartItemTechElement.LogicallyRelevant);
            Assert.True(freeByStartItemTechElement.LogicallyAlways);
            Assert.True(freeByStartItemTechElement.LogicallyFree);
            Assert.False(freeByStartItemTechElement.LogicallyNever);
        }

        #endregion
    }
}
