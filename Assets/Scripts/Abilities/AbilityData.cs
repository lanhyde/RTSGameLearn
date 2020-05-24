using PromiseCode.RTS.Storing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Abilities
{
    [CreateAssetMenu(fileName = "AbilityData", menuName ="RTS/Ability Data")]
    public class AbilityData : ScriptableObject
    {
        [Tooltip("Ability ID. Used only by code. No space is recommended")]
        public string textId;
        [Tooltip("Ability name. Shown on game UI")]
        public string abilityName;
        [Tooltip("Ability icon image. shown on game UI")]
        public Sprite icon;
        [Sound] public AudioClip soundToPlayOnUse;
        [Tooltip("Is ability can be used by default? If false, it can be enabled only from other code or ability (upgrades for example)")]
        public bool isActiveByDefault = true;
        [Header("Custom weapon ability")]
        [Tooltip("Attack distance of this weapon. If set to 0, it will be default unit attack distance. Other for next same parameters.")]
        public float newAttackRange;
        public float newAttackReloadTime;
        public float newAttackDamage;
        [Tooltip("Put here second weapon change ability, which should became active after using this - to change weapon to previous")]
        public AbilityData customWeaponAbilityToEnable;
    }

}