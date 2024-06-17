using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Rooms;

namespace sm_json_data_framework.Tests.Models.Rooms
{
    public class LinkToTest
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
            LinkTo linkTo = model.Rooms["Landing Site"].Links[5].To[7];
            Assert.Same(model.Rooms["Landing Site"].Nodes[7], linkTo.TargetNode);
            Assert.Equal(2, linkTo.Strats.Count);
            Assert.Contains("Base", linkTo.Strats.Keys);
            Assert.Contains("Gauntlet Walljumps", linkTo.Strats.Keys);
        }

        #endregion

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            SuperMetroidModel model = NewModelForOptions();
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Gravity")
                .RegisterDisabledTech("canSuitlessMaridia");

            // When
            model.ApplyLogicalOptions(logicalOptions);

            // Expect
            LinkTo noDestinationsLinkTo = model.Rooms["Crab Shaft"].Links[2].To[1];
            Assert.False(noDestinationsLinkTo.LogicallyRelevant);
            Assert.False(noDestinationsLinkTo.LogicallyAlways);
            Assert.False(noDestinationsLinkTo.LogicallyFree);
            Assert.True(noDestinationsLinkTo.LogicallyNever);

            LinkTo possibleLinkTo = model.Rooms["Landing Site"].Links[1].To[7];
            Assert.True(possibleLinkTo.LogicallyRelevant);
            Assert.False(possibleLinkTo.LogicallyAlways);
            Assert.False(possibleLinkTo.LogicallyFree);
            Assert.False(possibleLinkTo.LogicallyNever);

            LinkTo freeLinkTo = model.Rooms["Landing Site"].Links[5].To[4];
            Assert.True(freeLinkTo.LogicallyRelevant);
            Assert.True(freeLinkTo.LogicallyAlways);
            Assert.True(freeLinkTo.LogicallyFree);
            Assert.False(freeLinkTo.LogicallyNever);
        }

        #endregion
    }
}
