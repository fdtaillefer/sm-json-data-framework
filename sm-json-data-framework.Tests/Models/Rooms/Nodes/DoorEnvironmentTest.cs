using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Rules;

namespace sm_json_data_framework.Tests.Models.Rooms.Nodes
{
    public class DoorEnvironmentTest
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
            DoorEnvironment doorEnvironment = model.Rooms["Volcano Room"].Nodes[2].DoorEnvironments.First();
            Assert.Equal(PhysicsEnum.Lava, doorEnvironment.Physics);
            Assert.Equal(1, doorEnvironment.EntranceNodes.Count);
            Assert.Same(model.Rooms["Volcano Room"].Nodes[1], doorEnvironment.EntranceNodes[1]);
            Assert.Same(model.Rooms["Volcano Room"].Nodes[2], doorEnvironment.Node);
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
            foreach (DoorEnvironment doorEnvironment in model.Rooms["Volcano Room"].Nodes[2].DoorEnvironments)
            {
                Assert.True(doorEnvironment.LogicallyRelevant);
            }
        }

        #endregion
    }
}
