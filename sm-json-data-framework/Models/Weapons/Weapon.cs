using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Weapons
{
    public class Weapon
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public int Damage { get; set; }

        public int CooldownFrames { get; set; }

        public LogicalRequirements UseRequires { get; set; } = new LogicalRequirements();

        public LogicalRequirements ShotRequires { get; set; } = new LogicalRequirements();

        public bool Situational { get; set; }

        public bool HitsGroup { get; set; }

        public IEnumerable<WeaponCategoryEnum> Categories { get; set; } = Enumerable.Empty<WeaponCategoryEnum>();

        // STITCHME Note?
    }
}
