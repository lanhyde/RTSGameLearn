﻿using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using PromiseCode.RTS.Storing;
using PromiseCode.RTS.Units;
using UnityEngine;
using UnityEngine.AI;

namespace PromiseCode.RTS.Controls
{
    public class Selection
    {
        public static List<Unit> selectedUnits {get; private set;}

        public delegate void SelectionAction();
        public delegate void UnitSelectionAction(Unit unit);
        public delegate void MultiSelectionAction(List<Unit> units);
        public delegate void ProductionSelectionAction(Production productionModule);

        public static event SelectionAction selectionStarted, selectionEnded, selectionCleared;
        public static event UnitSelectionAction unitSelected;
        public static event MultiSelectionAction onUnitsListSelected, onUnitListChanged;
        public static event ProductionSelectionAction productionUnitSelected;

        public static Vector2 startMousePosition, endMousePosition;

        public static bool isSelectionStarted { get; private set; }

        static readonly string unitLayerName = "Unit";
        static float doubleClickTimer;
        static int unitLayerMask;
        static int unitAlternativeNumber;

        static List<KeyTimer> groupsKeyTimers = new List<KeyTimer>();
        static KeyCode keyToMultipleSelection;

        static Selection()
        {
            if(selectedUnits == null)
            {
                selectedUnits = new List<Unit>();
            }
            unitLayerMask = 1 << LayerMask.NameToLayer(unitLayerName);
            keyToMultipleSelection = Keymap.loadedKeymap.GetAction(KeyActionType.AddToSelectionHoldKey).key;
        }

        public static void OnStartSelection()
        {
            startMousePosition = Input.mousePosition;
            isSelectionStarted = true;
            selectionStarted?.Invoke();
        }

        public static void OnSingleSelection()
        {
            if(!Input.GetKey(keyToMultipleSelection) || selectedUnits.Count > 0 && selectedUnits[0] && selectedUnits[0].data.isBuilding)
            {
                OnClearSelection();
            }
            var ray = GameController.cachedMainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, 1000, unitLayerMask))
            {
                var unit = hit.collider.GetComponent<Unit>();
                if(!unit || !unit.IsOwnedByPlayer(Player.localPlayerId))
                {
                    CancelOrEndSelection();
                    return;
                }

                OnUnitAddToSelection(unit);

                if(doubleClickTimer > 0)
                {
                    SelectUnitsSameType(unit);
                    doubleClickTimer = 0;
                }
            }

