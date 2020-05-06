using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PromiseCode.RTS.UI
{
    public class PlayerEntry : MonoBehaviour
    {
        public PlayerSettings selfPlayerSettings { get; protected set; }

        [SerializeField] Text nickNameText;
        [SerializeField] ColorDropdown colorDropdown;
        [SerializeField] UI.FactionDropdown factionDropdown;
        [SerializeField] Dropdown teamDropdown;
        [SerializeField] Button removeButton;

        Lobby parentLobby;


    }

}
