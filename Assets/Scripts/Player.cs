using PromiseCode.RTS.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PromiseCode.RTS
{
    [System.Serializable]
    public class Player 
    {
        public static byte localPlayerId = 0;

        public delegate void PlayerMoneyChangedAction(int newMoneyValue);
        public delegate void PlayerElectricityChangedAction(int totalElectricity, int usedElectricity);
        public static event PlayerMoneyChangedAction localPlayerMoneyChangedEvent;
        public static event PlayerElectricityChangedAction localPlayerElectricityChangedEvent;

        public string userName;
        public Color color;
        public FactionData selectedFaction;
        public byte id;
        public byte teamIndex;

        public int money = 10000;
        public int electricity, usedElectricity;

        public bool isAIPlayer;
        public bool isDefeated = false;

        public List<Production> playerProductionBuildings = new List<Production>();
        public readonly Material playerMaterial;

        public Player(Color color)
        {
            playerProductionBuildings = new List<Production>();
            this.color = color;

            playerMaterial = new Material(GameController.instance.MainStorage.playerColorMaterialTemplate);
            playerMaterial.color = color;
        }

        public bool IsHaveMoney(int amount) => money >= amount;
        public void AddMoney(int amount)
        {
            money += amount;

            if(IsLocalPlayer())
            {
                localPlayerMoneyChangedEvent?.Invoke(money);
            }
        }

        public void SpendMoney(int amount)
        {
            money = Mathf.Clamp(money - amount, 0, 1000000);

            if(IsLocalPlayer())
            {
                localPlayerMoneyChangedEvent?.Invoke(money);
            }
        }

        public void AddElectricity(int amount)
        {
            electricity += amount;

            if(IsLocalPlayer())
            {
                localPlayerElectricityChangedEvent?.Invoke(electricity, usedElectricity);
            }
        }

        public void RemoveElectricity(int amount)
        {
            electricity -= amount;

            if(IsLocalPlayer())
            {
                localPlayerElectricityChangedEvent?.Invoke(electricity, usedElectricity);
            }
        }

        public void AddUsedElectricity(int amount)
        {
            usedElectricity += amount;
            if(IsLocalPlayer())
            {
                localPlayerElectricityChangedEvent?.Invoke(electricity, usedElectricity);
            }
        }

        public void RemoveUsedElectricity(int amount)
        {
            usedElectricity = Mathf.Clamp(usedElectricity - amount, 0, 9999);

            if(IsLocalPlayer())
            {
                localPlayerElectricityChangedEvent?.Invoke(electricity, usedElectricity);
            }
        }

        public float GetElectricityUsagePercent() => usedElectricity / (float)electricity;
        public bool IsLocalPlayer() => id == localPlayerId;
        public void AddProduction(Production production) => playerProductionBuildings.Add(production);
        public void RemoveProduction(Production production) => playerProductionBuildings.Remove(production);

        public void DefeatPlayer()
        {
            if(isDefeated)
            {
                return;
            }
            isDefeated = true;

            for(int i = Unit.allUnits.Count - 1; i >= 0; --i)
            {
                if(Unit.allUnits[i] && Unit.allUnits[i].OwnerPlayerId == id)
                {
                    var damageable = Unit.allUnits[i].GetModule<Damageable>();
                    if(damageable)
                    {
                        damageable.TakeDamage(99999);
                    }
                }
            }
            if(IsLocalPlayer())
            {
                UICharInfo.UIController.instance.ShowDefeatScreen();
            }
        }

        public List<Production> GetProductionBuildingsByCategory(ProductionCategory category)
        {
            var resultList = new List<Production>();

            resultList.AddRange(playerProductionBuildings.Where(p => p && p.GetProductionCategory == category));
            return resultList;
        }

        public bool IsHaveProductionOfCategory(ProductionCategory category)
        {
            return GetProductionBuildingsByCategory(category).Count > 0;
        }

        public List<Player> GetEnemyPlayers()
        {
            var allPlayers = GameController.instance.playersController.playersInGame;
            var enemyPlayers = new List<Player>();

            enemyPlayers.AddRange(allPlayers.Where(p => p != this && p.teamIndex != teamIndex));
            return enemyPlayers;
        }

        public static Player GetPlayerById(byte playerId) => GameController.instance.playersController.playersInGame[playerId];
        public static Player GetLocalPlayer() => GetPlayerById(localPlayerId);
    }

}