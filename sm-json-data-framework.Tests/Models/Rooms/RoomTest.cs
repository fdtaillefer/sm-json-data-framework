﻿using sm_json_data_framework.Models;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using sm_json_data_framework.Tests.TestTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms;

namespace sm_json_data_framework.Tests.Models.Rooms
{
    public class RoomTest
    {
        private static SuperMetroidModel Model = StaticTestObjects.UnmodifiableModel;
        private static SuperMetroidModel ModelWithOptions = StaticTestObjects.UnfinalizedModel.Finalize();

        #region Tests for construction from unfinalized model

        [Fact]
        public void CtorFromUnfinalized_SetsPropertiesCorrectly()
        {
            // Given/when standard model creation

            // Expect
            Room room = Model.Rooms["Climb"];
            Assert.Equal(11, room.Id);
            Assert.Equal("Climb", room.Name);
            Assert.Equal("Crateria", room.Area);
            Assert.Equal("Central", room.Subarea);
            Assert.True(room.Playable);
            Assert.Equal("0x796BA", room.RoomAddress);
            Assert.Equal(1, room.RoomEnvironments.Count);
            Assert.False(room.RoomEnvironments.First().Heated);

            Assert.Equal(6, room.Nodes.Count);
            Assert.Contains(1, room.Nodes.Keys);
            Assert.Contains(2, room.Nodes.Keys);
            Assert.Contains(3, room.Nodes.Keys);
            Assert.Contains(4, room.Nodes.Keys);
            Assert.Contains(5, room.Nodes.Keys);
            Assert.Contains(6, room.Nodes.Keys);

            Assert.Equal(6, room.Links.Count);
            Assert.Contains(1, room.Links.Keys);
            Assert.Contains(2, room.Links.Keys);
            Assert.Contains(3, room.Links.Keys);
            Assert.Contains(4, room.Links.Keys);
            Assert.Contains(5, room.Links.Keys);
            Assert.Contains(6, room.Links.Keys);

            Assert.Equal(1, room.Obstacles.Count);
            Assert.Contains("A", room.Obstacles.Keys);

            Assert.Equal(2, room.Enemies.Count);
            Assert.Contains("e1", room.Enemies.Keys);
            Assert.Contains("e2", room.Enemies.Keys);

            Room nonPlayableRoom = Model.Rooms["Toilet Bowl"];
            Assert.False(nonPlayableRoom.Playable);
        }

        #endregion

        #region Tests for GetLinkBetween()

        [Fact]
        public void GetLinkBetween_LinkExists_ReturnsLink()
        {
            // Given
            Room room = Model.Rooms["Climb"];

            // When
            LinkTo result = room.GetLinkBetween(2, 6);

            // Expect
            Assert.Same(room.Links[2].To[6], result);
        }

        [Fact]
        public void GetLinkBetween_LinkDoesntExist_ReturnsNull()
        {
            // Given
            Room room = Model.Rooms["Climb"];

            // When
            LinkTo result = room.GetLinkBetween(2, 5);

            // Expect
            Assert.Null(result);
        }

        #endregion
        // GetLinkBetween()

        #region Tests for ApplyLogicalOptions() that check applied logical properties

        [Fact]
        public void ApplyLogicalOptions_SetsLogicalProperties()
        {
            // Given
            LogicalOptions logicalOptions = new LogicalOptions();
            logicalOptions.InternalAvailableResourceInventory = new ResourceItemInventory(ResourceCount.CreateVanillaBaseResourceMaximums())
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Missile"], 46)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["Super"], 10)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ETank"], 14)
                .ApplyAddExpansionItem((ExpansionItem)ModelWithOptions.Items["ReserveTank"], 4);

            // When
            ModelWithOptions.ApplyLogicalOptions(logicalOptions);

            // Expect
            Room unreachableRoom = ModelWithOptions.Rooms["Norfair Map Room"];
            // Room accessibility is not considered in-scope for logical relevance
            Assert.True(unreachableRoom.LogicallyRelevant);

            Room reachableRoom = ModelWithOptions.Rooms["Landing Site"];
            Assert.True(reachableRoom.LogicallyRelevant);
        }

        #endregion
    }
}
