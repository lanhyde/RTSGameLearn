using System.Collections;
using System.Collections.Generic;
using PromiseCode.RTS.Storing;
using PromiseCode.RTS.Units;
using UnityEngine;

namespace PromiseCode.RTS
{
    public class SpawnController
    {
        static List<PlayerStartPoint> playerStartPoints;
        public static void InitializeStartPoints()
        {
            playerStartPoints = new List<PlayerStartPoint>(GameObject.FindObjectsOfType<PlayerStartPoint>());
        }

        public static void SpawnPlayerStab(byte playerId)
        {
            if(playerStartPoints == null)
            {
                InitializeStartPoints();
            }
            if(playerStartPoints.Count == 0)
            {
                Debug.LogWarning("Could not find start points for this map.");
                return;
            }

            var specificPointToSpawnPlayer = playerStartPoints.Find(point => point.IsHardLockerPlayerOnThisPoint && point.IdOfPlayerToSpawn == playerId);
            int randomedPointId = Random.Range(0, playerStartPoints.Count);
            var selectedPoint = specificPointToSpawnPlayer ?? playerStartPoints[randomedPointId];

            playerStartPoints.Remove(selectedPoint);

            var stabToSpawn = Player.GetPlayerById(playerId).selectedFaction.factionCommandCenter;

            var spawnedStabObject = GameObject.Instantiate(stabToSpawn.selfPrefab, selectedPoint.transform.position, Quaternion.identity);

            var stab = spawnedStabObject.GetComponent<Unit>();
            stab.SetOwner(playerId);

            if(playerId == Player.localPlayerId)
            {
                GameController.instance.cameraMover.SetPosition(spawnedStabObject.transform.position);
            }
        }

        public static Unit SpawnUnit(UnitData unitData, byte playerOwner, Transform spawnPoint)
        {
            return SpawnUnit(unitData, playerOwner, spawnPoint.position, spawnPoint.rotation);
        }

        public static Unit SpawnUnit(UnitData unitData, byte playerOwner, Vector3 position, Quaternion rotation)
        {
            var spawnedUnitObject = GameObject.Instantiate(unitData.selfPrefab, position, rotation);
            var spawnedUnit = spawnedUnitObject.GetComponent<Unit>();

            spawnedUnit.SetOwner(playerOwner);

            return spawnedUnit;
        }
    }

}
