using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PromiseCode.RTS.UI.Menu
{
    public class PlayerEntry : MonoBehaviour
    {
        public PlayerSettings selfPlayerSettings { get; protected set; }

        [SerializeField] Text nickNameText;
        [SerializeField] ColorDropdown colorDropdown;
        [SerializeField] FactionDropdown factionDropdown;
        [SerializeField] Dropdown teamDropdown;
        [SerializeField] Button removeButton;

        Lobby parentLobby;

        public void SetupWithPlayerSettings(PlayerSettings playerSettings, Lobby fromLobby)
        {
            selfPlayerSettings = playerSettings;

        }
    }

}
