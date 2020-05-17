using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Controls
{
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler sceneInstance {get; private set;}
        /// <summary>Contains current player world cursor hit point, get by ScreenPointToRay method.</summary>
        public static RaycastHit currentCursorWorldHit;
        private static CustomControls customControlsMode;
        private static HotKeysInputType hotkeysInputMode;
        private string buildingInputKeys = "qwerasdfzxcv";

        void Awake()
        {
            sceneInstance = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            hotkeysInputMode = HotKeysInputType.Default;


        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
