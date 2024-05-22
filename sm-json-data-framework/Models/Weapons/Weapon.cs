using sm_json_data_framework.Models.Raw.Weapons;
using sm_json_data_framework.Models.Requirements;
using sm_json_data_framework.Options;
using sm_json_data_framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sm_json_data_framework.Models.Weapons
{
    /// <summary>
    /// Represents a way Samus can cause damage to enemies.
    /// </summary>
    public class Weapon : AbstractModelElement<UnfinalizedWeapon, Weapon>
    {
        public Weapon(UnfinalizedWeapon sourceElement, Action<Weapon> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(sourceElement, mappingsInsertionCallback)
        {
            Id = sourceElement.Id;
            Name = sourceElement.Name;
            Damage = sourceElement.Damage;
            CooldownFrames = sourceElement.CooldownFrames;
            Situational = sourceElement.Situational;
            HitsGroup = sourceElement.HitsGroup;
            UseRequires = sourceElement.UseRequires.Finalize(mappings);
            ShotRequires = sourceElement.ShotRequires.Finalize(mappings);
            Categories = sourceElement.Categories.AsReadOnly();
        }

        /// <summary>
        /// A unique, arbitrary numerical ID that can identify this Weapon.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// A unique, human-legible name that can identify this Weapon.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The amount of damage one shot of this weapon does.
        /// </summary>
        public int Damage { get; }

        /// <summary>
        /// The amount of time (in frames) that must pass between two shots of this Weapon.
        /// </summary>
        public int CooldownFrames { get; }

        /// <summary>
        /// Logical requirements to fulfill in order to be able to ever use this Weapon.
        /// </summary>
        public LogicalRequirements UseRequires { get; }

        /// <summary>
        /// Logical requirements to fulfill on top of <see cref="UseRequires"/> for every shot of this WEapon that is fired.
        /// This is generally ammo costs.
        /// </summary>
        public LogicalRequirements ShotRequires { get; }

        /// <summary>
        /// Indicates whether this is a weapon that can be use only when explicitly referenced by logic.
        /// </summary>
        public bool Situational { get; }

        /// <summary>
        /// Indicates whether a shot of this Weapon hits all enemies in an enemy group.
        /// </summary>
        public bool HitsGroup { get; }

        /// <summary>
        /// The set of all weapon categories this Weapon is part of.
        /// Can be used to infer some properties of this Weapon.
        /// </summary>
        public IReadOnlySet<WeaponCategoryEnum> Categories { get; }

        protected override void PropagateLogicalOptions(ReadOnlyLogicalOptions logicalOptions)
        {
            UseRequires.ApplyLogicalOptions(logicalOptions);
            ShotRequires.ApplyLogicalOptions(logicalOptions);
        }

        protected override void UpdateLogicalProperties()
        {
            base.UpdateLogicalProperties();
            LogicallyNever = CalculateLogicallyNever();
            LogicallyAlways = CalculateLogicallyAlways();
            LogicallyFree = CalculateLogicallyFree();
        }

        public override bool CalculateLogicallyRelevant()
        {
            // A weapon that can never be used may as well not exist
            return !CalculateLogicallyNever();
        }

        /// <summary>
        /// If true, then this weapon is impossible to use given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyNever { get; private set; }

        protected bool CalculateLogicallyNever()
        {
            // Weapon is impossible to use if either using it altogether, or doing an individual shot, becomes impossible
            return UseRequires.LogicallyNever || ShotRequires.LogicallyNever;
        }

        /// <summary>
        /// If true, then this weapon is always possible to use given the current logical options, regardless of in-game state.
        /// </summary>
        public bool LogicallyAlways { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyAlways"/> should currently be.
        /// </summary>
        /// <returns></returns>
        protected bool CalculateLogicallyAlways()
        {
            // Weapon is always possible to use if using it altogether, and doing an individual shot, are both always possible
            return UseRequires.LogicallyAlways && ShotRequires.LogicallyAlways;
        }

        /// <summary>
        /// If true, not only can this weapon always be used given the current logical options, regardless of in-game state,
        /// but that fulfillment is also guaranteed to cost no resources.
        /// </summary>
        public bool LogicallyFree { get; private set; }

        /// <summary>
        /// Calculates what the value of <see cref="LogicallyFree"/> should currently be.
        /// </summary>
        /// <returns></returns>
        protected bool CalculateLogicallyFree()
        {
            // Weapon is always free to use if using it altogether, and doing an individual shot, are both always free
            return UseRequires.LogicallyFree && ShotRequires.LogicallyFree;
        }
    }

    public class UnfinalizedWeapon : AbstractUnfinalizedModelElement<UnfinalizedWeapon, Weapon>, InitializablePostDeserializeOutOfRoom
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Damage { get; set; }

        public int CooldownFrames { get; set; }

        public UnfinalizedLogicalRequirements UseRequires { get; set; } = new UnfinalizedLogicalRequirements();

        public UnfinalizedLogicalRequirements ShotRequires { get; set; } = new UnfinalizedLogicalRequirements();

        public bool Situational { get; set; }

        public bool HitsGroup { get; set; }

        public ISet<WeaponCategoryEnum> Categories { get; set; } = new HashSet<WeaponCategoryEnum>();

        public UnfinalizedWeapon()
        {

        }

        public UnfinalizedWeapon(RawWeapon rawWeapon, LogicalElementCreationKnowledgeBase knowledgeBase)
        {
            Id = rawWeapon.Id;
            Name = rawWeapon.Name;
            Damage = rawWeapon.Damage;
            CooldownFrames = rawWeapon.CooldownFrames;
            UseRequires = rawWeapon.UseRequires.ToLogicalRequirements(knowledgeBase);
            ShotRequires = rawWeapon.ShotRequires.ToLogicalRequirements(knowledgeBase);
            Situational = rawWeapon.Situational;
            HitsGroup = rawWeapon.HitsGroup;
            Categories = new HashSet<WeaponCategoryEnum>(rawWeapon.Categories);
        }

        protected override Weapon CreateFinalizedElement(UnfinalizedWeapon sourceElement, Action<Weapon> mappingsInsertionCallback, ModelFinalizationMappings mappings)
        {
            return new Weapon(sourceElement, mappingsInsertionCallback, mappings);
        }

        public void InitializeProperties(UnfinalizedSuperMetroidModel model)
        {
            // Nothing relevant to initialize
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(UnfinalizedSuperMetroidModel model)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(UseRequires.InitializeReferencedLogicalElementProperties(model, null));

            unhandled.AddRange(ShotRequires.InitializeReferencedLogicalElementProperties(model, null));

            return unhandled.Distinct();
        }
    }
}
