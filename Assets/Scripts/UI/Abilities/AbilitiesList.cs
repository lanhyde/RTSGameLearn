using PromiseCode.RTS.Abilities;
using PromiseCode.RTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.UI.Abilities
{
    public class AbilitiesList : MonoBehaviour
    {
        public Unit unitOwner { get; protected set; }
        public AbilitiesModule unitOwnerAbilities { get; protected set; }
        public bool isActive { get; set; }
        [SerializeField] AbilityData data;
        public AbilityData Data => data;
        UnitAbilities abilitiesListUI;

        void Awake()
        {
            unitOwner = GetComponent<Unit>();
            
            if(!data)
            {
                enabled = false;
                return;
            }
            isActive = data.isActiveByDefault;
        }

        void Start()
        {
            unitOwnerAbilities = unitOwner.GetModule<AbilitiesModule>();
            
            if(!unitOwnerAbilities)
            {
                unitOwnerAbilities = gameObject.AddComponent<AbilitiesModule>();
            }
            abilitiesListUI = FindObjectOfType<UnitAbilities>();
        }

        public void DoAction()
        {
            if(!CanUse())
            {
                return;
            }
            CustomAction();

            if(data.soundToPlayOnUse)
            {
                unitOwner.PlayCustomSound(data.soundToPlayOnUse);
            }
            abilitiesListUI.Redraw();
        }

        public virtual void CustomAction() { }
        public virtual bool CanUse() { return true; }
    }
}