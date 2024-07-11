using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using sm_json_data_framework.Models.Rooms;

namespace sm_json_data_framework.Tests.Models.Rooms
{
    public class LinkTest
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
            Link link = model.Rooms["Landing Site"].Links[5];
            Assert.Same(model.Rooms["Landing Site"].Nodes[5], link.FromNode);
            Assert.Equal(5, link.To.Count);
            Assert.Contains(2, link.To.Keys);
            Assert.Contains(3, link.To.Keys);
            Assert.Contains(4, link.To.Keys);
            Assert.Contains(6, link.To.Keys);
            Assert.Contains(7, link.To.Keys);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem(SuperMetroidModel.GRAVITY_SUIT_NAME)
                .RegisterDisabledTech("canSuitlessMaridia");

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            Link noDestinationsLink = model.Rooms["Crab Shaft"].Links[2];
            Assert.False(noDestinationsLink.LogicallyRelevant);

            Link possibleLink = model.Rooms["Landing Site"].Links[1];
            Assert.True(possibleLink.LogicallyRelevant);
        }

        #endregion
    }
}
