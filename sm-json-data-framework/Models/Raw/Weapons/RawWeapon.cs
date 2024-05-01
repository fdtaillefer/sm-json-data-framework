using sm_json_data_framework.Models.Raw.Requirements;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Weapons
{
    public class RawWeapon
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Damage { get; set; }

        public int CooldownFrames { get; set; }

        public RawLogicalRequirements UseRequires { get; set; } = new RawLogicalRequirements();

        public RawLogicalRequirements ShotRequires { get; set; } = new RawLogicalRequirements();

        public bool Situational { get; set; }

        public bool HitsGroup { get; set; }

        public ISet<WeaponCategoryEnum> Categories { get; set; } = new HashSet<WeaponCategoryEnum>();
    }
}
