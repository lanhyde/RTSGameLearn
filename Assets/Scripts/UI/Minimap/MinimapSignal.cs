using System.Collections;
using System.Collections.Generic;
using PromiseCode.RTS.Units;
using UnityEngine;

namespace PromiseCode.RTS.UI.Minimap
{
    public class MinimapSignal : MonoBehaviour
    {
        private const float soundWaitTime = 7f;

        private Minimap minimap;
        private AudioSource audioSource;

        private float soundTimer;

        readonly  List<UnitTimer> attackedUnits = new List<UnitTimer>();

        void Start()
        {
            minimap = FindObjectOfType<Minimap>();

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = GameController.instance.MainStorage.soundLibrary.GetSoundByPath("UI/MinimapAttackSound");
            audioSource.spatialBlend = 0f;
        }

        void Update()
        {
            if (soundTimer > 0)
            {
                soundTimer -= Time.deltaTime;
            }

            for (int i = attackedUnits.Count - 1; i >= 0; --i)
            {
                attackedUnits[i].Tick();

                if (attackedUnits[i].IsFinished())
                {
                    attackedUnits.RemoveAt(i);
                }
            }
        }

        public void ShowFor(Unit unit)
        {
            if (attackedUnits.Find(ut => ut.unit == unit) != null)
            {
                return;
            }
            attackedUnits.Add(new UnitTimer(unit, 5f));
            var positionOnMap = minimap.GetUnitOnMapPoint(unit, true);
            var spawnedSignal = Instantiate(GameController.instance.MainStorage.minimapSignalTemplate,
                minimap.IconsPanel);
            spawnedSignal.GetComponent<RectTransform>().anchoredPosition = positionOnMap;

            if (soundTimer <= 0)
            {
                soundTimer = soundTimer;
                audioSource.Play();
            }
        }
    }

    /// <summary>
    /// This timer class needed to count, which unit was attacked N seconds before.It prevents minimap signal every time unit being damaged.
    /// </summary>
    public class UnitTimer
    {
        public Unit unit;
        public float timer;

        public UnitTimer(Unit unit, float time)
        {
            this.unit = unit;
            this.timer = time;
        }

        public void Tick() => timer -= Time.deltaTime;
        public bool IsFinished() => timer <= 0;
    }
}
