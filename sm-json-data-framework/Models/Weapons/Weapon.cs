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

        /// <summary>
        /// Goes through all logical elements within this Weapon,
        /// attempting to initialize any property that is an object referenced by another property(which is its identifier).
        /// </summary>
        /// <param name="model">A SuperMetroidModel that contains global data</param>
        /// <returns>A sequence of strings describing references that could not be initialized properly.</returns>
        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(UseRequires.InitializeReferencedLogicalElementProperties(model, null));

            unhandled.AddRange(ShotRequires.InitializeReferencedLogicalElementProperties(model, null));

            return unhandled.Distinct();
        }
    }
}
