using PromiseCode.RTS.Storing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS
{
    [System.Serializable]
    public class PlayerSettings 
    {
        public string nickName = "Player";
        public byte team;
        public Color color = Color.white;
        public bool isAI;
        public FactionData selectedFaction;
        [Range(0, 100000)]
        public int startMoneyForSinglePlayer = 10000;

        public PlayerEntry playerLobbyEntry;

        public PlayerSettings(byte team, Color color, bool isAI = false)
        {
            this.team = team;
            this.color = color;
            this.isAI = isAI;
        }
    }

}