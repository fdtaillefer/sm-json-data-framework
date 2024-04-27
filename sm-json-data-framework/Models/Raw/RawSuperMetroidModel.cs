using sm_json_data_framework.Models.Connections;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.GameFlags;
using sm_json_data_framework.Models.Helpers;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Items;
using sm_json_data_framework.Models.Rooms.Nodes;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Models.Techs;
using sm_json_data_framework.Models.Weapons;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sm_json_data_framework.Models.Raw.Items;
using sm_json_data_framework.Models.Raw.Helpers;
using sm_json_data_framework.Models.Raw.Techs;
using sm_json_data_framework.Models.Raw.Rooms;
using sm_json_data_framework.Models.Raw.Connections;
using sm_json_data_framework.Models.Raw.Weapons;
using sm_json_data_framework.Models.Raw.Enemies;

namespace sm_json_data_framework.Models.Raw
{
    /// <summary>
    /// A model that can hold all the raw models obtained from reading all the files of the json model.
    /// </summary>
    public class RawSuperMetroidModel
    {
        public RawItemContainer ItemContainer { get; set; }

        public RawHelperContainer HelperContainer { get; set; }

        public RawTechContainer TechContainer { get; set; }

        public RawRoomContainer RoomContainer { get; set; }

        public RawConnectionContainer ConnectionContainer { get; set; }

        public RawWeaponContainer WeaponContainer { get; set; }

        public RawEnemyContainer EnemyContainer { get; set; }

        public RawEnemyContainer BossContainer { get; set; }
    }
}
