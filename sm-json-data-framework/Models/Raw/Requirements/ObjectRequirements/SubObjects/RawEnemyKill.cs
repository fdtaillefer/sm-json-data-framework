using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.SubObjects
{
    public class RawEnemyKill: AbstractRawObjectLogicalElement
    {
        public IEnumerable<IEnumerable<string>> Enemies { get; set; } = Enumerable.Empty<IEnumerable<string>>();

        public IEnumerable<string> ExplicitWeapons { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<string> ExcludedWeapons { get; set; } = Enumerable.Empty<string>();

        public IEnumerable<AmmoEnum> FarmableAmmo { get; set; } = Enumerable.Empty<AmmoEnum>();

        public override AbstractLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            if (knowledgeBase.ObjectLogicalElementTypes.TryGetValue(ObjectLogicalElementTypeEnum.EnemyKill, out Type type))
            {
                EnemyKill enemyKill = (EnemyKill)Activator.CreateInstance(type);
                enemyKill.GroupedEnemyNames = Enemies.Select(subGroup => new List<string>(subGroup));
                enemyKill.ExplicitWeaponNames = new List<string>(ExplicitWeapons);
                enemyKill.ExcludedWeaponNames = new List<string>(ExcludedWeapons);
                enemyKill.FarmableAmmo = new HashSet<AmmoEnum>(FarmableAmmo);
                return enemyKill;
            }
            else
            {
                throw new Exception($"Could not obtain a logical element type for EnemyKill.");
            }
        }
    }
}
