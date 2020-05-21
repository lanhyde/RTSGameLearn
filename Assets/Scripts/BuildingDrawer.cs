using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PromiseCode.RTS
{
    public class BuildingDrawer : MonoBehaviour
    {
        [SerializeField]
        Renderer[] renderers;
        static readonly int tintColor = Shader.PropertyToID("_TintColor");

        public void SetBuildingAllowedState(bool isAllowed)
        {
            SetMeshColor(isAllowed ? Color.green : Color.red);
        }

        void SetMeshColor(Color color)
        {
            for(int i = 0; i < renderers.Length; ++i)
            {
                renderers[i].material.SetColor(tintColor, color);
            }
        }
    }

}