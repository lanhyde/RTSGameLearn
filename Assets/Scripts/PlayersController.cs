using PromiseCode.RTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS
{
    public class PlayersController : MonoBehaviour
    {
        public List<Player> playersInGame { get; protected set; }
        public static event Production.ProductionAction productionModuleAddedToPlayer;
        /// <summary>
        /// Should be called from GameController's Awake before player's initialization
        /// </summary>
        public void PreAwake()
        {
            Production.productionModuleSpawned += AddProductionBuildingToPlayer;
        }

        void AddProductionBuildingToPlayer(Production productionModule)
        {
            var playerOwner = productionModule.selfUnit.OwnerPlayerId;
            if(!playersInGame[playerOwner].playerProductionBuildings.Contains(productionModule))
            {
                playersInGame[playerOwner].AddProduction(productionModule);
                productionModuleAddedToPlayer?.Invoke(productionModule);
            }
        }

        public bool IsPlayersInOneTeam(byte playerAId, byte playerBId)
        {
            if(playersInGame.Count <= playerAId || playersInGame.Count <= playerBId)
            {
                return false;
            }
            return playersInGame[playerAId].teamIndex == playersInGame[playerBId].teamIndex;
        }

        public void AddPlayer(Player player)
        {
            if(playersInGame == null)
            {
                playersInGame = new List<Player>();
            }
            player.id = (byte)playersInGame.Count;
            playersInGame.Add(player);
        }

        private void OnDestroy()
        {
            Production.productionModuleSpawned -= AddProductionBuildingToPlayer;
        }
    }
}