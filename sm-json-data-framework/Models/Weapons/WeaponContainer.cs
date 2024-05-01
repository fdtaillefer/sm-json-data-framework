using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Weapons
{
    public class WeaponContainer
    {
        public IList<Weapon> Weapons { get; set; } = new List<Weapon>();
    }
}
