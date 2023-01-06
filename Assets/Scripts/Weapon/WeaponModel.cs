using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponSystem
{
    [Serializable]
    public class WeaponAnimator
    {
        [SerializeField] Animator animator;
        [SerializeField] CharacterAnimator mainAnimator;

        public float DrawAnimationLength 
        {
            get { return 0.3f; }    
        }
        internal void Initialize(FireMode fireMode)
        {
            mainAnimator?.Initialize(fireMode);
        }

        internal void InsertRound()
        {
            mainAnimator?.Insert(1f);
        }

        internal void OutOfAmmo()
        {

        }

        internal void Reload(bool roundInChamber)
        {
            mainAnimator?.Reload(roundInChamber, 0.6f);
        }

        internal void Shot(bool lastRound)
        {
            float speed = (lastRound) ? 0.5f : 1.0f;
            if (mainAnimator == null)
            {
                animator?.SetFloat("Speed", speed);
                animator?.CrossFadeInFixedTime("Recoil", 0.1f);
            }
            mainAnimator?.Shot(speed);
        }         

        internal void StartReload(bool roundInChamber)
        {
            mainAnimator?.StartInserting(1f);
        }

        internal void StopReload()
        {
            mainAnimator?.StopInserting(1f);
        }

        internal void Overheat()
        {
            mainAnimator?.Overheat(1.5f);
        }

        internal void Draw()
        {
            mainAnimator?.Draw();
        }

        internal void Hide()
        {
            mainAnimator?.Hide();
        }
    }
    public class WeaponModel : MonoBehaviour
    {
        public WeaponAnimator Animator => animator;
        public Transform HandPoint { get => handPoint; private set => handPoint = value; }
        public Transform ShootPoint { get => shootPoint; private set => shootPoint = value; }
        public Transform MuzzlePoint { get => muzzlePoint; set => muzzlePoint = value; }
        public Transform ShellPoint { get => shellPoint; private set => shellPoint = value; }

        public WaitForSeconds ReloadDuration { get; private set; }
        public WaitForSeconds CompleteReloadDuration { get; private set; }
        public WaitForSeconds InsertDuration { get; private set; }
        public WaitForSeconds StartReloadDuration { get; private set; }
        public WaitForSeconds InsertInChamberDuration { get; private set; }
        public WaitForSeconds StopReloadDuration { get; private set; }
        public WaitForSeconds CompleteReloadDropDuration { get; internal set; }
        public WaitForSeconds ReloadDropDuration { get; internal set; }

        public float StopReloadTime => stopReloadTime;

        [SerializeField] private WeaponAnimator animator;
        [Header("Points")]
        [SerializeField] private Transform handPoint;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private Transform shellPoint;
        [SerializeField] private Transform magazinePoint;

        [Header("Magazine")]
        [SerializeField] private GameObject magazinePrefab;
        [SerializeField] private bool magazineDrop;
        [SerializeField] private int maxCountMagazineDrop = 3;
        [Header("Shell")]
        [SerializeField] private ParticleSystem shellPrefab;

        [SerializeField] private Rigidbody2D character;

        private float reloadTime;
        private float insertTime;
        private float insertChamberTime;
        private float startReloadTime;
        private float stopReloadTime;
        private Weapon weapon;
        private SpriteRenderer render;
        private ParticleSystem shell;
        private int lastMagazine;
        private List<GameObject> magazineList = new List<GameObject>();

        public void Init(Weapon weapon)
        {
            this.weapon = weapon;
            render = GetComponentInChildren<SpriteRenderer>();
            UpdateReloadTime();
        }
        void UpdateReloadTime()
        {
            reloadTime = this.weapon.Data.ReloadTime;
            insertTime = (reloadTime / this.weapon.Data.Consume) * 0.5f;
            insertChamberTime = (reloadTime / this.weapon.Data.Consume) * 0.55f;
            startReloadTime = (reloadTime / this.weapon.Data.Consume) * 0.25f;
            stopReloadTime = (reloadTime / this.weapon.Data.Consume);

            ReloadDuration = new WaitForSeconds(reloadTime * 0.75f);
            CompleteReloadDuration = new WaitForSeconds(reloadTime);
            InsertDuration = new WaitForSeconds(insertTime);
            StartReloadDuration = new WaitForSeconds(startReloadTime);
            InsertInChamberDuration = new WaitForSeconds(insertChamberTime);
            StopReloadDuration = new WaitForSeconds(stopReloadTime);
            CompleteReloadDropDuration = new WaitForSeconds(reloadTime * 0.25f);
            ReloadDropDuration = new WaitForSeconds((reloadTime * 0.75f) * 0.5f);
        }

        internal void CreateMuzzle()
        {
            if (!weapon.Data.Ammo.PrefabMuzzle)
            {
                Debug.LogWarning($"Weapon [{weapon.Origin.Name}] not Prefab Muzzle!");
                return;
            }
            GameObject muzzle = Instantiate(weapon.Data.Ammo.PrefabMuzzle, muzzlePoint);
            Destroy(muzzle, 1f);
        }
        internal void CreateShellEjection(Transform parent)
        {
            if(shell) RemoveShellEjection();
            if(shellPrefab)
                shell = Instantiate(shellPrefab, shellPoint.transform.position, shellPrefab.transform.rotation, parent);
        }
        internal void CreateShell()
        {
            for(int i = 0; i< weapon.Data.Consume; i++)
                shell?.Play();
        }
        internal void RemoveShellEjection()
        {
            Destroy(shell.gameObject);
            shell = null;
        }
        public void DropMagazine()
        {
            if (!magazineDrop|| !magazinePrefab || !magazinePoint) return;

            GameObject magazine = null;
            if (magazineList.Count == maxCountMagazineDrop)
            {
                int magazineIndex = lastMagazine++ % maxCountMagazineDrop;
                magazineList[magazineIndex].transform.SetPositionAndRotation(magazinePoint.transform.position, magazinePoint.transform.rotation);
                magazine = magazineList[magazineIndex];
            }
            else
            {
                magazine = Instantiate(magazinePrefab, magazinePoint.transform.position, magazinePoint.transform.rotation);
                magazineList.Add(magazine);
            }
            var rigidbody2D = magazine.GetComponent<Rigidbody2D>();
            float ejectionTorque = 1 * UnityEngine.Random.value;
            float ejectionForce = 1.5f;
            Vector2 force = magazinePoint.up.normalized * ejectionForce;//(magazine.transform.right - magazine.transform.up) * ejectionForce;
            Debug.DrawLine(force, force * ejectionForce);
            rigidbody2D.AddForce(force, ForceMode2D.Impulse);
            rigidbody2D.AddTorque(ejectionTorque, ForceMode2D.Impulse);
        }
        internal void CreateTracer(Vector2 direction, float duration)
        {
            if (!weapon.Data.Ammo.PrefabTracer)
            {
                Debug.LogWarning($"Weapon [{weapon.Origin.Name}] not Prefab Tracer!");
                return;
            }
            Debug.Log($"Tracer duration {duration}");
            Rigidbody2D tracer = Instantiate(weapon.Data.Ammo.PrefabTracer, muzzlePoint.position, Quaternion.identity).GetComponent<Rigidbody2D>();
            tracer.velocity = direction * weapon.Data.Ammo.TracerSpeed;
            Destroy(tracer.gameObject, duration);
        }

        internal void CreateImpact(Vector2 position)
        {
            if (!weapon.Data.Ammo.PrefabImpact)
            {
                Debug.LogWarning($"Weapon [{weapon.Origin.Name}] not Prefab Impact!");
                return;
            }
            GameObject impact = Instantiate(weapon.Data.Ammo.PrefabImpact, position, Quaternion.identity);
            Destroy(impact, 1f);
        }

        internal void CreateDecal(Vector2 position)
        {
            if (!weapon.Data.Ammo.PrefabDecal)
            {
                Debug.LogWarning($"Weapon [{weapon.Origin.Name}] not Prefab Decal!");
                return;
            }
            Quaternion randomRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f,360f));
            GameObject decal = Instantiate(weapon.Data.Ammo.PrefabDecal, position, randomRotation);
            Destroy(decal, 10f);
        }

        internal void Heating(float percentHeat, float delta)
        {
            percentHeat = 1 - percentHeat;
            float speedHeating = 10f * delta;
            Color percentRed = new Color(1, percentHeat, percentHeat);
            render.color = Color.Lerp(render.color, percentRed, speedHeating);
        }

        internal void Cooling(float percentHeat, float delta)
        {
            Heating(percentHeat, delta);
        }

        //Sound
        //Ability Option
        //Component Option
    }
}