            unitAlternativeNumber = 0;
            doubleClickTimer = 0.3f;
            CancelOrEndSelection();
        }

        static void SelectUnitsSameType(Unit unit, bool onlyOnScreen = true)
        {
            for(int i = 0; i < Unit.allUnits.Count; ++i)
            {
                var secondUnit = Unit.allUnits[i];
                if(secondUnit.OwnerPlayerId == unit.OwnerPlayerId && secondUnit.data == unit.data && (!onlyOnScreen || secondUnit.IsVisibleInViewport()))
                {
                    OnUnitAddToSelection(Unit.allUnits[i]);
                }
            }
        }

        static void CancelOrEndSelection()
        {
            isSelectionStarted = false;
            selectionEnded?.Invoke();
        }

        public static void OnEndSelection()
        {
            OnClearSelection();

            float minX, minY, maxX, maxY;
            if(startMousePosition.x < endMousePosition.x)
            {
                minX = startMousePosition.x;
                maxX = endMousePosition.x;
            }
            else
            {
                minX = endMousePosition.x;
                maxX = startMousePosition.x;
            }
            if(startMousePosition.y < endMousePosition.y)
            {
                minY = startMousePosition.y;
                maxY = endMousePosition.y;
            }
            else
            {
                minY = endMousePosition.y;
                maxY = startMousePosition.y;
            }

            var rect = new Rect
            {
                min = new Vector2(minX, minY),
                max = new Vector2(maxX, maxY)
            };

            for(int i = 0; i < Unit.allUnits.Count; ++i)
            {
                var unit = Unit.allUnits[i];

                if(!unit.IsOwnedByPlayer(Player.localPlayerId) || unit.data.isBuilding || unit.isBeingCarried)
                {
                    continue;
                }
                Vector2 screenPos = GameController.cachedMainCamera.WorldToScreenPoint(unit.transform.position);

                if(rect.Contains(screenPos))
                {
                    OnUnitAddToSelection(unit, isSoundNeeded: i == 0);
                }
            }

            isSelectionStarted = false;

            if(selectedUnits.Count > 1)
            {
                onUnitsListSelected?.Invoke(selectedUnits);
            }

            selectionEnded?.Invoke();
        }

        public static void OnUnitAddToSelection(Unit unit, bool isSoundNeeded = false)
        {
            if(selectedUnits.Contains(unit))
            {
                return;
            }
            if(unit.isBeingCarried)
            {
                return;
            }
            unit.Select(isSoundNeeded);

            if(unit.production)
            {
                productionUnitSelected?.Invoke(unit.production);
            }
            unitSelected?.Invoke(unit);

            if(selectedUnits.Count > 1)
            {
                onUnitsListSelected?.Invoke(selectedUnits);
            }
        }

        static void UnitDiedAction(Unit unit) => UnselectUnit(unit);

        public static void UnselectUnit(Unit unit)
        {
            unit.Unselect();
            selectedUnits.Remove(unit);

            if(selectedUnits.Count == 0)
            {
                selectionCleared?.Invoke();
            }
            else
            {
                onUnitsListSelected?.Invoke(selectedUnits);
            }
        }

        public static void OnClearSelection()
        {
            if (Input.GetKey(keyToMultipleSelection))
            {
                return;
            }
            for(int i = 0; i < selectedUnits.Count;++i)
            {
                selectedUnits[i].Unselect();
            }
            selectedUnits.Clear();

            selectionCleared?.Invoke();
        }

        public static void Initialize()
        {
            selectedUnits.Clear();
            isSelectionStarted = false;

            Damageable.onDamageableDied -= UnitDiedAction;
            Damageable.onDamageableDied += UnitDiedAction;

            InitializeHotkeys();
        }

        static void InitializeHotkeys()
        {
            Keymap.loadedKeymap.GetAction(KeyActionType.SelecteSameUnitOnScreen).onPressEvent += OnPressSelectAllSameUnitsOnScreen;
            Keymap.loadedKeymap.GetAction(KeyActionType.SelecteSameUnitOnScreen).onDoublePressEvent += OnPressSelectAllSameUnits;
            Keymap.loadedKeymap.GetAction(KeyActionType.SelectAllUnitsOnScreen).onPressEvent += OnPressSelectAllUnitsOnScreen;
            Keymap.loadedKeymap.GetAction(KeyActionType.SelectAllUnits).onPressEvent += OnPressSelectAllUnits;
            Keymap.loadedKeymap.GetAction(KeyActionType.StopOrder).onPressEvent += OnPressStopOrder;
            Keymap.loadedKeymap.GetAction(KeyActionType.UnitsMoveWithLowerestSpeed).onPressEvent += OnPressMoveOnLowerestSpeed;
            Keymap.loadedKeymap.GetAction(KeyActionType.MoveToBase).onPressEvent += OnPressMoveToBase;
            Keymap.loadedKeymap.GetAction(KeyActionType.SelectUnitAlternative).onPressEvent += OnPressSelectUnitAlternative;
            Keymap.loadedKeymap.GetAction(KeyActionType.SelectHarvesterAlternative).onPressEvent += OnPressSelectHarvesterAlternative;
            Keymap.loadedKeymap.GetAction(KeyActionType.LockUnitMovement).onPressEvent += OnPressLockMovement;
            Keymap.loadedKeymap.GetAction(KeyActionType.DisperseUnitsToCorners).onPressEvent += OnPressDisperseUnits;
            Keymap.loadedKeymap.GetAction(KeyActionType.UseFirstAbility).onPressEvent += OnPressUseFirstAbility;
        }

        static void OnPressSelectAllSameUnits() => SelectAllSameUnitsForCurrent(false);
        static void OnPressSelectAllSameUnitsOnScreen() => SelectAllSameUnitsForCurrent(true);
        static void OnPressSelectUnitAlternative() => SelectUnitAlternative(true);
        static void OnPressSelectHarvesterAlternative() => SelectHarvesterAlternative(true);
        static void OnPressDisperseUnits()
        {
            if(selectedUnits.Count == 0)
            {
                return;
            }

            var currentGroupCenterPoint = Vector3.zero;
            int aliveUnitCount = selectedUnits.Count;

            for(int i = 0; i < selectedUnits.Count; ++i)
            {
                if(selectedUnits[i])
                {
                    currentGroupCenterPoint += selectedUnits[i].transform.position;
                }
                else
                {
                    aliveUnitCount--;
                }
            }
            currentGroupCenterPoint /= aliveUnitCount;
            NavMeshHit hit;

            for(int i = 0; i < selectedUnits.Count; ++i)
            {
                currentGroupCenterPoint.y = selectedUnits[i].transform.position.y;
                var destination = (selectedUnits[i].transform.position - currentGroupCenterPoint).normalized * 3f;

                var order = new MovePositionOrder();

                bool foundPoint = NavMesh.SamplePosition(selectedUnits[i].transform.position + destination, out hit, 10f, NavMesh.AllAreas);
                order.movePosition = hit.position;

                if(foundPoint)
                {
                    selectedUnits[i].AddOrder(order, false);
                }
            }
        }
        static void OnPressLockMovement()
        {
            for(var i = 0; i < selectedUnits.Count; ++i)
            {
                selectedUnits[i].isMovementLockedByHotkey = !selectedUnits[i].isMovementLockedByHotkey;
                var movable = selectedUnits[i].GetModule<Movable>();

                movable?.Stop();
            }
        }
        static void OnPressMoveOnLowerestSpeed()
        {
            float lowerestSpeed = 300;
            bool useCustomSpeed = true;

            for(int i  = 0; i < selectedUnits.Count; ++i)
            {
                var moveModule = selectedUnits[i].GetModule<Movable>();

                if(moveModule && moveModule.useCustomSpeed)
                {
                    useCustomSpeed = false;
                    break;
                }
                lowerestSpeed = Mathf.Min(selectedUnits[i].data.moveSpeed, lowerestSpeed);
            }
            for(int i = 0; i < selectedUnits.Count; ++i)
            {
                var moveModule = selectedUnits[i].GetModule<Movable>();
                moveModule?.SetCustomSpeed(lowerestSpeed, useCustomSpeed);
            }
        }
        static void OnPressMoveToBase()
        {
            if(Player.GetLocalPlayer().playerProductionBuildings.Count < 1)
            {
                return;
            }

            var order = new MovePositionOrder();
            order.movePosition = Player.GetLocalPlayer().playerProductionBuildings[0].transform.position;

            selectedUnits.ForEach(unit => unit.AddOrder(order, false));
        }
        static void OnPressUseFirstAbility()
        {
            for(int i = 0; i < selectedUnits.Count; ++i)
            {
                if(!selectedUnits[i])
                {
                    continue;
                }

                var abilitiesModule = selectedUnits[i].GetComponent<AbilitiesModule>();
                if(abilitiesModule)
                {
                    var ability = abilitiesModule.GetAbilityById(0);
                    ability?.DoAction();
                }
            }
        }
        static void SelectAllSameUnitsForCurrent(bool onlyOnScreen)
        {
            if(selectedUnits.Count == 0)
            {
                return;
            }

            var unitTypes = new List<Unit>();

            for(int i = 0; i < selectedUnits.Count; ++i)
            {
                if(!unitTypes.Find(unitTypes => unitTypes.data == selectedUnits[i].data))
                {
                    unitTypes.Add(selectedUnits[i]);
                }
            }
            for(int i = 0;i < unitTypes.Count; ++i)
            {
                SelectUnitsSameType(unitTypes[i], onlyOnScreen);
            }
        }
        static void SelectUnitAlternative(bool focus)
        {
            if(selectedUnits.Count == 0 || selectedUnits.Count > 1)
            {
                return;
            }
            SelectUnitAlternative(selectedUnits[0].data, focus);
        }
        static void SelectUnitAlternative(UnitData unitType, bool focus)
        {
            var selectedUnit = selectedUnits[0];
            int unitId = 0;

            var unitsOfThisType = Unit.allUnits.FindAll(u => u && u.OwnerPlayerId == selectedUnit.OwnerPlayerId && u.data == selectedUnit.data);
            for(int i = 0; i < unitsOfThisType.Count; ++i)
            {
                var secondUnit = unitsOfThisType[i];
                if(unitId >= unitAlternativeNumber)
                {
                    OnClearSelection();
                    OnUnitAddToSelection(secondUnit, true);
                    unitAlternativeNumber++;

                    if(focus)
                    {
                        GameController.instance.cameraMover.SetPosition(secondUnit.transform.position);
                    }
                    break;
                }

                unitId++;

                if(unitAlternativeNumber >= unitId && i == unitsOfThisType.Count - 1)
                {
                    unitAlternativeNumber = 0;
                    OnClearSelection();
                    OnUnitAddToSelection(secondUnit, true);

                    if(focus)
                    {
                        GameController.instance.cameraMover.SetPosition(secondUnit.transform.position);
                    }
                }
            }
        }
        static void SelectHarvesterAlternative(bool focus)
        {
            int unitId = 0;
            var unitsOfThisType = Unit.allUnits.FindAll(u => u && u.OwnerPlayerId == Player.localPlayerId && u.data.isHarvester);

            for(int i = 0; i < unitsOfThisType.Count; ++i)
            {
                var unit = unitsOfThisType[i];

                if(unitId >= unitAlternativeNumber)
                {
                    OnClearSelection();
                    OnUnitAddToSelection(unit, true);
                    unitAlternativeNumber++;

                    if(focus)
                    {
                        GameController.instance.cameraMover.SetPosition(unit.transform.position);
                    }
                    break;
                }
                unitId++;

                if(unitAlternativeNumber >= unitId && i == unitsOfThisType.Count - 1)
                {
                    unitAlternativeNumber = 0;
                    OnClearSelection();
                    OnUnitAddToSelection(unit, true);

                    if(focus)
                    {
                        GameController.instance.cameraMover.SetPosition(unit.transform.position);
                    }
                }
            }
        }
        static void OnPressSelectAllUnitsOnScreen() => SelectAllUnits(true);
        static void OnPressSelectAllUnits() => SelectAllUnits(false);

        static void SelectAllUnits(bool onlyOnScreen = true)
        {
            for(int i = 0; i < Unit.allUnits.Count; ++i)
            {
                var unit = Unit.allUnits[i];
                if(!unit || unit.data.isBuilding || unit.data.isHarvester || unit.OwnerPlayerId != Player.localPlayerId)
                {
                    continue;
                }
                if(unit.IsVisibleInViewport() || !onlyOnScreen)
                {
                    OnUnitAddToSelection(Unit.allUnits[i]);
                }
            }
        }
        static void OnPressStopOrder()
        {
            selectedUnits.ForEach(u => u.EndCurrentOrder());
        }
        /// <summary>
        /// Call this method from GameController's update
        /// </summary>
        public static void Update()
        {
            GroupsWork();
            doubleClickTimer -= Time.deltaTime;

            for(int i = groupsKeyTimers.Count - 1; i>= 0; --i)
            {
                groupsKeyTimers[i].Tick();

                if(groupsKeyTimers[i].IsFinished())
                {
                    groupsKeyTimers.RemoveAt(i);
                }
            }
        }
        static void GroupsWork()
        {
            for(int i = 0; i <= 9; ++i)
            {
                if (!Input.GetKeyDown(i.ToString()))
                {
                    continue;
                }
                var keyToHold = Application.isEditor ? KeyCode.LeftShift : Keymap.loadedKeymap.GetAction(KeyActionType.GroupControlsHoldKey).key;

                if(Input.GetKey(keyToHold))
                {
                    for(int w = 0; w < Unit.allUnits.Count; ++w)
                    {
                        if(Unit.allUnits[w] && Unit.allUnits[w].unitSelectionGroup == i)
                        {
                            Unit.allUnits[w].SetUnitSelectionGroup(-1);
                        }
                    }
                    for(int w = 0; w< selectedUnits.Count; ++w)
                    {
                        if(selectedUnits[w])
                        {
                            selectedUnits[w].SetUnitSelectionGroup(i);
                        }
                    }
                }
                else
                {
                    OnClearSelection();

                    var wasFocused = false;
                    var isKeyWasPressedNearly = groupsKeyTimers.Find(gkt => gkt.numberKeyId == i) != null;

                    for(int w = 0; w < Unit.allUnits.Count; ++w)
                    {
                        if(Unit.allUnits[w] && Unit.allUnits[w].unitSelectionGroup == i)
                        {
                            if(!wasFocused && isKeyWasPressedNearly)
                            {
                                GameController.instance.cameraMover.SetPosition(Unit.allUnits[w].transform.position);
                                wasFocused = true;
                            }
                            OnUnitAddToSelection(Unit.allUnits[w]);
                        }
                    }
                    if(!isKeyWasPressedNearly)
                    {
                        groupsKeyTimers.Add(new KeyTimer(i));
                    }
                }
            }
        }
    }

    /// <summary>
    /// This class represents timer which helps to check double press for some key
    /// </summary>
    public class KeyTimer
    {
        public int numberKeyId;
        public float timeLeft;

        public KeyTimer(int keyNumber, float time = 0.4f)
        {
            numberKeyId = keyNumber;
            timeLeft = time;
        }

        public void Tick()
        {
            timeLeft -= Time.deltaTime;
        }

        public bool IsFinished()
        {
            return timeLeft <= 0;
        }
    }
}
