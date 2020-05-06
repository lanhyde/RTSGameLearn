using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Controls
{
    /// <summary>
    /// This enum used to change player control type for example when player is reparing or selling buildings.
    /// </summary>
    public enum CustomControls
    {
        None, Repair, Sell
    }
    /// <summary>
    /// Describe different hotkeys input modes, building works when building selected, default in other cases.
    /// </summary>
    public enum HotKeysInputType
    {
        Default, Building
    }
}