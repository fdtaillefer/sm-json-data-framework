using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Rooms.Nodes;

namespace sm_json_data_framework.Tests.Models.Rooms.Nodes
{
    public class IntiateRemotelyTest
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
            InitiateRemotely initiateRemotely = model.Rooms["Red Brinstar Fireflea Room"].Nodes[1].CanLeaveCharged.First().InitiateRemotely;
            Assert.Same(model.Rooms["Red Brinstar Fireflea Room"].Nodes[2], initiateRemotely.InitiateAtNode);
            Assert.Same(model.Rooms["Red Brinstar Fireflea Room"].Nodes[1], initiateRemotely.ExitNode);
            Assert.True(initiateRemotely.MustOpenDoorFirst);
            Assert.Equal(1, initiateRemotely.PathToDoor.Count);
            Assert.Same(model.Rooms["Red Brinstar Fireflea Room"].Nodes[1], initiateRemotely.PathToDoor[0].linkTo.TargetNode);
            Assert.Equal(1, initiateRemotely.PathToDoor[0].strats.Count);
            Assert.Same(model.Rooms["Red Brinstar Fireflea Room"].Links[2].To[1].Strats["In-Room Shinespark"], initiateRemotely.PathToDoor[0].strats["In-Room Shinespark"]);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalPropertiesOn()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Morph");

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            InitiateRemotely neverInitiateRemotely = model.Rooms["Warehouse Kihunter Room"].Nodes[3].CanLeaveCharged.First().InitiateRemotely;
            Assert.True(neverInitiateRemotely.LogicallyRelevant);
            Assert.False(neverInitiateRemotely.LogicallyAlways);
            Assert.False(neverInitiateRemotely.LogicallyFree);
            Assert.True(neverInitiateRemotely.LogicallyNever);

            InitiateRemotely freeInitiateRemotely = model.Rooms["Mt. Everest"].Nodes[3].CanLeaveCharged.First().InitiateRemotely;
            Assert.True(freeInitiateRemotely.LogicallyRelevant);
            Assert.True(freeInitiateRemotely.LogicallyAlways);
            Assert.True(freeInitiateRemotely.LogicallyFree);
            Assert.False(freeInitiateRemotely.LogicallyNever);

            InitiateRemotely possibleInitiateRemotely = model.Rooms["Early Supers Room"].Nodes[2].CanLeaveCharged.First().InitiateRemotely;
            Assert.True(possibleInitiateRemotely.LogicallyRelevant);
            Assert.False(possibleInitiateRemotely.LogicallyAlways);
            Assert.False(possibleInitiateRemotely.LogicallyFree);
            Assert.False(possibleInitiateRemotely.LogicallyNever);
        }

        #endregion
    }
}
