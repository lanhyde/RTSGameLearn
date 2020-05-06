using System.Collections;
using System.Collections.Generic;
using PromiseCode.RTS.Units;
using UnityEngine;

namespace PromiseCode.RTS.Controls
{
    public class Selection
    {
        public static List<Unit> selectedUnits {get; private set;}

        public delegate void SelectionAction();
        public delegate void UnitSelectionAction(Unit unit);
        public delegate void MultiSelectionAction(List<Unit> units);
        public delegate void ProductionSelectionAction(Production productionModule);
    }

}
