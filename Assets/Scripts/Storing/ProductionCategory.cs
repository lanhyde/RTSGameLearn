using System.Collections.Generic;
using PromiseCode.RTS.Storing;
using UnityEngine;

namespace PromiseCode.RTS.Storing
{
    [CreateAssetMenu(fileName = "ProductionCategory", menuName = "RTS/Production Category")]
    public class ProductionCategory: ScriptableObject
    {
        public string textId;
        public Sprite icon;
        public List<UnitData> availableUnits;
        public bool isBuildings;
    }
}