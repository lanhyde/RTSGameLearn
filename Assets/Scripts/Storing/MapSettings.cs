using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PromiseCode.RTS.Storing
{
    [CreateAssetMenu(fileName = "MapSettings", menuName = "RTS/Map Settings")]
    public class MapSettings : ScriptableObject
    {
        [Tooltip("Name of scene object of map. Note that this scene object should be added to Build Settings to work properly")]
        public string mapSceneName;
        [Tooltip("Max players count which can play on this map. This value should be similar (or smaller) to map Player Start Points count")]
        [Range(1, 16)] public int maxMapPlayerCount = 4;
        public Sprite mapPreviewImage;
        [Range(16, 1024)] public int mapSize = 256;

        [Tooltip("If you need ambient sound or music on your map, place sounds in this array, and they will be played randomly during game match on this map")]
        public AudioClip[] ambientSoundTracks;

        [Header("Single-Player parameters")]
        [Tooltip("toggle this, if you're working on single-player and this map should work as single-player (run without lobby)")]
        public bool isThisMapForSinglePlayer;
        [Tooltip("Setup all players parameters on this map. You can set colors, teams, etc. Note that one player should be non-AI")]
        public List<PlayerSettings> playerSettingsForSinglePlayer;
        [Tooltip("Do you want game to auto-spawn player command center buildings on default map spawn points on start? Set this to false to single-player games, if you have custom player base")]
        public bool autoSpawnPlayerStabs = true;

        public void LoadMap()
        {
            SceneManager.LoadScene(mapSceneName);
        }
    }
}
