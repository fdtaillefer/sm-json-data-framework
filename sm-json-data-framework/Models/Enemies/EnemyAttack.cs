using sm_json_data_framework.Models.Raw.Enemies;
using sm_json_data_framework.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace sm_json_data_framework.Models.Enemies
{
    /// <summary>
    /// Describes a way an enemy can damage Samus.
    /// </summary>
    public class EnemyAttack : AbstractModelElement<UnfinalizedEnemyAttack, EnemyAttack>
    {
        public EnemyAttack(UnfinalizedEnemyAttack sourceElement, Action<EnemyAttack> mappingsInsertionCallback)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Name = sourceElement.Name;
            BaseDamage = sourceElement.BaseDamage;
            AffectedByVaria = sourceElement.AffectedByVaria;
            AffectedByGravity = sourceElement.AffectedByGravity;
        }

        /// <summary>
        /// The name of this enemy attack.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The amount of damage this attack does when unmitigated.
        /// </summary>
        public int BaseDamage { get; }

        /// <summary>
        /// Indicates whether this attack is mitigated by Varia suit.
        /// </summary>
        public bool AffectedByVaria { get; }

        /// <summary>
        /// Indicates whether this attack is mitigated by Gravity suit.
        /// </summary>
        public bool AffectedByGravity { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            // Nothing to do here
        }

        public override bool CalculateLogicallyRelevant()
        {
            // An enemy attack always has some relevance
            return true;
        }
    }

    public class UnfinalizedEnemyAttack: AbstractUnfinalizedModelElement<UnfinalizedEnemyAttack, EnemyAttack>
    {
        public string Name { get; set; }

        public int BaseDamage { get; set; }

        public bool AffectedByVaria { get; set; } = true;

        public bool AffectedByGravity { get; set; } = true;

        public UnfinalizedEnemyAttack()
        {

        }

        public UnfinalizedEnemyAttack(RawEnemyAttack rawAttack)
        {
            Name = rawAttack.Name;
            BaseDamage = rawAttack.BaseDamage;
            AffectedByVaria = rawAttack.AffectedByVaria;
            AffectedByGravity = rawAttack.AffectedByGravity;
        }

        protected override EnemyAttack CreateFinalizedElement(UnfinalizedEnemyAttack sourceElement, Action<EnemyAttack> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new EnemyAttack(sourceElement, mappingsInsertionCallback);
        }
    }
}
