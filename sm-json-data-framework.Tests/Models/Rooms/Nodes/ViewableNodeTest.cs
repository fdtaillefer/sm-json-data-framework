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
using sm_json_data_framework.Models.Rooms;

namespace sm_json_data_framework.Tests.Models.Rooms.Nodes
{
    public class ViewableNodeTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            ViewableNode viewableNode = Model.Rooms["Blue Brinstar Energy Tank Room"].Nodes[1].ViewableNodes[3];
            Assert.Same(Model.Rooms["Blue Brinstar Energy Tank Room"].Nodes[3], viewableNode.Node);
            Assert.Equal(1, viewableNode.Strats.Count);
            Assert.Contains("Base", viewableNode.Strats.Keys);
    }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            // There's only one ViewableNode in the model right now, and it's always free, so we can only test that
            ViewableNode viewableNode = ModelWithOptions.Rooms["Blue Brinstar Energy Tank Room"].Nodes[1].ViewableNodes[3];
            Assert.True(viewableNode.LogicallyRelevant);
            Assert.False(viewableNode.LogicallyNever);
            Assert.True(viewableNode.LogicallyAlways);
            Assert.True(viewableNode.LogicallyFree);
        }

        #endregion
    }
}
