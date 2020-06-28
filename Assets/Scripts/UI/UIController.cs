using PromiseCode.RTS.UI.Minimap;
using System.Collections;
using System.Collections.Generic;
using PromiseCode.RTS.UI.Production;
using UnityEngine;
using UnityEngine.UI;

namespace PromiseCode.RTS.UI
{
    public class UIController : MonoBehaviour
    {
        public static UIController instance { get; protected set; }
        [SerializeField] Canvas mainCanvas;
        [SerializeField] Text moneyText;
        [SerializeField] GameObject winScreen;
        [SerializeField] GameObject loseScreen;

        public Minimap.Minimap minimapComponent { get; protected set; }
        public ProductionHint productionHint { get; protected set; }
        public SelectProductionNumberPanel selectProductionNumberPanel { get; protected set;}
        public MinimapSignal minimapSignal {get; protected set;}
        public CarryingUnitList carryingUnitList {get; protected set;}
        public PauseMenu pauseMenu { get; protected set; }

        SelectProductionTypePanel selectProductionTypePanel;

        public Canvas MainCanvas => mainCanvas;

        void Awake()
        {
            instance = this;

        }

    }
}