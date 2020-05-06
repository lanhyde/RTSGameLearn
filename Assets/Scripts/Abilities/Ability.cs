using PromiseCode.RTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Abilities
{
    /// <summary>
    /// This class is base for any unity ability. To create new ability - derive from it in your new classes. To add this ability to unit - add your new component to the unit prefab.
    /// </summary>
    public class Ability : MonoBehaviour
    {
        public Unit unitOwner { get; protected set; }
        public AbilitiesModule unitOwnerAbilities { get; protected set; }
        public bool isActive { get; set;  }

        [SerializeField] AbilityData data;
        public AbilityData Data => data;
        // TODO: move from here
        UI.UnitAbilities abilitiesListUI;

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
            // TODO: move from here
            abilitiesListUI = FindObjectOfType<UI.UnitAbilities>();
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
            // TODO: move from here.
            abilitiesListUI.Redraw();
        }

        public virtual void CustomAction() { }

        public bool CanUse() => true;
    }

}
