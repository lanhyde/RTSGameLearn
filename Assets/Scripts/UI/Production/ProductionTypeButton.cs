using System.Collections;
using System.Collections.Generic;
using PromiseCode.RTS.Storing;
using UnityEngine;
using UnityEngine.UI;

namespace PromiseCode.RTS.UI.Production
{
    public class ProductionTypeButton : MonoBehaviour
    {
        [SerializeField] private ProductionCategory productionCategory;
        private SelectProductionTypePanel controllerPanel;

        private Image selfImage;
        private Button selfButton;
        private Color defaultColor;

        public ProductionCategory GetProductionCategory => productionCategory;

        void Awake()
        {
            selfImage = GetComponent<Image>();
            selfButton = GetComponent<Button>();
            defaultColor = selfImage.color;
        }

        void Start() => Redraw();

        public void SetActive() => selfImage.color = Color.green;
        public void SetUnactive() => selfImage.color = defaultColor;
        public void OnClick() => controllerPanel.OnSelectButtonClick(productionCategory);

        public void Redraw() => selfButton.interactable =
            Player.GetLocalPlayer().IsHaveProductionOfCategory(productionCategory);

        public void SetupWithController(SelectProductionTypePanel typePanel) => controllerPanel = typePanel;

        public void SetupWithProductionCategory(ProductionCategory category)
        {
            productionCategory = category;
            selfImage.sprite = category.icon;
            Redraw();
        }
    }
}
