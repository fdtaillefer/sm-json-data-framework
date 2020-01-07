using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_parser.Models.Weapons
{
    public class WeaponContainer
    {
        public IEnumerable<Weapon> Weapons { get; set; } = Enumerable.Empty<Weapon>();
    }
}
