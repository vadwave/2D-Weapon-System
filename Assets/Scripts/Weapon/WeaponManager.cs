using UnityEngine;
using UnityEngine.UI;

namespace WeaponSystem
{
    public class WeaponManager : MonoBehaviour
    {
        [SerializeField] LayerMask collidedMask;
        [SerializeField] Weapon[] weapons;
        [SerializeField] Transform[] arms;
        [SerializeField] Text text;
        private bool isReloaded;
        private float currentCapacity;
        private float maxCapacity;
        private CapacityType typeCapacity;

        private void Awake()
        {
            foreach (Weapon weapon in weapons)
            {
                weapon.Initialize(collidedMask);
                weapon.OnChangedCapacity += ChangeCapacity;
                weapon.OnStartReloaded += StartReloaded;
                weapon.OnStopReloaded += StopReloaded;
            }
        }
        private void Update()
        {
            Rotate();
            if (Input.GetMouseButtonDown(0))
            {
                Trigger(true, false);
            }
            else if (Input.GetMouseButton(0))
            {
                Trigger(false, true);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                Reload();
            }
        }
        void ChangeCapacity(float current, float max, CapacityType type)
        {
            currentCapacity = current;
            maxCapacity = max;
            typeCapacity = type;
            Show();
        }
        void Show()
        {
            switch (typeCapacity)
            {
                case CapacityType.Numeric: text.text = $"{currentCapacity}/{maxCapacity}"; break;
                case CapacityType.Percent: text.text = $"{(float)System.Math.Round((currentCapacity / maxCapacity) * 100, 2)}%"; break;
                default: text.text = $"{currentCapacity}"; break;
            }
            if(isReloaded) text.text += " Reloading";
        }
        void StartReloaded()
        {
            isReloaded = true;
            Show();
        }
        void StopReloaded()
        {
            isReloaded = false;
            Show();
        }

        void Trigger(bool isTrigger, bool isHold)
        {
            foreach (Weapon weapon in weapons)
            {
                if(weapon.enabled && weapon.gameObject.activeInHierarchy)
                    weapon.Trigger(isTrigger, isHold);
            }
        }
        void Reload()
        {
            foreach (Weapon weapon in weapons)
            {
                if (weapon.enabled && weapon.gameObject.activeInHierarchy)
                    weapon.TriggerReload();
            }
        }
        void Rotate()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            foreach(Transform transform in arms)
            {
                transform.right = mousePos - (Vector2)transform.position;
            }
        }
        private void OnDrawGizmos()
        {
            foreach (Transform transform in arms)
            {
                Weapon weapon = transform.GetComponentInChildren<Weapon>();
                if(weapon != null && weapon.Data != null)
                {
                    Debug.DrawRay(transform.position, transform.right * weapon.Data.Range, Color.red);
                    Debug.DrawRay(weapon.Model.ShootPoint.position, weapon.Model.ShootPoint.right * weapon.Data.Range, Color.blue);
                }
            }
        }
    }
}