using System;
using UnityEngine;

namespace WeaponSystem
{
    [Serializable]
    public class WeaponAnimator
    {
        [SerializeField] Animator animator;

        internal void InsertRound()
        {

        }

        internal void OutOfAmmo()
        {

        }

        internal void Reload(bool roundInChamber)
        {

        }

        internal void Shot(bool lastRound)
        {
            animator.SetFloat("Speed", 1);
            animator.CrossFadeInFixedTime("Recoil", 0.1f);
        }         

        internal void StartReload(bool roundInChamber)
        {

        }

        internal void StopReload()
        {

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

        [SerializeField] private Transform handPoint;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private Transform shellPoint;
        [SerializeField] private Transform magazinePoint;

        [SerializeField] private GameObject magazinePrefab;

        private float reloadTime;
        private float insertTime;
        private float insertChamberTime;
        private float startReloadTime;
        private float stopReloadTime;
        private Weapon weapon;

        public void Init(Weapon weapon)
        {
            this.weapon = weapon;
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
            CompleteReloadDropDuration = new WaitForSeconds(reloadTime * 0.5f);
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

        //Sound
        //Ability Option
        //Component Option
    }
}

