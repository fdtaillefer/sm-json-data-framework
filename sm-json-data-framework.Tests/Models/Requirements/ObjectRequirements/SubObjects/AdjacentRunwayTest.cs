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
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubRequirements;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements;
using System.Reflection;
using sm_json_data_framework.Models.Items;

namespace sm_json_data_framework.Tests.Models.Requirements.ObjectRequirements.SubObjects
{
    public class AdjacentRunwayTest
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
            AdjacentRunway adjacentRunway = model.Rooms["Blue Brinstar Boulder Room"].Links[2].To[1].Strats["Boulder Room Jump Through the Door"].Requires
                .LogicalElement<AdjacentRunway>(0);
            Assert.Same(model.Rooms["Blue Brinstar Boulder Room"].Nodes[2], adjacentRunway.FromNode);
            Assert.Empty(adjacentRunway.InRoomPath);
            foreach (PhysicsEnum physics in Enum.GetValues(typeof(PhysicsEnum)))
            {
                Assert.Contains(physics, adjacentRunway.Physics);
            }
            Assert.Equal(0, adjacentRunway.UseFrames);
            Assert.False(adjacentRunway.OverrideRunwayRequirements);

            AdjacentRunway xRayClimbAdjacentRunway = model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Links[7].To[12]
                .Strats["Green Brinstar Main Shaft Right-Side X-Ray Climb"].Requires.LogicalElement<AdjacentRunway>(0);
            Assert.Equal(1, xRayClimbAdjacentRunway.Physics.Count);
            Assert.Contains(PhysicsEnum.Normal, xRayClimbAdjacentRunway.Physics);
            Assert.Equal(200, xRayClimbAdjacentRunway.UseFrames);
            Assert.True(xRayClimbAdjacentRunway.OverrideRunwayRequirements);

