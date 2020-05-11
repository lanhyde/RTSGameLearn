using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.UI
{
    public static class Cursors
    {
        public static bool lockCursorChange;

        public static void SetDefaultCursor()
        {
            SetCursor(GameController.instance.MainStorage.defaultCursor);
        }

        public static void SetAttackCursor()
        {
            SetCursor(GameController.instance.MainStorage.attackCursor, new Vector2(0.5f, 0.5f));
        }

        public static void SetRestrictCursor()
        {
            SetCursor(GameController.instance.MainStorage.restrictCursor, new Vector2(0.5f, 0.5f));
        }

        public static void SetResourcesCursor()
        {
            SetCursor(GameController.instance.MainStorage.gatherResourcesCursor, new Vector2(0.5f, 0.5f));
        }

        public static void SetGiveResourcesCursor()
        {
            SetCursor(GameController.instance.MainStorage.giveResourcesCursor, new Vector2(0.5f, 0.5f));
        }

        public static void SetSellCursor()
        {
            SetCursor(GameController.instance.MainStorage.sellCursor, new Vector2(0.5f, 0.5f));
            lockCursorChange = true;
        }

        public static void SetRepairCursor()
        {
            SetCursor(GameController.instance.MainStorage.repairCursor, new Vector2(0.5f, 0.5f));
            lockCursorChange = true;
        }

        public static void SetMapOrderCursor()
        {
            SetCursor(GameController.instance.MainStorage.mapOrderCursor);
        }

        public static void SetCursor(Texture2D cursorTexture)
        {
            SetCursor(cursorTexture, Vector2.up);
        }

        public static void SetCursor(Texture2D cursorTexture, Vector2 hotspot)
        {
            if(lockCursorChange)
            {
                return;
            }
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        }
    }
}