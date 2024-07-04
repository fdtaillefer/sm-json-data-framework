using sm_json_data_framework.Models;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.Requirements.StringRequirements;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules.InitialState;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.SubRequirements
{
    public class NotTest
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
            Not not = model.Rooms["Sponge Bath"].Links[1].To[2].Strats["Ship Unpowered"].Requires.LogicalElement<Not>(0);
            Assert.NotNull(not.LogicalRequirements);
            Assert.Equal(1, not.LogicalRequirements.LogicalElements.Count);
            Assert.NotNull(not.LogicalRequirements.LogicalElement<GameFlagLogicalElement>(0));
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_SubRequirementsMet_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Not not = model.Rooms["Sponge Bath"].Links[1].To[2].Strats["Ship Unpowered"].Requires.LogicalElement<Not>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddGameFlag("f_DefeatedPhantoon")
                .ApplyEnterRoom("Sponge Bath", 1);

            //  When
            ExecutionResult result = not.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_SubRequirementsNotMet_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            Not not = model.Rooms["Sponge Bath"].Links[1].To[2].Strats["Ship Unpowered"].Requires.LogicalElement<Not>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Sponge Bath", 1);

            //  When
            ExecutionResult result = not.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SometimesPossible_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Not not = model.Rooms["Sponge Bath"].Links[1].To[2].Strats["Ship Unpowered"].Requires.LogicalElement<Not>(0);
            Assert.True(not.LogicallyRelevant);
            Assert.False(not.LogicallyNever);
            Assert.False(not.LogicallyAlways);
            Assert.False(not.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_NeverPossible_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalStartConditions = StartConditions.CreateVanillaStartConditionsBuilder(model)
                .StartingGameFlags(new List<string> { "f_DefeatedPhantoon" })
                .Build();

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Not not = model.Rooms["Sponge Bath"].Links[1].To[2].Strats["Ship Unpowered"].Requires.LogicalElement<Not>(0);
            Assert.True(not.LogicallyRelevant);
            Assert.True(not.LogicallyNever);
            Assert.False(not.LogicallyAlways);
            Assert.False(not.LogicallyFree);
        }

        [Fact]
        public void ApplyLogicalOptions_AlwaysFree_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterDisabledGameFlag("f_DefeatedPhantoon");

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Not not = model.Rooms["Sponge Bath"].Links[1].To[2].Strats["Ship Unpowered"].Requires.LogicalElement<Not>(0);
            Assert.True(not.LogicallyRelevant);
            Assert.False(not.LogicallyNever);
            Assert.True(not.LogicallyAlways);
            Assert.True(not.LogicallyFree);
        }

        #endregion
    }
}
