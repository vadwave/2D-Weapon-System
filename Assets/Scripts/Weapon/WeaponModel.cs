using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private GameObject muzzlePrefab;

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
            GameObject muzzle = Instantiate(muzzlePrefab, muzzlePoint);
            Destroy(muzzle, 1f);
        }

        //Sound
        //Ability Option
        //Component Option
    }
}

