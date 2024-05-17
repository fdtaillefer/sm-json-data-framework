using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.SubObjects
{
    public class RawEnemyKill: AbstractRawObjectLogicalElement
    {
        public IList<IList<string>> Enemies { get; set; } = new List<IList<string>>();

        public ISet<string> ExplicitWeapons { get; set; } = new HashSet<string>();

        public ISet<string> ExcludedWeapons { get; set; } = new HashSet<string>();

        public ISet<AmmoEnum> FarmableAmmo { get; set; } = new HashSet<AmmoEnum>();

        public override IUnfinalizedLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            if (knowledgeBase.ObjectLogicalElementTypes.TryGetValue(ObjectLogicalElementTypeEnum.EnemyKill, out Type type))
            {
                UnfinalizedEnemyKill enemyKill = (UnfinalizedEnemyKill)Activator.CreateInstance(type);
                enemyKill.GroupedEnemyNames = Enemies.Select(subGroup => (IList<string>) new List<string>(subGroup)).ToList();
                enemyKill.ExplicitWeaponNames = new HashSet<string>(ExplicitWeapons);
                enemyKill.ExcludedWeaponNames = new HashSet<string>(ExcludedWeapons);
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
