using PromiseCode.RTS.Storing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PromiseCode.RTS.Units
{
	/// <summary> Production module allows unit to produce other units. Usually it should be added to the buildings like Factory etc. </summary>
	public class Production : Module
	{
		public static event ProductionAction productionModuleSpawned, productionSelected, productionUnselected;
		public delegate void ProductionAction(Production productionModule);

		[Tooltip("Id of production category in UnitData settings")]
		[SerializeField, Range(0, 4)] int categoryId;
		[Tooltip("Point where units will be appeared after building finished.")]
		[SerializeField] Transform spawnPoint;
		[Tooltip("Point where units will move after spawn.")]
		[SerializeField] Transform moveWaypoint;

		bool isBuildingNow;
		ProductionCategory productionCategory;

		public List<UnitData> unitsQueue { get; protected set; }
		public float timeToBuildCurrentUnit { get; protected set; }

		public List<UnitData> AvailableUnits { get { return productionCategory.availableUnits; } }
		public ProductionCategory GetProductionCategory { get { return productionCategory; } }
		public Transform SpawnPoint { get { return spawnPoint; } }
		public Transform SpawnWaypoint { get { return moveWaypoint; } }

		protected override void AwakeAction()
		{
			unitsQueue = new List<UnitData>();
			timeToBuildCurrentUnit = 999f;
		}

		void Start()
		{
			if (selfUnit.data.productionCategories.Count <= categoryId)
			{
				Debug.LogWarning("[Production module] Your unit " + name + " have incorrectly setted up Production categories.");
				enabled = false;
				return;
			}

			productionCategory = selfUnit.data.productionCategories[categoryId];

			if (productionModuleSpawned != null)
				productionModuleSpawned.Invoke(this);
		}

		void Update()
		{
			if (isBuildingNow)
				HandleProductionProgress();
			else if (unitsQueue.Count > 0)
				StartProduction();
		}

		void HandleProductionProgress()
		{
			if (timeToBuildCurrentUnit > 0)
			{
				timeToBuildCurrentUnit -= Time.deltaTime * GetBuildingSpeedCoefficient();
				return;
			}

			if (!productionCategory.isBuildings)
			{
				SpawnCurrentUnit(unitsQueue[0]);
				FinishBuilding();
			}
		}

		void SpawnCurrentUnit(UnitData unitData)
		{
			var spawnedUnit = SpawnController.SpawnUnit(unitData, selfUnit.OwnerPlayerId, spawnPoint);

			if (spawnedUnit.GetComponent<Harvester>()) // resource harvesters have their own code to move to the resource field.
				return;

			var order = new MovePositionOrder
			{
				movePosition = moveWaypoint.position
			};

			if (unitData.moveType == UnitData.MoveType.Flying)
				order.movePosition += new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));

			spawnedUnit.AddOrder(order, false);

			ShuffleUnitsOnExit(order.movePosition, selfUnit);
		}

		void StartProduction()
		{
			isBuildingNow = true;
			timeToBuildCurrentUnit = unitsQueue[0].buildTime;
		}

		public void AddUnitToQueue(UnitData unitData)
		{
			if (unitData.isBuilding && unitsQueue.Count > 0)
				return;

			var playerOwner = Player.GetPlayerById(selfUnit.OwnerPlayerId);
			if (playerOwner.IsHaveMoney(unitData.price))
			{
				unitsQueue.Add(unitData);
				playerOwner.SpendMoney(unitData.price);
			}
		}

		public void AddUnitToQueueByIndex(int index, int productionCategoryId = 0)
		{
			var prodCats = selfUnit.data.productionCategories;

			productionCategoryId = Mathf.Clamp(productionCategoryId, 0, prodCats.Count - 1);

			var category = prodCats[productionCategoryId];

			if (index > category.availableUnits.Count - 1)
				return;

			index = Mathf.Clamp(index, 0, category.availableUnits.Count - 1);

			AddUnitToQueue(category.availableUnits[index]);
		}

		public void RemoveUnitFromQueue(UnitData unitData, bool isCancel)
		{
			if(unitsQueue.Count == 0)
			{
				return;
			}

			bool isFirstUnitTypeLikeThis = unitsQueue[0] == unitData;
			if(unitsQueue.Remove(unitData) && isFirstUnitTypeLikeThis)
			{
				isBuildingNow = false;
			}
			if(isCancel)
			{
				Player playerOwner = Player.GetPlayerById(selfUnit.OwnerPlayerId);
				playerOwner.AddMoney(unitData.price);
			}
		}

		public int GetUnitsOfSpecificTypeInQueue(UnitData unitData)
		{
			return unitsQueue.Count > 0 ? unitsQueue.Count(unit => unit == unitData) : 0;
		}

		public bool IsUnitOfTypeCurrentlyBuilding(UnitData unitData) => unitsQueue.Count > 0 && unitsQueue[0] == unitData;
		public bool IsUnitOfTypeInQueue(UnitData unitData) => unitsQueue.Count > 0 && unitsQueue.Contains(unitData);
		public float GetBuildProgressPercents() => 1 - timeToBuildCurrentUnit / unitsQueue[0].buildTime;
		public bool IsBuildingReady() => timeToBuildCurrentUnit <= 0;
		public void FinishBuilding()
		{
			if(unitsQueue.Count > 0)
			{
				unitsQueue.RemoveAt(0);
			}
			isBuildingNow = false;
		}

		public void OnSelected() => productionSelected?.Invoke(this);
		public void OnUnselected() => productionUnselected?.Invoke(this);

		public int GetProductionNumber()
		{
			var productionOfThisType = selfUnit.GetOwnerPlayer().GetProductionBuildingsByCategory(productionCategory);
			return productionCategory.IndexOf(this);
		}

		public void ShuffleUnitsOnExit(Vector3 origin, Unit askedFromUnit, int depth = 0)
		{
			var colliders = Physics.OverlapCapsule(origin, origin + Vector3.up * 20f, 1f);
			for(int i = 0; i < colliders.Length; ++i)
			{
				var unit = colliders[i].GetComponent<Unit>();
				if(unit && askedFromUnit != unit && unit.IsInMyTeam(selfUnit) && unit.data.moveType == askedFromUnit.data.moveType)
				{
					int randomizeDirection = Random.Range(0, 2);
					var direction = randomizeDirection == 0 ? 1 : -1;

					var moveOrder = new MovePositionOrder
					{
						movePosition = unit.transform.position + (3 * direction * unit.transform.right)
					};
					if(depth < 2)
					{
						ShuffleUnitsOnExit(moveOrder.movePosition, unit, depth + 1);
					}
					unit.AddOrder(moveOrder, false);
				}
			}
		}

		float GetBuildingSpeedCoefficient()
		{
			var storage = GameController.instance.MainStorage;
			if(!storage.isElectricityUsedInGame || selfUnit.GetOwnerPlayer().GetElectricityUsagePercent() < 1)
			{
				return 1;
			}
			return 1 * GameController.instance.MainStorage.speedCoefForProductionWithoutElectricity;
		}

		public void SetCategoryId(int id) => categoryId = id;
	}
}