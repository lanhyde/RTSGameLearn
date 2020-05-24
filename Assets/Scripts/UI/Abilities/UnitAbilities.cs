using PromiseCode.RTS.Abilities;
using PromiseCode.RTS.Controls;
using PromiseCode.RTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.UI.Abilities
{
    public class UnitAbilities : MonoBehaviour
    {
        [SerializeField] GameObject selfObject;
        [SerializeField] Transform abilitiesPanel;

        readonly List<GameObject> drawnIcons = new List<GameObject>();
        Unit selectedUnit;

        void Start()
        {
            Selection.onUnitsListSelected += OnUnitsListSelected;
            Selection.unitSelected += Show;
            Selection.selectionCleared += Hide;

            Hide();
        }

        void OnUnitsListSelected(List<Unit> units)
        {
            // TODO: temporary impl.
            units.ForEach(unit =>
            {
                var carryModule = unit.GetModule<CarryModule>();

                if(!carryModule)
                {
                    Hide();
                    return;
                }
            });
            if(IsNeedToBeShown(units[0]))
            {
                Show(units[0]);
            }
        }

        public bool IsNeedToBeShown(Unit forUnit)
        {
            var abilitiesModule = forUnit.GetModule<AbilitiesModule>();
            bool needToShow = true;

            if(!abilitiesModule || abilitiesModule.abilities.Count == 0)
            {
                needToShow = false;
            }
            if(forUnit.data.canCarryUnitsCount > 0)
            {
                needToShow = true;
            }
            return needToShow;
        }

        public void Show(Unit forUnit)
        {
            if(!IsNeedToBeShown(forUnit))
            {
                return;
            }
            selectedUnit = forUnit;
            selfObject.SetActive(true);

            Redraw();
        }
        public void Redraw()
        {
            var abilitiesModule = selectedUnit.GetModule<AbilitiesModule>();

            ClearDrawn();

            var iconTemplate = GameController.instance.MainStorage.unitAbilityIcon;

            abilitiesModule.abilities.ForEach(ability =>
            {
                GameObject iconObject = Instantiate(iconTemplate, abilitiesPanel);
                var spawnedIconComponent = iconObject.GetComponent<AbilityIcon>();
                spawnedIconComponent.Setup(ability);

                drawnIcons.Add(iconObject);
            });
        }

        void ClearDrawn()
        {
            for(int i = 0; i < drawnIcons.Count; ++i)
            {
                if(drawnIcons[i] != null)
                {
                    Destroy(drawnIcons[i]);
                }
            }
            drawnIcons.Clear();
        }

        public void Hide()
        {
            selfObject.SetActive(false);
        }

        void OnDestroy()
        {
            Selection.onUnitsListSelected -= OnUnitsListSelected;
            Selection.unitSelected -= Show;
            Selection.selectionCleared -= Hide;
        }
    }

}
