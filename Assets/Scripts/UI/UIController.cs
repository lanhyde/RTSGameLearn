using PromiseCode.RTS.UI.Minimap;
using System.Collections;
using System.Collections.Generic;
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

    }
}