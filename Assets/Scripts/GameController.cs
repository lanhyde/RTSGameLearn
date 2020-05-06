using Mirror.Examples.Additive;
using PromiseCode.RTS.Storing;
using PromiseCode.RTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PromiseCode.RTS
{
    [RequireComponent(typeof(PlayersController))]
    public class GameController : MonoBehaviour
    {
        public static GameController instance { get; protected set; }
        public static Camera cachedMainCamera;

        public static bool isGamePaused { get; protected set; }

        [SerializeField] Storage storage;
        [SerializeField] AI.AISettings defaultAIPreset;
        [SerializeField] MapSettings mapSettings;
        [SerializeField] Renderer mapBorderRenderer;

        public PlayersController playersController { get; protected set; }
        public Controls.Build build { get; protected set; }
        public Controls.CameraMover cameraMover { get; protected set; }
        public TextsLibrary textsLibrary { get; protected set; }
        public Storage MainStorage => storage;
        public MapSettings MapSettings => mapSettings;

        bool isGameInitialized;
        bool isAIInitialized;

        void Awake()
        {
            instance = this;

            playersController = GetComponent<PlayersController>();
            build = GetComponent<Controls.Build>();

            cameraMover = FindObjectOfType<Controls.CameraMover>();
            cachedMainCamera = Camera.main;

            playersController.PreAwake();

            if(mapSettings != null && mapSettings.isThisMapForSinglePlayer)
            {
                MatchSettings.currentMatchSettings = new MatchSettings();
                MatchSettings.currentMatchSettings.playersSettings = mapSettings.playerSettingsForSinglePlayer;
                MatchSettings.currentMatchSettings.SelectMap(mapSettings);
            }
            else if(MatchSettings.currentMatchSettings == null)
            {
                Debug.LogWarning("<b>You can run non-single-player map only from Lobby.</b> To test map correctly -save it, open Lobby scene, select players, and run map");
                SceneManager.LoadScene(0);
                return;
            }
            if(mapSettings == null)
            {
                mapSettings = MatchSettings.currentMatchSettings.selectedMap;
            }
            Controls.Selection.Initialize();

            if(Unit.allUnits != null)
            {
                Unit.allUnits.Clear();
            }
            UI.HealthBar.ResetPool();

            InitializePlayers();
            if(mapBorderRenderer)
            {
                mapBorderRenderer.material.SetInt("_MapSize", MatchSettings.currentMatchSettings.selectedMap.mapSize);
                if(!MainStorage.showMapBorders)
                {
                    mapBorderRenderer.enabled = false;
                }
            }
            textsLibrary = storage.textsLibrary;
            if(!textsLibrary)
            {
                Debug.LogWarning("<b>No Texts Library added to the Storage.</b> Please add it otherwise possible game texts problems.");
            }
        }

        void Update()
        {
            if(!isGameInitialized)
            {
                isGameInitialized = true;
                UI.UIController.instance.OnGameInitialized();

                if(!isAIInitialized)
                {
                    InitializeAI();
                }
                return;
            }
            Controls.Selection.Update();
        }

        void InitializePlayers()
        {
            SpawnController.InitializeStartPoints();

            for(int i = 0; i < MatchSettings.currentMatchSettings.playersSettings.Count; ++i)
            {
                var currentPlayerSettings = MatchSettings.currentMatchSettings.playersSettings[i];

                Player player = new Player(currentPlayerSettings.color);
                player.teamIndex = currentPlayerSettings.team;
                player.selectedFaction = currentPlayerSettings.selectedFaction;
                player.isAIPlayer = currentPlayerSettings.isAI;

                if(MapSettings.isThisMapForSinglePlayer)
                {
                    player.money = currentPlayerSettings.startMoneyForSinglePlayer;
                }
                else
                {
                    player.money = MainStorage.startPlayerMoney;
                }
                playersController.AddPlayer(player);
            }
            if(mapSettings.autoSpawnPlayerStabs)
            {
                for(int i = 0; i < playersController.playersInGame.Count; ++i)
                {
                    SpawnController.SpawnPlayerStab((byte)i);
                }
            }
        }

        void InitializeAI()
        {
            for(int i = 0; i < playersController.playersInGame.Count; ++i)
            {
                if(playersController.playersInGame[i].isAIPlayer)
                {
                    var aiObject = new GameObject("AI Controller" + i);
                    var aiController = aiObject.AddComponent<AI.AIController>();
                    aiController.SetupWithAISettings(defaultAIPreset);
                    aiController.SetupAIForPlayer((byte)i);
                }
            }
            isAIInitialized = true;
        }

        public void CheckWinConditions()
        {
            var allBuildings = Unit.allUnits.FindAll(unit => unit.data.isBuilding);
            for(int i = 0; i < playersController.playersInGame.Count; ++i)
            {
                if(!allBuildings.Find(Unit => Unit.OwnerPlayerId == i))
                {
                    playersController.playersInGame[i].DefeatPlayer();
                }
            }

            var allUndefeatedPlayers = playersController.playersInGame.FindAll(player => !player.isDefeated);

            int winnerTeam = 0;
            for(int i =0; i < allUndefeatedPlayers.Count; ++i)
            {
                if(i == 0)
                {
                    winnerTeam = allUndefeatedPlayers[i].teamIndex;
                }
                else if(allUndefeatedPlayers[i].teamIndex != winnerTeam)
                {
                    return;
                }
            }

            if(!Player.GetLocalPlayer().isDefeated)
            {
                UI.UIController.instance.ShowWinScreen();
            }
        }
        public void ReturnToLobby()
        {
            Cursors.SetDefaultCursor();
            SceneManager.LoadScene("Lobby");
        }

        void OnDestroy()
        {
            instance = null;    
        }

        public static void SetPauseState(bool isPaused)
        {
            isGamePaused = isPaused;
            Time.timeScale = isGamePaused ? 0f : 1f;
        }
    }

}