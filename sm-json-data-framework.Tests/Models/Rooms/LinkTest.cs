﻿using sm_json_data_framework.Models;
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
    public class LinkTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            Link link = Model.Rooms["Landing Site"].Links[5];
            Assert.Same(Model.Rooms["Landing Site"].Nodes[5], link.FromNode);
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
            LogicalOptions logicalOptions = new LogicalOptions()
                .RegisterRemovedItem("Gravity")
                .RegisterDisabledTech("canSuitlessMaridia");

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Link noDestinationsLink = ModelWithOptions.Rooms["Crab Shaft"].Links[2];
            Assert.False(noDestinationsLink.LogicallyRelevant);

            Link possibleLink = ModelWithOptions.Rooms["Landing Site"].Links[1];
            Assert.True(possibleLink.LogicallyRelevant);
        }

        #endregion
    }
}
