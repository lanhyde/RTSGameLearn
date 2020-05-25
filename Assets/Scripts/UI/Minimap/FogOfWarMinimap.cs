using PromiseCode.RTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PromiseCode.RTS.UI.Minimap
{
    public class FogOfWarMinimap : MonoBehaviour
    {
        readonly List<Unit> unitsToShowInFOW = new List<Unit>();

        [SerializeField] Material fogOfWarUIMaterial;

        float updateTime = 0.5f;
        float updateTimer;

        static readonly int positionsTextureId = Shader.PropertyToID("_FOWMinimapPositionsTexture");

        Minimap minimap;
        Texture2D positionsTexture;

        void Awake()
        {
            Unit.unitSpawnedEvent += OnUnitSpawned;
            Unit.unitDestroyedEvent += OnUnitDestroyed;
        }

        void Start()
        {
            minimap = UIController.instance.minimapComponent;
            updateTime = GameController.instance.MainStorage.fowUpdateDelay;

            fogOfWarUIMaterial.SetFloat("_Enabled", GameController.instance.MainStorage.isFogOfWarOn ? 1 : 0);
            fogOfWarUIMaterial.SetFloat("_MapSize", MatchSettings.currentMatchSettings.selectedMap.mapSize);
            fogOfWarUIMaterial.SetFloat("_MinimapSize", minimap.MapImageSize);

            positionsTexture = new Texture2D(FogOfWar.unitsLimit, 1, TextureFormat.RGBAFloat, false, true);
        }

        private void Update()
        {
            if(updateTimer > 0)
            {
                updateTimer -= Time.deltaTime;
                return;
            }
            RecalculateUnitsVisibilityInFOW();

            updateTimer = updateTime;
        }

        void RecalculateUnitsVisibilityInFOW()
        {
            for(int i = 0; i < unitsToShowInFOW.Count; ++i)
            {
                if(i >= FogOfWar.unitsLimit)
                {
                    break;
                }
                var icon = minimap.GetUnitIcon(unitsToShowInFOW[i]);

                if(!icon)
                {
                    continue;
                }
                var pos = minimap.GetUnitOnMapPoint(unitsToShowInFOW[i]);
                var positionColor = new Color(pos.x / 1024, pos.y / 1024, 0, 1);

                positionsTexture.SetPixel(i, 0, positionColor);
            }
            positionsTexture.Apply();
            Shader.SetGlobalTexture(positionsTextureId, positionsTexture);
        }

        void OnUnitSpawned(Unit unit)
        {
            if(unit.IsInMyTeam(Player.GetLocalPlayer().teamIndex))
            {
                unitsToShowInFOW.Add(unit);
            }
        }

        void OnUnitDestroyed(Unit unit)
        {
            unitsToShowInFOW.Remove(unit);
        }

        void OnDestroy()
        {
            Unit.unitSpawnedEvent -= OnUnitSpawned;
            Unit.unitDestroyedEvent -= OnUnitDestroyed;
        }
    }

}