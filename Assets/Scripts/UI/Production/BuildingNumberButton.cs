using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PromiseCode.RTS.UI.Production
{
    public class BuildingNumberButton : MonoBehaviour
    {
        [SerializeField] private Text numberText;
        [SerializeField] private Button selfButton;
        [SerializeField, Range(0, 4)] private int buildingId;
        private SelectProductionNumberPanel controllerPanel;

        private Image selfImage;
        private Color defaultColor;

        void Awake()
        {
            selfImage = GetComponent<Image>();
            defaultColor = selfImage.color;
        }

        public void SetActive() => selfImage.color = Color.green;
        public void SetUnactive() => selfImage.color = defaultColor;
        public void SetEnabled() => selfButton.interactable = true;
        public void SetDisabled() => selfButton.interactable = false;
        public void OnClick() => controllerPanel.SelectBuildingWithNumber(buildingId);

        public void SetupBuilding(int id)
        {
            buildingId = id;
            numberText.text = (buildingId + 1).ToString();
        }

        public void SetupWithController(SelectProductionNumberPanel controllerPanel) =>
            this.controllerPanel = controllerPanel;
    }

}
