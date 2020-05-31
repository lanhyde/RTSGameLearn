using UnityEngine;
using System.Collections.Generic;

namespace PromiseCode.RTS
{
    public class ResourcesField: MonoBehaviour
    {
        public static List<ResourcesField> sceneResourceFields { get; private set; }

        void Awake()
        {
            if(sceneResourceFields == null)
            {
                sceneResourceFields = new List<ResourcesField>();
            }
            sceneResourceFields.Add(this);
        }

        public void OnMouseEnter()
        {
            if(Selection.selectedUnits.Count == 0 || !Selection.selectedUnits[0].data.isHarvester)
            {
                return;
            }
            var selectedHarvester = Selection.selectedUnits[0].GetModule<Harvester>();
            var needResourcesCursor = selectedHarvester.harvestedResources < selectedHarvester.MaxResources;

            if(needResourcesCursor)
            {
                Cursors.SetResourcesCursor();
            }
            else
            {
                Cursors.SetRestrictCurosr();
            }
        }

        public void OnMouseExit()
        {
            Cursors.SetDefaultCursor();
        }

        void OnDestroy()
        {
            sceneResourceFields.Remove(this);
        }
    }
}