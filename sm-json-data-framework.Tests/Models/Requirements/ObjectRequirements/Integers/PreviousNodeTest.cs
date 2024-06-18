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
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using System.Reflection;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.Integers
{
    public class PreviousNodeTest
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
            PreviousNode previousNode = model.Rooms["Early Supers Room"].Links[3].To[1].Strats["Early Supers Quick Crumble Escape (Dual Quick Crumble)"].Requires.LogicalElement<PreviousNode>(0);
            Assert.Equal(1, previousNode.NodeId);
        }

        #endregion

        #region Tests for IsFulfilled()

        [Fact]
        public void IsFulfilled_NoPreviousNode_ReturnsFalse()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            PreviousNode previousNode = model.Rooms["Early Supers Room"].Links[3].To[1].Strats["Early Supers Quick Crumble Escape (Dual Quick Crumble)"].Requires.LogicalElement<PreviousNode>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Early Supers Room"].Nodes[3]);

            // When
            bool result = previousNode.IsFulfilled(inGameState);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsFulfilled_DifferentPreviousNode_ReturnsFalse()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            PreviousNode previousNode = model.Rooms["Early Supers Room"].Links[3].To[1].Strats["Early Supers Quick Crumble Escape (Dual Quick Crumble)"].Requires.LogicalElement<PreviousNode>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Early Supers Room"].Nodes[5])
                .ApplyVisitNode(3, "Base");

            // When
            bool result = previousNode.IsFulfilled(inGameState);

            // Expect
            Assert.False(result);
        }

        [Fact]
        public void IsFulfilled_CorrectPreviousNode_ReturnsTrue()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            PreviousNode previousNode = model.Rooms["Early Supers Room"].Links[3].To[1].Strats["Early Supers Quick Crumble Escape (Dual Quick Crumble)"].Requires.LogicalElement<PreviousNode>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Early Supers Room"].Nodes[1])
                .ApplyVisitNode(3, "Base");

            // When
            bool result = previousNode.IsFulfilled(inGameState);

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
            PreviousNode previousNode = model.Rooms["Early Supers Room"].Links[3].To[1].Strats["Early Supers Quick Crumble Escape (Dual Quick Crumble)"].Requires.LogicalElement<PreviousNode>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Early Supers Room"].Nodes[3]);

            // When
            ExecutionResult result = previousNode.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Executes_DifferentPreviousNode_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            PreviousNode previousNode = model.Rooms["Early Supers Room"].Links[3].To[1].Strats["Early Supers Quick Crumble Escape (Dual Quick Crumble)"].Requires.LogicalElement<PreviousNode>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Early Supers Room"].Nodes[5])
                .ApplyVisitNode(3, "Base");

            // When
            ExecutionResult result = previousNode.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Executes_CorrectPreviousNode_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            PreviousNode previousNode = model.Rooms["Early Supers Room"].Links[3].To[1].Strats["Early Supers Quick Crumble Escape (Dual Quick Crumble)"].Requires.LogicalElement<PreviousNode>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Early Supers Room"].Nodes[1])
                .ApplyVisitNode(3, "Base");

            // When
            ExecutionResult result = previousNode.Execute(model, inGameState);

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
            PreviousNode previousNode = model.Rooms["Early Supers Room"].Links[3].To[1].Strats["Early Supers Quick Crumble Escape (Dual Quick Crumble)"].Requires.LogicalElement<PreviousNode>(0);
            Assert.True(previousNode.LogicallyRelevant);
            Assert.False(previousNode.LogicallyNever);
            Assert.False(previousNode.LogicallyAlways);
            Assert.False(previousNode.LogicallyFree);
        }

        #endregion
    }
}
