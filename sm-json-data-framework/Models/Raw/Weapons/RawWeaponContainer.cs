﻿using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Weapons
{
    public class RawWeaponContainer
    {
        public IEnumerable<RawWeapon> Weapons { get; set; } = Enumerable.Empty<RawWeapon>();
    }
}
