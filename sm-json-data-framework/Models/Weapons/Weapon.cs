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
        private UnfinalizedWeapon InnerElement { get; set; }

        public Weapon(UnfinalizedWeapon innerElement, Action<Weapon> mappingsInsertionCallback, ModelFinalizationMappings mappings)
            : base(innerElement, mappingsInsertionCallback)
        {
            InnerElement = innerElement;
            UseRequires = InnerElement.UseRequires.Finalize(mappings);
            ShotRequires = InnerElement.ShotRequires.Finalize(mappings);
            Categories = InnerElement.Categories.AsReadOnly();
        }

        /// <summary>
        /// A unique, arbitrary numerical ID that can identify this Weapon.
        /// </summary>
        public int Id { get { return InnerElement.Id; }  }

        /// <summary>
        /// A unique, human-legible name that can identify this Weapon.
        /// </summary>
        public string Name { get { return InnerElement.Name; } }

        /// <summary>
        /// The amount of damage one shot of this weapon does.
        /// </summary>
        public int Damage { get { return InnerElement.Damage; } }

        /// <summary>
        /// The amount of time (in frames) that must pass between two shots of this Weapon.
        /// </summary>
        public int CooldownFrames { get { return InnerElement.CooldownFrames; } }

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
        public bool Situational { get { return InnerElement.Situational; } }

        /// <summary>
        /// Indicates whether a shot of this Weapon hits all enemies in an enemy group.
        /// </summary>
        public bool HitsGroup { get { return InnerElement.HitsGroup; } }

        /// <summary>
        /// The set of all weapon categories this Weapon is part of.
        /// Can be used to infer some properties of this Weapon.
        /// </summary>
        public IReadOnlySet<WeaponCategoryEnum> Categories { get; }
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

        protected override bool ApplyLogicalOptionsEffects(ReadOnlyLogicalOptions logicalOptions)
        {
            UseRequires.ApplyLogicalOptions(logicalOptions);
            ShotRequires.ApplyLogicalOptions(logicalOptions);

            // This weapon is rendered useless if either using it altogether, or doing an individual shot, becomes impossible
            return UseRequires.UselessByLogicalOptions || ShotRequires.UselessByLogicalOptions;
        }

        public void InitializeProperties(SuperMetroidModel model)
        {
            // Nothing relevant to initialize
        }

        public IEnumerable<string> InitializeReferencedLogicalElementProperties(SuperMetroidModel model)
        {
            List<string> unhandled = new List<string>();

            unhandled.AddRange(UseRequires.InitializeReferencedLogicalElementProperties(model, null));

            unhandled.AddRange(ShotRequires.InitializeReferencedLogicalElementProperties(model, null));

            return unhandled.Distinct();
        }
    }
}
