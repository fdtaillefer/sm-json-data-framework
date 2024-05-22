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
        public EnemyDamage(UnfinalizedEnemyDamage sourceElement, Action<EnemyDamage> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Hits = sourceElement.Hits;
            Enemy = sourceElement.Enemy.Finalize(mappings);
            Attack = sourceElement.Attack.Finalize(mappings);
        }

        /// <summary>
        /// The enemy that this element's EnemyName references.
        /// </summary>
        public Enemy Enemy { get; }

        /// <summary>
        /// The enemy attack that this element's AttackName references.
        /// </summary>
        public EnemyAttack Attack { get; }

        /// <summary>
        /// The number of hits if the enemy attack Samus must take.
        /// </summary>
        public int Hits { get; }

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

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing to do here
        }

        protected override bool CalculateLogicallyNever()
        {
            // This could become impossible if the minimum damage it can inflict is more than the max energy we can ever get,
            // but max energy is not available in logical options.
            return false;
        }

        protected override bool CalculateLogicallyAlways()
        {
            // This could be always if it ends up being to 0 damage suitless, but that would be defined by the rules, which aren't available here
            return false;
        }

        protected override bool CalculateLogicallyFree()
        {
            // This could be free if it ends up being to 0 damage suitless, but that would be defined by the rules, which aren't available here
            return false;
        }
    }

    public class UnfinalizedEnemyDamage : AbstractUnfinalizedObjectLogicalElement<UnfinalizedEnemyDamage, EnemyDamage>
    {
        public string EnemyName { get; set; }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The enemy that this element's EnemyName references. </para>
        /// </summary>
        public UnfinalizedEnemy Enemy { get; set; }

        public string AttackName { get; set; }

        /// <summary>
        /// <para>Only available after a call to <see cref="InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel, UnfinalizedRoom)"/>.</para>
        /// <para>The enemy attack that this element's AttackName references. </para>
        /// </summary>
        public UnfinalizedEnemyAttack Attack { get; set; }

        public int Hits { get; set; }

        protected override EnemyDamage CreateFinalizedElement(UnfinalizedEnemyDamage sourceElement, Action<EnemyDamage> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new EnemyDamage(sourceElement, mappingsInsertionCallback, mappings);
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
