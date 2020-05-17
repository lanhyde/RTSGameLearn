using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PromiseCode.RTS.Controls
{
    public enum KeyActionType
    {
        SelecteSameUnitOnScreen,
        SelectAllUnitsOnScreen,
        SelectAllUnits,
        AttackOrder,
        StopOrder,
        ShowCommCenter,
        GroupControlsHoldKey,
        AddToSelectionHoldKey,
        MoveToBase,
        SelectUnitAlternative,
        UnitsMoveWithLowerestSpeed,
        SelectHarvesterAlternative,
        LockUnitMovement,
        DisperseUnitsToCorners,
        UseFirstAbility,
    }

    [System.Serializable]
    public class Keymap
    {
        public static Keymap loadedKeymap;
        public List<KeyAction> registeredKeys = new List<KeyAction>();

        static Keymap()
        {
            Load(true);
        }

        public Keymap()
        {
            SetupDefaultScheme();
        }

        public void CheckAllKeys()
        {
            for(int i = 0; i < registeredKeys.Count; ++i)
            {
                registeredKeys[i].IsKeyDown();
            }
        }

        public void SetupDefaultScheme()
        {
            RegisterKey(new KeyAction(KeyActionType.SelecteSameUnitOnScreen, KeyCode.Q, true));
            RegisterKey(new KeyAction(KeyActionType.SelectAllUnitsOnScreen, KeyCode.E));
            RegisterKey(new KeyAction(KeyActionType.SelectAllUnits, KeyCode.W));
            RegisterKey(new KeyAction(KeyActionType.AttackOrder, KeyCode.A));
            RegisterKey(new KeyAction(KeyActionType.StopOrder, KeyCode.S));
            RegisterKey(new KeyAction(KeyActionType.ShowCommCenter, KeyCode.Space));
            RegisterKey(new KeyAction(KeyActionType.GroupControlsHoldKey, KeyCode.LeftControl));
            RegisterKey(new KeyAction(KeyActionType.AddToSelectionHoldKey, KeyCode.LeftShift));
            RegisterKey(new KeyAction(KeyActionType.MoveToBase, KeyCode.G));
            RegisterKey(new KeyAction(KeyActionType.SelectUnitAlternative, KeyCode.L));
            RegisterKey(new KeyAction(KeyActionType.SelectHarvesterAlternative, KeyCode.O));
            RegisterKey(new KeyAction(KeyActionType.UnitsMoveWithLowerestSpeed, KeyCode.R));
            RegisterKey(new KeyAction(KeyActionType.LockUnitMovement, KeyCode.F));
            RegisterKey(new KeyAction(KeyActionType.DisperseUnitsToCorners, KeyCode.X));
            RegisterKey(new KeyAction(KeyActionType.UseFirstAbility, KeyCode.D));
        }

        public KeyAction GetAction(KeyActionType type)
        {
            var keyAction = registeredKeys.Find(k => k.type == type);
            if(keyAction != null)
            {
                return keyAction;
            }
            var action = new KeyAction(type, KeyCode.K);
            RegisterKey(action);

            return action;
        }

        void RegisterKey(KeyAction keyAction)
        {
            var match = registeredKeys.Find(ka => ka.type == keyAction.type);
            if(match != null)
            {
                registeredKeys.Remove(match);
            }
            registeredKeys.Add(keyAction);
        }

        public static void Save()
        {
            var jsonString = JsonUtility.ToJson(loadedKeymap);
            PlayerPrefs.SetString("Keymap", jsonString);
        }

        public static void Load(bool ignoreLoaded = false)
        {
            if(loadedKeymap != null && !ignoreLoaded)
            {
                return;
            }
            if(PlayerPrefs.HasKey("Keymap"))
            {
                loadedKeymap = JsonUtility.FromJson<Keymap>(PlayerPrefs.GetString("Keymap"));
                return;
            }
            loadedKeymap = new Keymap();
        }
    }

    [System.Serializable]
    public class KeyAction
    {
        public KeyActionType type;
        public KeyCode key;
        public bool haveDoublePress;

        public delegate void OnKeyPressed();
        public event OnKeyPressed onPressEvent, onDoublePressEvent;

        float doublePressTimer;

        public KeyAction(KeyActionType newType, KeyCode keyToUse, bool haveDoublePress = false)
        {
            type = newType;
            key = keyToUse;
            this.haveDoublePress = haveDoublePress;
        }

        public bool IsKeyActive()
        {
            return Input.GetKey(key);
        }

        public bool IsKeyDown()
        {
            if(haveDoublePress && doublePressTimer > 0)
            {
                doublePressTimer -= Time.deltaTime;
            }
            bool keyDown = Input.GetKeyDown(key);

            if(keyDown)
            {
                if((doublePressTimer <= 0 || !haveDoublePress) && onPressEvent != null)
                {
                    onPressEvent.Invoke();
                    doublePressTimer = 0.2f;
                }
                if(haveDoublePress && doublePressTimer > 0 && onDoublePressEvent != null)
                {
                    onDoublePressEvent.Invoke();
                    doublePressTimer = 0;
                }
            }
            return keyDown;
        }
    }
}

