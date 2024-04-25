using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects;
using sm_json_data_framework.Models.Requirements.ObjectRequirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace sm_json_data_framework.Models.Raw.Requirements.ObjectRequirements.SubObjects
{
    public class RawEnemyDamage: AbstractRawObjectLogicalElement
    {
        public string Enemy { get; set; }

        public string Type { get; set; }

        public int Hits { get; set; }

        public override AbstractLogicalElement ToLogicalElement(LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            if (knowledgeBase.ObjectLogicalElementTypes.TryGetValue(ObjectLogicalElementTypeEnum.EnemyDamage, out Type type))
            {
                EnemyDamage enemyDamage = (EnemyDamage)Activator.CreateInstance(type);
                enemyDamage.EnemyName = Enemy;
                enemyDamage.AttackName = Type;
                enemyDamage.Hits = Hits;
                return enemyDamage;
            }
            else
            {
                throw new Exception($"Could not obtain a logical element type for EnemyDamage.");
            }
        }
    }
}
