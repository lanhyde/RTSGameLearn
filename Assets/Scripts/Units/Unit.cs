using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using PromiseCode.RTS.Storing;
using PromiseCode.RTS.Controls;
using PromiseCode.RTS.UI;

namespace PromiseCode.RTS.Units
{
    [RequireComponent(typeof(BoxCollider))]
    public class Unit : MonoBehaviour
    {
        public static List<Unit> allUnits { get; private set; }
        public static event UnitAction unitSpawnedEvent, unitHoveredEvent, unitUnhoveredEvent, unitDestroyEvent;
        public static event UnitChangedOwnerAction unitChangedOwnerEvent;
        public event UnitOrderReceived unitReceivedOrderEvent;

        public delegate void UnitAction(Unit unit);
        public delegate void UnitChangedOwnerAction(Unit unit, int newOwner);
        public delegate void UnitOrderReceived(Unit unit, Order order);

        [Tooltip("Unit will get parameters from Unit Data file, so move data of this unit to this field.")]
        [SerializeField] UnitData unitData;
        [Tooltip("List of renderers, which should be colored in team colors. You can also select number of material to colorize, if mesh have several materials.")]
        [SerializeField] List<ColoredRenderer> coloredRenderers;
        [SerializeField] [Range(0, 255)] byte ownerPlayerId;
        [Tooltip("This sound source will be used to play unit sounds like shoot effects, etc.")]
        [SerializeField] AudioSource unitSoundSource;

        public UnitData data => unitData;
        public Damageable damageable { get; protected set; }
        public Movable movable { get; protected set; }
        public Attackable attackable { get; protected set; }
        public Tower tower { get; protected set; }
        public Production production { get; protected set; }
        public int unitSelectionGroup { get; protected set; }

        readonly List<Module> modules = new List<Module>();
        public List<Order> orders { get; protected set; }
        public byte OwnerPlayerId
        {
            get => ownerPlayerId;
            set => ownerPlayerId = value;
        }

        public bool isSelected { get; protected set; }
        public bool isBeingCarried { get; protected set; }
        public bool isMovementLockedByHotkey { get; set; }

        bool isHovered;
        GameObject selectionIndicator;
        GameObject model;

        void Awake()
        {
            orders = new List<Order>();

            if (allUnits == null)
            {
                allUnits = new List<Unit>();
            }
            allUnits.Add(this);

            damageable = GetComponent<Damageable>();
            movable = GetComponent<Movable>();
            attackable = GetComponent<Attackable>();
            tower = GetComponent<Tower>();
            production = GetComponent<Production>();
            unitSelectionGroup = -1;

            if (data.moveType == UnitData.MoveType.Flying)
            {
                GetComponent<BoxCollider>().isTrigger = true;
                var newRigidbody = GetComponent<Rigidbody>();
                if (!newRigidbody)
                {
                    newRigidbody = gameObject.AddComponent<Rigidbody>();
                }
                newRigidbody.isKinematic = true;
            }
            // TODO: Check for abilities on unit before adding
            if (!GetComponent<AbilitiesModule>())
            {
                gameObject.AddComponent<AbilitiesModule>();
            }
            var transformModel = transform.Find("Model");

            if (transformModel)
            {
                model = transform.gameObject;
            }
            else
            {
                Debug.LogWarning("Unit " + name + " doesn't have Model child object which should contain mesh etc.");
            }
        }

        void Start()
        {
            if (!unitSoundSource)
            {
                unitSoundSource = GetComponent<AudioSource>();
            }
            if (unitSoundSource)
            {
                unitSoundSource.dopplerLevel = 0;
            }

            selectionIndicator = Instantiate(GameController.instance.MainStorage.selectionIndicatorTemplate, transform);
            selectionIndicator.transform.localScale = Vector3.one * (Mathf.Max(GetComponent<BoxCollider>().size.x, GetComponent<BoxCollider>().size.z) + 0.1f);
            selectionIndicator.SetActive(false);

            if (damageable)
            {
                Damageable.onDamageableDied += UnitDiedAction;
            }

            UpdateColorByOwner();

            if (data.isBuilding)
            {
                if (GameController.instance.MainStorage.addNavMeshObstacleToBuildings && !GetComponent<NavMeshObstacle>())
                {
                    var navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();

                    navMeshObstacle.shape = NavMeshObstacleShape.Box;

                    var boxCollider = GetComponent<BoxCollider>();
                    if (boxCollider)
                    {
                        navMeshObstacle.center = boxCollider.center;
                        navMeshObstacle.size = boxCollider.size;
                    }
                    navMeshObstacle.carving = true;
                }
            }

            if ((data.addsElectricity > 0 || data.usesElectricity > 0) && GameController.instance.MainStorage.isElectricityUsedInGame)
            {
                gameObject.AddComponent<ElectricityModule>();
            }
            unitSpawnedEvent?.Invoke(this);
            if (GameController.instance.MainStorage.isFogOfWarOn && !GetComponent<FogOfWarModule>())
            {
                gameObject.AddComponent<FogOfWarModule>();
            }
        }

