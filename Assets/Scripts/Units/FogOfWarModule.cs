using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace PromiseCode.RTS.Units
{
    public class FogOfWarModule : Module
    {
        const float updateRate = 0.25f;
        [Tooltip("List of all unit Renderer components which should be hidden when enemy unit enters FOW and shown when exits FOW")]
        [SerializeField] Renderer[] renderersToHide = new Renderer[0];
        [Tooltip("Can be empty. List of all unit child game objects which should be hidden when enemy unit enters FOW andshown when exits FOW.")]
        [SerializeField] GameObject[] gameObjectsToDisable = new GameObject[0];

        float updateVisibilityTimer;

        public bool isVisibleInFOW { get; protected set; }

        void Start()
        {
            if(!GameController.instance.MainStorage.isFogOfWarOn)
            {
                enabled = false;
                return;
            }
            for(int i = 0; i < gameObjectsToDisable.Length; ++i)
            {
                if(gameObjectsToDisable[i] == gameObject)
                {
                    Debug.LogWarning($"Fog of war moudle of unit {name} is set up incorrectly. You shouldn't add self unit object to Game Objects to DISABLE fields! Add models here.");
                    enabled = false;
                    return;
                }
            }
            CheckVisibleState();
            if(IsPlayerTeamUnit(selfUnit))
            {
                isVisibleInFOW = true;
            }
        }

        void Update()
        {
            if(updateVisibilityTimer >= 0)
            {
                updateVisibilityTimer -= Time.deltaTime;
                return;
            }
            CheckVisibleState();
            updateVisibilityTimer = updateRate;
        }

        void CheckVisibleState()
        {
            if(IsPlayerTeamUnit(selfUnit))
            {
                return;
            }
            var allLocalPlayerTeamUnits = GetAllLocalPlayerTeamUnits();

            bool isVisible = false;
            var selfPosition = transform.position;

            for(int i = 0; i < allLocalPlayerTeamUnits.Count; ++i)
            {
                if(!allLocalPlayerTeamUnits[i])
                {
                    continue;
                }
                var otherUnitSqrVisibility = Mathf.Pow(allLocalPlayerTeamUnits[i].data.visionDistance, 2f);
                var otherPosition = allLocalPlayerTeamUnits[i].transform.position;
                if((selfPosition - otherPosition).sqrMagnitude <= otherUnitSqrVisibility)
                {
                    isVisible = true;
                    break;
                }
            }
            SetShownState(isVisible);
        }

        List<Unit> GetAllLocalPlayerTeamUnits()
        {
            var allUnits = Unit.allUnits;
            var resultUnits = new List<Unit>();

            resultUnits.AddRange(allUnits.Where(unit => IsPlayerTeamUnit(unit)));
            return resultUnits;
        }

        bool IsPlayerTeamUnit(Unit unit)
        {
            return unit.IsOwnedByPlayer(Player.localPlayerId) || unit.IsInMyTeam(Player.localPlayerId);
        }

        public void OnShownFromFOW()
        {
            SetShownState(true);
        }

        public void OnHideInFOW()
        {
            SetShownState(false);
        }

        void SetShownState(bool visibility)
        {
            for(int i = 0; i < renderersToHide.Length; ++i)
            {
                renderersToHide[i].enabled = visibility;
            }
            for(int i = 0; i < gameObjectsToDisable.Length; ++i)
            {
                if(gameObjectsToDisable[i] == selfUnit.gameObject)
                {
                    Debug.LogWarning($"In FOW unit module of unit {name} is hideable game object set its self object, which is wrong. Ignoring.");
                    continue;
                }
                gameObjectsToDisable[i].SetActive(visibility);
            }
            isVisibleInFOW = visibility;
        }
    }

}
