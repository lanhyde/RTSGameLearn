using PromiseCode.RTS.Abilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PromiseCode.RTS.UI.Abilities
{
    public class AbilityIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Image iconImage;
        [SerializeField] AbilityData customAbilityData;

        Button button;
        Ability selfAbility;
        RectTransform rectTransform;

        void Start()
        {
            rectTransform = GetComponent<RectTransform>();    
        }

        void Awake()
        {
            button = GetComponent<Button>();    
        }

        public void Setup(Ability ability)
        {
            iconImage.sprite = ability.Data.icon;

            selfAbility = ability;

            if(!button)
            {
                button = GetComponent<Button>();
            }

            button.interactable = ability.isActive;
        }

        public void OnClick()
        {
            selfAbility?.DoAction();
        }

        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            var data = customAbilityData;
            if(selfAbility)
            {
                data = selfAbility.Data;
            }
            UIController.instance.productionHint.ShowForAbility(data, rectTransform.position + new Vector3(0, rectTransform.sizeDelta.y / 2 + 10));
        }

        public void OnPointerExit(PointerEventData pointerEventData)
        {
            UIController.instance.productionHint.Hide();
        }
    }

}