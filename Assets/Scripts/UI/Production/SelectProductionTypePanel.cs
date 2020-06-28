using PromiseCode.RTS.Storing;
using System.Collections;
using System.Collections.Generic;
using PromiseCode.RTS.Controls;
using UnityEngine;

namespace PromiseCode.RTS.UI.Production
{
    public class SelectProductionTypePanel : MonoBehaviour
    {
        public static ProductionCategory selectedProductionCategory { get; protected set; }
        
        public static event ProductionCategoryChangedAction productionCategoryChanged;
        public delegate void ProductionCategoryChangedAction(ProductionCategory productionCategory);

        [SerializeField] private RectTransform productionIconsPanel;
        readonly List<ProductionTypeButton> productionTypeButtons = new List<ProductionTypeButton>();

        void Awake()
        {
            PlayersController.productionModuleAddedToPlayer += OnProductionModuleSpawned;
            Selection.productionUnitSelected += OnProductionSelected;
        }

        void Start()
        {
            var productionCategories = GameController.instance.MainStorage.availableProductionCategories;
            for (int i = 0; i < productionCategories.Count; ++i)
            {
                if (productionCategories[i] == null)
                {
                    Debug.LogWarning("Storage contains empty field in Available Production Categories, please remove it, now it will be ignored.");
                    continue;
                }

                if (!Player.GetLocalPlayer().selectedFaction.ownProductionCategories.Contains(productionCategories[i]))
                {
                    continue;
                }

                var spawnedObject = Instantiate(GameController.instance.MainStorage.productionButtonTemplate,
                    productionIconsPanel);
                var productionIcon = spawnedObject.GetComponent<ProductionTypeButton>();

                productionIcon.SetupWithController(this);
                productionIcon.SetupWithProductionCategory(productionCategories[i]);
                productionTypeButtons.Add(productionIcon);
            }

            OnSelectButtonClick(GameController.instance.MainStorage.availableProductionCategories[0]);
        }

        void Redraw()
        {
            for (int i = 0; i < productionTypeButtons.Count; ++i)
            {
                if (productionTypeButtons[i].GetProductionCategory == selectedProductionCategory)
                {
                    productionTypeButtons[i].SetActive();
                }
                else
                {
                    productionTypeButtons[i].SetUnactive();
                }

                productionTypeButtons[i].Redraw();
            }
        }

        public void OnProductionSelected(Units.Production production)
        {
            int productionNumber = production.GetProductionNumber();
            OnSelectButtonClick(production.GetProductionCategory);
            UIController.instance.selectProductionNumberPanel.SelectBuildingWithNumber(productionNumber);
        }

        public void OnSelectButtonClick(ProductionCategory productionType)
        {
            selectedProductionCategory = productionType;
            productionCategoryChanged?.Invoke(selectedProductionCategory);

            Redraw();
        }

        public void OnProductionModuleSpawned(Units.Production production) => Redraw();

        void OnDestroy()
        {
            Selection.productionUnitSelected -= OnProductionSelected;
            PlayersController.productionModuleAddedToPlayer -= OnProductionModuleSpawned;
        }
    }

}
