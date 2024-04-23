using sm_json_data_framework.Models.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Weapons
{
    public class Weapon : InitializablePostDeserializeOutOfRoom
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

        public void InitializeForeignProperties(SuperMetroidModel model)
        {
            // Nothing relevant to initialize
        }

        public void InitializeOtherProperties(SuperMetroidModel model)
        {
            // Nothing relevant to initialize
        }

        public bool CleanUpUselessValues(SuperMetroidModel model)
        {
            // Nothing relevant to cleanup

            // Technically the requires to use or shoot the weapon could be never, but in fact that's not expected.
            // Besides, an unusable weapon is probably still useful to reference that the weapon exists.
            return true;
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(UseRequires.InitializeReferencedLogicalElementProperties(model, null));

            unhandled.AddRange(ShotRequires.InitializeReferencedLogicalElementProperties(model, null));

            return unhandled.Distinct();
        }
    }
}
