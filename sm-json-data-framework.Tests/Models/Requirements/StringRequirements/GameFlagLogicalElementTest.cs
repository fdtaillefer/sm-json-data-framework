using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Requirements.StringRequirements
{
    public class GameFlagLogicalElementTest
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
            GameFlagLogicalElement gameFlagLogicalElement = model.Rooms["Glass Tunnel"].Links[5].To[6].Strats["Suitless Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_MaridiaTubeBroken");
            Assert.Same(model.GameFlags["f_MaridiaTubeBroken"], gameFlagLogicalElement.GameFlag);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_FlagNotEnabled_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            GameFlagLogicalElement gameFlagLogicalElement = model.Rooms["Glass Tunnel"].Links[5].To[6].Strats["Suitless Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_MaridiaTubeBroken");
            InGameState inGameState = model.CreateInitialGameState();

            // When
            ExecutionResult result = gameFlagLogicalElement.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_FlagEnabled_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            GameFlagLogicalElement gameFlagLogicalElement = model.Rooms["Glass Tunnel"].Links[5].To[6].Strats["Suitless Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_MaridiaTubeBroken");
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddGameFlag("f_MaridiaTubeBroken");

            // When
            ExecutionResult result = gameFlagLogicalElement.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_FlagEnabledButLogicallyDisabled_Fails()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledGameFlag("f_MaridiaTubeBroken");
            model.ApplyLogicalOptions(logicalOptions);
            GameFlagLogicalElement gameFlagLogicalElement = model.Rooms["Glass Tunnel"].Links[5].To[6].Strats["Suitless Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_MaridiaTubeBroken");
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddGameFlag("f_MaridiaTubeBroken"); ;

            // When
            ExecutionResult result = gameFlagLogicalElement.Execute(model, inGameState);

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
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledGameFlag("f_AnimalsSaved");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingGameFlags(new List<string> { "f_DefeatedCeresRidley" })
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            GameFlagLogicalElement alwaysFlagElement = model.Locks["Ceres Ridley Room Grey Lock (to 58 Escape)"].UnlockStrats["Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_DefeatedCeresRidley");
            Assert.True(alwaysFlagElement.LogicallyRelevant);
            Assert.False(alwaysFlagElement.LogicallyNever);
            Assert.True(alwaysFlagElement.LogicallyAlways);
            Assert.True(alwaysFlagElement.LogicallyFree);

            GameFlagLogicalElement removedFlagElement = model.Locks["Animal Escape Grey Lock (to Flyway)"].UnlockStrats["Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_AnimalsSaved");
            Assert.True(removedFlagElement.LogicallyRelevant);
            Assert.True(removedFlagElement.LogicallyNever);
            Assert.False(removedFlagElement.LogicallyAlways);
            Assert.False(removedFlagElement.LogicallyFree);

            GameFlagLogicalElement obtainableFlagElement = model.Locks["Blue Brinstar Power Bombs Spawn Lock"].UnlockStrats["Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_ZebesAwake");
            Assert.True(obtainableFlagElement.LogicallyRelevant);
            Assert.False(obtainableFlagElement.LogicallyNever);
            Assert.False(obtainableFlagElement.LogicallyAlways);
            Assert.False(obtainableFlagElement.LogicallyFree);
        }

        #endregion
    }
}
