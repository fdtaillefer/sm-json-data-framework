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
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Strings;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Requirements;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.Strings
{
    public class PreviousStratPropertyTest
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
            PreviousStratProperty previousStratProperty = model.Rooms["Crumble Shaft"].Links[3].To[1].Strats["Space Jump"].Requires.LogicalElement<PreviousStratProperty>(0);
            Assert.Equal("spinjump", previousStratProperty.StratProperty);
        }

        #endregion

        #region Tests for IsFulfilled()

        [Fact]
        public void IsFulfilled_NoPreviousNode_ReturnsFalse()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            PreviousStratProperty previousStratProperty = model.Rooms["Crumble Shaft"].Links[3].To[1].Strats["Space Jump"].Requires.LogicalElement<PreviousStratProperty>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Crumble Shaft"].Nodes[3]);

            // When
            bool result = previousStratProperty.IsFulfilled(inGameState);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsFulfilled_DifferentPreviousNode_ReturnsFalse()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            PreviousStratProperty previousStratProperty = model.Rooms["Crumble Shaft"].Links[3].To[1].Strats["Space Jump"].Requires.LogicalElement<PreviousStratProperty>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .ApplyAddItem("Morph")
                .ApplyAddItem("SpringBall")
                .ApplyEnterRoom(model.Rooms["Crumble Shaft"].Nodes[1])
                .ApplyVisitNode(3, "SpringBall");

            // When
            bool result = previousStratProperty.IsFulfilled(inGameState);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsFulfilled_CorrectPreviousNode_ReturnsTrue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            PreviousStratProperty previousStratProperty = model.Rooms["Crumble Shaft"].Links[3].To[1].Strats["Space Jump"].Requires.LogicalElement<PreviousStratProperty>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .ApplyAddItem("SpaceJump")
                .ApplyEnterRoom(model.Rooms["Crumble Shaft"].Nodes[1])
                .ApplyVisitNode(3, "Space Jump");

            // When
            bool result = previousStratProperty.IsFulfilled(inGameState);

            // Expect
            Assert.True(result);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_NoPreviousNode_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            PreviousStratProperty previousStratProperty = model.Rooms["Crumble Shaft"].Links[3].To[1].Strats["Space Jump"].Requires.LogicalElement<PreviousStratProperty>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Crumble Shaft"].Nodes[3]);

            // When
            ExecutionResult result = previousStratProperty.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_DifferentPreviousNode_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            PreviousStratProperty previousStratProperty = model.Rooms["Crumble Shaft"].Links[3].To[1].Strats["Space Jump"].Requires.LogicalElement<PreviousStratProperty>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .ApplyAddItem("Morph")
                .ApplyAddItem("SpringBall")
                .ApplyEnterRoom(model.Rooms["Crumble Shaft"].Nodes[1])
                .ApplyVisitNode(3, "SpringBall");

            // When
            ExecutionResult result = previousStratProperty.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_CorrectPreviousNode_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            PreviousStratProperty previousStratProperty = model.Rooms["Crumble Shaft"].Links[3].To[1].Strats["Space Jump"].Requires.LogicalElement<PreviousStratProperty>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem(SuperMetroidModel.VARIA_SUIT_NAME)
                .ApplyAddItem("SpaceJump")
                .ApplyEnterRoom(model.Rooms["Crumble Shaft"].Nodes[1])
                .ApplyVisitNode(3, "Space Jump");

            // When
            ExecutionResult result = previousStratProperty.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
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

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            PreviousStratProperty previousStratProperty = model.Rooms["Crumble Shaft"].Links[3].To[1].Strats["Space Jump"].Requires.LogicalElement<PreviousStratProperty>(0);
            Assert.True(previousStratProperty.LogicallyRelevant);
            Assert.False(previousStratProperty.LogicallyNever);
            Assert.False(previousStratProperty.LogicallyAlways);
            Assert.False(previousStratProperty.LogicallyFree);
        }

        #endregion
    }
}
