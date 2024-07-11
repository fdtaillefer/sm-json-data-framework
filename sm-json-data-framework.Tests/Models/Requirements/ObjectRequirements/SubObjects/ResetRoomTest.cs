using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.InGameStates;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.SubObjects
{
    public class ResetRoomTest
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
            ResetRoom resetRoomObstaclesToAvoid = model.Runways["Base Runway - Green Shaft Mid-Low Left Door (to Firefleas)"].Strats["Base"].Requires
                .LogicalElement<ResetRoom>(0);
            Assert.Equal(10, resetRoomObstaclesToAvoid.Nodes.Count);
            Assert.Same(model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[1], resetRoomObstaclesToAvoid.Nodes[1]);
            Assert.Same(model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[2], resetRoomObstaclesToAvoid.Nodes[2]);
            Assert.Same(model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[3], resetRoomObstaclesToAvoid.Nodes[3]);
            Assert.Same(model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[4], resetRoomObstaclesToAvoid.Nodes[4]);
            Assert.Same(model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[5], resetRoomObstaclesToAvoid.Nodes[5]);
            Assert.Same(model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[6], resetRoomObstaclesToAvoid.Nodes[6]);
            Assert.Same(model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[7], resetRoomObstaclesToAvoid.Nodes[7]);
            Assert.Same(model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[8], resetRoomObstaclesToAvoid.Nodes[8]);
            Assert.Same(model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[9], resetRoomObstaclesToAvoid.Nodes[9]);
            Assert.Same(model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Nodes[10], resetRoomObstaclesToAvoid.Nodes[10]);
            Assert.Equal(1, resetRoomObstaclesToAvoid.ObstaclesToAvoid.Count);
            Assert.Same(model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Obstacles["A"], resetRoomObstaclesToAvoid.ObstaclesToAvoid["A"]);
            Assert.Empty(resetRoomObstaclesToAvoid.NodesToAvoid);
            Assert.False(resetRoomObstaclesToAvoid.MustStayPut);

            ResetRoom resetRoomMustStayPut = model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Links[7].To[12].Strats["Green Brinstar Main Shaft Right-Side X-Ray Climb"]
                .Requires.LogicalElement<ResetRoom>(0);
            Assert.True(resetRoomMustStayPut.MustStayPut);
            Assert.Empty(resetRoomMustStayPut.ObstaclesToAvoid);
            Assert.Empty(resetRoomMustStayPut.NodesToAvoid);

            ResetRoom resetRoomNodesToAvoid = model.Rooms["Warehouse Kihunter Room"].Nodes[1].CanLeaveCharged.First().Strats["Base"].Requires
                .LogicalElement<ResetRoom>(0);
            Assert.Empty(resetRoomNodesToAvoid.ObstaclesToAvoid);
            Assert.Equal(1, resetRoomNodesToAvoid.NodesToAvoid.Count);
            Assert.Same(model.Rooms["Warehouse Kihunter Room"].Nodes[2], resetRoomNodesToAvoid.NodesToAvoid[2]);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_ObstaclesToAvoid_RespectingIt_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ResetRoom resetRoom = model.Runways["Base Runway - Beta Power Bombs Door (to Caterpillar Room)"].Strats["Base"].Requires.LogicalElement<ResetRoom>(0,
                resetRoom => !resetRoom.MustStayPut && !resetRoom.NodesToAvoid.Any() && resetRoom.Nodes.ContainsKey(1) && resetRoom.ObstaclesToAvoid.ContainsKey("B"));
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Beta Power Bomb Room"].Nodes[1])
                .ApplyVisitNode(3, "Morph Power Beam Sidehopper Kill")
                .ApplyDestroyObstacle("A")
                .ApplyVisitNode(1, "Base");

            // When
            ExecutionResult result = resetRoom.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_ObstaclesToAvoid_DestroyingObstacle_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ResetRoom resetRoom = model.Runways["Base Runway - Beta Power Bombs Door (to Caterpillar Room)"].Strats["Base"].Requires.LogicalElement<ResetRoom>(0, 
                resetRoom => !resetRoom.MustStayPut && !resetRoom.NodesToAvoid.Any() && resetRoom.Nodes.ContainsKey(1) && resetRoom.ObstaclesToAvoid.ContainsKey("B"));
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Beta Power Bomb Room"].Nodes[1])
                .ApplyVisitNode(3, "PB Sidehopper Kill")
                .ApplyDestroyObstacle("A")
                .ApplyDestroyObstacle("B")
                .ApplyVisitNode(1, "Base");

            // When
            ExecutionResult result = resetRoom.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_MustStayPut_RespectingIt_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ResetRoom resetRoom = model.Rooms["Construction Zone"].Links[3].To[4].Strats["Construction Room X-Ray Climb"].Requires
                .LogicalElement<ResetRoom>(0, resetRoom => resetRoom.MustStayPut && resetRoom.Nodes.ContainsKey(3));
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Construction Zone"].Nodes[3]);

            // When
            ExecutionResult result = resetRoom.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_MustStayPut_NotRespectingIt_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ResetRoom resetRoom = model.Rooms["Construction Zone"].Links[3].To[4].Strats["Construction Room X-Ray Climb"].Requires
                .LogicalElement<ResetRoom>(0, resetRoom => resetRoom.MustStayPut && !resetRoom.NodesToAvoid.Any() && resetRoom.Nodes.ContainsKey(3));
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Construction Zone"].Nodes[3])
                .ApplyVisitNode(4, "Base")
                .ApplyVisitNode(3, "Base");

            // When
            ExecutionResult result = resetRoom.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_NodesToAvoid_RespectingIt_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ResetRoom resetRoom = model.Runways["Base Runway - Caterpillar Room Center Left Door (to Hellway)"].Strats["Base"].Requires
                .LogicalElement<ResetRoom>(0, resetRoom => !resetRoom.MustStayPut && resetRoom.NodesToAvoid.ContainsKey(3) && resetRoom.Nodes.ContainsKey(4));
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Caterpillar Room"].Nodes[4])
                .ApplyVisitNode(2, "Base");

            // When
            ExecutionResult result = resetRoom.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_NodesToAvoid_NotRespectingIt_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ResetRoom resetRoom = model.Runways["Base Runway - Caterpillar Room Center Left Door (to Hellway)"].Strats["Base"].Requires
                .LogicalElement<ResetRoom>(0, resetRoom => !resetRoom.MustStayPut && resetRoom.NodesToAvoid.ContainsKey(3) && resetRoom.Nodes.ContainsKey(4));
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Caterpillar Room"].Nodes[4])
                .ApplyVisitNode(2, "Base")
                .ApplyVisitNode(3, "Base")
                .ApplyVisitNode(2, "Base");

            // When
            ExecutionResult result = resetRoom.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_NotReespectingEntryNode_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            ResetRoom resetRoom = model.Rooms["Green Hill Zone"].Nodes[3].CanLeaveCharged.Where(clc => clc.Strats.ContainsKey("Arrive From Left")).First().Strats["Arrive From Left"]
                .Requires.LogicalElement<ResetRoom>(0, resetRoom => !resetRoom.MustStayPut && !resetRoom.NodesToAvoid.ContainsKey(3) && !resetRoom.Nodes.ContainsKey(3));
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom(model.Rooms["Green Hill Zone"].Nodes[3]);

            // When
            ExecutionResult result = resetRoom.Execute(model, inGameState);

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

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            ResetRoom resetRoom = model.Runways["Base Runway - Green Shaft Mid-Low Left Door (to Firefleas)"].Strats["Base"].Requires.LogicalElement<ResetRoom>(0);
            Assert.True(resetRoom.LogicallyRelevant);
            Assert.False(resetRoom.LogicallyNever);
            Assert.False(resetRoom.LogicallyAlways);
            Assert.False(resetRoom.LogicallyFree);
        }

        #endregion
    }
}
