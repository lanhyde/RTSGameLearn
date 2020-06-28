using System.Collections;
using System.Collections.Generic;
using PromiseCode.RTS.Storing;
using UnityEngine;

namespace PromiseCode.RTS.UI.Production
{
    public class SelectProductionNumberPanel : MonoBehaviour
    {
        public static int selectedBuildingNumber { get; protected set; }
        public static Units.Production selectedBuildingProduction { get; protected set; }
        public static event SelectedProductionChangedAction selectedProductionChanged;
        public delegate void SelectedProductionChangedAction(Units.Production selectedProdution);

        [SerializeField] private RectTransform iconsPanel;
        [SerializeField] List<BuildingNumberButton> buildNumberIcons = new List<BuildingNumberButton>();

        private void Awake()
        {
            SelectProductionTypePanel.productionCategoryChanged += OnProductionCategoryChanged;
        }

        void OnProductionCategoryChanged(ProductionCategory newCategory)
        {
            SelectBuildingWithNumber(0);
        }

        public void SelectBuildingWithNumber(int number)
        {
            List<Units.Production> playerProductions =
                GetPlayerProductionsByCategory(SelectProductionTypePanel.selectedProductionCategory);
            if (number >= playerProductions.Count)
                return;

            selectedBuildingNumber= number;
            selectedBuildingProduction =
                GetPlayerProductionByTypeAndNumber(SelectProductionTypePanel.selectedProductionCategory,
                    selectedBuildingNumber);
            Redraw(SelectProductionTypePanel.selectedProductionCategory);

            selectedProductionChanged?.Invoke(selectedBuildingProduction);
        }

        void Redraw(ProductionCategory newCategory)
        {
            List<Units.Production> playerProductions = GetPlayerProductionsByCategory(newCategory);
            for (int i = 0; i < buildNumberIcons.Count; ++i)
            {
                var buildingNumberButton = buildNumberIcons[i];

                buildingNumberButton.SetupBuildingId(i);
                buildingNumberButton.SetupWithController(this);

                if (i < playerProductions.Count)
                {
                    buildingNumberButton.SetEnabled();
                }
                else
                {
                    buildingNumberButton.SetDisabled();
                }

                if (i == selectedBuildingNumber)
                {
                    buildingNumberButton.SetActive();
                }
                else
                {
                    buildingNumberButton.SetUnactive();
                }
            }
        }

        Units.Production GetPlayerProductionByTypeAndNumber(ProductionCategory category, int number)
        {
            List<Units.Production> playerProductions = GetPlayerProductionsByCategory(category);
            return playerProductions.Count > 0 ? playerProductions[number] : null;
        }

        List<Units.Production> GetPlayerProductionsByCategory(ProductionCategory category)
        {
            Player localPlayer = GameController.instance.playersController.playersInGame[Player.localPlayerId];
            List<Units.Production> playerProductions = localPlayer.GetProductionBuildingsByCategory(category);

            return playerProductions;
        }

        void OnDestroy()
        {
            SelectProductionTypePanel.productionCategoryChanged -= OnProductionCategoryChanged;
        }
    }

}
