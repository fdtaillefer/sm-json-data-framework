﻿using sm_json_data_framework.Models.Enemies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Enemies
{
    public class RawEnemyContainer
    {
        public IList<RawEnemy> Enemies { get; set; } = new List<RawEnemy>();
    }
}