        void Update()
        {
            if (HasOrders())
            {
                orders[0].Execute();
            }
        }

        public void AddOrder(Order order, bool isAdditive, bool isSoundNeeded = true, bool isReceivedEventNeeded = true)
        {
            if (!isAdditive)
            {
                orders.Clear();
            }
            var personalOrder = order.Clone();
            personalOrder.executor = this;

            orders.Add(personalOrder);

            if (isReceivedEventNeeded)
            {
                unitReceivedOrderEvent?.Invoke(this, order);
            }
            if (isSoundNeeded)
            {
                PlayOrderSound();
            }
        }

        public void SetOwner(byte playerId)
        {
            ownerPlayerId = playerId;
            UpdateColorByOwner();
            Unselect();
            Selection.UnselectUnit(this);

            unitChangedOwnerEvent?.Invoke(this, playerId);
        }

        void UpdateColorByOwner()
        {
            var material = GameController.instance.playersController.playersInGame[ownerPlayerId].playerMaterial;

            for (int i = 0; i < coloredRenderers.Count; ++i)
            {
                if (coloredRenderers[i].usesHouseColorShader)
                {
                    coloredRenderers[i].SetColor(GameController.instance.playersController.playersInGame[ownerPlayerId].color);
                }
                else
                {
                    coloredRenderers[i].SetMaterial(material);
                }
            }
        }

        public void EndCurrentOrder()
        {
            if (HasOrders())
            {
                orders.RemoveAt(0);
            }

            movable?.Stop();
        }

        public void EndAllOrders()
        {
            if (HasOrders())
            {
                orders.Clear();
            }
            movable?.Stop();
        }

        public void Select(bool isSoundNeeded = true)
        {
            isSelected = true;
            if (selectionIndicator)
            {
                selectionIndicator.SetActive(true);
            }
            if (isSoundNeeded)
            {
                PlaySelectionSound();
            }
            if (production)
            {
                production.OnSelected();
            }
            var harvester = GetModule<Harvester>();
            if (harvester)
            {
                UI.HarvesterBar.SpawnForHarvester(harvester);
            }
        }

        public void Unselect()
        {
            isSelected = false;
            if (selectionIndicator)
            {
                selectionIndicator.SetActive(false);
            }
            if (production)
            {
                production.OnUnselected();
            }
            var harvester = GetModule<Harvester>();
            if (harvester)
            {
                UI.HarvesterBar.RemoveBarOfHarvester(harvester);
            }
        }

        void UnitDiedAction(Unit unit)
        {
            if (unit != this)
            {
                return;
            }

            // TODO: Move it to carry module?
            var carryModule = GetModule<CarryModule>();
            if (carryModule)
            {
                carryModule.ExitAllUnits(true);
            }
            allUnits.Remove(unit);

            if (unitData.isBuilding)
            {
                // TODO: move to destroy event?
                GameController.instance.CheckWinConditions();
            }
            unitDestroyEvent?.Invoke(this);
        }

        public bool HasOrders() => orders != null && orders.Count > 0;
        public bool IsOwnedByPlayer(int playerId) => ownerPlayerId == playerId;
        public Player GetOwnerPlayer() => Player.GetPlayerById(ownerPlayerId);
        public bool IsInMyTeam(Unit other) => GameController.instance.playersController.IsPlayersInOneTeam(ownerPlayerId, other.ownerPlayerId);
        public bool IsInMyTeam(byte otherPlayerId) => GameController.instance.playersController.IsPlayersInOneTeam(ownerPlayerId, otherPlayerId);
        public void PlaySelectionSound() => PlayUnitSound(data.selectionSoundVariations);
        public void PlayOrderSound()
        {
            if (ownerPlayerId == Player.localPlayerId)
            {
                PlayUnitSound(data.orderSoundVariations);
            }
        }
        public void PlayCustomSound(AudioClip audioClip) => PlayUnitSound(new AudioClip[1] { audioClip });
        public void PlayShootSound()
        {
            PlayUnitSound(data.shootSoundVariations, Random.Range(-data.shootSoundPitchRandomization, data.shootSoundPitchRandomization));
        }

