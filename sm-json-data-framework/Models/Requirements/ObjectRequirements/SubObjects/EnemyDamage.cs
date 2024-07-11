using sm_json_data_framework.InGameStates;
using sm_json_data_framework.Models.Enemies;
using sm_json_data_framework.Models.Requirements.ObjectRequirements.Integers;
using sm_json_data_framework.Models.Rooms;
using sm_json_data_framework.Options;
using sm_json_data_framework.Rules;
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

        protected override ExecutionResult ExecutePossible(SuperMetroidModel model, ReadOnlyInGameState inGameState, int times = 1, int previousRoomCount = 0)
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

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions, SuperMetroidRules rules)
        {
            // Nothing to do here
        }

        protected override bool CalculateLogicallyNever(SuperMetroidRules rules)
        {
            // This could be impossible if the smallest possible damage is more than the max we can get
            int? maxEnergy = AppliedLogicalOptions.MaxPossibleAmount(ConsumableResourceEnum.Energy);
            // We can't check that if the max possible energy isn't provided
            if (maxEnergy == null)
            {
                return false;
            }
            return rules.CalculateBestCaseEnemyDamage(Attack, AppliedLogicalOptions.RemovedItems) * Hits >= maxEnergy;
        }

        protected override bool CalculateLogicallyAlways(SuperMetroidRules rules)
        {
            return rules.CalculateWorstCaseEnemyDamage(Attack, AppliedLogicalOptions.StartConditions.StartingInventory) * Hits <= 0;
        }

        protected override bool CalculateLogicallyFree(SuperMetroidRules rules)
        {
            return rules.CalculateWorstCaseEnemyDamage(Attack, AppliedLogicalOptions.StartConditions.StartingInventory) * Hits <= 0;
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