            AdjacentRunway adjacentRunwayWithInRoomPath = model.Rooms["Glass Tunnel"].Links[5].To[4].Strats["Maridia Tube Transition Gravity Jump"].Requires
                .LogicalElement<Or>(0).LogicalRequirements.LogicalElement<AdjacentRunway>(0);
            Assert.Equal(2, adjacentRunwayWithInRoomPath.InRoomPath.Count);
            Assert.Contains(1, adjacentRunwayWithInRoomPath.InRoomPath);
            Assert.Contains(5, adjacentRunwayWithInRoomPath.InRoomPath);
        }

        #endregion

        #region Tests for UseFramesExecution.Execute()

        [Fact]
        public void UseFramesExecutionExecute_NormalPhysics_SucceedsForFree()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Links[7].To[12]
                .Strats["Green Brinstar Main Shaft Right-Side X-Ray Climb"].Requires.LogicalElement<AdjacentRunway>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Green Brinstar Main Shaft / Etecoon Room", 9)
                .ApplyEnterRoom("Green Brinstar Main Shaft / Etecoon Room", 7);

            // When
            ExecutionResult result = adjacentRunway.UseFramesExecution.Execute(model, inGameState, previousRoomCount: 1);

            // Expect - note that we don't expect the runway to be reported as used since its requirement are overridden
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void UseFramesExecutionExecute_DamagingRoomPhysics_CostsResources()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Wasteland"].Links[1].To[6].Strats["Wasteland X-Ray Climb"].Requires
                .LogicalElement<AdjacentRunway>(0, adjacentRunway => adjacentRunway.UseFrames == 200);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Metal Pirates Room", 2)
                .ApplyEnterRoom("Wasteland", 1);

            // When
            ExecutionResult result = adjacentRunway.UseFramesExecution.Execute(model, inGameState, previousRoomCount: 1);

            // Expect - note that we don't expect the runway to be reported as used since its requirement are overridden
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(sm_json_data_framework.Models.Items.RechargeableResourceEnum.RegularEnergy, -50)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void UseFramesExecutionExecute_DamagingDoorPhysics_CostsResources()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Green Brinstar Main Shaft / Etecoon Room"].Links[7].To[12]
                .Strats["Green Brinstar Main Shaft Right-Side X-Ray Climb"].Requires.LogicalElement<AdjacentRunway>(0);
            // We're going to make an impossible sequence of events to test that door physics are taken into account,
            // By teleporting from a door in Volcano Room to a node that has a proper AdjacentRunway
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyAddItem("Morph")
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyAddItem(SuperMetroidModel.ENERGY_TANK_NAME)
                .ApplyRefillResources()
                .ApplyEnterRoom("Volcano Room", 1)
                .ApplyVisitNode(2, "Suitless Volcano Dive")
                .ApplyEnterRoom("Green Brinstar Main Shaft / Etecoon Room", 7)
                .ApplyRefillResources();

            // When
            ExecutionResult result = adjacentRunway.UseFramesExecution.Execute(model, inGameState, previousRoomCount: 1);

            // Expect - note that we don't expect the runway to be reported as used since its requirement are overridden
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(sm_json_data_framework.Models.Items.RechargeableResourceEnum.RegularEnergy, -100)
                .AssertRespectedBy(result);
        }

        #endregion

        #region Tests for Execute()

        [Fact]
        public void Execute_AdjacentRunwayTooShort_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Ice Beam Gate Room"].Links[2].To[4].Strats["Mockball"].Requires.LogicalElement<AdjacentRunway>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Ice Beam Acid Room", 2)
                .ApplyEnterRoom("Ice Beam Gate Room", 2);

            // When
            ExecutionResult result = adjacentRunway.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RunwayRequirementsNotMet_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Lava Dive Room"].Links[2].To[1].Strats["Lava Dive (Morph)"].Requires.LogicalElement<AdjacentRunway>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 80)
                .ApplyEnterRoom("Kronic Boost Room", 3)
                .ApplyEnterRoom("Lava Dive Room", 2);

            // When
            ExecutionResult result = adjacentRunway.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_RunwayRequirementsMet_ExecutesThemAndSucceeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Lava Dive Room"].Links[2].To[1].Strats["Lava Dive (Morph)"].Requires.LogicalElement<AdjacentRunway>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Kronic Boost Room", 3)
                .ApplyEnterRoom("Lava Dive Room", 2);

            // When
            ExecutionResult result = adjacentRunway.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -50)
                .ExpectRunwayUsed("Base Runway - Kronic Boost Room Bottom Left Door (to Lava Dive)", "Base")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_UseFramesRequirementsNotMet_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Wasteland"].Links[1].To[6].Strats["Wasteland X-Ray Climb"].Requires
                .LogicalElement<AdjacentRunway>(0, adjacentRunway => adjacentRunway.UseFrames == 200);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 50)
                .ApplyEnterRoom("Metal Pirates Room", 2)
                .ApplyEnterRoom("Wasteland", 1);

            // When
            ExecutionResult result = adjacentRunway.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_HasUseFrames_ExecutesUseFrames()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Wasteland"].Links[1].To[6].Strats["Wasteland X-Ray Climb"].Requires
                .LogicalElement<AdjacentRunway>(0, adjacentRunway => adjacentRunway.UseFrames == 200);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Metal Pirates Room", 2)
                .ApplyEnterRoom("Wasteland", 1);

            // When
            ExecutionResult result = adjacentRunway.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectResourceVariation(RechargeableResourceEnum.RegularEnergy, -50)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_OverridesAdjacentRunwayRequirements_IgnoresAdjacentRunwayRequirements()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Aqueduct"].Links[1].To[9].Strats["Aqueduct Left-Side X-Ray Climb (Upper)"].Requires
                .LogicalElement<AdjacentRunway>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyConsumeResource(ConsumableResourceEnum.Energy, 50)
                // This runway requires Gravity so it will fail if requirements not ignored
                .ApplyEnterRoom("Crab Shaft", 2)
                .ApplyEnterRoom("Aqueduct", 1);

            // When
            ExecutionResult result = adjacentRunway.Execute(model, inGameState);

            // Expect - note that we don't expect the runway to be reported as used since its requirement are overridden
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_AdjacentPhysicsNotMet_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Aqueduct"].Links[5].To[8].Strats["Aqueduct Right-Side X-Ray Climb"].Requires
                .LogicalElement<AdjacentRunway>(0);
            InGameState inGameState = model.CreateInitialGameState()
                // Door physics are water but normal physics are required
                .ApplyEnterRoom("Below Botwoon Energy Tank", 1)
                .ApplyEnterRoom("Aqueduct", 5);

            // When
            ExecutionResult result = adjacentRunway.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_AdjacentPhysicsMet_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Warehouse Kihunter Room"].Links[2].To[1].Strats["Warehouse Kihunter Room X-Ray Climb"].Requires
                .LogicalElement<AdjacentRunway>(0);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Baby Kraid Room", 1)
                .ApplyEnterRoom("Warehouse Kihunter Room", 2);

            // When
            ExecutionResult result = adjacentRunway.Execute(model, inGameState);

            // Expect - note that we don't expect the runway to be reported as used since its requirement are overridden
            new ExecutionResultValidator(model, inGameState)
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_InRoomPathNotMet_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Glass Tunnel"].Links[5].To[4].Strats["Maridia Tube Transition Gravity Jump"].Requires
                .LogicalElement<Or>(0).LogicalRequirements.LogicalElement<AdjacentRunway>(0, adjacentRunway => adjacentRunway.FromNode.Id == 3);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Boyon Gate Hall", 1)
                .ApplyEnterRoom("Glass Tunnel", 3)
                .ApplyVisitNode(5, "Base")
                .ApplyVisitNode(1, "Base")
                .ApplyVisitNode(5, "Base");

            // When
            ExecutionResult result = adjacentRunway.Execute(model, inGameState);

            // Expect
            Assert.Null(result);
        }

        [Fact]
        public void Execute_InRoomPathMet_Succeeds()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Glass Tunnel"].Links[5].To[4].Strats["Maridia Tube Transition Gravity Jump"].Requires
                .LogicalElement<Or>(0).LogicalRequirements.LogicalElement<AdjacentRunway>(0, adjacentRunway => adjacentRunway.FromNode.Id == 3);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Boyon Gate Hall", 1)
                .ApplyEnterRoom("Glass Tunnel", 3)
                .ApplyVisitNode(5, "Base");

            // When
            ExecutionResult result = adjacentRunway.Execute(model, inGameState);

            // Expect
            new ExecutionResultValidator(model, inGameState)
                .ExpectRunwayUsed("Base Runway - Boyon Gate Hall Left Door (to Glass Tube)", "Base")
                .AssertRespectedBy(result);
        }

        [Fact]
        public void Execute_NodeNotConnected_Fails()
        {
            // Given
            SuperMetroidModel model = ReusableModel();
            AdjacentRunway adjacentRunway = model.Rooms["Bubble Mountain"].Links[9].To[7].Strats["Bubble Mountain Save Room Jump"].Requires
                .LogicalElement<AdjacentRunway>(0, adjacentRunway => adjacentRunway.FromNode.Id == 2);
            InGameState inGameState = model.CreateInitialGameState()
                .ApplyEnterRoom("Lower Mushrooms", 2)
                .ApplyEnterRoom("Bubble Mountain", 2);

            // When
            ExecutionResult result = adjacentRunway.Execute(model, inGameState);

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
            AdjacentRunway adjacentRunway = model.Rooms["Screw Attack Room"].Links[2].To[3].Strats["Screw Attack Room Transition Screwjump"].Requires.LogicalElement<AdjacentRunway>(0);
            Assert.True(adjacentRunway.LogicallyRelevant);
            Assert.False(adjacentRunway.LogicallyNever);
            Assert.False(adjacentRunway.LogicallyAlways);
            Assert.False(adjacentRunway.LogicallyFree);
        }

        #endregion
    }
}
