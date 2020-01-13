using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    public class EnemyDamage : AbstractObjectLogicalElement
    {
        [JsonPropertyName("enemy")]
        public string EnemyName { get; set; }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The enemy that this element's EnemyName references. </para>
        /// </summary>
        [JsonIgnore]
        public Enemy Enemy { get; set; }

        [JsonPropertyName("type")]
        public string AttackName { get; set; }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(SuperMetroidModel, Room)"/>.</para>
        /// <para>The enemy attack that this element's AttackName references. </para>
        /// </summary>
        [JsonIgnore]
        public EnemyAttack Attack { get; set; }

        public int Hits { get; set; }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model, Room room)
        {
            if(model.Enemies.TryGetValue(EnemyName, out Enemy enemy))
            {
                Enemy = enemy;
                if(enemy.Attacks.TryGetValue(AttackName, out EnemyAttack attack))
                {
                    Attack = attack;
                }
                else
                {
                    return new[] { $"Attack {AttackName} of Enemy {EnemyName}" };
                }
            }
            else
            {
                return new[] { $"Enemy {EnemyName}" };
            }

            return Enumerable.Empty<string>();
        }
    }
}
