using System.Collections;
using System.Collections.Generic;
using PromiseCode.RTS.Controls;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PromiseCode.RTS.UI.Minimap
{
    public class MinimapPanel : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Minimap minimap;
        private MinimapCameraIcon minimapCameraIcon;

        private bool isPointerOn;

        void Start()
        {
            minimapCameraIcon = FindObjectOfType<MinimapCameraIcon>();
            minimap = FindObjectOfType<Minimap>();
        }

        void Update()
        {
            if (isPointerOn)
            {
                Cursors.SetMapOrderCursor();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                minimapCameraIcon.OnMapClick(eventData);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                var offset = eventData.position - (Vector2) minimap.IconsPanel.pivot +
                             Vector2.one * minimap.MapImageSize / 2f;
                offset /= minimap.GetScaleFactor();

                var boundedMapPos = Minimap.InboundPositionToMap(offset, minimap.MapImageSize);
                Ordering.GiveMapOrder(boundedMapPos);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Selection.selectedUnits.Count > 0 && Selection.selectedUnits[0].data.hasMoveModule)
            {
                isPointerOn = true;
                Cursors.SetMapOrderCursor();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Cursors.SetDefaultCursor();
            isPointerOn = false;
        }
    }

}
