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
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;

namespace sm_json_data_framework.Tests.Models.Requirements.StringRequirements
{
    public class GameFlagLogicalElementTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            GameFlagLogicalElement gameFlagLogicalElement = Model.Rooms["Glass Tunnel"].Links[5].To[6].Strats["Suitless Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_MaridiaTubeBroken");
            Assert.Same(Model.GameFlags["f_MaridiaTubeBroken"], gameFlagLogicalElement.GameFlag);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_FlagNotEnabled_Fails()
        {
            // Given
            GameFlagLogicalElement gameFlagLogicalElement = Model.Rooms["Glass Tunnel"].Links[5].To[6].Strats["Suitless Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_MaridiaTubeBroken");
            InGameState inGameState = Model.CreateInitialGameState();

            // When
            ExecutionResult result = gameFlagLogicalElement.Execute(Model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_FlagEnabled_Succeeds()
        {
            // Given
            GameFlagLogicalElement gameFlagLogicalElement = Model.Rooms["Glass Tunnel"].Links[5].To[6].Strats["Suitless Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_MaridiaTubeBroken");
            InGameState inGameState = Model.CreateInitialGameState()
                .ApplyAddGameFlag("f_MaridiaTubeBroken");

            // When
            ExecutionResult result = gameFlagLogicalElement.Execute(Model, inGameState);

            // Expect
            new ExecutionResultValidator(Model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_FlagEnabledButLogicallyDisabled_Fails()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledGameFlag("f_MaridiaTubeBroken");
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);
            GameFlagLogicalElement gameFlagLogicalElement = ModelWithOptions.Rooms["Glass Tunnel"].Links[5].To[6].Strats["Suitless Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_MaridiaTubeBroken");
            InGameState inGameState = ModelWithOptions.CreateInitialGameState()
                .ApplyAddGameFlag("f_MaridiaTubeBroken"); ;

            // When
            ExecutionResult result = gameFlagLogicalElement.Execute(ModelWithOptions, inGameState);

            // Expect
            Assert.Null(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.RegisterDisabledGameFlag("f_AnimalsSaved");
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(ModelWithOptions)
                .StartingGameFlags(new List<string> { "f_DefeatedCeresRidley" })
                .Build();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            GameFlagLogicalElement alwaysFlagElement = ModelWithOptions.Locks["Ceres Ridley Room Grey Lock (to 58 Escape)"].UnlockStrats["Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_DefeatedCeresRidley");
            Assert.True(alwaysFlagElement.LogicallyRelevant);
            Assert.False(alwaysFlagElement.LogicallyNever);
            Assert.True(alwaysFlagElement.LogicallyAlways);
            Assert.True(alwaysFlagElement.LogicallyFree);

            GameFlagLogicalElement removedFlagElement = ModelWithOptions.Locks["Animal Escape Grey Lock (to Flyway)"].UnlockStrats["Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_AnimalsSaved");
            Assert.True(removedFlagElement.LogicallyRelevant);
            Assert.True(removedFlagElement.LogicallyNever);
            Assert.False(removedFlagElement.LogicallyAlways);
            Assert.False(removedFlagElement.LogicallyFree);

            GameFlagLogicalElement obtainableFlagElement = ModelWithOptions.Locks["Blue Brinstar Power Bombs Spawn Lock"].UnlockStrats["Base"].Requires
                .LogicalElement<GameFlagLogicalElement>(0, element => element.GameFlag.Name == "f_ZebesAwake");
            Assert.True(obtainableFlagElement.LogicallyRelevant);
            Assert.False(obtainableFlagElement.LogicallyNever);
            Assert.False(obtainableFlagElement.LogicallyAlways);
            Assert.False(obtainableFlagElement.LogicallyFree);
        }

        #endregion
    }
}
