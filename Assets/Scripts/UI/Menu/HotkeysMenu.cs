using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UniRx;
using System;

namespace PromiseCode.RTS.UI.Menu
{
    public class HotkeysMenu : MonoBehaviour
    {
        [SerializeField] GameObject pressAnyKeyText;
        HotkeySelectEntry[] hotkeyEntries;

        private void Start()
        {
            pressAnyKeyText.SetActive(false);
            hotkeyEntries = FindObjectOfType<HotkeySelectEntry>();
        }

        public void SetPressTextState(bool isEnabled)
        {
            pressAnyKeyText.SetActive(isEnabled);
        }

        public void BackToLobby()
        {
            SceneManager.LoadScene("Lobby");
        }

        public void ResetToDefault()
        {
            Keymap.loadedKeymap.SetupDefaultScheme();
            for(var i = 0; i <hotkeyEntries.Length; ++i)
            {
                hotkeyEntries[i].Reload();
            }
        }
    }

}