using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace sm_json_data_framework.Models.Requirements.ObjectRequirements.SubObjects
{
    /// <summary>
    /// A logical element which requires Samus to take damage from an enemy.
    /// </summary>
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

        public override ExecutionResult Execute(SuperMetroidModel model, InGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int damage = model.Rules.CalculateEnemyDamage(inGameState, Attack) * Hits * times;

            if (inGameState.IsResourceAvailable(model, ConsumableResourceEnum.ENERGY, damage))
            {
                InGameState resultingState = inGameState.Clone();
                resultingState.ApplyConsumeResource(model, ConsumableResourceEnum.ENERGY, damage);
                ExecutionResult result = new ExecutionResult(resultingState);
                result.AddDamageReducingItemsInvolved(model.Rules.GetEnemyDamageReducingItems(model, inGameState, Attack));
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}