        void PlayUnitSound(AudioClip[] clipVariations, float randomPitch = 0)
        {
            if (!unitSoundSource || clipVariations.Length == 0 || unitSoundSource.isPlaying)
            {
                return;
            }
            unitSoundSource.pitch = 1 + randomPitch;
            unitSoundSource.clip = clipVariations[Random.Range(0, clipVariations.Length)];
            unitSoundSource.Play();
        }

        public Vector3 GetSize()
        {
            var collider = GetComponent<Collider>();

            if (collider is BoxCollider)
            {
                return (collider as BoxCollider).size;
            }
            else if (collider is SphereCollider)
            {
                return (collider as SphereCollider).radius * Vector3.one;
            }
            else
            {
                return Vector3.zero;
            }
        }

        public Vector3 GetCenterPoint()
        {
            return transform.position + transform.up * GetSize().y / 2f;
        }
        public void RegisterModule(Module module)
        {
            modules.Add(module);
        }
        public T GetModule<T>() where T : Module
        {
            for (int i = 0; i < modules.Count; ++i)
            {
                if (modules[i].GetType() == typeof(T))
                {
                    return modules[i] as T;
                }
            }
            return default(T);
        }

        public void SetUnitSelectionGroup(int value)
        {
            if(unitSelectionGroup == value)
            {
                unitSelectionGroup = -1;
            }
            else
            {
                unitSelectionGroup = value;
            }
        }

        void OnDestroy()
        {
            Damageable.onDamageableDied -= UnitDiedAction;
            if(isHovered)
            {
                Cursors.SetDefaultCursor();
            }
        }
        public void SetUnitData(UnitData unitData) => this.unitData = unitData;
        public void OnMouseEnter()
        {
            isHovered = true;
            unitHoveredEvent?.Invoke();

            if(Selection.selectedUnits.Count == 0 || !Selection.selectedUnits[0])
            {
                return;
            }

            var mainUnit = Selection.selectedUnits[0];
            if(!IsInMyTeam(Player.GetLocalPlayer().teamIndex) && mainUnit.data.hasAttackModule)
            {
                var fowModule = GetModule<FogOfWarModule>();

                if(!fowModule || fowModule.isVisibleInFOW)
                {
                    Cursors.SetAttackCursor();
                }
            }
            if(IsInMyTeam(Player.GetLocalPlayer().teamIndex) && mainUnit.data.isHarvester && data.isRefinery)
            {
                Cursors.SetGiveResourcesCursor();
            }
            if(data.canCarryUnitsCount > 0 && IsInMyTeam(Player.GetLocalPlayer().teamIndex))
            {
                bool anyCanBeCarries = false;
                for(int i = 0; i < Selection.selectedUnits.Count; ++i)
                {
                    if(Selection.selectedUnits[i] && Selection.selectedUnits[i].data.canBeCarried)
                    {
                        anyCanBeCarries = true;
                        break;
                    }
                }

                if(anyCanBeCarries)
                {
                    var carryModule = GetModule<CarryModule>();
                    if(carryModule && carryModule.CanCarryOneMoreUnit())
                    {
                        Cursors.SetGiveResourcesCursor();
                    }
                }
            }
        }

        public void OnMouseExit()
        {
            isHovered = false;
            unitUnhoveredEvent?.Invoke(this);

            Cursors.SetDefaultCursor();
        }

        public bool IsUnitVisibleOnScreen()
        {
            return GetComponent<Renderer>().isVisible;
        }

        public bool IsVisibleInViewport()
        {
            var coords = GetViewportPosition(GameController.cachedMainCamera);
            return coords.x > 0 && coords.x < 1 && coords.y > 0 && coords.y < 1;
        }

        public Vector2 GetViewportPosition(Camera forCamera)
        {
            return forCamera.WorldToViewportPoint(transform.position);
        }

        public void SetCarryState(bool isCarried)
        {
            isBeingCarried = isCarried;
            if(isCarried)
            {
                Selection.UnselectUnit(this);
            }
            var active = !isCarried;

            if(model)
            {
                model.SetActive(active);
            }

            GetModule<Movable>().enabled = active;
            GetComponent<NavMeshAgent>().enabled = active;
            GetComponent<Collider>().enabled = active;
        }
    }
}