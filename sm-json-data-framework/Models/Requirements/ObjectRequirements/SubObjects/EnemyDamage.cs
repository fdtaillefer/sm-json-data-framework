using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.InGameStates;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Options;
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
    public class EnemyDamage : AbstractObjectLogicalElement<UnfinalizedEnemyDamage, EnemyDamage>
    {
        private UnfinalizedEnemyDamage InnerElement { get; set; }
        public EnemyDamage(UnfinalizedEnemyDamage innerElement, Action<EnemyDamage> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            Enemy = innerElement.Enemy.Finalize(mappings);
            Attack = innerElement.Attack.Finalize(mappings);
        }

        public string EnemyName { get { return InnerElement.EnemyName; } }

        /// <summary>
        /// The enemy that this element's EnemyName references.
        /// </summary>
        public Enemy Enemy { get; }

        public string AttackName { get { return InnerElement.AttackName; } }

        /// <summary>
        /// The enemy attack that this element's AttackName references.
        /// </summary>
        public EnemyAttack Attack { get; }

        /// <summary>
        /// The number of hits if the enemy attack Samus must take.
        /// </summary>
        public int Hits { get { return InnerElement.Hits; } }

        public override bool IsNever()
        {
            return false;
        }

        protected override ExecutionResult ExecuteUseful(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
        {
            int damage = model.Rules.CalculateEnemyDamage(inGameState, Attack) * Hits * times;

            if (inGameState.IsResourceAvailable(ConsumableResourceEnum.Energy, damage))
            {
                InGameState resultingState = inGameState.Clone();
                resultingState.ApplyConsumeResource(ConsumableResourceEnum.Energy, damage);
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

    public class UnfinalizedEnemyDamage : AbstractUnfinalizedObjectLogicalElement<UnfinalizedEnemyDamage, EnemyDamage>
    {
        [JsonPropertyName("enemy")]
        public string EnemyName { get; set; }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The enemy that this element's EnemyName references. </para>
        /// </summary>
        [JsonIgnore]
        public UnfinalizedEnemy Enemy { get; set; }

        [JsonPropertyName("type")]
        public string AttackName { get; set; }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The enemy attack that this element's AttackName references. </para>
        /// </summary>
        [JsonIgnore]
        public UnfinalizedEnemyAttack Attack { get; set; }

        public int Hits { get; set; }

        protected override EnemyDamage CreateFinalizedElement(UnfinalizedEnemyDamage sourceElement, Action<EnemyDamage> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new EnemyDamage(sourceElement, mappingsInsertionCallback, mappings);
        }

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing in logical options can alter this
            return false;
        }

        public override bool IsNever()
        {
            return false;
        }

        public override IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model, UnfinalizedRoom room)
        {
            if(model.Enemies.TryGetValue(EnemyName, out UnfinalizedEnemy enemy))
            {
                Enemy = enemy;
                if(enemy.Attacks.TryGetValue(AttackName, out UnfinalizedEnemyAttack attack))
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
