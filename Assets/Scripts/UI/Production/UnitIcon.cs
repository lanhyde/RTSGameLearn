using System.Collections;
using System.Collections.Generic;
using PromiseCode.RTS.Storing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PromiseCode.RTS.UI.Production
{
    public class UnitIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button button;
        [SerializeField] private Text countText;

        private UnitData unitDataTemplate;
        private ProductionsIconsPanel selfProductionIconsPanel;

        private RectTransform rectTransform;

        void Start() => rectTransform = GetComponent<RectTransform>();
        void Update() => Redraw();

        public void Redraw()
        {
            var selectedProduction = SelectProductionNumberPanel.selectedBuildingProduction;
            bool isBuilding = IsBuildingType();
            bool isInProductionQueue = selectedProduction.IsUnitOfTypeInQueue(unitDataTemplate);
            if (!selectedProduction)
            {
                return;
            }

            iconImage.sprite = unitDataTemplate.icon;
            if (selectedProduction.IsUnitOfTypeCurrentlyBuilding(unitDataTemplate))
            {
                fillImage.fillAmount = 1f - selectedProduction.GetBuildProgressPercents();
            }
            else if ((isBuilding && isInProductionQueue) || (!isBuilding && isInProductionQueue))
            {
                fillImage.fillAmount = 1f;
            }
            else
            {
                fillImage.fillAmount = 0f;
            }

            int unitsCount = selectedProduction.GetUnitsOfSpecificTypeInQueue(unitDataTemplate);
            countText.text = unitsCount > 0 ? unitsCount.ToString() : "";

            if (isBuilding && IsAnyBuildingInQueue(selectedProduction))
            {
                SetActive(IsCurrentBuildingInQueue(selectedProduction));
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnClick();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick();
            }
        }

        public void OnClick()
        {
            var selectedProduction = SelectProductionNumberPanel.selectedBuildingProduction;
            if (!selectedProduction)
            {
                return;
            }

            if (IsBuildingType())
            {
                bool isBuildingReady =
                    IsCurrentBuildingInQueue(selectedProduction) && selectedProduction.IsBuildingReady();
                if (IsAnyBuildingInQueue(selectedProduction))
                {
                    if (isBuildingReady)
                    {
                        GameController.instance.build.EnableBuildMode(unitDataTemplate.selfPrefab, () =>
                        {
                            selectedProduction.FinishBuilding();
                            selfProductionIconsPanel.Redraw();
                        });
                    }
                    return;
                }
            }
            selectedProduction.AddUnitToQueue(unitDataTemplate);
        }

        void OnRightClick()
        {
            var selectedProduction = SelectProductionNumberPanel.selectedBuildingProduction;
            if (!selectedProduction)
            {
                return;
            }
            selectedProduction.RemoveUnitFromQueue(unitDataTemplate, true);
            selfProductionIconsPanel.Redraw();
        }

        bool IsAnyBuildingInQueue(Units.Production production) => production.unitsQueue.Count > 0;

        bool IsCurrentBuildingInQueue(Units.Production production) =>
            production.unitsQueue.Count > 0 && production.unitsQueue[0] == unitDataTemplate;

        bool IsBuildingType() => SelectProductionTypePanel.selectedProductionCategory.isBuildings;
        public void SetActive(bool value) => button.interactable = value;

        public void SetupWithUnitData(ProductionsIconsPanel selfPanel, UnitData unitData)
        {
            selfProductionIconsPanel = selfPanel;
            unitDataTemplate = unitData;
            Redraw();
        }

        public void OnPointerEnter(PointerEventData pointerEventData) =>
            UIController.instance.productionHint.Show(unitDataTemplate,
                rectTransform.position + new Vector3(0, rectTransform.sizeDelta.y / 2f + 10));

        public void OnPointerExit(PointerEventData pointerEventData) => UIController.instance.productionHint.Hide();
    }

}
