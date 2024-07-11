using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Models.Items;
using System.Reflection;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Rooms
{
    public class StratTest
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
            Strat strat = model.Rooms["Early Supers Room"].Links[1].To[2].Strats["Early Supers Mockball"];
            Assert.Equal("Early Supers Mockball", strat.Name);
            Assert.True(strat.Notable);
            Assert.NotNull(strat.Requires);

            Assert.Equal(2, strat.Requires.LogicalElements.Count);
            Assert.NotNull(strat.Requires.LogicalElement<ResetRoom>(0));
            Assert.NotNull(strat.Requires.LogicalElement<TechLogicalElement>(0));

            Assert.Empty(strat.Obstacles);

            Assert.Equal(1, strat.Failures.Count);
            Assert.Contains("Crumble Fall", strat.Failures.Keys);

            Assert.Empty(strat.StratProperties);


            Strat stratWithObstacles = model.Rooms["Morph Ball Room"].Links[4].To[5].Strats["Base"];
            Assert.False(stratWithObstacles.Notable);
            Assert.Equal(1, stratWithObstacles.Obstacles.Count);
            Assert.Contains("B", stratWithObstacles.Obstacles.Keys);
            Assert.NotNull(stratWithObstacles.Requires);
            Assert.Empty(stratWithObstacles.Requires.LogicalElements);
            Assert.Empty(stratWithObstacles.Failures);

            Strat stratWithProperties = model.Rooms["Crumble Shaft"].Links[1].To[3].Strats["Space Jump"];
            Assert.Equal(1, stratWithProperties.StratProperties.Count);
            Assert.Contains("spinjump", stratWithProperties.StratProperties);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_PossibleStrat_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Strat strat = model.Rooms["Morph Ball Room"].Links[6].To[5].Strats["Bomb the Blocks"];
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Morph")
                .ApplyAddItem("Bombs")
                .ApplyEnterRoom("Morph Ball Room", 6);

            // When
            ExecutionResult result = strat.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved("Morph")
                .ExpectItemInvolved("Bombs")
                .ExpectDestroyedObstacle("A")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_StratRequirementsNotMet_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Strat strat = model.Rooms["Construction Zone"].Links[3].To[4].Strats["Base"];
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Construction Zone",3);

            // When
            ExecutionResult result = strat.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RequirementsMetButObstacleLocalRequirementsNotMet_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Morph Ball Room", 6);
            Strat strat = model.Rooms["Morph Ball Room"].Links[6].To[1].Strats["Laugh at Dead Sidehoppers"];

            // When
            ExecutionResult result = strat.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_LocallyIndestructibleObstacleButAlreadyDestroyed_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Morph Ball Room", 6)
                .ApplyDestroyObstacle("C");
            Strat strat = model.Rooms["Morph Ball Room"].Links[6].To[1].Strats["Laugh at Dead Sidehoppers"];

            // When
            ExecutionResult result = strat.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_ObstacleCanBeBypassedButNotDestroyed_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Morph")
                .ApplyAddItem("Ice")
                .ApplyEnterRoom("Post Crocomire Jump Room", 5);
            Strat strat = model.Rooms["Post Crocomire Jump Room"].Links[5].To[1].Strats["PCJR Frozen Mella Door"];

            // When
            ExecutionResult result = strat.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved("Morph")
                .ExpectItemInvolved("Ice")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_ObstacleHasUnmetCommonRequirements_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Climb", 2);
            Strat strat = model.Rooms["Climb"].Links[2].To[6].Strats["Base"];

            // When
            ExecutionResult result = strat.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_ObstacleHasMetCommonRequirements_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("ScrewAttack")
                .ApplyEnterRoom("Climb", 2);
            Strat strat = model.Rooms["Climb"].Links[2].To[6].Strats["Base"];

            // When
            ExecutionResult result = strat.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved("ScrewAttack")
                .ExpectDestroyedObstacle("A")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_DestroyingObstacleDestroysAdditionalObstacle_DestroysBoth()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Strat strat = model.Rooms["Wrecked Ship Main Shaft"].Links[7].To[9].Strats["Power Bombs"];
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Morph")
                .ApplyAddItem(SuperMetroidModel.POWER_BOMB_NAME)
                .ApplyRefillResources()
                .ApplyEnterRoom("Wrecked Ship Main Shaft", 7);

            // When
            ExecutionResult result = strat.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved("Morph")
                .ExpectItemInvolved(SuperMetroidModel.POWER_BOMB_NAME)
                .ExpectDestroyedObstacle("A")
                .ExpectDestroyedObstacle("B")
                .ExpectResourceVariation(RechargeableResourceEnum.PowerBomb, -1)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_ObstacleDestroysAdditionalObstacleButIsAlreadyDestroyed_DoesNotDestroyAdditional()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Strat strat = model.Rooms["Wrecked Ship Main Shaft"].Links[7].To[9].Strats["Power Bombs"];
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Morph")
                .ApplyAddItem(SuperMetroidModel.POWER_BOMB_NAME)
                .ApplyRefillResources()
                .ApplyEnterRoom("Wrecked Ship Main Shaft", 7)
                .ApplyDestroyObstacle("B");

            // When
            ExecutionResult result = strat.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectItemInvolved("Morph")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_ConfiguredToTakeMultipleTries_UsesMoreResources()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterStratTries("Ceiling E-Tank Dboost", 3);
            model.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddGameFlag("f_ZebesAwake")
                .ApplyEnterRoom("Blue Brinstar Energy Tank Room", 2);
            Strat strat = model.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Dboost"];

            // When
            ExecutionResult result = strat.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                // Should have taken 3 Reo hits of damage rather than 1
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -45)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_DisabledByConfiguration_Fails()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledStrat("Ceiling E-Tank Dboost");
            model.ApplyLogicalOptions(logicalOptions);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddGameFlag("f_ZebesAwake")
                .ApplyEnterRoom("Blue Brinstar Energy Tank Room", 2);
            Strat strat = model.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Dboost"];

            // When
            ExecutionResult result = strat.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Grapple")
                .RegisterDisabledStrat("Ceiling E-Tank Dboost");

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Strat impossibleRequirementsStrat = model.Rooms["Pants Room"].Links[4].To[5].Strats["Base"];
            Assert.False(impossibleRequirementsStrat.LogicallyRelevant);
            Assert.False(impossibleRequirementsStrat.LogicallyAlways);
            Assert.False(impossibleRequirementsStrat.LogicallyFree);
            Assert.True(impossibleRequirementsStrat.LogicallyNever);

            Strat disabledStrat = model.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Dboost"];
            Assert.False(disabledStrat.LogicallyRelevant);
            Assert.False(disabledStrat.LogicallyAlways);
            Assert.False(disabledStrat.LogicallyFree);
            Assert.True(disabledStrat.LogicallyNever);

            Strat nonFreeStrat = model.Rooms["Blue Brinstar Energy Tank Room"].Links[1].To[3].Strats["Ceiling E-Tank Speed Jump"];
            Assert.True(nonFreeStrat.LogicallyRelevant);
            Assert.False(nonFreeStrat.LogicallyAlways);
            Assert.False(nonFreeStrat.LogicallyFree);
            Assert.False(nonFreeStrat.LogicallyNever);

            Strat freeStrat = model.Rooms["Landing Site"].Links[5].To[4].Strats["Over the Top"];
            Assert.True(freeStrat.LogicallyRelevant);
            Assert.True(freeStrat.LogicallyAlways);
            Assert.True(freeStrat.LogicallyFree);
            Assert.False(freeStrat.LogicallyNever);
        }

        #endregion
    }
}
