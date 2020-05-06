using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Storing
{
    [CreateAssetMenu(fileName ="Storage", menuName = "RTS/Storage")]
    public class Storage : ScriptableObject
    {
        [Header("Default Game Settings")]
        [Tooltip("Here you should add all created Map settings objects, otherwise map don't appear in maps list in game")]
        public List<MapSettings> availableMaps;
        public List<ProductionCategory> availableProductionCategories;
        [Tooltip("List of all available factions in game")]
        public List<FactionData> availableFactions;
        [Tooltip("List of all available player colors. You can add new colors or remove existing")]
        public List<Color> availablePlayerColors;

        [Tooltip("Money value, which will be added to each player on game match start.")]
        [Range(0, 100000)] public int startPlayerMoney = 10000;
        [Tooltip("This field contains maximum building distance, Player will be able to create buildings only in this radius from start point.")]
        [Range(10, 1000)] public int maxBuildDistance = 40;
        public bool allowBuildingsRotation = true;
        public bool useGridForBuildingMode = true;
        public bool allowCameraRotation = true;
        public bool allowCameraZoom = true;

        [Tooltip("indicate whether the black borders outside the map bounds is visible")]
        public bool showMapBorders = true;
        [Tooltip("If you needn't automatic NavMeshObstacle component addition to your buildings, turn this off")]
        public bool addNavMeshObstacleToBuildings = true;
        [Tooltip("Units formation type. Default formation keeps units position same as it was before order, Square Predict is better for square formations")]
        public UnitsFormation unitsFormation = UnitsFormation.Default;

        [Header("GamePlay - Electricity")]
        [Tooltip("Check this if your game uses electricity 'model' of gameplay. It means that some buildings uses electricity to work, and there exists some powerplants which gives electricity")]
        public bool isElectricityUsedInGame;
        [Tooltip("Speed decrease value when electricity limit is reached. If you set to 1, there will be original production speed (100%), if you set to 0.5, it would be 50%. Set to 0 to pause production until electricity restores")]
        [Range(0, 1)] public float speedCoefForProductionsWithoutElectricity = 0.75f;

        [Header("GamePlay - Fog of War")]
        [Tooltip("Is Fog of War used in the game?")]
        public bool isFogOfWarOn = true;
        [Tooltip("Delay between updates of fog of war visual part. Smaller values can cause bad performance, but better quality")]
        [Range(0, 0.5f)] public float fowUpdateDelay = 0.05f;

        [Header("GamePlay - other")]
        [Tooltip("Health being restored per one second of building repair")]
        public int buildingRepairHealthPerSecond = 5;
        [Tooltip("Money cost for one second of building repair")]
        public int buildingRepairCostPerSecond = 2;
        [Tooltip("How much player will receive for selling building (in percents of default price")]
        [Range(0, 1)] public float buildingSellReturnPercent = 0.5f;

        [Header("UI Templates")]
        [Tooltip("This and other templates used by asset UI elements. Keep it default or customize if you want")]
        public GameObject unitMinimapIconTemplate;
        public GameObject healthBarTemplate;
        public GameObject productionButtonTemplate;
        public GameObject productionNumberButtonTemplate;
        public GameObject unitProductionIconTemplate;
        public GameObject unitMultiselectionIconTemplate;
        public GameObject harvesterBarTemplate;
        public GameObject minimapSIgnalTemplate;
        public GameObject unitCarryingIcon;
        public GameObject unitAbilityIcon;
        public GameObject carryCellTemplate;

        [Header("Cursors")]
        [Tooltip("Default cursor used in game. You can setup different cursors for different actions.")]
        public Texture2D defaultCursor;
        public Texture2D attackCursor;
        public Texture2D gatherResourcesCursor;
        public Texture2D giveResourcesCursor;
        [Tooltip("Cursor when player tries to do something that not allowed")]
        public Texture2D restrictCursor;
        [Tooltip("Cursor when player orders units to move using minimap")]
        public Texture2D mapOrderCursor;
        public Texture2D repairCursor, sellCursor;

        [Header("Menu UI Templates")]
        public GameObject playerEntry;

        [Header("Misc")]
        public Material playerColorMaterialTemplate;

        [Tooltip("Link to Sounds Library data file, which will be used in game")]
        public SoundLibrary soundLibrary;
        [Tooltip("Link to Texts Library data file, which will be used in game")]
        public TextsLibrary textsLibrary;

        [Header("Layers")]
        [Tooltip("Units layer. Used for calculation in asset code")]
        public LayerMask unitLayerMask;
        [Tooltip("List of layers which will be obstacle for shooting units when aiming target")]
        public LayerMask obstalcesToUnitShoots;
        [Tooltip("List of layers which will be obstacle for shooting units when aiming target")]
        public LayerMask obstaclesToUnitShootsWithoutUnitLayer;

        public MapSettings GetMapBySceneName(string name)
        {
            for(int i = 0; i < availableMaps.Count; ++i)
            {
                if(availableMaps[i].mapSceneName == name)
                {
                    return availableMaps[i];
                }
            }
            throw new System.Exception("No map with name " + name + " found!");
        }

        public enum UnitsFormation
        {
            Default, SquarePredict
        }
    }

}